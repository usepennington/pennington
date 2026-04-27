namespace Pennington.Generation;

using System.Collections.Concurrent;
using System.IO.Abstractions;
using Content;
using Diagnostics;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Routing;

/// <summary>
/// Generates a static site by HTTP-crawling the running app.
/// Pages are fetched in priority order: HTML content first, then MapGet routes (like /styles.css) last.
/// This ensures CSS class collectors have observed all HTML before the stylesheet is generated.
/// </summary>
public sealed class OutputGenerationService
{
    /// <summary>
    /// Sentinel URL fetched during site generation to produce <c>404.html</c>.
    /// The path is not a real content route — it exists only to trigger the
    /// catch-all fallback handler whose rendered HTML is written to disk.
    /// Other parts of the engine (e.g. <c>LocalizationOptions.GetAlternateLanguages</c>)
    /// must recognize this sentinel so language switchers on <c>404.html</c>
    /// don't emit phantom <c>/{locale}/__pennington-404-generator/</c> links.
    /// </summary>
    public const string NotFoundGeneratorPath = "/__pennington-404-generator";

    private readonly IEnumerable<IContentService> _contentServices;
    private readonly OutputOptions _outputOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IFileSystem _fileSystem;
    private readonly IInProcessHttpDispatcher _dispatcher;
    private readonly ILogger<OutputGenerationService> _logger;

    /// <summary>
    /// Initializes the service with the dependencies required to crawl the running app and write output.
    /// </summary>
    public OutputGenerationService(
        IEnumerable<IContentService> contentServices,
        OutputOptions outputOptions,
        PenningtonOptions pennOptions,
        IWebHostEnvironment environment,
        EndpointDataSource endpointDataSource,
        IFileSystem fileSystem,
        IInProcessHttpDispatcher dispatcher,
        ILogger<OutputGenerationService> logger)
    {
        _contentServices = contentServices;
        _outputOptions = outputOptions;
        _environment = environment;
        _endpointDataSource = endpointDataSource;
        _fileSystem = fileSystem;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    /// <summary>Crawls the running app and writes every discovered route to the output directory.</summary>
    public Task<BuildReport> GenerateAsync() => GenerateAsync(writeToDisk: true);

    /// <summary>
    /// Crawls the running app and returns a <see cref="BuildReport"/>. When
    /// <paramref name="writeToDisk"/> is <c>false</c> the output directory is left
    /// untouched — the HTTP crawl, diagnostic collection, and link verification still
    /// run, which makes this mode suitable for dev-time validators that want the
    /// same warnings a real build would produce.
    /// </summary>
    public async Task<BuildReport> GenerateAsync(bool writeToDisk)
    {
        var reportBuilder = new BuildReportBuilder();
        using var client = _dispatcher.CreateClient();

        var outputDir = _outputOptions.OutputDirectory.Value;

        // Phase 0: Detect markdown content source overlap (e.g. a catch-all source
        // whose ContentPath includes a subfolder owned by a more specific source).
        // Surfaced as warnings so users can see and fix the misconfig; duplicate
        // routes still flow through and will race in Phase 6 below.
        var markdownSources = _contentServices.OfType<IMarkdownContentSource>();
        foreach (var warning in MarkdownSourceOverlapDetector.DetectOverlaps(markdownSources))
        {
            reportBuilder.AddWarning(warning);
        }

        // Phase 1: Collect pages from content services. Deduplicate by output file
        // so Phase 6's parallel fetcher cannot race on the same path. When two
        // services emit the same output file (a common misconfiguration when a
        // custom IContentService overlaps the primary markdown source), the first
        // discovery wins and the duplicate is reported as a warning.
        var contentPages = new List<PageToGenerate>();
        var claimedOutputFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var service in _contentServices)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                var outputFile = item.Route.OutputFile.Value;
                if (!claimedOutputFiles.Add(outputFile))
                {
                    reportBuilder.AddWarning(
                        $"Duplicate route: output file '{outputFile}' is emitted by more than one " +
                        $"content service (URL: {item.Route.CanonicalPath.Value}). Keeping the first " +
                        $"discovery; the duplicate would otherwise race on the output file during parallel fetch.",
                        sourceFile: outputFile);
                    continue;
                }

                contentPages.Add(new PageToGenerate(item.Route));
            }
        }

        // Phase 2: Discover MapGet routes (includes /styles.css). A MapGet handler
        // whose URL is already claimed by a content-service discovery is dropped —
        // the content-service version wins. Without this, a route emitted as both
        // a DiscoveredItem AND a dedicated MapGet would be fetched twice (once in
        // Phase 6, once in Phase 7), wasting work and producing duplicate writes
        // to the same output file. The common fix at the call site is to serve
        // non-markdown content from a single catch-all MapGet("/{*path}") rather
        // than a dedicated endpoint that shadows the content service.
        var mapGetPages = new List<PageToGenerate>();
        foreach (var page in DiscoverMapGetRoutes())
        {
            if (!claimedOutputFiles.Add(page.OutputFile.Value))
            {
                reportBuilder.AddWarning(
                    $"Duplicate route: '{page.Url}' is emitted by both a content service and a MapGet handler. " +
                    $"The content-service discovery wins; the MapGet handler is skipped for static build. " +
                    $"Prefer serving this route from only one source — a catch-all MapGet(\"/{{*path}}\") is " +
                    $"the usual fix when custom endpoints need to coexist with content discovery.",
                    sourceFile: page.Url);
                continue;
            }

            mapGetPages.Add(page);
        }

        _logger.LogDebug("Static generation: {ContentCount} content pages, {MapGetCount} MapGet routes",
            contentPages.Count, mapGetPages.Count);

        // Phase 3: Clean output directory (if configured) and ensure it exists
        if (writeToDisk)
        {
            if (_outputOptions.CleanOutput && _fileSystem.Directory.Exists(outputDir))
                _fileSystem.Directory.Delete(outputDir, true);
            _fileSystem.Directory.CreateDirectory(outputDir);
        }

        // Phase 4: Copy static assets. In dry-run mode we still enumerate so copiedAssetPaths
        // is populated — LinkVerificationService needs those paths to recognize legitimate
        // asset URLs, otherwise every <img src="/media/foo.svg"> would flag as broken.
        var copiedAssetPaths = new List<string>();
        try
        {
            copiedAssetPaths = await CopyStaticAssetsAsync(outputDir, reportBuilder, writeToDisk);
        }
        catch (Exception ex)
        {
            reportBuilder.AddError("Failed to copy static assets", ex);
        }

        // Phase 5: Create dynamic content files (sitemap.xml, rss, llms.txt, etc.)
        if (writeToDisk)
        {
            try
            {
                await CreateContentFilesAsync(outputDir, reportBuilder);
            }
            catch (Exception ex)
            {
                reportBuilder.AddError("Failed to create dynamic content files", ex);
            }
        }

        // Phase 6: Fetch all HTML content pages (in parallel)
        var contentResults = await FetchPagesAsync(client, contentPages, outputDir, writeToDisk);
        ProcessFetchResults(reportBuilder, contentResults);

        // Phase 7: Fetch MapGet routes LAST (CSS needs all HTML to have been processed first)
        var mapGetResults = await FetchPagesAsync(client, mapGetPages, outputDir, writeToDisk);
        ProcessFetchResults(reportBuilder, mapGetResults);

        // Phase 8: Generate 404.html by fetching a non-existent URL (triggers fallback route)
        if (writeToDisk)
        {
            try
            {
                var notFoundResponse = await client.GetAsync(NotFoundGeneratorPath);
                var notFoundHtml = await notFoundResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(notFoundHtml))
                {
                    var notFoundPath = _fileSystem.Path.Combine(outputDir, "404.html");
                    await _fileSystem.File.WriteAllTextAsync(notFoundPath, notFoundHtml);
                    _logger.LogDebug("Generated 404.html");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to generate 404.html");
            }
        }

        // Phase 9: Verify internal links across all fetched HTML pages
        var allRoutes = contentPages.Concat(mapGetPages).Select(p => p.Route);
        var linkVerifier = new LinkVerificationService(
            allRoutes,
            copiedAssetPaths,
            _outputOptions.BaseUrl.Value);
        foreach (var result in contentResults.Concat(mapGetResults))
        {
            if (result is { Outcome: FetchOutcome.Generated, HtmlContent: { } html })
            {
                var linkResults = linkVerifier.VerifyLinks(result.Page.Route, html);
                foreach (var linkResult in linkResults)
                {
                    if (linkResult.Value is BrokenLinkResult broken)
                    {
                        reportBuilder.AddBrokenLink(
                            new BrokenLink(broken.SourcePage, broken.Url, broken.Type, broken.Reason));
                    }
                }
            }
        }

        return reportBuilder.Build();
    }

    private static void ProcessFetchResults(BuildReportBuilder reportBuilder, List<FetchResult> results)
    {
        foreach (var result in results)
        {
            switch (result.Outcome)
            {
                case FetchOutcome.Generated:
                case FetchOutcome.Redirect:
                    reportBuilder.AddGeneratedPage(result.Page.Route);
                    break;
                case FetchOutcome.Failed:
                case FetchOutcome.Error:
                    reportBuilder.AddError(result.Page.Route, result.Detail ?? "Unknown error");
                    break;
            }

            // Add per-request diagnostics from response headers
            foreach (var diag in result.Diagnostics)
            {
                reportBuilder.AddDiagnostic(new BuildDiagnostic(
                    diag.Severity, result.Page.Route, diag.Message, SourceFile: diag.Source));
            }
        }
    }

    private List<PageToGenerate> DiscoverMapGetRoutes()
    {
        var pages = new List<PageToGenerate>();

        foreach (var endpoint in _endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
                continue;

            var httpMethods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (httpMethods?.HttpMethods.Contains("GET") != true)
                continue;

            // Skip endpoints registered by app.MapStaticAssets(). Their DisplayName does not
            // contain "static files" (that pattern matched the legacy UseStaticFiles middleware),
            // so the old string filter missed them. Letting these routes through caused Phase 7's
            // parallel HTTP fetcher to overwrite files that Phase 4 (CopyStaticAssetsAsync) had
            // already emitted, producing "file is being used by another process" races on Windows
            // for /_content/<Rcl>/*.js and their fingerprinted aliases.
            if (routeEndpoint.Metadata.GetMetadata<StaticAssetDescriptor>() is not null)
                continue;

            var rawText = routeEndpoint.RoutePattern.RawText;
            if (string.IsNullOrWhiteSpace(rawText))
                continue;

            // Skip Blazor component routes, framework routes, legacy static files, and parameterized routes
            if (rawText.Contains("{") ||
                rawText.Contains("_framework") ||
                rawText.Contains("_blazor") ||
                endpoint.DisplayName?.Contains("static files") == true)
                continue;

            // Skip fallback routes (catch-all page routes)
            if (endpoint.Metadata.Any(m => m.GetType().Name == "FallbackMetadata"))
                continue;

            // Skip component-based routes
            if (endpoint.Metadata.Any(m => m.GetType().Name == "ComponentTypeMetadata"))
                continue;

            var url = rawText.StartsWith('/') ? rawText : "/" + rawText;

            // Determine output file — for something like /styles.css, output as styles.css
            var outputPath = url.TrimStart('/');
            if (string.IsNullOrEmpty(outputPath)) outputPath = "index.html";

            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath(url),
                OutputFile = new FilePath(outputPath),
            };
            pages.Add(new PageToGenerate(route));
            _logger.LogDebug("Discovered MapGet route: {Url} -> {OutputFile}", url, outputPath);
        }

        return pages;
    }

    private async Task<List<string>> CopyStaticAssetsAsync(string outputDir, BuildReportBuilder reportBuilder, bool writeToDisk)
    {
        // Track the relative output paths of every asset we copied from a content
        // service. These get handed to LinkVerificationService so it doesn't flag
        // <img src="/media/foo.svg"> as broken when the engine already copied the
        // file there itself.
        var copiedPaths = new List<string>();

        // Copy content directory assets (non-markdown files)
        foreach (var service in _contentServices)
        {
            var toCopy = await service.GetContentToCopyAsync();
            foreach (var item in toCopy)
            {
                if (writeToDisk)
                    CopyFile(item.SourcePath.Value, _fileSystem.Path.Combine(outputDir, item.OutputPath.Value), reportBuilder);
                copiedPaths.Add(item.OutputPath.Value);
            }
        }

        // Copy wwwroot + RCL static web assets. WebRootFileProvider is a CompositeFileProvider
        // whose GetDirectoryContents concatenates each child provider's entries without
        // deduping by name — so when the physical wwwroot and an RCL manifest provider both
        // own `_content/<Rcl>/`, the same logical path is yielded more than once and File.Copy
        // races itself on Windows ("file is being used by another process"). Dedupe by
        // relative path as we walk.
        if (writeToDisk)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CopyFileProvider(_environment.WebRootFileProvider, "", outputDir, reportBuilder, visited);
        }

        return copiedPaths;
    }

    private async Task CreateContentFilesAsync(string outputDir, BuildReportBuilder reportBuilder)
    {
        foreach (var service in _contentServices)
        {
            var toCreate = await service.GetContentToCreateAsync();
            foreach (var item in toCreate)
            {
                try
                {
                    var targetPath = _fileSystem.Path.Combine(outputDir, item.OutputPath.Value);
                    var dir = _fileSystem.Path.GetDirectoryName(targetPath);
                    if (dir != null) _fileSystem.Directory.CreateDirectory(dir);

                    var bytes = await item.ContentGenerator();
                    await _fileSystem.File.WriteAllBytesAsync(targetPath, bytes);
                }
                catch (Exception ex)
                {
                    reportBuilder.AddError($"Failed to create content file: {item.OutputPath.Value}", ex,
                        sourceFile: item.OutputPath.Value);
                }
            }
        }
    }

    private async Task<List<FetchResult>> FetchPagesAsync(HttpClient client, List<PageToGenerate> pages, string outputDir, bool writeToDisk)
    {
        var results = new ConcurrentBag<FetchResult>();
        await Parallel.ForEachAsync(pages, async (page, ct) =>
        {
            try
            {
                var response = await client.GetAsync(page.Url, ct);
                var outputPath = _fileSystem.Path.Combine(outputDir, page.OutputFile.Value);
                if (writeToDisk)
                {
                    var dir = _fileSystem.Path.GetDirectoryName(outputPath);
                    if (dir != null) _fileSystem.Directory.CreateDirectory(dir);
                }

                // Extract per-request diagnostics from response headers
                var diagnostics = ParseDiagnosticHeaders(response);

                if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                    response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    var location = response.Headers.Location?.ToString() ?? "/";
                    if (writeToDisk)
                    {
                        var redirectHtml = $"""
                            <!DOCTYPE html>
                            <html><head>
                            <meta http-equiv="refresh" content="0;url={location}">
                            <link rel="canonical" href="{location}">
                            </head></html>
                            """;
                        await _fileSystem.File.WriteAllTextAsync(outputPath, redirectHtml, ct);
                    }
                    _logger.LogDebug("Redirect: {Url} -> {Location}", page.Url, location);
                    results.Add(new FetchResult(page, FetchOutcome.Redirect, Diagnostics: diagnostics));
                }
                else if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                    if (contentType.StartsWith("text/") || contentType.Contains("json") || contentType.Contains("xml"))
                    {
                        var content = await response.Content.ReadAsStringAsync(ct);
                        if (writeToDisk)
                            await _fileSystem.File.WriteAllTextAsync(outputPath, content, ct);
                        _logger.LogDebug("Generated: {Url} ({StatusCode})", page.Url, (int)response.StatusCode);

                        // Capture HTML content for link verification
                        var htmlContent = contentType.Contains("html") ? content : null;
                        results.Add(new FetchResult(page, FetchOutcome.Generated, HtmlContent: htmlContent, Diagnostics: diagnostics));
                    }
                    else
                    {
                        if (writeToDisk)
                        {
                            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                            await _fileSystem.File.WriteAllBytesAsync(outputPath, bytes, ct);
                        }
                        _logger.LogDebug("Generated: {Url} ({StatusCode})", page.Url, (int)response.StatusCode);
                        results.Add(new FetchResult(page, FetchOutcome.Generated, Diagnostics: diagnostics));
                    }
                }
                else
                {
                    _logger.LogDebug("Failed: {Url} ({StatusCode})", page.Url, (int)response.StatusCode);
                    results.Add(new FetchResult(page, FetchOutcome.Failed,
                        $"HTTP {(int)response.StatusCode} fetching {page.Url}", Diagnostics: diagnostics));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating {Url}", page.Url);
                results.Add(new FetchResult(page, FetchOutcome.Error, ex.Message));
            }
        });
        return [.. results];
    }

    private void CopyFile(string source, string target, BuildReportBuilder reportBuilder)
    {
        try
        {
            var dir = _fileSystem.Path.GetDirectoryName(target);
            if (dir != null) _fileSystem.Directory.CreateDirectory(dir);
            _fileSystem.File.Copy(source, target, true);
        }
        catch (Exception ex)
        {
            reportBuilder.AddWarning($"Failed to copy {source} to {target}", sourceFile: source);
            _logger.LogDebug(ex, "Failed to copy {Source} to {Target}", source, target);
        }
    }

    private void CopyFileProvider(IFileProvider provider, string subpath, string outputDir, BuildReportBuilder reportBuilder, HashSet<string> visited)
    {
        var contents = provider.GetDirectoryContents(subpath);
        foreach (var item in contents)
        {
            var relativePath = string.IsNullOrEmpty(subpath) ? item.Name : $"{subpath}/{item.Name}";

            if (!visited.Add(relativePath))
                continue;

            if (item.IsDirectory)
            {
                CopyFileProvider(provider, relativePath, outputDir, reportBuilder, visited);
            }
            else if (item.PhysicalPath != null)
            {
                var targetPath = _fileSystem.Path.Combine(outputDir, relativePath);
                CopyFile(item.PhysicalPath, targetPath, reportBuilder);
            }
        }
    }

    private static List<Diagnostic> ParseDiagnosticHeaders(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("X-Pennington-Diagnostic", out var values))
            return [];

        return ParseDiagnosticHeaderValues(values);
    }

    internal static List<Diagnostic> ParseDiagnosticHeaderValues(IEnumerable<string> values)
    {
        var diagnostics = new List<Diagnostic>();
        foreach (var value in values)
        {
            var parts = value.Split('|', 3);
            if (parts.Length < 2) continue;
            if (!Enum.TryParse<DiagnosticSeverity>(Uri.UnescapeDataString(parts[0]), out var severity)) continue;
            var message = Uri.UnescapeDataString(parts[1]);
            var source = parts.Length > 2 ? Uri.UnescapeDataString(parts[2]) : null;
            diagnostics.Add(new Diagnostic(severity, message, source));
        }
        return diagnostics;
    }

    private record PageToGenerate(ContentRoute Route)
    {
        public string Url => Route.CanonicalPath.Value;
        public FilePath OutputFile => Route.OutputFile;
    }

    private enum FetchOutcome { Generated, Redirect, Failed, Error }

    private record FetchResult(
        PageToGenerate Page,
        FetchOutcome Outcome,
        string? Detail = null,
        string? HtmlContent = null,
        IReadOnlyList<Diagnostic>? Diagnostics = null)
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = Diagnostics ?? [];
    }
}
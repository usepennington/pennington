namespace Pennington.Generation;

using System.Collections.Concurrent;
using System.IO.Abstractions;
using Content;
using Diagnostics;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Pipeline;
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
    private readonly IEnumerable<IContentEmitter> _contentEmitters;
    private readonly OutputOptions _outputOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IFileSystem _fileSystem;
    private readonly IInProcessHttpDispatcher _dispatcher;
    private readonly IAuditCache _auditCache;
    private readonly ILogger<OutputGenerationService> _logger;

    /// <summary>
    /// Initializes the service with the dependencies required to crawl the running app and write output.
    /// </summary>
    public OutputGenerationService(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IContentEmitter> contentEmitters,
        OutputOptions outputOptions,
        IWebHostEnvironment environment,
        EndpointDataSource endpointDataSource,
        IFileSystem fileSystem,
        IInProcessHttpDispatcher dispatcher,
        IAuditCache auditCache,
        ILogger<OutputGenerationService> logger)
    {
        _contentServices = contentServices;
        _contentEmitters = contentEmitters;
        _outputOptions = outputOptions;
        _environment = environment;
        _endpointDataSource = endpointDataSource;
        _fileSystem = fileSystem;
        _dispatcher = dispatcher;
        _auditCache = auditCache;
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

        _logger.LogInformation("Discovering content");

        // Phase 1: Collect pages from content services. Deduplicate by output file
        // so Phase 6's parallel fetcher cannot race on the same path. When two
        // services emit the same output file (a common misconfiguration when a
        // custom IContentService overlaps the primary markdown source), the first
        // discovery wins and the duplicate is reported as a warning.
        var contentPages = new List<PageToGenerate>();
        var claimedOutputFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await foreach (var item in _contentServices.DiscoverAllAsync())
        {
            // Llms-only items contribute to the llms.txt sidecar and front
            // door but never produce an HTML page. Skip them so the crawler
            // doesn't try to fetch a route that has no rendered output and
            // so the output file isn't claimed (the same canonical slug may
            // legitimately host an HTML page from another service).
            if (item.Source is LlmsOnlySource)
            {
                continue;
            }

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

        _logger.LogInformation("Found {ContentCount} content pages, {EndpointCount} static endpoints",
            contentPages.Count, mapGetPages.Count);

        // Phase 3: Clean output directory (if configured) and ensure it exists
        if (writeToDisk)
        {
            if (_outputOptions.CleanOutput && _fileSystem.Directory.Exists(outputDir))
            {
                _fileSystem.Directory.Delete(outputDir, true);
            }

            _fileSystem.Directory.CreateDirectory(outputDir);
        }

        // Phase 4: Copy static assets. LinkAuditor walks IContentService.GetContentToCopyAsync()
        // directly to build its known-asset set, so this phase only needs to copy bytes.
        try
        {
            await CopyStaticAssetsAsync(outputDir, reportBuilder, writeToDisk);
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
        _logger.LogInformation("Generating pages");
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

        // Phase 8.5: Copy auditor diagnostics into the report. AuditRunner has been
        // priming IAuditCache on every file change since startup, so by the time the
        // build crawls the running app the cache reflects the same state the dev
        // overlay was showing locally — same diagnostics, two surfaces.
        foreach (var diag in _auditCache.Diagnostics)
        {
            reportBuilder.AddDiagnostic(diag);
            // Mirror LinkAuditor diagnostics into BuildReport.BrokenLinks for
            // backward compatibility. The diagnostics list is the canonical channel;
            // BrokenLinks remains populated until consumers migrate.
            if (TryParseBrokenLink(diag, out var brokenLink))
            {
#pragma warning disable CS0618
                reportBuilder.AddBrokenLink(brokenLink);
#pragma warning restore CS0618
            }
        }

        return reportBuilder.Build();
    }

    private static bool TryParseBrokenLink(BuildDiagnostic diagnostic, out BrokenLink brokenLink)
    {
        brokenLink = default!;
        if (diagnostic.Route is not { } route)
        {
            return false;
        }

        if (diagnostic.SourceFile is not { } source)
        {
            return false;
        }

        const string prefix = "content.links/";
        if (!source.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var rest = source[prefix.Length..];
        var slash = rest.IndexOf('/');
        if (slash <= 0)
        {
            return false;
        }

        if (!Enum.TryParse<LinkType>(rest[..slash], out var type))
        {
            return false;
        }

        var url = rest[(slash + 1)..];
        var reason = ExtractReason(diagnostic.Message);
        brokenLink = new BrokenLink(route, url, type, reason);
        return true;
    }

    private static string ExtractReason(string message)
    {
        var open = message.LastIndexOf('(');
        var close = message.LastIndexOf(')');
        if (open < 0 || close <= open)
        {
            return message;
        }

        return message[(open + 1)..close];
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
        foreach (var route in MapGetRouteDiscovery.Discover(_endpointDataSource))
        {
            pages.Add(new PageToGenerate(route));
            _logger.LogDebug("Discovered MapGet route: {Url} -> {OutputFile}", route.CanonicalPath.Value, route.OutputFile.Value);
        }
        return pages;
    }

    private async Task CopyStaticAssetsAsync(string outputDir, BuildReportBuilder reportBuilder, bool writeToDisk)
    {
        // Copy content directory assets (non-markdown files)
        foreach (var item in await _contentServices.CollectContentToCopyAsync())
        {
            if (writeToDisk)
            {
                CopyFile(item.SourcePath.Value, _fileSystem.Path.Combine(outputDir, item.OutputPath.Value), reportBuilder);
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
    }

    private async Task CreateContentFilesAsync(string outputDir, BuildReportBuilder reportBuilder)
    {
        foreach (var emitter in _contentServices.WithStandaloneEmitters(_contentEmitters))
        {
            var toCreate = await emitter.GetContentToCreateAsync();
            foreach (var item in toCreate)
            {
                try
                {
                    var targetPath = _fileSystem.Path.Combine(outputDir, item.OutputPath.Value);
                    var dir = _fileSystem.Path.GetDirectoryName(targetPath);
                    if (dir != null)
                    {
                        _fileSystem.Directory.CreateDirectory(dir);
                    }

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
                    if (dir != null)
                    {
                        _fileSystem.Directory.CreateDirectory(dir);
                    }
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
                        {
                            await _fileSystem.File.WriteAllTextAsync(outputPath, content, ct);
                        }

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
            if (dir != null)
            {
                _fileSystem.Directory.CreateDirectory(dir);
            }

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
            {
                continue;
            }

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
        {
            return [];
        }

        return ParseDiagnosticHeaderValues(values);
    }

    internal static List<Diagnostic> ParseDiagnosticHeaderValues(IEnumerable<string> values)
    {
        var diagnostics = new List<Diagnostic>();
        foreach (var value in values)
        {
            var parts = value.Split('|', 3);
            if (parts.Length < 2)
            {
                continue;
            }

            if (!Enum.TryParse<DiagnosticSeverity>(Uri.UnescapeDataString(parts[0]), out var severity))
            {
                continue;
            }

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
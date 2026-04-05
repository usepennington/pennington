namespace Penn.Generation;

using System.IO.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Penn.Content;
using Penn.Infrastructure;
using Penn.Routing;

/// <summary>
/// Generates a static site by HTTP-crawling the running app.
/// Pages are fetched in priority order: HTML content first, then MapGet routes (like /styles.css) last.
/// This ensures CSS class collectors have observed all HTML before the stylesheet is generated.
/// </summary>
public sealed class OutputGenerationService
{
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly OutputOptions _outputOptions;
    private readonly PennOptions _pennOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<OutputGenerationService> _logger;

    public OutputGenerationService(
        IEnumerable<IContentService> contentServices,
        OutputOptions outputOptions,
        PennOptions pennOptions,
        IWebHostEnvironment environment,
        EndpointDataSource endpointDataSource,
        IFileSystem fileSystem,
        ILogger<OutputGenerationService> logger)
    {
        _contentServices = contentServices;
        _outputOptions = outputOptions;
        _pennOptions = pennOptions;
        _environment = environment;
        _endpointDataSource = endpointDataSource;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task GenerateAsync(string appUrl)
    {
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        client.BaseAddress = new Uri(appUrl);

        var outputDir = _outputOptions.OutputDirectory.Value;

        // Phase 1: Collect pages from content services (Priority.Normal)
        var contentPages = new List<PageToGenerate>();
        foreach (var service in _contentServices)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                var url = item.Route.CanonicalPath.Value;
                contentPages.Add(new PageToGenerate(url, item.Route.OutputFile));
            }
        }

        // Phase 2: Discover MapGet routes (Priority.MustBeLast — includes /styles.css)
        var mapGetPages = DiscoverMapGetRoutes();

        _logger.LogInformation("Static generation: {ContentCount} content pages, {MapGetCount} MapGet routes",
            contentPages.Count, mapGetPages.Count);

        // Phase 3: Clear and recreate output directory
        if (_fileSystem.Directory.Exists(outputDir))
            _fileSystem.Directory.Delete(outputDir, true);
        _fileSystem.Directory.CreateDirectory(outputDir);

        // Phase 4: Copy static assets
        await CopyStaticAssetsAsync(outputDir);

        // Phase 5: Create dynamic content files
        await CreateContentFilesAsync(outputDir);

        // Phase 6: Fetch all HTML content pages (in parallel)
        _logger.LogInformation("Fetching {Count} content pages...", contentPages.Count);
        await FetchPagesAsync(client, contentPages, outputDir);

        // Phase 7: Fetch MapGet routes LAST (CSS needs all HTML to have been processed first)
        _logger.LogInformation("Fetching {Count} MapGet routes (styles.css etc.)...", mapGetPages.Count);
        await FetchPagesAsync(client, mapGetPages, outputDir);

        _logger.LogInformation("Static generation complete. Output: {OutputDir}", outputDir);
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

            var rawText = routeEndpoint.RoutePattern.RawText;
            if (string.IsNullOrWhiteSpace(rawText))
                continue;

            // Skip Blazor component routes, framework routes, static files, and parameterized routes
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

            pages.Add(new PageToGenerate(url, new FilePath(outputPath)));
            _logger.LogDebug("Discovered MapGet route: {Url} -> {OutputFile}", url, outputPath);
        }

        return pages;
    }

    private async Task CopyStaticAssetsAsync(string outputDir)
    {
        // Copy content directory assets (non-markdown files)
        foreach (var service in _contentServices)
        {
            var toCopy = await service.GetContentToCopyAsync();
            foreach (var item in toCopy)
            {
                CopyFile(item.SourcePath.Value, _fileSystem.Path.Combine(outputDir, item.OutputPath.Value));
            }
        }

        // Copy wwwroot static web assets
        CopyFileProvider(_environment.WebRootFileProvider, "", outputDir);

        // Copy RCL static web assets (Penn.UI scripts, etc.)
        // CompositeFileProvider contains all registered static web asset providers
        if (_environment.WebRootFileProvider is CompositeFileProvider composite)
        {
            foreach (var provider in composite.FileProviders)
            {
                if (provider != _environment.WebRootFileProvider)
                {
                    CopyFileProvider(provider, "", outputDir);
                }
            }
        }
    }

    private async Task CreateContentFilesAsync(string outputDir)
    {
        foreach (var service in _contentServices)
        {
            var toCreate = await service.GetContentToCreateAsync();
            foreach (var item in toCreate)
            {
                var targetPath = _fileSystem.Path.Combine(outputDir, item.OutputPath.Value);
                var dir = _fileSystem.Path.GetDirectoryName(targetPath);
                if (dir != null) _fileSystem.Directory.CreateDirectory(dir);

                var bytes = await item.ContentGenerator();
                await _fileSystem.File.WriteAllBytesAsync(targetPath, bytes);
            }
        }
    }

    private async Task FetchPagesAsync(HttpClient client, List<PageToGenerate> pages, string outputDir)
    {
        await Parallel.ForEachAsync(pages, async (page, ct) =>
        {
            try
            {
                var response = await client.GetAsync(page.Url, ct);
                var outputPath = _fileSystem.Path.Combine(outputDir, page.OutputFile.Value);
                var dir = _fileSystem.Path.GetDirectoryName(outputPath);
                if (dir != null) _fileSystem.Directory.CreateDirectory(dir);

                if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                    response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    var location = response.Headers.Location?.ToString() ?? "/";
                    var redirectHtml = $"""
                        <!DOCTYPE html>
                        <html><head>
                        <meta http-equiv="refresh" content="0;url={location}">
                        <link rel="canonical" href="{location}">
                        </head></html>
                        """;
                    await _fileSystem.File.WriteAllTextAsync(outputPath, redirectHtml, ct);
                    _logger.LogInformation("  Redirect: {Url} -> {Location}", page.Url, location);
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Check if response is binary
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                    if (contentType.StartsWith("text/") || contentType.Contains("json") || contentType.Contains("xml"))
                    {
                        var content = await response.Content.ReadAsStringAsync(ct);
                        await _fileSystem.File.WriteAllTextAsync(outputPath, content, ct);
                    }
                    else
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                        await _fileSystem.File.WriteAllBytesAsync(outputPath, bytes, ct);
                    }
                    _logger.LogDebug("  Generated: {Url} ({StatusCode})", page.Url, (int)response.StatusCode);
                }
                else
                {
                    _logger.LogWarning("  Failed: {Url} ({StatusCode})", page.Url, (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "  Error generating {Url}", page.Url);
            }
        });
    }

    private void CopyFile(string source, string target)
    {
        try
        {
            var dir = _fileSystem.Path.GetDirectoryName(target);
            if (dir != null) _fileSystem.Directory.CreateDirectory(dir);
            _fileSystem.File.Copy(source, target, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to copy {Source} to {Target}", source, target);
        }
    }

    private void CopyFileProvider(IFileProvider provider, string subpath, string outputDir)
    {
        var contents = provider.GetDirectoryContents(subpath);
        foreach (var item in contents)
        {
            var relativePath = string.IsNullOrEmpty(subpath) ? item.Name : $"{subpath}/{item.Name}";

            if (item.IsDirectory)
            {
                CopyFileProvider(provider, relativePath, outputDir);
            }
            else if (item.PhysicalPath != null)
            {
                var targetPath = _fileSystem.Path.Combine(outputDir, relativePath);
                CopyFile(item.PhysicalPath, targetPath);
            }
        }
    }

    private record PageToGenerate(string Url, FilePath OutputFile);
}

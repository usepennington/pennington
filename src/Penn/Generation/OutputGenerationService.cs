namespace Penn.Generation;

using Penn.Content;
using Penn.Infrastructure;
using Penn.Routing;

/// <summary>Generates static HTML by HTTP-crawling the running app.</summary>
public sealed class OutputGenerationService
{
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly OutputOptions _outputOptions;
    private readonly PennOptions _pennOptions;

    public OutputGenerationService(
        IEnumerable<IContentService> contentServices,
        OutputOptions outputOptions,
        PennOptions pennOptions)
    {
        _contentServices = contentServices;
        _outputOptions = outputOptions;
        _pennOptions = pennOptions;
    }

    public async Task GenerateAsync(string appUrl)
    {
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        client.BaseAddress = new Uri(appUrl);

        // Collect all pages to generate via discovery
        var pages = new List<(string Url, FilePath OutputFile)>();
        foreach (var service in _contentServices)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                var url = item.Route.CanonicalPath.EnsureTrailingSlash().Value;
                pages.Add((url, item.Route.OutputFile));
            }
        }

        // Create output directory
        var outputDir = _outputOptions.OutputDirectory.Value;
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        // Copy static assets
        foreach (var service in _contentServices)
        {
            var toCopy = await service.GetContentToCopyAsync();
            foreach (var item in toCopy)
            {
                var targetPath = Path.Combine(outputDir, item.OutputPath.Value);
                var targetDir = Path.GetDirectoryName(targetPath);
                if (targetDir != null) Directory.CreateDirectory(targetDir);
                File.Copy(item.SourcePath.Value, targetPath, true);
            }
        }

        // Fetch and save each page
        await Parallel.ForEachAsync(pages, async (page, ct) =>
        {
            try
            {
                var response = await client.GetAsync(page.Url, ct);
                var outputPath = Path.Combine(outputDir, page.OutputFile.Value);
                var dir = Path.GetDirectoryName(outputPath);
                if (dir != null) Directory.CreateDirectory(dir);

                if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                {
                    var location = response.Headers.Location?.ToString() ?? "/";
                    var redirectHtml = $"<!DOCTYPE html><html><head><meta http-equiv=\"refresh\" content=\"0;url={location}\"></head></html>";
                    await File.WriteAllTextAsync(outputPath, redirectHtml, ct);
                }
                else if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    await File.WriteAllTextAsync(outputPath, content, ct);
                }
            }
            catch
            {
                // Log but don't fail the whole build for one page
            }
        });
    }
}

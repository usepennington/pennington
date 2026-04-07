namespace Penn.LlmsTxt;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.Navigation;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Generates llms.txt index and stripped markdown files.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — no manual watcher subscription needed.
/// </summary>
public sealed class LlmsTxtService
{
    private readonly AsyncLazy<LlmsTxtData> _dataLazy;

    public LlmsTxtService(
        IServiceProvider serviceProvider,
        PennOptions pennOptions,
        LlmsTxtOptions llmsTxtOptions,
        NavigationBuilder navigationBuilder)
    {
        _dataLazy = new AsyncLazy<LlmsTxtData>(
            () => BuildAsync(serviceProvider, pennOptions, llmsTxtOptions, navigationBuilder));
    }

    public async Task<string> GetLlmsTxtAsync() => (await _dataLazy.Value).IndexContent;

    public async Task<ImmutableList<MarkdownFile>> GetMarkdownFilesAsync() => (await _dataLazy.Value).MarkdownFiles;

    public async Task<string?> GetLlmsFullTxtAsync() => (await _dataLazy.Value).FullContent;

    private static async Task<LlmsTxtData> BuildAsync(
        IServiceProvider sp,
        PennOptions pennOptions,
        LlmsTxtOptions llmsTxtOptions,
        NavigationBuilder navigationBuilder)
    {
        var contentServices = sp.GetServices<IContentService>();
        var parser = sp.GetRequiredService<IContentParser>();

        // Collect TOC entries and parsed content from all services
        var allTocItems = new List<ContentTocItem>();
        var parsedByPath = new Dictionary<string, ParsedItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var service in contentServices)
        {
            // Skip the LlmsTxtContentService to avoid circular queries
            if (service is LlmsTxtContentService) continue;

            var tocItems = await service.GetContentTocEntriesAsync();
            allTocItems.AddRange(tocItems);

            await foreach (var discovered in service.DiscoverAsync())
            {
                if (discovered.Source is not MarkdownFileSource) continue;

                var parseResult = await parser.ParseAsync(discovered);
                if (parseResult is not ParsedItem parsed) continue;

                // Skip redirects
                if (parsed.Metadata is IRedirectable { RedirectUrl: not null }) continue;

                var key = NormalizePath(parsed.Route.CanonicalPath.Value);
                parsedByPath[key] = parsed;
            }
        }

        // Build navigation tree for hierarchical structure
        var tree = navigationBuilder.BuildTree(allTocItems);

        // Check for a user-provided llms.txt header in the content root
        var header = await ReadUserHeaderAsync(sp, pennOptions);

        // Generate index and collect markdown files
        var sb = new StringBuilder();
        if (header is not null)
        {
            sb.AppendLine(header.TrimEnd());
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine($"# {pennOptions.SiteTitle}");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(pennOptions.SiteDescription))
            {
                sb.AppendLine($"> {pennOptions.SiteDescription}");
                sb.AppendLine();
            }
        }

        var markdownFiles = ImmutableList.CreateBuilder<MarkdownFile>();
        var fullContentBuilder = llmsTxtOptions.GenerateFullFile ? new StringBuilder() : null;

        WriteTree(tree, sb, markdownFiles, fullContentBuilder, parsedByPath, llmsTxtOptions.OutputDirectory);

        return new LlmsTxtData(
            sb.ToString().TrimEnd() + "\n",
            markdownFiles.ToImmutable(),
            fullContentBuilder?.ToString().TrimEnd());
    }

    private static void WriteTree(
        ImmutableList<NavigationTreeItem> items,
        StringBuilder sb,
        ImmutableList<MarkdownFile>.Builder markdownFiles,
        StringBuilder? fullContent,
        Dictionary<string, ParsedItem> parsedByPath,
        string outputDirectory)
    {
        foreach (var item in items)
        {
            if (item.Children.Count > 0)
            {
                // Section header
                sb.AppendLine($"## {item.Title}");
                sb.AppendLine();

                // Write leaf entries for this section, then recurse into subsections
                WriteTree(item.Children, sb, markdownFiles, fullContent, parsedByPath, outputDirectory);
            }
            else
            {
                // Leaf node — write entry if we have parsed content
                var key = NormalizePath(item.Route.CanonicalPath.Value);
                if (!parsedByPath.TryGetValue(key, out var parsed)) continue;

                var mdPath = $"{outputDirectory}/{key}.md";
                var description = parsed.Metadata is IDescribable { Description: { } desc }
                    ? $": {desc}"
                    : "";

                sb.AppendLine($"- [{item.Title}]({mdPath}){description}");

                markdownFiles.Add(new MarkdownFile(
                    new FilePath(mdPath),
                    Encoding.UTF8.GetBytes(parsed.RawMarkdown)));

                if (fullContent is not null)
                {
                    fullContent.AppendLine($"# {item.Title}");
                    fullContent.AppendLine();
                    fullContent.AppendLine(parsed.RawMarkdown);
                    fullContent.AppendLine();
                    fullContent.AppendLine("---");
                    fullContent.AppendLine();
                }
            }
        }

        // Add blank line after a section's entries for readability
        if (items.Any(i => i.Children.Count == 0))
            sb.AppendLine();
    }

    private static async Task<string?> ReadUserHeaderAsync(IServiceProvider sp, PennOptions pennOptions)
    {
        var fileSystem = sp.GetRequiredService<IFileSystem>();
        var env = sp.GetService<IWebHostEnvironment>();
        if (env is null) return null;

        var contentRoot = Path.IsPathRooted(pennOptions.ContentRootPath)
            ? pennOptions.ContentRootPath
            : Path.Combine(env.ContentRootPath, pennOptions.ContentRootPath);

        var llmsTxtPath = fileSystem.Path.Combine(contentRoot, "llms.txt");
        if (!fileSystem.File.Exists(llmsTxtPath)) return null;

        return await fileSystem.File.ReadAllTextAsync(llmsTxtPath);
    }

    private static string NormalizePath(string canonicalPath)
        => canonicalPath.Trim('/');

    internal record LlmsTxtData(
        string IndexContent,
        ImmutableList<MarkdownFile> MarkdownFiles,
        string? FullContent);

    public record MarkdownFile(FilePath OutputPath, byte[] Content);
}

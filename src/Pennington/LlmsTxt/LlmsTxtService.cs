namespace Pennington.LlmsTxt;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text;
using Content;
using FrontMatter;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigation;
using Pipeline;
using Routing;

/// <summary>
/// Generates llms.txt index and stripped markdown files.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — no manual watcher subscription needed.
/// <para>
/// Post-pipeline HTML is fetched from the running host via <see cref="RenderedHtmlFetcher"/>
/// and converted to markdown, so the output reflects Markdig extensions, Razor SSR,
/// xref resolution, and other middleware transforms.
/// </para>
/// </summary>
public sealed class LlmsTxtService
{
    private readonly AsyncLazy<LlmsTxtData> _dataLazy;

    /// <summary>Creates the service; data is computed lazily on first request.</summary>
    public LlmsTxtService(
        IServiceProvider serviceProvider,
        PenningtonOptions pennOptions,
        LlmsTxtOptions llmsTxtOptions,
        NavigationBuilder navigationBuilder,
        RenderedHtmlFetcher fetcher,
        ILogger<LlmsTxtService> logger)
    {
        _dataLazy = new AsyncLazy<LlmsTxtData>(
            () => BuildAsync(serviceProvider, pennOptions, llmsTxtOptions, navigationBuilder, fetcher, logger));
    }

    /// <summary>Returns the generated llms.txt index content.</summary>
    public async Task<string> GetLlmsTxtAsync() => (await _dataLazy.Value).IndexContent;

    /// <summary>Returns the per-page stripped markdown files emitted alongside llms.txt.</summary>
    public async Task<ImmutableList<MarkdownFile>> GetMarkdownFilesAsync() => (await _dataLazy.Value).MarkdownFiles;

    /// <summary>Returns the optional concatenated llms-full.txt content, or null when disabled.</summary>
    public async Task<string?> GetLlmsFullTxtAsync() => (await _dataLazy.Value).FullContent;

    private static async Task<LlmsTxtData> BuildAsync(
        IServiceProvider sp,
        PenningtonOptions pennOptions,
        LlmsTxtOptions llmsTxtOptions,
        NavigationBuilder navigationBuilder,
        RenderedHtmlFetcher fetcher,
        ILogger<LlmsTxtService> logger)
    {
        var contentServices = sp.GetServices<IContentService>();
        var parser = sp.GetRequiredService<IContentParser>();

        // Collect TOC entries and parsed metadata from all services
        var allTocItems = new List<ContentTocItem>();
        var metadataByPath = new Dictionary<string, IFrontMatter>(StringComparer.OrdinalIgnoreCase);

        foreach (var service in contentServices)
        {
            // Skip the LlmsTxtContentService to avoid circular queries
            if (service is LlmsTxtContentService) continue;

            var tocItems = await service.GetIndexableEntriesAsync();
            allTocItems.AddRange(tocItems.Where(t => !t.ExcludeFromLlms));

            await foreach (var discovered in service.DiscoverAsync())
            {
                if (discovered.Source is not MarkdownFileSource) continue;

                var parseResult = await parser.ParseAsync(discovered);
                if (parseResult is not ParsedItem parsed) continue;

                // Skip redirects
                if (parsed.Metadata is IRedirectable { RedirectUrl: not null }) continue;

                var key = NormalizePath(parsed.Route.CanonicalPath.Value);
                metadataByPath[key] = parsed.Metadata;
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

        // Collect the set of leaf paths that will have _llms/*.md files so we
        // can rewrite intra-site links inside the converted markdown to point
        // at sibling stripped files.
        var linkablePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectLeafPaths(tree, linkablePaths);
        var rewriteHref = BuildLinkRewriter(linkablePaths, llmsTxtOptions.OutputDirectory);

        await WriteTreeAsync(tree, sb, markdownFiles, fullContentBuilder, metadataByPath,
            llmsTxtOptions, fetcher, rewriteHref, logger);

        return new LlmsTxtData(
            sb.ToString().TrimEnd() + "\n",
            markdownFiles.ToImmutable(),
            fullContentBuilder?.ToString().TrimEnd());
    }

    private static void CollectLeafPaths(ImmutableList<NavigationTreeItem> items, HashSet<string> acc)
    {
        foreach (var item in items)
        {
            if (item.Children.Count > 0)
                CollectLeafPaths(item.Children, acc);
            else
                acc.Add(NormalizePath(item.Route.CanonicalPath.Value));
        }
    }

    private static Func<string, string> BuildLinkRewriter(HashSet<string> linkablePaths, string outputDirectory)
    {
        return href =>
        {
            // Pass through anything that isn't a path on this site.
            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("//")
                || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            {
                return href;
            }

            // Split off query and fragment so we can keep them on the rewritten URL.
            var fragmentIdx = href.IndexOf('#');
            var fragment = fragmentIdx >= 0 ? href[fragmentIdx..] : "";
            var beforeFragment = fragmentIdx >= 0 ? href[..fragmentIdx] : href;

            var queryIdx = beforeFragment.IndexOf('?');
            var query = queryIdx >= 0 ? beforeFragment[queryIdx..] : "";
            var pathPart = queryIdx >= 0 ? beforeFragment[..queryIdx] : beforeFragment;

            var key = NormalizePath(pathPart);
            if (string.IsNullOrEmpty(key) || !linkablePaths.Contains(key))
                return href;

            return $"{outputDirectory}/{key}.md{query}{fragment}";
        };
    }

    private static async Task WriteTreeAsync(
        ImmutableList<NavigationTreeItem> items,
        StringBuilder sb,
        ImmutableList<MarkdownFile>.Builder markdownFiles,
        StringBuilder? fullContent,
        Dictionary<string, IFrontMatter> metadataByPath,
        LlmsTxtOptions llmsTxtOptions,
        RenderedHtmlFetcher fetcher,
        Func<string, string> rewriteHref,
        ILogger logger)
    {
        foreach (var item in items)
        {
            if (item.Children.Count > 0)
            {
                // Section header
                sb.AppendLine($"## {item.Title}");
                sb.AppendLine();

                // Write leaf entries for this section, then recurse into subsections
                await WriteTreeAsync(item.Children, sb, markdownFiles, fullContent, metadataByPath,
                    llmsTxtOptions, fetcher, rewriteHref, logger);
            }
            else
            {
                // Leaf node — fetch rendered HTML and write entry
                var key = NormalizePath(item.Route.CanonicalPath.Value);
                // May be absent for Razor pages or synthetic nav leaves — treat as no metadata.
                metadataByPath.TryGetValue(key, out var metadata);

                var element = await fetcher.FetchContentAsync(
                    item.Route.CanonicalPath.Value, llmsTxtOptions.ContentSelector);

                if (element is null)
                {
                    logger.LogWarning("LlmsTxtService: failed to fetch {Path}, skipping", item.Route.CanonicalPath.Value);
                    continue;
                }

                var markdown = HtmlToMarkdownConverter.Convert(element, rewriteHref);

                var mdPath = $"{llmsTxtOptions.OutputDirectory}/{key}.md";
                var description = metadata?.Description is { } desc
                    ? $": {desc}"
                    : "";

                sb.AppendLine($"- [{item.Title}]({mdPath}){description}");

                markdownFiles.Add(new MarkdownFile(
                    new FilePath(mdPath),
                    Encoding.UTF8.GetBytes(markdown)));

                if (fullContent is not null)
                {
                    fullContent.AppendLine($"# {item.Title}");
                    fullContent.AppendLine();
                    fullContent.AppendLine(markdown);
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

    private static async Task<string?> ReadUserHeaderAsync(IServiceProvider sp, PenningtonOptions pennOptions)
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

    /// <summary>A stripped markdown file produced for the llms output.</summary>
    /// <param name="OutputPath">Relative output path for the markdown file.</param>
    /// <param name="Content">UTF-8 bytes of the stripped markdown body.</param>
    public record MarkdownFile(FilePath OutputPath, byte[] Content);
}
namespace Pennington.LlmsTxt;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using AngleSharp.Dom;
using Content;
using FrontMatter;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Navigation;
using Pipeline;
using Routing;

/// <summary>
/// Generates llms.txt index and stripped markdown files.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — no manual watcher subscription needed.
/// <para>
/// Folds over <see cref="ISiteProjection"/>: every renderable page's post-pipeline
/// HTML is already captured by the shared projection, so this service is a pure
/// adapter from <see cref="RenderedPage"/> + <see cref="HtmlToMarkdownConverter"/>
/// to the front-door index, per-subtree index files, and per-page sidecar markdown.
/// </para>
/// </summary>
public sealed class LlmsTxtService : IFileWatchAware
{
    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    private static readonly string PackageVersion = PenningtonVersion.Value;

    private readonly AsyncLazy<LlmsTxtData> _dataLazy;

    /// <summary>Creates the service; data is computed lazily on first request.</summary>
    public LlmsTxtService(
        ISiteProjection projection,
        IEnumerable<IContentService> contentServices,
        IEnumerable<LlmsSubtree> subtrees,
        IFileSystem fileSystem,
        IWebHostEnvironment hostingEnvironment,
        PenningtonOptions pennOptions,
        LlmsTxtOptions llmsTxtOptions,
        CanonicalBaseUrl canonicalBase,
        NavigationBuilder navigationBuilder,
        ILogger<LlmsTxtService> logger)
    {
        _dataLazy = new AsyncLazy<LlmsTxtData>(
            () => BuildAsync(
                projection, contentServices, subtrees,
                fileSystem, hostingEnvironment,
                pennOptions, llmsTxtOptions, canonicalBase, navigationBuilder,
                logger));
    }

    /// <summary>Returns the generated llms.txt index content.</summary>
    public async Task<string> GetLlmsTxtAsync() => (await _dataLazy).IndexContent;

    /// <summary>Returns the per-page stripped markdown files emitted alongside llms.txt.</summary>
    public async Task<ImmutableList<MarkdownFile>> GetMarkdownFilesAsync() => (await _dataLazy).MarkdownFiles;

    /// <summary>Returns per-subtree <c>{prefix}llms.txt</c> index files split out of the front door.</summary>
    public async Task<ImmutableList<MarkdownFile>> GetSubtreeFilesAsync() => (await _dataLazy).SubtreeFiles;

    /// <summary>Returns the optional concatenated llms-full.txt content, or null when disabled.</summary>
    public async Task<string?> GetLlmsFullTxtAsync() => (await _dataLazy).FullContent;

    private static async Task<LlmsTxtData> BuildAsync(
        ISiteProjection projection,
        IEnumerable<IContentService> contentServices,
        IEnumerable<LlmsSubtree> programmaticSubtrees,
        IFileSystem fileSystem,
        IWebHostEnvironment hostingEnvironment,
        PenningtonOptions pennOptions,
        LlmsTxtOptions llmsTxtOptions,
        CanonicalBaseUrl canonicalBase,
        NavigationBuilder navigationBuilder,
        ILogger<LlmsTxtService> logger)
    {
        // Fold the shared projection into a path-keyed map. The projection already
        // walks the corpus, fetches each route, and parses the DOM once — every
        // sidecar entry below just consumes the cached RenderedPage.
        var pageByPath = new Dictionary<string, RenderedPage>(StringComparer.OrdinalIgnoreCase);
        var allTocItems = new List<ContentTocItem>();
        await foreach (var page in projection.GetPagesAsync())
        {
            if (page.Toc.ExcludeFromLlms)
            {
                continue;
            }

            allTocItems.Add(page.Toc);
            pageByPath[NormalizePath(page.Route.CanonicalPath.Value)] = page;
        }

        var tree = await navigationBuilder.BuildTreeAsync(allTocItems);
        var subtrees = await CollectSubtreesAsync(contentServices, programmaticSubtrees);
        var header = await ReadUserHeaderAsync(fileSystem, hostingEnvironment, pennOptions);

        // SearchOnly entries (blog posts, for example) are filtered out of the
        // navigation tree, so the walk below never reaches them. Collect the ones
        // that fall under a declared subtree into their own tree so they still get
        // a {prefix}llms.txt — clearing SearchOnly so BuildTree keeps them.
        var subtreeOnlyItems = allTocItems
            .Where(t => t.SearchOnly && MatchSubtree(t.Route.CanonicalPath.Value, subtrees) is not null)
            .Select(t => t with { SearchOnly = false })
            .ToList();
        var subtreeOnlyTree = subtreeOnlyItems.Count > 0
            ? await navigationBuilder.BuildTreeAsync(subtreeOnlyItems)
            : ImmutableList<NavigationTreeItem>.Empty;

        var linkablePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectLeafPaths(tree, linkablePaths);
        CollectLeafPaths(subtreeOnlyTree, linkablePaths);

        var ctx = new BuildContext(
            Options: llmsTxtOptions,
            CanonicalBase: canonicalBase,
            Logger: logger,
            PageByPath: pageByPath,
            RewriteHref: BuildLinkRewriter(linkablePaths, llmsTxtOptions.OutputDirectory, canonicalBase),
            Nodes: new List<RenderedNode>(),
            MarkdownFiles: ImmutableList.CreateBuilder<MarkdownFile>(),
            FullContent: llmsTxtOptions.GenerateFullFile ? new StringBuilder() : null);

        await CollectAsync(tree, depth: 0, ctx);
        await CollectAsync(subtreeOnlyTree, depth: 0, ctx);

        // Front-door index
        var mainSb = new StringBuilder();
        AppendFrontDoorPreamble(mainSb, header, pennOptions, canonicalBase);
        AppendMapBlock(mainSb, subtrees, ctx.Nodes, canonicalBase);
        RenderBucket(ctx.Nodes, mainSb, leaf => MatchSubtree(leaf.CanonicalPath, subtrees) is null);

        // Per-subtree index files
        var subtreeFiles = ImmutableList.CreateBuilder<MarkdownFile>();
        foreach (var subtree in subtrees)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {subtree.Title}");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(subtree.Description))
            {
                sb.AppendLine($"> {subtree.Description}");
                sb.AppendLine();
            }

            var (entryCount, totalTokens) = SummarizeSubtree(ctx.Nodes, subtree, subtrees);
            var canonicalSelf = canonicalBase.Combine(new UrlPath($"/{subtree.RoutePrefix.TrimStart('/')}llms.txt")).Value;
            sb.AppendLine($"canonical: {canonicalSelf}");
            sb.AppendLine($"entries: {entryCount}");
            sb.AppendLine($"tokens: ~{FormatTokenEstimate(totalTokens)}");
            sb.AppendLine();

            RenderBucket(ctx.Nodes, sb,
                leaf => ReferenceEquals(MatchSubtree(leaf.CanonicalPath, subtrees), subtree));

            var path = new FilePath(subtree.RoutePrefix.TrimStart('/') + "llms.txt");
            subtreeFiles.Add(new MarkdownFile(path, Encoding.UTF8.GetBytes(sb.ToString().TrimEnd() + "\n")));
        }

        return new LlmsTxtData(
            mainSb.ToString().TrimEnd() + "\n",
            ctx.MarkdownFiles.ToImmutable(),
            subtreeFiles.ToImmutable(),
            ctx.FullContent?.ToString().TrimEnd());
    }

    private static async Task<ImmutableList<LlmsSubtree>> CollectSubtreesAsync(
        IEnumerable<IContentService> contentServices,
        IEnumerable<LlmsSubtree> programmaticSubtrees)
    {
        // Programmatic registrations win on prefix collision: collect discovered first, overwrite with programmatic.
        var byPrefix = new Dictionary<string, LlmsSubtree>(StringComparer.OrdinalIgnoreCase);

        foreach (var service in contentServices)
        {
            if (service is not ILlmsSubtreeProvider provider)
            {
                continue;
            }

            var discovered = await provider.GetLlmsSubtreesAsync();
            foreach (var s in discovered)
            {
                byPrefix[s.RoutePrefix] = s;
            }
        }

        foreach (var s in programmaticSubtrees)
        {
            byPrefix[s.RoutePrefix] = s;
        }

        // Order by descending prefix length so longest-prefix match is first when iterating.
        return byPrefix.Values
            .OrderByDescending(s => s.RoutePrefix.Length)
            .ThenBy(s => s.RoutePrefix, StringComparer.Ordinal)
            .ToImmutableList();
    }

    private static LlmsSubtree? MatchSubtree(string canonicalPath, ImmutableList<LlmsSubtree> subtrees)
    {
        // Subtrees are pre-sorted by descending RoutePrefix length, so the first match is the longest.
        foreach (var s in subtrees)
        {
            if (canonicalPath.StartsWith(s.RoutePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return s;
            }
        }
        return null;
    }

    private static void AppendFrontDoorPreamble(StringBuilder sb, string? userHeader, PenningtonOptions pennOptions, CanonicalBaseUrl canonicalBase)
    {
        if (userHeader is not null)
        {
            sb.AppendLine(userHeader.TrimEnd());
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

        var canonicalSelf = canonicalBase.Combine(new UrlPath("/llms.txt")).Value;
        sb.AppendLine($"site: {canonicalBase.Value.Value}");
        sb.AppendLine($"canonical: {canonicalSelf}");
        sb.AppendLine($"generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine($"penningtonVersion: {PackageVersion}");
        sb.AppendLine();
    }

    private static void AppendMapBlock(StringBuilder sb, ImmutableList<LlmsSubtree> subtrees, List<RenderedNode> renderedNodes, CanonicalBaseUrl canonicalBase)
    {
        if (subtrees.Count == 0)
        {
            return;
        }

        sb.AppendLine("## Map");
        sb.AppendLine();
        // Subtrees are sorted by descending RoutePrefix length for matching; readers
        // prefer top-level prefixes first.
        foreach (var s in subtrees.OrderBy(x => x.RoutePrefix, StringComparer.Ordinal))
        {
            var url = canonicalBase.Combine(new UrlPath($"{s.RoutePrefix}llms.txt")).Value;
            var (entryCount, totalTokens) = SummarizeSubtree(renderedNodes, s, subtrees);
            var tokenLabel = FormatTokenEstimate(totalTokens);
            var entryLabel = entryCount == 1 ? "1 entry" : $"{entryCount} entries";
            var desc = string.IsNullOrWhiteSpace(s.Description) ? "" : $" — {s.Description}";
            sb.AppendLine($"- [{s.Title}]({url}) ({entryLabel}, ~{tokenLabel} tokens){desc}");
        }
        sb.AppendLine();
    }

    /// <summary>Counts entries and sums token estimates for leaves whose nearest matching subtree is <paramref name="target"/>.</summary>
    private static (int Count, int Tokens) SummarizeSubtree(List<RenderedNode> renderedNodes, LlmsSubtree target, ImmutableList<LlmsSubtree> allSubtrees)
    {
        var count = 0;
        var tokens = 0;
        foreach (var node in renderedNodes)
        {
            if (node is LeafNode leaf && ReferenceEquals(MatchSubtree(leaf.CanonicalPath, allSubtrees), target))
            {
                count++;
                tokens += leaf.Tokens;
            }
        }
        return (count, tokens);
    }

    /// <summary>Formats an integer token count as <c>1.2k</c> / <c>84k</c> / <c>500</c>.</summary>
    private static string FormatTokenEstimate(int tokens)
    {
        if (tokens >= 1000)
        {
            var thousands = tokens / 1000.0;
            return thousands >= 10 ? $"{(int)thousands}k" : $"{thousands:0.#}k";
        }
        return tokens.ToString();
    }

    private static async Task CollectAsync(ImmutableList<NavigationTreeItem> items, int depth, BuildContext ctx)
    {
        foreach (var item in items)
        {
            if (item.Children.Count > 0)
            {
                ctx.Nodes.Add(new SectionNode(depth, item.Title));
                await CollectAsync(item.Children, depth + 1, ctx);
                continue;
            }

            var key = NormalizePath(item.Route.CanonicalPath.Value);
            if (!ctx.PageByPath.TryGetValue(key, out var page))
            {
                continue;
            }

            // Endpoint entries: the user-defined response URL is the link target.
            // No sidecar is manufactured — clients pull markdown directly from the URL
            // the user registered via WithLlmsTxtEntry.
            if (page.Origin?.Value is EndpointOrigin endpoint)
            {
                ctx.Nodes.Add(new LeafNode(
                    Title: item.Title,
                    CanonicalPath: item.Route.CanonicalPath.Value,
                    SidecarUrl: ctx.CanonicalBase.Combine(new UrlPath(endpoint.DirectUrl)).Value,
                    Description: page.Toc.Description,
                    Tokens: 0));
                continue;
            }

            if (page.Content is null)
            {
                continue;
            }

            // Markdown pages whose front matter opts out (`llms: false`) shouldn't
            // produce a sidecar even though they remain in the navigation tree.
            var frontMatter = page.Origin?.Value is MarkdownOrigin md ? md.Parsed.Metadata : null;
            if (frontMatter is not null && !frontMatter.Llms)
            {
                continue;
            }

            var markdown = HtmlToMarkdownConverter.Convert(page.Content, ctx.RewriteHref).Trim();
            if (string.IsNullOrWhiteSpace(markdown))
            {
                continue;
            }

            var rendition = BuildRendition(markdown);
            var body = rendition.MarkdownBody;
            // Root-level pages (canonical path "/") have an empty key; use "index" as the
            // slug so the sidecar lands at /_llms/index.md instead of /_llms/.md.
            var sidecarKey = string.IsNullOrEmpty(key) ? "index" : key;
            var mdPath = $"{ctx.Options.OutputDirectory}/{sidecarKey}.md";
            var linkUrl = BuildStrippedMarkdownUrl(ctx.CanonicalBase, ctx.Options.OutputDirectory, sidecarKey);
            var description = frontMatter?.Description ?? page.Toc.Description;
            var derived = page.Origin?.Value is MarkdownOrigin md2 ? md2.Parsed.Derived : null;
            var sidecarHeader = BuildSidecarHeader(item, frontMatter, description, ctx.CanonicalBase, linkUrl, rendition, derived);
            var sidecarContent = sidecarHeader + body;

            ctx.MarkdownFiles.Add(new MarkdownFile(new FilePath(mdPath), Encoding.UTF8.GetBytes(sidecarContent)));

            ctx.Nodes.Add(new LeafNode(
                Title: item.Title,
                CanonicalPath: item.Route.CanonicalPath.Value,
                SidecarUrl: linkUrl,
                Description: description,
                Tokens: rendition.TokenEstimate));

            if (ctx.FullContent is not null)
            {
                ctx.FullContent.AppendLine($"# {item.Title}");
                ctx.FullContent.AppendLine();
                ctx.FullContent.AppendLine(body);
                ctx.FullContent.AppendLine();
                ctx.FullContent.AppendLine("---");
                ctx.FullContent.AppendLine();
            }
        }
    }

    private static LlmRendition BuildRendition(string markdown)
    {
        var bytes = Encoding.UTF8.GetBytes(markdown);
        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes));
        var tokens = markdown.Length / 4;
        return new LlmRendition(markdown, hash, tokens);
    }

    private static void RenderBucket(List<RenderedNode> nodes, StringBuilder sb, Func<LeafNode, bool> include)
    {
        var pendingSections = new List<SectionNode>();
        var anyLeafEmittedSinceFlush = false;

        foreach (var node in nodes)
        {
            switch (node)
            {
                case SectionNode section:
                    // Pop pending sections at depth >= this one — they're now closed without contributing.
                    while (pendingSections.Count > 0 && pendingSections[^1].Depth >= section.Depth)
                    {
                        pendingSections.RemoveAt(pendingSections.Count - 1);
                    }

                    pendingSections.Add(section);
                    if (anyLeafEmittedSinceFlush)
                    {
                        sb.AppendLine();
                        anyLeafEmittedSinceFlush = false;
                    }
                    break;

                case LeafNode leaf when include(leaf):
                    foreach (var s in pendingSections)
                    {
                        // Heading level reflects depth in the original nav tree:
                        // top-level area → ##, subsection → ###, etc. Capped at H6 per HTML.
                        var level = Math.Min(s.Depth + 2, 6);
                        sb.AppendLine($"{new string('#', level)} {s.Title}");
                        sb.AppendLine();
                    }
                    pendingSections.Clear();
                    var desc = leaf.Description is { Length: > 0 } d ? $": {d}" : "";
                    sb.AppendLine($"- [{leaf.Title}]({leaf.SidecarUrl}){desc}");
                    anyLeafEmittedSinceFlush = true;
                    break;
            }
        }

        if (anyLeafEmittedSinceFlush)
        {
            sb.AppendLine();
        }
    }

    private static string BuildSidecarHeader(
        NavigationTreeItem item,
        IFrontMatter? metadata,
        string? description,
        CanonicalBaseUrl canonicalBase,
        string sidecarUrl,
        LlmRendition rendition,
        IReadOnlyDictionary<string, object?>? derived)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"title: {YamlScalar(metadata?.Title ?? item.Title)}");
        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"description: {YamlScalar(description)}");
        }
        // URLs and hashes contain `:` but never `: ` (colon-space) or whitespace, so they
        // parse correctly as bare YAML scalars. Bare emission keeps them readable.
        sb.AppendLine($"canonical_url: {canonicalBase.Combine(new UrlPath(item.Route.CanonicalPath.Value)).Value}");
        sb.AppendLine($"sidecar_url: {sidecarUrl}");
        sb.AppendLine($"content_hash: {rendition.ContentHash}");
        sb.AppendLine($"tokens: {rendition.TokenEstimate}");
        if (metadata?.Uid is { Length: > 0 } uid)
        {
            sb.AppendLine($"uid: {YamlScalar(uid)}");
        }

        if (metadata?.Date is { } date)
        {
            sb.AppendLine($"last_modified: {date:yyyy-MM-dd}");
        }

        // Derived metadata from IMetadataEnricher (reading time, …). Keys are
        // enricher-defined snake_case; values are emitted as bare/quoted scalars so
        // future enrichers surface here with no further change.
        if (derived is not null)
        {
            foreach (var (key, value) in derived)
            {
                sb.AppendLine($"{key}: {YamlScalar(value?.ToString() ?? "")}");
            }
        }

        sb.AppendLine("---");
        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>Quotes a YAML scalar when it contains characters that would change parsing semantics; otherwise returns it bare.</summary>
    private static string YamlScalar(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "\"\"";
        }
        // Conservative: quote anything that isn't plain printable text to avoid YAML edge cases.
        var needsQuote = false;
        foreach (var c in s)
        {
            if (c is ':' or '#' or '\n' or '\r' or '\t' or '"' or '\'' or '\\' or '{' or '}' or '[' or ']' or ',' or '&' or '*' or '!' or '|' or '>' or '%' or '@' or '`')
            {
                needsQuote = true;
                break;
            }
        }
        if (!needsQuote && (s.StartsWith(' ') || s.EndsWith(' ')))
        {
            needsQuote = true;
        }

        if (!needsQuote)
        {
            return s;
        }

        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    private static void CollectLeafPaths(ImmutableList<NavigationTreeItem> items, HashSet<string> acc)
    {
        foreach (var item in items)
        {
            if (item.Children.Count > 0)
            {
                CollectLeafPaths(item.Children, acc);
            }
            else
            {
                acc.Add(NormalizePath(item.Route.CanonicalPath.Value));
            }
        }
    }

    private static Func<string, string> BuildLinkRewriter(
        HashSet<string> linkablePaths,
        string outputDirectory,
        CanonicalBaseUrl canonicalBase)
    {
        return href =>
        {
            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("//")
                || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            {
                return href;
            }

            var fragmentIdx = href.IndexOf('#');
            var fragment = fragmentIdx >= 0 ? href[fragmentIdx..] : "";
            var beforeFragment = fragmentIdx >= 0 ? href[..fragmentIdx] : href;

            var queryIdx = beforeFragment.IndexOf('?');
            var query = queryIdx >= 0 ? beforeFragment[queryIdx..] : "";
            var pathPart = queryIdx >= 0 ? beforeFragment[..queryIdx] : beforeFragment;

            var key = NormalizePath(pathPart);
            if (string.IsNullOrEmpty(key) || !linkablePaths.Contains(key))
            {
                return href;
            }

            return BuildStrippedMarkdownUrl(canonicalBase, outputDirectory, key) + query + fragment;
        };
    }

    /// <summary>
    /// Builds the public URL for a stripped markdown file, combining the canonical
    /// base with the <c>{outputDirectory}/{key}.md</c> suffix. Produces an absolute
    /// URL when the base has an http(s) scheme; otherwise a root-relative path.
    /// </summary>
    private static string BuildStrippedMarkdownUrl(CanonicalBaseUrl canonicalBase, string outputDirectory, string key)
    {
        var relative = new UrlPath($"/{outputDirectory}/{key}.md");
        return canonicalBase.Combine(relative).Value;
    }

    private static async Task<string?> ReadUserHeaderAsync(
        IFileSystem fileSystem,
        IWebHostEnvironment hostingEnvironment,
        PenningtonOptions pennOptions)
    {
        var contentRoot = Path.IsPathRooted(pennOptions.ContentRootPath.Value)
            ? pennOptions.ContentRootPath.Value
            : Path.Combine(hostingEnvironment.ContentRootPath, pennOptions.ContentRootPath.Value);

        var llmsTxtPath = fileSystem.Path.Combine(contentRoot, "llms.txt");
        if (!fileSystem.File.Exists(llmsTxtPath))
        {
            return null;
        }

        return await fileSystem.File.ReadAllTextAsync(llmsTxtPath);
    }

    private static string NormalizePath(string canonicalPath)
        => canonicalPath.Trim('/');

    internal record LlmsTxtData(
        string IndexContent,
        ImmutableList<MarkdownFile> MarkdownFiles,
        ImmutableList<MarkdownFile> SubtreeFiles,
        string? FullContent);

    /// <summary>A stripped markdown file produced for the llms output.</summary>
    /// <param name="OutputPath">Relative output path for the markdown file.</param>
    /// <param name="Content">UTF-8 bytes of the stripped markdown body.</param>
    public record MarkdownFile(FilePath OutputPath, byte[] Content);

    private abstract record RenderedNode;
    private sealed record SectionNode(int Depth, string Title) : RenderedNode;
    private sealed record LeafNode(
        string Title,
        string CanonicalPath,
        string SidecarUrl,
        string? Description,
        int Tokens) : RenderedNode;

    private sealed record LlmRendition(string MarkdownBody, string ContentHash, int TokenEstimate);

    private sealed record BuildContext(
        LlmsTxtOptions Options,
        CanonicalBaseUrl CanonicalBase,
        ILogger Logger,
        Dictionary<string, RenderedPage> PageByPath,
        Func<string, string> RewriteHref,
        List<RenderedNode> Nodes,
        ImmutableList<MarkdownFile>.Builder MarkdownFiles,
        StringBuilder? FullContent);
}

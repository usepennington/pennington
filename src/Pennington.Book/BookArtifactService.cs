namespace Pennington.Book;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Composition;
using Content;
using Infrastructure;
using Localization;
using Microsoft.Extensions.Logging;
using Navigation;
using Pipeline;
using Rendering;

/// <summary>
/// Single source of truth for every book artifact, behind the artifact-tier façade
/// <see cref="BookArtifactContentService"/> (which serves dev requests and enumerates the PDFs
/// for the static build) — the same shape as <c>SearchArtifactService</c>.
/// <para>
/// The projection fold (post-pipeline HTML per page) and the static-asset map are computed once,
/// lazily; each book's composed HTML is cached per (book, locale); PDF bytes are rendered on demand
/// through the singleton <see cref="ChromiumBrowserProvider"/> and cached until a file change drops the
/// whole service. <see cref="EnumerateArtifacts"/> is deliberately cheap — pure options × locales, no
/// projection, no Chromium — so build enumeration and link verification never trigger a render.
/// </para>
/// </summary>
public sealed class BookArtifactService : IFileWatchAware
{
    private readonly BookOptions _options;
    private readonly PenningtonOptions _penn;
    private readonly LocalizationOptions _localization;
    private readonly ISiteProjection _projection;
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly NavigationBuilder _navigationBuilder;
    private readonly BookComposer _composer;
    private readonly AssetInliner _assetInliner;
    private readonly ChromiumBrowserProvider _chromium;
    private readonly TimeProvider _clock;
    private readonly ILogger<BookArtifactService> _logger;

    private readonly AsyncLazy<ProjectionData> _projectionLazy;
    private readonly ConcurrentDictionary<string, AsyncLazy<ComposedBook>> _composed = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, AsyncLazy<byte[]>> _pdfs = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>Creates the service; all artifacts are computed lazily on first request.</summary>
    public BookArtifactService(
        BookOptions options,
        PenningtonOptions penn,
        LocalizationOptions localization,
        ISiteProjection projection,
        IEnumerable<IContentService> contentServices,
        NavigationBuilder navigationBuilder,
        BookComposer composer,
        AssetInliner assetInliner,
        ChromiumBrowserProvider chromium,
        TimeProvider clock,
        ILogger<BookArtifactService> logger)
    {
        _options = options;
        _penn = penn;
        _localization = localization;
        _projection = projection;
        _contentServices = contentServices;
        _navigationBuilder = navigationBuilder;
        _composer = composer;
        _assetInliner = assetInliner;
        _chromium = chromium;
        _clock = clock;
        _logger = logger;
        _projectionLazy = new AsyncLazy<ProjectionData>(BuildProjectionAsync);
    }

    /// <summary>
    /// Enumerates every (book × locale) artifact from options alone — no projection, no Chromium —
    /// so the build emitter and link verifier can list output paths without rendering.
    /// </summary>
    internal IReadOnlyList<BookArtifact> EnumerateArtifacts()
    {
        var books = _options.ResolveBooks(_penn);
        var locales = _localization.IsMultiLocale
            ? _localization.Locales.Keys.Cast<string?>().ToList()
            : [null];

        var artifacts = new List<BookArtifact>(books.Count * locales.Count);
        foreach (var book in books)
        {
            foreach (var locale in locales)
            {
                artifacts.Add(new BookArtifact(
                    book,
                    locale,
                    book.EffectiveSlug,
                    BookRoutes.PdfPath(book.EffectiveSlug, locale, _localization.DefaultLocale),
                    BookRoutes.PreviewPath(book.EffectiveSlug, locale, _localization.DefaultLocale)));
            }
        }

        return artifacts;
    }

    /// <summary>Renders the PDF for the artifact at <paramref name="pdfPath"/> (e.g. <c>pdf/tutorials.pdf</c>), or null when no book matches.</summary>
    public async Task<byte[]?> GetPdfAsync(string pdfPath)
    {
        var key = pdfPath.Trim('/');
        var artifact = EnumerateArtifacts().FirstOrDefault(a => a.PdfPath.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (artifact is null)
        {
            return null;
        }

        return await _pdfs.GetOrAdd(artifact.PdfPath, _ => new AsyncLazy<byte[]>(() => RenderAsync(artifact)));
    }

    /// <summary>Returns the composed HTML for the preview route at <paramref name="previewPath"/>, or null when no book matches.</summary>
    public async Task<string?> GetPreviewHtmlAsync(string previewPath)
    {
        var key = NormalizePreview(previewPath);
        var artifact = EnumerateArtifacts().FirstOrDefault(a => NormalizePreview(a.PreviewPath) == key);
        if (artifact is null)
        {
            return null;
        }

        return (await GetComposedLazy(artifact)).Html;
    }

    private async Task<byte[]> RenderAsync(BookArtifact artifact)
    {
        var composed = await GetComposedLazy(artifact);
        _logger.LogInformation("Rendering book {Slug} ({Pages} pages)", artifact.Slug, composed.PageCount);
        return await _chromium.RenderPdfAsync(composed.Html);
    }

    private AsyncLazy<ComposedBook> GetComposedLazy(BookArtifact artifact)
        => _composed.GetOrAdd(artifact.PdfPath, _ => new AsyncLazy<ComposedBook>(() => ComposeAsync(artifact)));

    private async Task<ComposedBook> ComposeAsync(BookArtifact artifact)
    {
        var data = await _projectionLazy;
        var scoped = BookScoping.ScopeToc(data.TocItems, artifact.Book.NormalizedRoutePrefix, _localization, artifact.Locale);
        var tree = await _navigationBuilder.BuildTreeAsync(scoped, currentPath: null, locale: artifact.Locale);

        // Version auto-detection lives here (not in the composer) so composer tests stay
        // deterministic — under a test host the entry assembly is the runner, not the site.
        var stamp = new BookStamp(BookVersion.EntryAssembly(), _clock.GetUtcNow(), artifact.Locale);
        var html = _composer.Compose(
            artifact.Book,
            tree,
            data.PageByPath,
            _options.AdditionalCss,
            src => _assetInliner.Resolve(src, data.AssetMap),
            _options.Monochrome,
            stamp);
        return new ComposedBook(html, CountPages(tree, data.PageByPath));
    }

    private async Task<ProjectionData> BuildProjectionAsync()
    {
        var pageByPath = new Dictionary<string, RenderedPage>(StringComparer.OrdinalIgnoreCase);
        var toc = new List<ContentTocItem>();

        await foreach (var page in _projection.GetPagesAsync())
        {
            // Reuse the existing "don't extract me" opt-out, and skip pages with no body to compose.
            if (page.Toc.ExcludeFromLlms || page.Content is null)
            {
                continue;
            }

            toc.Add(page.Toc);
            pageByPath[page.Route.CanonicalPath.Value.Trim('/')] = page;
        }

        var assetMap = await AssetInliner.BuildContentMapAsync(_contentServices);
        return new ProjectionData(pageByPath, toc, assetMap);
    }

    private static string NormalizePreview(string path) => path.Trim('/');

    private static int CountPages(ImmutableList<NavigationTreeItem> tree, IReadOnlyDictionary<string, RenderedPage> pages)
    {
        var count = 0;
        Walk(tree);
        return count;

        void Walk(ImmutableList<NavigationTreeItem> nodes)
        {
            foreach (var node in nodes)
            {
                var key = node.Route.CanonicalPath.Value.Trim('/');
                if (!string.IsNullOrEmpty(key) && pages.ContainsKey(key))
                {
                    count++;
                }

                Walk(node.Children);
            }
        }
    }

    private sealed record ProjectionData(
        IReadOnlyDictionary<string, RenderedPage> PageByPath,
        IReadOnlyList<ContentTocItem> TocItems,
        IReadOnlyDictionary<string, string> AssetMap);

    private sealed record ComposedBook(string Html, int PageCount);
}

/// <summary>One book artifact: a book in one locale, with its output and preview paths.</summary>
/// <param name="Book">The book definition.</param>
/// <param name="Locale">Locale code, or null for a single-locale site.</param>
/// <param name="Slug">Output slug.</param>
/// <param name="PdfPath">Site-relative PDF output path.</param>
/// <param name="PreviewPath">Site-relative live HTML preview path.</param>
internal sealed record BookArtifact(
    BookDefinition Book,
    string? Locale,
    string Slug,
    string PdfPath,
    string PreviewPath);

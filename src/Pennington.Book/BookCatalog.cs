namespace Pennington.Book;

using Infrastructure;
using Localization;
using Navigation;

/// <summary>
/// Advertises each configured book as a <see cref="DownloadLink"/>. Derives the link list purely
/// from <see cref="BookOptions"/>, <see cref="PenningtonOptions"/>, and
/// <see cref="LocalizationOptions"/> — it never touches the site projection, so it is safe to
/// resolve on the request path (the DocSite sidebar resolves it in <c>MainLayout.OnInitializedAsync</c>).
/// The link label comes from the <c>pennington.book.download</c> translation when one is registered,
/// falling back to "Download as PDF".
/// </summary>
public sealed class BookCatalog : IDownloadLinkProvider
{
    /// <summary>Translation key for the download-link label.</summary>
    internal const string DownloadLabelKey = "pennington.book.download";

    private const string DefaultLabel = "Download as PDF";

    private readonly BookOptions _options;
    private readonly PenningtonOptions _penn;
    private readonly LocalizationOptions _localization;
    private readonly TranslationOptions _translations;

    /// <summary>Creates the catalog over the configured book, localization, and translation options.</summary>
    public BookCatalog(BookOptions options, PenningtonOptions penn, LocalizationOptions localization, TranslationOptions translations)
    {
        _options = options;
        _penn = penn;
        _localization = localization;
        _translations = translations;
    }

    /// <inheritdoc/>
    public IReadOnlyList<DownloadLink> GetLinks(string? locale = null)
    {
        var label = ResolveLabel(locale);
        var books = _options.ResolveBooks(_penn);
        var result = new List<DownloadLink>(books.Count);
        foreach (var book in books)
        {
            var url = BookRoutes.PdfPath(book.EffectiveSlug, locale, _localization.DefaultLocale);
            result.Add(new DownloadLink(label, url, book.NormalizedRoutePrefix));
        }

        return result;
    }

    private string ResolveLabel(string? locale)
        => _translations.Get(locale ?? _localization.DefaultLocale, DownloadLabelKey)
            ?? _translations.Get(_localization.DefaultLocale, DownloadLabelKey)
            ?? DefaultLabel;
}

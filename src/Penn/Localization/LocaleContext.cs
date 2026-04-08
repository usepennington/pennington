namespace Penn.Localization;

using Penn.Infrastructure;

/// <summary>
/// Scoped per-request locale context, set by <see cref="LocaleDetectionMiddleware"/>.
/// Provides the current locale and locale-aware URL building for Razor components.
/// Analogous to Astro's <c>Astro.currentLocale</c>.
/// </summary>
public sealed class LocaleContext
{
    private readonly LocalizationOptions _localization;

    public LocaleContext(LocalizationOptions localization)
    {
        _localization = localization;

        // Default to the default locale until middleware overrides
        Locale = localization.DefaultLocale;
        Info = localization.Locales.TryGetValue(localization.DefaultLocale, out var info)
            ? info
            : new LocaleInfo(localization.DefaultLocale);
        ContentPath = "/";
        IsDefaultLocale = true;
    }

    /// <summary>The Penn locale code for this request (e.g., "en", "fr", "gen-z").</summary>
    public string Locale { get; internal set; }

    /// <summary>Metadata for this locale.</summary>
    public LocaleInfo Info { get; internal set; }

    /// <summary>The request URL with locale prefix stripped (e.g., "/schedule" regardless of locale).</summary>
    public string ContentPath { get; internal set; }

    /// <summary>True when the current locale is the default locale.</summary>
    public bool IsDefaultLocale { get; internal set; }

    /// <summary>The HTML <c>lang</c> attribute value for this locale.</summary>
    public string HtmlLang => Info.HtmlLang ?? Locale;

    /// <summary>Text direction for this locale ("ltr" or "rtl").</summary>
    public string Direction => Info.Direction;

    /// <summary>
    /// Builds a locale-aware URL from a content path.
    /// For the default locale, returns the path as-is.
    /// For other locales, prefixes with <c>/{locale}/</c>.
    /// </summary>
    public string Url(string path) => _localization.BuildLocaleUrl(path.Trim('/'), Locale);
}

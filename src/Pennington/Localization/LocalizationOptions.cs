namespace Pennington.Localization;

/// <summary>Options for localization.</summary>
public sealed class LocalizationOptions
{
    /// <summary>Locale code used when no URL locale prefix is present.</summary>
    public string DefaultLocale { get; set; } = "en";
    private readonly Dictionary<string, LocaleInfo> _locales = [];

    /// <summary>Registers a locale with the supplied metadata.</summary>
    public void AddLocale(string code, LocaleInfo info)
        => _locales[code] = info;

    /// <summary>Registers a locale with just a display name.</summary>
    public void AddLocale(string code, string displayName)
        => _locales[code] = new LocaleInfo(displayName);

    /// <summary>Configured locales keyed by locale code.</summary>
    public IReadOnlyDictionary<string, LocaleInfo> Locales => _locales;

    /// <summary>True when more than one locale is configured.</summary>
    public bool IsMultiLocale => _locales.Count > 1;

    /// <summary>
    /// Extracts the locale code from a URL path.
    /// Returns the default locale when the first segment is not a known non-default locale.
    /// </summary>
    public string GetLocaleFromUrl(string url)
    {
        if (!IsMultiLocale)
        {
            return DefaultLocale;
        }

        var trimmed = url.TrimStart('/');
        var firstSlash = trimmed.IndexOf('/');
        var firstSegment = firstSlash >= 0 ? trimmed[..firstSlash] : trimmed;

        if (!string.IsNullOrEmpty(firstSegment)
            && _locales.ContainsKey(firstSegment)
            && !string.Equals(firstSegment, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            return firstSegment;
        }

        return DefaultLocale;
    }

    /// <summary>
    /// Strips the locale prefix from a URL, returning the content-relative path.
    /// For the default locale (no prefix), returns the URL unchanged.
    /// </summary>
    public string StripLocalePrefix(string url, string locale)
    {
        if (string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var trimmed = url.TrimStart('/');
        var prefix = locale + "/";

        if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return "/" + trimmed[prefix.Length..];
        }

        // URL is just the locale with no trailing path
        if (string.Equals(trimmed, locale, StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        return url;
    }

    /// <summary>
    /// Builds a full URL for a content path in a specific locale.
    /// </summary>
    public string BuildLocaleUrl(string contentPath, string locale)
    {
        var path = contentPath.Trim('/');

        if (string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrEmpty(path) ? "/" : $"/{path}/";
        }

        return string.IsNullOrEmpty(path) ? $"/{locale}/" : $"/{locale}/{path}/";
    }

    /// <summary>
    /// Gets alternate language versions for a page URL across all configured locales.
    /// Pure URL math — does not check if content exists (fallback handles that).
    /// </summary>
    public IReadOnlyList<AlternateLanguage> GetAlternateLanguages(string url)
    {
        if (!IsMultiLocale)
        {
            return [];
        }

        // The 404-generation sentinel is not a real content page. Treat it as
        // the locale root so language switcher links resolve to each locale's
        // landing page instead of phantom /<locale>/__pennington-404-generator/.
        if (url.Equals(Generation.OutputGenerationService.NotFoundGeneratorPath, StringComparison.Ordinal)
            || url.Equals(Generation.OutputGenerationService.NotFoundGeneratorPath + "/", StringComparison.Ordinal))
        {
            url = "/";
        }

        url = "/" + url.Trim('/');
        if (url.Equals("/index", StringComparison.OrdinalIgnoreCase))
        {
            url = "/";
        }

        var locale = GetLocaleFromUrl(url);
        var contentPath = StripLocalePrefix(url, locale);

        var result = new List<AlternateLanguage>();
        foreach (var (code, info) in _locales)
        {
            var localeUrl = BuildLocaleUrl(contentPath.Trim('/'), code);
            result.Add(new AlternateLanguage(
                Locale: code,
                DisplayName: info.DisplayName,
                HtmlLang: info.HtmlLang ?? code,
                Url: localeUrl,
                IsCurrentLocale: string.Equals(code, locale, StringComparison.OrdinalIgnoreCase)));
        }

        return result;
    }
}

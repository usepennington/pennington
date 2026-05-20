namespace Pennington.Localization;

using System.Globalization;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

/// <summary>
/// An <see cref="IRequestCultureProvider"/> that reads the locale from the URL
/// path prefix and maps it to the closest <see cref="CultureInfo"/> for
/// ASP.NET's request localization pipeline.
/// </summary>
public sealed class PenningtonUrlRequestCultureProvider : IRequestCultureProvider
{
    private readonly LocalizationOptions _localization;

    /// <summary>Creates the provider.</summary>
    public PenningtonUrlRequestCultureProvider(LocalizationOptions localization)
    {
        _localization = localization;
    }

    /// <summary>Derives the request culture from the URL locale prefix.</summary>
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value ?? "/";
        var locale = _localization.GetLocaleFromUrl(path);

        var cultureName = MapToCultureName(locale);
        var result = new ProviderCultureResult(cultureName);
        return Task.FromResult<ProviderCultureResult?>(result);
    }

    /// <summary>
    /// Maps a Pennington locale code to a valid .NET culture name.
    /// Uses <see cref="LocaleInfo.HtmlLang"/> when available, otherwise tries the
    /// locale code directly, and falls back to the default locale's culture.
    /// </summary>
    internal string MapToCultureName(string locale)
    {
        if (_localization.Locales.TryGetValue(locale, out var info) && info.HtmlLang is not null)
        {
            // Try the HtmlLang value as a culture name
            if (TryGetCulture(info.HtmlLang, out _))
            {
                return info.HtmlLang;
            }

            // HtmlLang might be a custom code like "en-genz" — try parent (before the hyphen)
            var dash = info.HtmlLang.IndexOf('-');
            if (dash > 0)
            {
                var parent = info.HtmlLang[..dash];
                if (TryGetCulture(parent, out _))
                {
                    return parent;
                }
            }
        }

        // Try the locale code directly as a culture name (works for "fr", "de", "ja", etc.)
        if (TryGetCulture(locale, out _))
        {
            return locale;
        }

        // Fall back to default locale's culture
        return MapDefaultCulture();
    }

    private string MapDefaultCulture()
    {
        var defaultLocale = _localization.DefaultLocale;
        if (_localization.Locales.TryGetValue(defaultLocale, out var defaultInfo) && defaultInfo.HtmlLang is not null)
        {
            if (TryGetCulture(defaultInfo.HtmlLang, out _))
            {
                return defaultInfo.HtmlLang;
            }
        }

        if (TryGetCulture(defaultLocale, out _))
        {
            return defaultLocale;
        }

        return "en";
    }

    private static bool TryGetCulture(string name, out CultureInfo? culture)
    {
        try
        {
            culture = CultureInfo.GetCultureInfo(name);

            // On Linux/macOS, ICU accepts any string without throwing,
            // synthesizing a culture for unknown tags. Reject those.
            if (culture.LCID == 4096
                && (culture.TwoLetterISOLanguageName.Length > 3
                    || culture.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal)))
            {
                culture = null;
                return false;
            }

            return true;
        }
        catch (CultureNotFoundException)
        {
            culture = null;
            return false;
        }
    }
}
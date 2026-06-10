namespace Pennington.Localization;

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

/// <summary>
/// An <see cref="IStringLocalizer"/> backed by <see cref="TranslationOptions"/>.
/// Reads the locale that <see cref="LocaleDetectionMiddleware"/> already detected for the
/// request from the scoped <see cref="LocaleContext"/>, and looks up translations with
/// fallback to the default locale, then to the key itself.
/// </summary>
public sealed class PenningtonStringLocalizer : IStringLocalizer
{
    private readonly TranslationOptions _translations;
    private readonly LocalizationOptions _localization;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>Creates the localizer. <paramref name="httpContextAccessor"/> supplies the per-request <see cref="LocaleContext"/>; when absent (outside a request) the default locale is used.</summary>
    public PenningtonStringLocalizer(
        TranslationOptions translations,
        LocalizationOptions localization,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _translations = translations;
        _localization = localization;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Returns the translation for <paramref name="name"/>, or the key itself when no translation is registered.</summary>
    public LocalizedString this[string name]
    {
        get
        {
            var value = GetTranslation(name);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    /// <summary>Returns the translation for <paramref name="name"/> formatted with <paramref name="arguments"/>.</summary>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = GetTranslation(name);
            var formatted = value is not null
                ? string.Format(CultureInfo.CurrentCulture, value, arguments)
                : string.Format(CultureInfo.CurrentCulture, name, arguments);
            return new LocalizedString(name, formatted, resourceNotFound: value is null);
        }
    }

    /// <summary>Enumerates registered strings for the active locale, optionally including default-locale fallbacks.</summary>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var locale = ResolveLocale();
        var entries = _translations.GetAll(locale);

        foreach (var (key, value) in entries)
        {
            yield return new LocalizedString(key, value, resourceNotFound: false);
        }

        if (includeParentCultures && !string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            var defaultEntries = _translations.GetAll(_localization.DefaultLocale);
            foreach (var (key, value) in defaultEntries)
            {
                if (!entries.ContainsKey(key))
                {
                    yield return new LocalizedString(key, value, resourceNotFound: false);
                }
            }
        }
    }

    private string? GetTranslation(string key)
    {
        var locale = ResolveLocale();

        // Try current locale first
        var value = _translations.Get(locale, key);
        if (value is not null)
        {
            return value;
        }

        // Fall back to default locale
        if (!string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            value = _translations.Get(_localization.DefaultLocale, key);
        }

        return value;
    }

    /// <summary>
    /// Returns the request's Pennington locale code straight from the scoped
    /// <see cref="LocaleContext"/> that <see cref="LocaleDetectionMiddleware"/> populated —
    /// no <see cref="CultureInfo"/> round-trip. Falls back to the default locale outside a request.
    /// </summary>
    private string ResolveLocale() =>
        _httpContextAccessor?.HttpContext?.RequestServices.GetService<LocaleContext>()?.Locale
            ?? _localization.DefaultLocale;
}
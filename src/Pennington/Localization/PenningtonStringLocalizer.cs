namespace Pennington.Localization;

using System.Globalization;
using Microsoft.Extensions.Localization;
using LocalizationOptions = Infrastructure.LocalizationOptions;

/// <summary>
/// An <see cref="IStringLocalizer"/> backed by <see cref="TranslationOptions"/>.
/// Reads <see cref="CultureInfo.CurrentUICulture"/> to determine the current locale,
/// maps it back to a Pennington locale code, and looks up translations with fallback to the
/// default locale, then to the key itself.
/// </summary>
public sealed class PenningtonStringLocalizer : IStringLocalizer
{
    private readonly TranslationOptions _translations;
    private readonly LocalizationOptions _localization;

    /// <summary>Creates the localizer.</summary>
    public PenningtonStringLocalizer(TranslationOptions translations, LocalizationOptions localization)
    {
        _translations = translations;
        _localization = localization;
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
                    yield return new LocalizedString(key, value, resourceNotFound: false);
            }
        }
    }

    private string? GetTranslation(string key)
    {
        var locale = ResolveLocale();

        // Try current locale first
        var value = _translations.Get(locale, key);
        if (value is not null) return value;

        // Fall back to default locale
        if (!string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            value = _translations.Get(_localization.DefaultLocale, key);
        }

        return value;
    }

    /// <summary>
    /// Maps CultureInfo.CurrentUICulture back to a Pennington locale code.
    /// Checks the culture name against registered locales and their HtmlLang values.
    /// </summary>
    private string ResolveLocale()
    {
        var culture = CultureInfo.CurrentUICulture;
        var cultureName = culture.Name;

        // Direct match: culture name is a registered Pennington locale
        if (_localization.Locales.ContainsKey(cultureName))
            return cultureName;

        // Check if any locale has this culture as its HtmlLang
        foreach (var (code, info) in _localization.Locales)
        {
            if (info.HtmlLang is not null &&
                string.Equals(info.HtmlLang, cultureName, StringComparison.OrdinalIgnoreCase))
            {
                return code;
            }
        }

        // Try parent culture (e.g., "en-US" -> "en")
        if (!string.IsNullOrEmpty(culture.Parent?.Name))
        {
            var parentName = culture.Parent!.Name;
            if (_localization.Locales.ContainsKey(parentName))
                return parentName;
        }

        return _localization.DefaultLocale;
    }
}
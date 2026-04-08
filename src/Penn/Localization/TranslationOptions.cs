namespace Penn.Localization;

/// <summary>
/// In-memory store for UI string translations, keyed by locale and string key.
/// Configured in <see cref="Penn.Infrastructure.PennOptions.Translations"/>
/// and consumed by <see cref="PennStringLocalizer"/>.
/// </summary>
public sealed class TranslationOptions
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Add a single translation entry.</summary>
    public void Add(string locale, string key, string value)
    {
        if (!_translations.TryGetValue(locale, out var dict))
        {
            dict = new(StringComparer.OrdinalIgnoreCase);
            _translations[locale] = dict;
        }

        dict[key] = value;
    }

    /// <summary>Add multiple translation entries for a locale.</summary>
    public void Add(string locale, Dictionary<string, string> entries)
    {
        foreach (var (key, value) in entries)
            Add(locale, key, value);
    }

    /// <summary>Look up a translation by locale and key. Returns null if not found.</summary>
    internal string? Get(string locale, string key)
        => _translations.TryGetValue(locale, out var dict) && dict.TryGetValue(key, out var value)
            ? value
            : null;

    /// <summary>Returns all translations for a given locale, or empty if none.</summary>
    internal IReadOnlyDictionary<string, string> GetAll(string locale)
        => _translations.TryGetValue(locale, out var dict)
            ? dict
            : new Dictionary<string, string>();
}

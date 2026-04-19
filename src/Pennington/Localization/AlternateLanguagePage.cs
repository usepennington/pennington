namespace Pennington.Localization;

using Routing;

/// <summary>Links a page to its translation in another locale.</summary>
/// <param name="Locale">Locale code for the translated page.</param>
/// <param name="DisplayName">User-visible language name.</param>
/// <param name="Route">Content route of the translated page.</param>
/// <param name="IsCurrentLocale">True when this entry represents the current request locale.</param>
public record AlternateLanguagePage(
    string Locale,
    string DisplayName,
    ContentRoute Route,
    bool IsCurrentLocale = false
);
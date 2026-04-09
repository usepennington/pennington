namespace Pennington.Localization;

using Pennington.Routing;

/// <summary>Links a page to its translation in another locale.</summary>
public record AlternateLanguagePage(
    string Locale,
    string DisplayName,
    ContentRoute Route,
    bool IsCurrentLocale = false
);

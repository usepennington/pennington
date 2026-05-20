namespace Pennington.Localization;

/// <summary>Metadata describing a configured locale.</summary>
/// <param name="DisplayName">Human-readable name for the locale (e.g., "English", "Français").</param>
/// <param name="Direction">Text direction, "ltr" or "rtl".</param>
/// <param name="HtmlLang">Optional HTML <c>lang</c> attribute value; when null, the locale code is used.</param>
public record LocaleInfo(
    string DisplayName,
    string Direction = "ltr",
    string? HtmlLang = null
);
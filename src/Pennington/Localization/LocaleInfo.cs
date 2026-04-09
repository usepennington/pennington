namespace Pennington.Localization;

public record LocaleInfo(
    string DisplayName,
    string Direction = "ltr",
    string? HtmlLang = null
);

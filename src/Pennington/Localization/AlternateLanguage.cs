namespace Pennington.Localization;

/// <summary>
/// One language version of a page, used for language switchers and <c>hreflang</c> link tags.
/// Content-route-independent (pure URL math).
/// </summary>
/// <param name="Locale">Locale code (e.g. <c>en</c>, <c>fr</c>, <c>pt-BR</c>).</param>
/// <param name="DisplayName">User-visible language name.</param>
/// <param name="HtmlLang">Value to emit in <c>hreflang</c> and <c>lang</c> attributes.</param>
/// <param name="Url">URL of the page in this locale.</param>
/// <param name="IsCurrentLocale">True when this entry represents the current request locale.</param>
public record AlternateLanguage(
    string Locale,
    string DisplayName,
    string HtmlLang,
    string Url,
    bool IsCurrentLocale = false
);

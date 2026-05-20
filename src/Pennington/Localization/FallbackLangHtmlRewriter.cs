namespace Pennington.Localization;

using AngleSharp.Dom;
using Infrastructure;
using Microsoft.AspNetCore.Http;

/// <summary>
/// When a non-default-locale request was served with content from a different
/// locale (fallback), rewrites <c>&lt;html lang="..." dir="..."&gt;</c> to
/// match the actual content locale so screen readers and lang-aware tooling
/// don't misidentify the body's language.
/// <para>
/// Signal carrier: pages that resolve a fallback set
/// <c>HttpContext.Items["Pennington.FallbackContentLocale"]</c> to the
/// locale code whose content was actually rendered. Absence of that key
/// means no rewrite.
/// </para>
/// </summary>
public sealed class FallbackLangHtmlRewriter : IHtmlResponseRewriter
{
    /// <summary>Key written by content-resolving pages when a fallback was used.</summary>
    public const string FallbackContentLocaleKey = "Pennington.FallbackContentLocale";

    private readonly LocalizationOptions _localization;

    /// <summary>Creates the rewriter.</summary>
    public FallbackLangHtmlRewriter(LocalizationOptions localization)
    {
        _localization = localization;
    }

    // Cosmetic — runs after locale-link rewriting because the lang attribute
    // is independent of link rewriting. Late order keeps it out of the
    // outside-in URL pipeline.
    /// <inheritdoc/>
    public int Order => 40;

    /// <inheritdoc/>
    public bool ShouldApply(HttpContext context)
    {
        if (!_localization.IsMultiLocale)
        {
            return false;
        }

        return context.Items[FallbackContentLocaleKey] is string s && !string.IsNullOrEmpty(s);
    }

    /// <inheritdoc/>
    public Task ApplyAsync(IDocument document, HttpContext context)
    {
        if (context.Items[FallbackContentLocaleKey] is not string contentLocale)
        {
            return Task.CompletedTask;
        }

        var info = _localization.Locales.TryGetValue(contentLocale, out var li) ? li : new LocaleInfo(contentLocale);
        var html = document.DocumentElement;
        if (html is null)
        {
            return Task.CompletedTask;
        }

        html.SetAttribute("lang", info.HtmlLang ?? contentLocale);
        html.SetAttribute("dir", info.Direction);
        return Task.CompletedTask;
    }
}
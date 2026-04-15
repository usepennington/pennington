namespace Pennington.Infrastructure;

using AngleSharp.Dom;
using Generation;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Prefixes root-relative URLs (<c>href</c>, <c>src</c>, <c>action</c>)
/// with the configured base URL. Also stamps <c>data-base-url</c> on
/// <c>&lt;body&gt;</c> so client-side code can reproduce the same prefix
/// on dynamically-generated links.
/// <para>
/// Outermost transport layer — <see cref="Order"/> 30, so xref (10) and
/// locale rewriting (20) both operate on logical root-relative paths
/// without having to strip the base URL first.
/// </para>
/// </summary>
public sealed class BaseUrlHtmlRewriter : IHtmlResponseRewriter
{
    private readonly string _baseUrl;

    public BaseUrlHtmlRewriter(OutputOptions? outputOptions)
    {
        _baseUrl = outputOptions?.BaseUrl.Value.TrimEnd('/') ?? "";
    }

    public int Order => 30;

    public bool ShouldApply(HttpContext context)
        => !string.IsNullOrEmpty(_baseUrl) && _baseUrl != "/";

    public Task ApplyAsync(IDocument document, HttpContext context)
    {
        document.Body?.SetAttribute("data-base-url", _baseUrl);

        foreach (var element in document.QuerySelectorAll("[href], [src], [action]"))
        {
            RewriteAttribute(element, "href");
            RewriteAttribute(element, "src");
            RewriteAttribute(element, "action");
        }

        return Task.CompletedTask;
    }

    private void RewriteAttribute(IElement element, string attrName)
    {
        var value = element.GetAttribute(attrName);
        if (value is not null && value.StartsWith('/') && !value.StartsWith("//"))
        {
            element.SetAttribute(attrName, _baseUrl + value);
        }
    }
}
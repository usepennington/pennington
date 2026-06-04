namespace Pennington.Pipeline;

using System.Collections.Immutable;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

/// <summary>
/// Base <see cref="IContentRenderer"/> that renders a Razor component <typeparamref name="TComponent"/> to HTML.
/// A subclass projects a <see cref="ParsedItem"/> into the component's parameters via <see cref="BuildParameters"/>;
/// this base owns the Blazor <see cref="HtmlRenderer"/> dispatch, heading-anchor assignment, and outline extraction.
/// A structured content format therefore renders through Razor markup the way markdown renders through Markdig — both
/// produce a <see cref="RenderedContent"/>. The host must register Razor component services (<c>AddRazorComponents()</c>).
/// </summary>
/// <typeparam name="TComponent">The Razor component that renders the page body.</typeparam>
public abstract class RazorContentRenderer<TComponent> : IContentRenderer
    where TComponent : IComponent
{
    private readonly HtmlRenderer _renderer;

    /// <summary>Creates the renderer over the Blazor <see cref="HtmlRenderer"/> resolved from DI.</summary>
    protected RazorContentRenderer(HtmlRenderer renderer) => _renderer = renderer;

    /// <summary>
    /// Projects a parsed item into the parameters bound by <typeparamref name="TComponent"/>. Throw to fail the
    /// item — the exception is captured as a <see cref="FailedItem"/>, matching the markdown renderer.
    /// </summary>
    protected abstract IReadOnlyDictionary<string, object?> BuildParameters(ParsedItem item);

    /// <inheritdoc/>
    public async Task<ContentItem> RenderAsync(ParsedItem item)
    {
        try
        {
            var parameters = new Dictionary<string, object?>(BuildParameters(item));
            var html = await _renderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await _renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
                return output.ToHtmlString();
            });

            var (processedHtml, outline) = ProcessHeadings(html);
            var content = new RenderedContent(
                Html: processedHtml,
                Outline: outline,
                Tags: ImmutableList<Tag>.Empty,
                CrossReferences: ImmutableList<CrossReference>.Empty,
                Social: null);
            return new RenderedItem(item.Route, item.Metadata, content);
        }
        catch (Exception ex)
        {
            return new FailedItem(item.Route, new ContentError($"Razor render failed: {ex.Message}", ex));
        }
    }

    /// <summary>
    /// Assigns a slugified <c>id</c> to every <c>h2</c>–<c>h6</c> that lacks one and returns the rewritten HTML plus
    /// the heading outline, so a Razor-rendered page gets the same anchors and table-of-contents a markdown page does.
    /// Override to change or skip this behavior.
    /// </summary>
    protected virtual (string Html, OutlineEntry[] Outline) ProcessHeadings(string html)
    {
        var document = new HtmlParser().ParseDocument(html);
        var entries = new List<OutlineEntry>();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var heading in document.QuerySelectorAll("h2, h3, h4, h5, h6"))
        {
            var text = heading.TextContent.Trim();
            if (text.Length == 0)
            {
                continue;
            }

            var existing = heading.GetAttribute("id");
            var id = EnsureUnique(string.IsNullOrEmpty(existing) ? Slugify(text) : existing, used);
            heading.SetAttribute("id", id);
            entries.Add(new OutlineEntry(id, text, heading.LocalName[1] - '0'));
        }

        return (document.Body?.InnerHtml ?? html, entries.ToArray());
    }

    private static string EnsureUnique(string id, HashSet<string> used)
    {
        if (used.Add(id))
        {
            return id;
        }

        var n = 2;
        string candidate;
        do
        {
            candidate = $"{id}-{n++}";
        }
        while (!used.Add(candidate));
        return candidate;
    }

    private static string Slugify(string text)
    {
        var sb = new StringBuilder(text.Length);
        var prevDash = false;
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                prevDash = false;
            }
            else if (!prevDash && sb.Length > 0)
            {
                sb.Append('-');
                prevDash = true;
            }
        }

        var slug = sb.ToString().Trim('-');
        return slug.Length == 0 ? "section" : slug;
    }
}

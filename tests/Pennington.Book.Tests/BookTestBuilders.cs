namespace Pennington.Book.Tests;

using System.Collections.Immutable;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Pennington.Content;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.Search;

/// <summary>Shared builders for fake navigation trees and inline-HTML rendered pages.</summary>
internal static class BookTestBuilders
{
    private static readonly HtmlParser Parser = new();

    public static NavigationTreeItem Node(string title, string path, params NavigationTreeItem[] children)
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(path),
            OutputFile = new FilePath(path.Trim('/')),
        };
        return new NavigationTreeItem(title, route, 0, null, false, false, [.. children]);
    }

    /// <summary>A section node with no page of its own (empty route), like an auto-created folder header.</summary>
    public static NavigationTreeItem Section(string title, params NavigationTreeItem[] children)
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(""),
            OutputFile = new FilePath(""),
        };
        return new NavigationTreeItem(title, route, 0, null, false, false, [.. children]);
    }

    /// <summary>
    /// Builds a rendered page whose HTML mirrors a DocSite article: an <c>#main-content</c> wrapper
    /// with a leading <c>&lt;h1&gt;</c> (the page title chrome the composer drops) plus the body HTML.
    /// </summary>
    public static RenderedPage Page(string path, string bodyHtml, string title = "Page Title")
    {
        var html = $"<article id=\"main-content\"><h1>{title}</h1>{bodyHtml}</article>";
        var content = Parser.ParseDocument(html).Body!;
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(path),
            OutputFile = new FilePath(path.Trim('/')),
        };
        var toc = new ContentTocItem(title, route, 0, path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries), null, null);
        return new RenderedPage(route, toc, null, html, content, new Lazy<IReadOnlyList<HeadingSection>>(() => []));
    }

    public static IReadOnlyDictionary<string, RenderedPage> PageMap(params RenderedPage[] pages)
    {
        var map = new Dictionary<string, RenderedPage>(StringComparer.OrdinalIgnoreCase);
        foreach (var page in pages)
        {
            map[page.Route.CanonicalPath.Value.Trim('/')] = page;
        }

        return map;
    }

    /// <summary>Parses composed book HTML and returns the document for querying.</summary>
    public static IDocument Parse(string html) => Parser.ParseDocument(html);
}

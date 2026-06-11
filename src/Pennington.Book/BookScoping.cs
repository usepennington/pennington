namespace Pennington.Book;

using System.Collections.Immutable;
using Content;
using Localization;
using Navigation;

/// <summary>Scopes a flat TOC to a single book by route prefix, shared by the artifact service and <c>diag books</c>.</summary>
internal static class BookScoping
{
    /// <summary>
    /// Unwraps a scoped tree whose single root merely restates the book itself — a per-area book's
    /// tree is one wrapper node (the area's folder section, or its index page) holding everything
    /// else. The root's children become the chapters and the root itself is returned — children
    /// emptied — for the composer to render as an unnumbered introduction when it has a page of its
    /// own; without the unwrap the whole book degenerates into one chapter whose title repeats the
    /// book. A root that is neither a bare section (no route) nor the book's own index page is a
    /// deliberate single-chapter structure and comes back unchanged with a null introduction.
    /// </summary>
    public static (NavigationTreeItem? Intro, ImmutableList<NavigationTreeItem> Chapters) UnwrapBookRoot(
        ImmutableList<NavigationTreeItem> tree,
        BookDefinition book)
    {
        if (tree is not [var root] || root.Children.Count == 0)
        {
            return (null, tree);
        }

        var rootKey = root.Route.CanonicalPath.Value.Trim('/');
        var prefixKey = book.NormalizedRoutePrefix.Trim('/');
        if (rootKey.Length != 0 && !string.Equals(rootKey, prefixKey, StringComparison.OrdinalIgnoreCase))
        {
            return (null, tree);
        }

        return (root with { Children = [] }, root.Children);
    }

    /// <summary>
    /// Returns the TOC entries that fall under <paramref name="routePrefix"/> in <paramref name="locale"/>.
    /// A <c>/</c> prefix matches the whole site. Paths are locale-stripped before matching so a book's
    /// prefix (<c>/tutorials/</c>) matches both <c>/tutorials/x</c> and <c>/fr/tutorials/x</c>.
    /// </summary>
    public static IReadOnlyList<ContentTocItem> ScopeToc(
        IReadOnlyList<ContentTocItem> toc,
        string routePrefix,
        LocalizationOptions localization,
        string? locale)
    {
        if (routePrefix == "/")
        {
            return toc;
        }

        var localeForStrip = locale ?? localization.DefaultLocale;
        var result = new List<ContentTocItem>();
        foreach (var item in toc)
        {
            var stripped = localization.StripLocalePrefix(item.Route.CanonicalPath.Value, localeForStrip);
            if (EnsureLeadingTrailingSlash(stripped).StartsWith(routePrefix, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static string EnsureLeadingTrailingSlash(string path)
    {
        var trimmed = path.Trim('/');
        return trimmed.Length == 0 ? "/" : $"/{trimmed}/";
    }
}

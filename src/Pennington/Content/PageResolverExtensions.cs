namespace Pennington.Content;

using FrontMatter;
using Pipeline;
using Routing;

/// <summary>Convenience extensions over <see cref="IPageResolver"/>.</summary>
public static class PageResolverExtensions
{
    /// <summary>
    /// Resolves <paramref name="requested"/> and narrows the rendered page's front matter to
    /// <typeparamref name="TFrontMatter"/> in one step, so callers read typed properties without a cast.
    /// Returns <c>null</c> when nothing matches, the match fails to render, or the matched page's front
    /// matter is not a <typeparamref name="TFrontMatter"/>.
    /// </summary>
    /// <typeparam name="TFrontMatter">The expected front-matter type.</typeparam>
    /// <param name="resolver">The resolver to query.</param>
    /// <param name="requested">Canonical URL to resolve.</param>
    public static async Task<RenderedItem<TFrontMatter>?> ResolveAsync<TFrontMatter>(
        this IPageResolver resolver, UrlPath requested)
        where TFrontMatter : IFrontMatter
    {
        if (await resolver.ResolveAsync(requested) is { } item && item.Metadata is TFrontMatter typed)
        {
            return new RenderedItem<TFrontMatter>(item.Route, typed, item.Content);
        }

        return null;
    }
}

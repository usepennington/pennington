namespace Pennington.Content;

using FrontMatter;
using Routing;

/// <summary>A blog post's typed front matter paired with its canonical URL, for listings.</summary>
/// <typeparam name="TFrontMatter">The post front-matter type.</typeparam>
/// <param name="FrontMatter">Parsed front matter for the post.</param>
/// <param name="Url">Canonical URL of the post.</param>
public sealed record BlogPostRef<TFrontMatter>(TFrontMatter FrontMatter, UrlPath Url)
    where TFrontMatter : IFrontMatter;

/// <summary>A blog post's front matter, canonical URL, and rendered HTML body.</summary>
/// <typeparam name="TFrontMatter">The post front-matter type.</typeparam>
/// <param name="FrontMatter">Parsed front matter for the post.</param>
/// <param name="Url">Canonical URL of the post.</param>
/// <param name="Html">Rendered HTML body.</param>
public sealed record RenderedBlogPost<TFrontMatter>(TFrontMatter FrontMatter, UrlPath Url, string Html)
    where TFrontMatter : IFrontMatter;

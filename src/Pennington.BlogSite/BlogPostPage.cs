namespace Pennington.BlogSite;

/// <summary>
/// A blog post with its front matter and metadata.
/// </summary>
/// <param name="FrontMatter">Parsed front matter for the post.</param>
/// <param name="Url">Canonical URL of the post.</param>
/// <param name="Tags">Tags applied to the post.</param>
public record BlogPostPage(BlogSiteFrontMatter FrontMatter, string Url, BlogTag[] Tags);

/// <summary>
/// A rendered blog post with HTML content.
/// </summary>
/// <param name="Page">The post being rendered.</param>
/// <param name="Html">Rendered HTML body.</param>
public record RenderedBlogPost(BlogPostPage Page, string Html);

/// <summary>
/// A tag with its display name and URL.
/// </summary>
/// <param name="Name">Tag display name.</param>
/// <param name="Url">URL of the tag index page.</param>
public record BlogTag(string Name, string Url);
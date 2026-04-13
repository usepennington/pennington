namespace Pennington.BlogSite;

/// <summary>
/// A blog post with its front matter and metadata.
/// </summary>
public record BlogPostPage(BlogSiteFrontMatter FrontMatter, string Url, BlogTag[] Tags);

/// <summary>
/// A rendered blog post with HTML content.
/// </summary>
public record RenderedBlogPost(BlogPostPage Page, string Html);

/// <summary>
/// A tag with its display name and URL.
/// </summary>
public record BlogTag(string Name, string Url);
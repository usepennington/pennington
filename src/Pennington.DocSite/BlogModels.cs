namespace Pennington.DocSite;

/// <summary>A blog post's front matter paired with its canonical URL, for listings.</summary>
/// <param name="FrontMatter">Parsed post front matter.</param>
/// <param name="Url">Canonical URL of the post.</param>
public record BlogPostSummary(BlogPostFrontMatter FrontMatter, string Url);

/// <summary>A blog post summary paired with its rendered HTML body.</summary>
/// <param name="Post">Post summary (front matter and URL).</param>
/// <param name="Html">Rendered HTML body of the post.</param>
public record RenderedBlogPost(BlogPostSummary Post, string Html);

/// <summary>A blog tag and the URL of its browse-by-tag page.</summary>
/// <param name="Name">Display name of the tag.</param>
/// <param name="Url">URL of the tag's listing page.</param>
public record BlogTag(string Name, string Url);
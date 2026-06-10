namespace Pennington.BlogSite;

/// <summary>
/// A rendered not-found body sourced from a content-root <c>404.md</c>. Carries only a title and
/// HTML — the post chrome (date, tags, series) does not apply to an error page.
/// </summary>
/// <param name="Title">Title from the <c>404.md</c> front matter (or a default when absent).</param>
/// <param name="Html">Rendered HTML body.</param>
public record RenderedNotFound(string Title, string Html);

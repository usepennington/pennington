namespace BlogKitchenSinkExample;

using Pennington.BlogSite;
using Pennington.StructuredData;

/// <summary>
/// Consumer-side helpers that build <see cref="JsonLdArticle"/>,
/// <see cref="JsonLdBreadcrumbList"/>, and <see cref="JsonLdWebSite"/>
/// records from a blog post's front matter and serialize them through
/// <see cref="JsonLdSerializer"/>. Fenced by the example block in
/// reference/structured-data/types.
/// </summary>
public static class StructuredDataBuilder
{
    /// <summary>
    /// Projects a <see cref="BlogSiteFrontMatter"/> into a
    /// <see cref="JsonLdArticle"/>. Every nullable field is left null when
    /// front matter is silent so <see cref="JsonLdSerializer"/> can skip it.
    /// </summary>
    public static JsonLdArticle BuildArticle(BlogSiteFrontMatter post, string canonicalUrl) =>
        new(
            Headline: post.Title,
            Description: post.Description,
            Url: canonicalUrl,
            DatePublished: post.Date,
            AuthorName: string.IsNullOrEmpty(post.Author) ? null : post.Author);

    /// <summary>
    /// Builds a two-rung <see cref="JsonLdBreadcrumbList"/> — site home,
    /// then the post's canonical URL — so crawlers see the post's place in
    /// the tree without needing to infer it from navigation.
    /// </summary>
    public static JsonLdBreadcrumbList BuildBreadcrumbs(BlogSiteFrontMatter post, string canonicalUrl, string homeUrl) =>
        new(new List<JsonLdBreadcrumbItem>
        {
            new(Position: 1, Name: "Home", Url: homeUrl),
            new(Position: 2, Name: post.Title, Url: canonicalUrl),
        });

    /// <summary>
    /// The <see cref="JsonLdWebSite"/> for the whole site. Emitted once per
    /// homepage so crawlers know the site's name and root URL.
    /// </summary>
    public static JsonLdWebSite BuildWebSite(string siteName, string homeUrl, string? description) =>
        new(Name: siteName, Url: homeUrl, Description: description);

    /// <summary>
    /// Returns the serialized Article JSON ready to drop into a
    /// <c>&lt;script type="application/ld+json"&gt;</c> tag. Mirrors the
    /// shape the <c>&lt;StructuredData&gt;</c> component emits.
    /// </summary>
    public static string BuildArticleJson(BlogSiteFrontMatter post, string canonicalUrl) =>
        JsonLdSerializer.SerializeArticle(BuildArticle(post, canonicalUrl));
}
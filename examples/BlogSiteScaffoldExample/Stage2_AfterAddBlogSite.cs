namespace BlogSiteScaffoldExample;

using Pennington.BlogSite; // [!code ++]

/// <summary>
/// Stage 2 — the fully-wired BlogSite host. `AddBlogSite` registers
/// Pennington core, MonorailCSS, the Mdazor component set, Razor-component
/// routing, the <c>BlogContentResolver</c>, and the <c>BlogSiteContentService</c>
/// that yields per-tag index routes and the <c>/rss.xml</c> feed.
/// `UseBlogSite` mounts the middleware stack in the right order and —
/// when <see cref="BlogSiteOptions.EnableRss"/> is true (the default) —
/// maps the <c>/rss.xml</c> endpoint. `RunBlogSiteAsync` delegates to
/// <c>RunOrBuildAsync</c> so the same host serves live and generates
/// static HTML. Identical in shape to <c>Program.cs</c>. Tutorial prose
/// extracts the body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>.
/// This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>The fully-wired BlogSite host — identical in shape to <c>Program.cs</c>.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddBlogSite(() => new BlogSiteOptions // [!code ++]
        { // [!code ++]
            SiteTitle = "Scaffold Blog", // [!code ++]
            Description = "A minimal BlogSite scaffold showing AddBlogSite, UseBlogSite, and RunBlogSiteAsync.", // [!code ++]
            CanonicalBaseUrl = "https://example.com", // [!code ++]

            ContentRootPath = "Content", // [!code ++]
            BlogContentPath = "Blog", // [!code ++]
            BlogBaseUrl = "/blog", // [!code ++]
            TagsPageUrl = "/tags", // [!code ++]

            AuthorName = "Author Name", // [!code ++]
            AuthorBio = "Writing about software, tools, and the occasional side project.", // [!code ++]
        }); // [!code ++]

        var app = builder.Build();

        app.UseBlogSite(); // [!code ++]

        await app.RunBlogSiteAsync(args); // [!code ++]
    }
}
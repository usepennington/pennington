namespace BlogSiteScaffoldExample;

using Pennington.BlogSite;

/// <summary>
/// Stage 3 — the final wired state. `UseBlogSite` mounts the middleware stack
/// in the right order (antiforgery, static files, Razor component routing,
/// MonorailCSS, Pennington core) and — when <see cref="BlogSiteOptions.EnableRss"/>
/// is true (the default) — maps the <c>/rss.xml</c> endpoint. `RunBlogSiteAsync`
/// delegates to <c>RunOrBuildAsync</c> so the same host serves live in dev and
/// generates static HTML when invoked as <c>dotnet run -- build &lt;baseUrl&gt;
/// &lt;outputDir&gt;</c> (both args optional; defaults <c>/</c> and <c>output</c>).
/// Identical in shape to <c>Program.cs</c>. Tutorial prose extracts the body of
/// <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>The fully-wired BlogSite host — identical in shape to <c>Program.cs</c>.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddBlogSite(() => new BlogSiteOptions
        {
            SiteTitle = "Scaffold Blog",
            SiteDescription = "A minimal BlogSite scaffold showing AddBlogSite, UseBlogSite, and RunBlogSiteAsync.",
            CanonicalBaseUrl = "https://example.com",

            ContentRootPath = "Content",
            BlogContentPath = "Blog",
            BlogBaseUrl = "/blog",
            TagsPageUrl = "/tags",

            AuthorName = "Author Name",
            AuthorBio = "Writing about software, tools, and the occasional side project.",
        });

        var app = builder.Build();

        app.UseBlogSite(); // [!code ++]

        await app.RunBlogSiteAsync(args); // [!code ++]
    }
}
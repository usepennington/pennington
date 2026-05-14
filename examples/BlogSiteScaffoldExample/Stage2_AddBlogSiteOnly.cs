namespace BlogSiteScaffoldExample;

using Pennington.BlogSite; // [!code ++]

/// <summary>
/// Stage 2 — swap `AddPennington` for `AddBlogSite` and populate
/// <see cref="BlogSiteOptions"/>. The BlogSite template registers Pennington
/// core internally, wires the Razor component layout, Mdazor, MonorailCSS,
/// the file-watched <c>BlogContentResolver</c>, and the
/// <c>BlogSiteContentService</c> — all in a single call. The middleware
/// (`UseBlogSite`) and run/build entry point arrive in <see cref="Stage3"/>.
/// Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Register BlogSite services with the core options.</summary>
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

        await app.RunAsync();
    }
}

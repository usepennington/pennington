namespace BlogSiteHeroProjectsSocialsExample;

using Pennington.BlogSite;

/// <summary>
/// Stage 1 — the reader starts from the tutorial-8 host and populates just
/// <see cref="Pennington.BlogSite.BlogSiteOptions.HeroContent"/>. The
/// <see cref="Pennington.BlogSite.HeroContent"/> record is two strings
/// (<c>Title</c> and <c>Description</c>) rendered into a prose block at the
/// top of the home page. Tutorial prose extracts the body of
/// <see cref="Run"/> via <c>csharp:xmldocid,bodyonly</c>. This class is
/// never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>Stage-1 host wiring — HeroContent populated, projects and socials still empty.</summary>
    public static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddBlogSite(() => new BlogSiteOptions
        {
            SiteTitle = "Hero Blog",
            Description = "A BlogSite tutorial app demonstrating hero, projects, and social links.",
            CanonicalBaseUrl = "https://example.com",

            AuthorName = "Author Name",
            AuthorBio = "Writing about software, tools, and the occasional side project.",

            HeroContent = new HeroContent( // [!code ++]
                Title: "Field notes from a weekend content engine", // [!code ++]
                Description: "I build small tools for small problems. This is where I write about them."), // [!code ++]
        });

        var app = builder.Build();
        app.UseBlogSite();
        app.RunBlogSiteAsync(args).GetAwaiter().GetResult();
    }
}
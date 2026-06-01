namespace BlogSiteHeroProjectsSocialsExample;

using Pennington.BlogSite;

/// <summary>
/// Stage 2 — adds <see cref="BlogSiteOptions.MyWork"/>,
/// a <see cref="Project"/><c>[]</c>. Each project is a
/// three-field record: <c>Title</c>, <c>Description</c>, <c>Url</c>. The
/// BlogSite home page renders the array as a "My Work" card in the right
/// rail, each entry linking to its <c>Url</c>. Tutorial prose extracts the
/// body of <see cref="Run"/> via <c>csharp:xmldocid,bodyonly</c>. This
/// class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Stage-2 host wiring — HeroContent and MyWork populated.</summary>
    public static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddBlogSite(() => new BlogSiteOptions
        {
            SiteTitle = "Hero Blog",
            SiteDescription = "A BlogSite tutorial app demonstrating hero, projects, and social links.",
            CanonicalBaseUrl = "https://example.com",

            AuthorName = "Author Name",
            AuthorBio = "Writing about software, tools, and the occasional side project.",

            HeroContent = new HeroContent(
                Title: "Field notes from a weekend content engine",
                Description: "I build small tools for small problems. This is where I write about them."),

            MyWork = // [!code ++]
            [ // [!code ++]
                new Project( // [!code ++]
                    Title: "Pennington", // [!code ++]
                    Description: "A tiny .NET content engine for docs and blogs.", // [!code ++]
                    Url: "https://github.com/example/pennington"), // [!code ++]
                new Project( // [!code ++]
                    Title: "MonorailCSS", // [!code ++]
                    Description: "Utility-first CSS generation for Razor.", // [!code ++]
                    Url: "https://github.com/example/monorailcss"), // [!code ++]
                new Project( // [!code ++]
                    Title: "Mdazor", // [!code ++]
                    Description: "Inline Razor components inside Markdown.", // [!code ++]
                    Url: "https://github.com/example/mdazor"), // [!code ++]
            ], // [!code ++]
        });

        var app = builder.Build();
        app.UseBlogSite();
        app.RunBlogSiteAsync(args).GetAwaiter().GetResult();
    }
}
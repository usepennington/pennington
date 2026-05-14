namespace BlogSiteHeroProjectsSocialsExample;

using Pennington.BlogSite;
using Pennington.BlogSite.Components; // [!code ++]

/// <summary>
/// Stage 3 — the final state, matching the top-level <c>Program.cs</c>. Adds
/// <see cref="Pennington.BlogSite.BlogSiteOptions.Socials"/> (a
/// <see cref="Pennington.BlogSite.SocialLink"/><c>[]</c>, each carrying a
/// <c>RenderFragment</c> icon and a <c>Url</c>) and
/// <see cref="Pennington.BlogSite.BlogSiteOptions.MainSiteLinks"/> (a
/// <see cref="Pennington.BlogSite.HeaderLink"/><c>[]</c>, each a
/// <c>Title</c>/<c>Url</c> pair for the top-nav). The four built-in icon
/// <c>RenderFragment</c>s live as <c>static readonly</c> fields on the
/// <see cref="Pennington.BlogSite.Components.SocialIcons"/> component:
/// <c>GithubIcon</c>, <c>BlueskyIcon</c>, <c>LinkedInIcon</c>,
/// <c>MastodonIcon</c>. Tutorial prose extracts the body of
/// <see cref="Run"/> via <c>csharp:xmldocid,bodyonly</c>. This class is
/// never instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>Stage-3 host wiring — all four homepage surfaces populated.</summary>
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

            HeroContent = new HeroContent(
                Title: "Field notes from a weekend content engine",
                Description: "I build small tools for small problems. This is where I write about them."),

            MyWork =
            [
                new Project(
                    Title: "Pennington",
                    Description: "A tiny .NET content engine for docs and blogs.",
                    Url: "https://github.com/example/pennington"),
                new Project(
                    Title: "MonorailCSS",
                    Description: "Utility-first CSS generation for Razor.",
                    Url: "https://github.com/example/monorailcss"),
                new Project(
                    Title: "Mdazor",
                    Description: "Inline Razor components inside Markdown.",
                    Url: "https://github.com/example/mdazor"),
            ],

            Socials = // [!code ++]
            [ // [!code ++]
                new SocialLink(SocialIcons.GithubIcon, "https://github.com/example"), // [!code ++]
                new SocialLink(SocialIcons.BlueskyIcon, "https://bsky.app/profile/example.bsky.social"), // [!code ++]
                new SocialLink(SocialIcons.LinkedInIcon, "https://www.linkedin.com/in/example"), // [!code ++]
                new SocialLink(SocialIcons.MastodonIcon, "https://hachyderm.io/@example"), // [!code ++]
            ], // [!code ++]

            MainSiteLinks = // [!code ++]
            [ // [!code ++]
                new HeaderLink("Home", "/"), // [!code ++]
                new HeaderLink("Archive", "/archive"), // [!code ++]
                new HeaderLink("Tags", "/tags"), // [!code ++]
            ], // [!code ++]
        });

        var app = builder.Build();
        app.UseBlogSite();
        app.RunBlogSiteAsync(args).GetAwaiter().GetResult();
    }
}
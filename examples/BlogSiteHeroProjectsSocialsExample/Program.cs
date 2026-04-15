using Pennington.BlogSite;
using Pennington.BlogSite.Components;

var builder = WebApplication.CreateBuilder(args);

// Tutorial 1.3.30 extends the BlogSiteFirstPostExample host by populating the
// four homepage surfaces on BlogSiteOptions: HeroContent (the headline block
// at the top of "/"), MyWork (the "My Work" card in the home page sidebar),
// Socials (the icon row under it), and MainSiteLinks (the top-nav links in
// the site header). Each surface is a record type on Pennington.BlogSite.
// The four icon RenderFragments on SocialIcons (GithubIcon, BlueskyIcon,
// LinkedInIcon, MastodonIcon) are the built-ins the tutorial teaches.
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "Hero Blog",
    Description = "A BlogSite tutorial app demonstrating hero, projects, and social links.",
    CanonicalBaseUrl = "https://example.com",

    AuthorName = "Author Name",
    AuthorBio = "Writing about software, tools, and the occasional side project.",

    HeroContent = new HeroContent(
        Title: "Field notes from a weekend content engine",
        Description: "I build small tools for small problems. This is where I write about them — Pennington, static sites, and the rabbit holes in between."),

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

    Socials =
    [
        new SocialLink(SocialIcons.GithubIcon, "https://github.com/example"),
        new SocialLink(SocialIcons.BlueskyIcon, "https://bsky.app/profile/example.bsky.social"),
        new SocialLink(SocialIcons.LinkedInIcon, "https://www.linkedin.com/in/example"),
        new SocialLink(SocialIcons.MastodonIcon, "https://hachyderm.io/@example"),
    ],

    MainSiteLinks =
    [
        new HeaderLink("Home", "/"),
        new HeaderLink("Archive", "/archive"),
        new HeaderLink("Tags", "/tags"),
    ],
});

var app = builder.Build();

app.UseBlogSite();

await app.RunBlogSiteAsync(args);
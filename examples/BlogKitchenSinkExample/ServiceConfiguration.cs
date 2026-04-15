namespace BlogKitchenSinkExample;

using Pennington.BlogSite;
using Pennington.BlogSite.Components;

/// <summary>
/// Small, focused configuration helpers called from <c>Program.cs</c>.
/// Each method populates exactly one surface on <see cref="BlogSiteOptions"/>
/// so how-to pages can embed the method body with <c>csharp:xmldocid,bodyonly</c>.
/// </summary>
internal static class ServiceConfiguration
{
    /// <summary>
    /// The hero headline block at the top of <c>/</c>. <see cref="HeroContent.Description"/>
    /// is rendered through <c>MarkupString</c>, so inline HTML passes through.
    /// </summary>
    public static HeroContent BuildHero() => new(
        Title: "Field notes from the Pennington workshop",
        Description: "I build small tools for small problems. This is where I write about them — Pennington, MonorailCSS, Mdazor, and the rabbit holes in between.");

    /// <summary>
    /// Projects rendered in the "My Work" card on the home page sidebar.
    /// Each <see cref="Project"/> is a <c>Title</c> / <c>Description</c> /
    /// <c>Url</c> triple wrapped in an anchor.
    /// </summary>
    public static Project[] BuildMyWork() =>
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
        new Project(
            Title: "TextMateSharp",
            Description: "Server-side TextMate grammar highlighting.",
            Url: "https://github.com/example/textmatesharp"),
        new Project(
            Title: "Field Notes",
            Description: "Long-running notebook of small experiments.",
            Url: "https://example.com/notebook"),
    ];

    /// <summary>
    /// Social links rendered as an icon row under the "My Work" card.
    /// Uses every built-in <c>RenderFragment</c> on
    /// <see cref="SocialIcons"/> — <c>GithubIcon</c>, <c>BlueskyIcon</c>,
    /// <c>LinkedInIcon</c>, and <c>MastodonIcon</c>. The field syntax
    /// (not a component tag) is important: <see cref="SocialLink.Icon"/>
    /// is a <c>RenderFragment</c>, not a component type.
    /// </summary>
    public static SocialLink[] BuildSocials() =>
    [
        new SocialLink(SocialIcons.GithubIcon, "https://github.com/example"),
        new SocialLink(SocialIcons.BlueskyIcon, "https://bsky.app/profile/example.bsky.social"),
        new SocialLink(SocialIcons.LinkedInIcon, "https://www.linkedin.com/in/example"),
        new SocialLink(SocialIcons.MastodonIcon, "https://hachyderm.io/@example"),
    ];

    /// <summary>
    /// Header links rendered in both the top-nav and the footer nav of
    /// <c>MainLayout.razor</c> (so every link appears twice in the output).
    /// </summary>
    public static HeaderLink[] BuildMainSiteLinks() =>
    [
        new HeaderLink("Home", "/"),
        new HeaderLink("Archive", "/archive"),
        new HeaderLink("Tags", "/tags"),
        new HeaderLink("About", "https://example.com/about"),
    ];

    /// <summary>
    /// Builds the final <see cref="BlogSiteOptions"/> used by <c>AddBlogSite</c>.
    /// Every homepage surface plus the RSS and sitemap toggles are wired here
    /// so each how-to can fence into one helper method above.
    /// </summary>
    public static BlogSiteOptions BuildBlogSiteOptions() => new()
    {
        SiteTitle = "Pennington Kitchen Sink Blog",
        Description = "A kitchen-sink BlogSite example that backs two how-to pages and two reference pages.",
        CanonicalBaseUrl = "https://blog.example.com",

        AuthorName = "Jamie Rivers",
        AuthorBio = "Writing about content engines, static sites, and the tools in between.",

        // Explicit for teaching value even though both default to true.
        EnableRss = true,
        EnableSitemap = true,

        HeroContent = BuildHero(),
        MyWork = BuildMyWork(),
        Socials = BuildSocials(),
        MainSiteLinks = BuildMainSiteLinks(),
    };
}
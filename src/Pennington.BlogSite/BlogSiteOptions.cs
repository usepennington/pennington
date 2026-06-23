namespace Pennington.BlogSite;

using System.Reflection;
using Content;
using Favicon;
using Infrastructure;
using Microsoft.AspNetCore.Components;
using MonorailCss;
using Routing;
using SocialCards;
using StandardSite;

/// <summary>
/// Options record passed to <see cref="BlogSiteServiceExtensions.AddBlogSite"/> that configures
/// the BlogSite template: site identity, content paths, typography, author chrome, homepage
/// composition (hero, project cards, social links, nav), and feed toggles (RSS, sitemap).
/// </summary>
public record BlogSiteOptions
{
    /// <summary>Site title shown in the header, OpenGraph tags, and RSS channel.</summary>
    public required string SiteTitle { get; init; }

    /// <summary>Short description used for the meta description tag and RSS channel.</summary>
    public required string SiteDescription { get; init; }

    /// <summary>Absolute base URL used to build canonical links, sitemap, and RSS entries.</summary>
    public string? CanonicalBaseUrl { get; init; }

    /// <summary>Color scheme driving the MonorailCSS theme. Defaults to the built-in BlogSite palette when null.</summary>
    public IColorScheme? ColorScheme { get; init; }

    /// <summary>Root folder (relative to the content project) that holds the content tree.</summary>
    public FilePath ContentRootPath { get; init; } = new("Content");

    /// <summary>Folder (relative to <see cref="ContentRootPath"/>) containing blog post markdown files.</summary>
    public string BlogContentPath { get; init; } = "Blog";

    /// <summary>URL prefix under which blog posts are published.</summary>
    public string BlogBaseUrl { get; init; } = "/blog";

    /// <summary>Number of posts per page on the archive listing. Set to a non-positive value to disable pagination.</summary>
    public int PostsPerPage { get; init; } = 10;

    /// <summary>Additional CSS appended to the generated stylesheet.</summary>
    public string? ExtraStyles { get; init; }

    /// <summary>Raw HTML appended to the document <c>&lt;head&gt;</c> (for analytics, meta tags, etc.).</summary>
    public string? AdditionalHtmlHeadContent { get; init; }

    /// <summary>Fonts to preload via <c>&lt;link rel="preload"&gt;</c> for faster first paint.</summary>
    public FontPreload[] FontPreloads { get; init; } = [];

    /// <summary>Additional assemblies scanned for Razor components so out-of-project pages participate in routing.</summary>
    public Assembly[] AdditionalRoutingAssemblies { get; init; } = [];

    /// <summary>Author name displayed in the byline and RSS channel.</summary>
    public string? AuthorName { get; init; }

    /// <summary>Short author bio displayed on the homepage and post pages.</summary>
    public string? AuthorBio { get; init; }

    /// <summary>When true, an RSS feed is generated for blog posts.</summary>
    public bool EnableRss { get; init; } = true;

    /// <summary>When true, a sitemap.xml is generated for the site.</summary>
    public bool EnableSitemap { get; init; } = true;

    /// <summary>Homepage hero content. When null, the hero section is not rendered.</summary>
    public HeroContent? HeroContent { get; init; }

    /// <summary>Featured projects displayed on the homepage.</summary>
    public Project[] MyWork { get; init; } = [];

    /// <summary>Social media links rendered in the site chrome.</summary>
    public SocialLink[] Socials { get; init; } = [];

    /// <summary>Navigation links rendered in the site header.</summary>
    public HeaderLink[] MainSiteLinks { get; init; } = [];

    /// <summary>Factory producing a social-share image URL for a given post. Return null to fall back to defaults.</summary>
    public Func<BlogPostRef<BlogSiteFrontMatter>, string?>? SocialMediaImageUrlFactory { get; init; }

    /// <summary>
    /// Enables generated per-post social cards. When set, each post's <c>og:image</c>/<c>twitter:image</c>
    /// points at an on-demand-rendered card (unless <see cref="SocialMediaImageUrlFactory"/> returns a URL
    /// for that post, which wins). The host supplies the drawing via <see cref="SocialCardOptions.Render"/>.
    /// </summary>
    public SocialCardOptions? SocialCards { get; init; }

    /// <summary>
    /// Standard Site (AT Protocol) integration. Forwarded to <see cref="PenningtonOptions.StandardSite"/>.
    /// </summary>
    public StandardSiteOptions? StandardSite { get; init; }

    /// <summary>
    /// Favicon / icon links. Forwarded to <see cref="PenningtonOptions.Favicons"/>.
    /// </summary>
    public FaviconOptions? Favicons { get; init; }
}

/// <summary>Icon and URL for a social media link.</summary>
/// <param name="Icon">Rendered icon markup.</param>
/// <param name="Url">Target URL.</param>
public record SocialLink(RenderFragment Icon, string Url);

/// <summary>Link rendered in the main site navigation.</summary>
/// <param name="Title">Display text.</param>
/// <param name="Url">Target URL.</param>
public record HeaderLink(string Title, string Url);

/// <summary>Featured project card shown on the blog homepage.</summary>
/// <param name="Title">Project title.</param>
/// <param name="Description">Short project description.</param>
/// <param name="Url">Link target for the card.</param>
public record Project(string Title, string Description, string Url);

/// <summary>Hero block at the top of the blog homepage.</summary>
/// <param name="Title">Hero headline.</param>
/// <param name="Description">Hero subhead/body text.</param>
public record HeroContent(string Title, string Description);
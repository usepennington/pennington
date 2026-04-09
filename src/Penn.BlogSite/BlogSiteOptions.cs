namespace Pennington.BlogSite;

using System.Reflection;
using Microsoft.AspNetCore.Components;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Routing;

/// <summary>
/// Configuration options for a blog site.
/// </summary>
public record BlogSiteOptions
{
    public required string SiteTitle { get; init; }
    public required string Description { get; init; }
    public string? CanonicalBaseUrl { get; init; }

    public IColorScheme? ColorScheme { get; init; }

    public string ContentRootPath { get; init; } = "Content";
    public string BlogContentPath { get; init; } = "Blog";
    public string BlogBaseUrl { get; init; } = "/blog";
    public string TagsPageUrl { get; init; } = "/tags";

    public string? ExtraStyles { get; init; }
    public string? DisplayFontFamily { get; init; }
    public string? BodyFontFamily { get; init; }
    public string? AdditionalHtmlHeadContent { get; init; }
    public FontPreload[] FontPreloads { get; init; } = [];

    public Assembly[] AdditionalRoutingAssemblies { get; init; } = [];

    public string? AuthorName { get; init; }
    public string? AuthorBio { get; init; }

    public bool EnableRss { get; init; } = true;
    public bool EnableSitemap { get; init; } = true;

    public HeroContent? HeroContent { get; init; }
    public Project[] MyWork { get; init; } = [];
    public SocialLink[] Socials { get; init; } = [];
    public HeaderLink[] MainSiteLinks { get; init; } = [];

    public Func<BlogPostPage, string>? SocialMediaImageUrlFactory { get; init; }
}

public record SocialLink(RenderFragment Icon, string Url);
public record HeaderLink(string Title, string Url);
public record Project(string Title, string Description, string Url);
public record HeroContent(string Title, string Description);

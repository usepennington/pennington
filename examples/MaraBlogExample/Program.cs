using MonorailCss.Theme;
using Penn.BlogSite;
using Penn.BlogSite.Components;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "Mara Writes Code",
    Description = "Performance engineering for .NET",
    ContentRootPath = "Content",
    BlogContentPath = "Posts",
    BlogBaseUrl = "/blog",
    TagsPageUrl = "/topics",
    CanonicalBaseUrl = "https://mara.dev",
    AuthorName = "Mara Chen",
    AuthorBio = "Performance engineer at a .NET shop. I write about making things fast.",
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 25,
        BaseColorName = ColorNames.Zinc,
    },
    HeroContent = new HeroContent(
        "Performance engineer & writer",
        "I'm <strong>Mara</strong>. I help .NET teams ship faster software — literally."),
    MyWork =
    [
        new Project("HotPath", "Zero-allocation pipeline for .NET", "https://github.com/example/hotpath"),
        new Project("BenchTool", "CLI benchmarking harness", "https://github.com/example/benchtool"),
    ],
    Socials =
    [
        new SocialLink(SocialIcons.GithubIcon, "https://github.com/example-mara"),
        new SocialLink(SocialIcons.MastodonIcon, "https://mastodon.social/@example-mara"),
        new SocialLink(SocialIcons.LinkedInIcon, "https://linkedin.com/in/example-mara"),
    ],
    EnableRss = true,
    EnableSitemap = true,
});

var app = builder.Build();
app.UseBlogSite();
app.MapStaticAssets();

await app.RunBlogSiteAsync(args);

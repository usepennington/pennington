using Pennington.BlogSite;
using Pennington.BlogSite.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "Alex's Dev Blog",
    Description = "Notes on .NET, tools, and developer life",
    ContentRootPath = "Content",
    BlogContentPath = "Blog",
    BlogBaseUrl = "/blog",
    TagsPageUrl = "/tags",
    EnableRss = true,
    EnableSitemap = true,
    CanonicalBaseUrl = "https://alexchen.dev",
    AuthorName = "Alex Chen",
    AuthorBio = "Software engineer who loves building tools and sharing what I learn along the way.",
    HeroContent = new HeroContent(
        "Hi, I'm Alex",
        "I write about .NET, developer tooling, and the occasional deep dive into something unexpected."),
    Socials =
    [
        new SocialLink(SocialIcons.GithubIcon, "https://github.com/example-alex"),
        new SocialLink(SocialIcons.MastodonIcon, "https://mastodon.social/@example-alex"),
    ],
    MyWork =
    [
        new Project("Tempo", "A lightweight task scheduler for .NET", "https://github.com/example/tempo"),
    ],
});

var app = builder.Build();
app.UseBlogSite();
app.MapStaticAssets();

await app.RunBlogSiteAsync(args);
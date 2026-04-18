using Pennington.BlogSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "A blog built with Pennington.",
    CanonicalBaseUrl = "https://example.com",

    ContentRootPath = "Content",
    BlogContentPath = "Blog",
    BlogBaseUrl = "/blog",
    TagsPageUrl = "/tags",

    AuthorName = "Author Name",
    AuthorBio = "Writing about software, tools, and the occasional side project.",
});

var app = builder.Build();

app.UseBlogSite();

await app.RunBlogSiteAsync(args);

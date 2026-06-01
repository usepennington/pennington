using Pennington.BlogSite;

var builder = WebApplication.CreateBuilder(args);

// Tutorial 1.3.20 extends the BlogSiteScaffoldExample host with a fully
// populated post. The host shape is identical to tutorial 1.3.10 — the
// teaching surface this tutorial adds is entirely in Content/Blog/my-first-post.md,
// where every field on `BlogSiteFrontMatter` that the reader will ever touch is
// populated with a meaningful value. `EnableRss = true` is the default (see
// `BlogSiteOptions`) — we set it explicitly so the tutorial's RSS step has a
// symbol to point at. `EnableSitemap` likewise defaults to true.
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "First Post Blog",
    SiteDescription = "A BlogSite tutorial app demonstrating a fully-populated BlogSiteFrontMatter.",
    CanonicalBaseUrl = "https://example.com",

    AuthorName = "Author Name",
    AuthorBio = "Writing about software, tools, and the occasional side project.",

    // Explicit for teaching value even though both default to true.
    EnableRss = true,
    EnableSitemap = true,
});

var app = builder.Build();

app.UseBlogSite();

await app.RunBlogSiteAsync(args);
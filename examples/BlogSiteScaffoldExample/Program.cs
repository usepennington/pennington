using Pennington.BlogSite;

var builder = WebApplication.CreateBuilder(args);

// Swap the bare `AddPennington` host for the BlogSite template. `AddBlogSite`
// wires the full blog experience on top of Pennington core — a Blazor
// layout with a home page that lists recent posts, an /archive page,
// /blog/<slug> post pages, /tags and /tags/<name> listings, and an
// /rss.xml feed — all driven from `BlogSiteOptions`.
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "Scaffold Blog",
    SiteDescription = "A minimal BlogSite scaffold showing AddBlogSite, UseBlogSite, and RunBlogSiteAsync.",
    CanonicalBaseUrl = "https://example.com",

    // BlogSite defaults put posts under `{ContentRootPath}/{BlogContentPath}`
    // (Content/Blog) and serves them at `BlogBaseUrl` (/blog); tag listings live
    // at /tags. Overriding the defaults is as simple as setting the matching
    // property — shown here with the defaults for clarity.
    ContentRootPath = "Content",
    BlogContentPath = "Blog",
    BlogBaseUrl = "/blog",

    // Author identity feeds into the RSS channel, JSON-LD article markup,
    // and any post that omits its own `author:` front-matter value.
    AuthorName = "Author Name",
    AuthorBio = "Writing about software, tools, and the occasional side project.",
});

var app = builder.Build();

// `UseBlogSite` mounts antiforgery, static files, Razor component routing
// (Home/Archive/Blog/Tag/Tags live inside Pennington.BlogSite.dll), the
// MonorailCSS `/styles.css` endpoint, and the core Pennington middleware —
// all in the right order. When `EnableRss` is true (the default) it also
// maps `/rss.xml` so the static crawler picks the feed up.
app.UseBlogSite();

// `RunBlogSiteAsync` delegates to `RunOrBuildAsync`, so `dotnet run` serves the
// blog live and `dotnet run -- build <baseUrl> <outputDir>` generates static
// HTML. Both positional args are optional (defaults: `/` and `output`).
await app.RunBlogSiteAsync(args);
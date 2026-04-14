using Pennington.BlogSite;

var builder = WebApplication.CreateBuilder(args);

// Kitchen-sink BlogSite host. The configuration surface is deliberately wide —
// hero, projects, all four built-in social icons, multiple header links, RSS
// and sitemap enabled — so each configuration how-to can `xmldocid,bodyonly`
// fence into one of the small helper methods on `ServiceConfiguration` to show
// exactly the surface that how-to teaches. Three dated posts keep the archive,
// the tags index, and the RSS channel populated.
builder.Services.AddBlogSite(BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions);

var app = builder.Build();

app.UseBlogSite();

await app.RunBlogSiteAsync(args);

using DocSiteChromeOverridesExample;
using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// Live wiring referenced from step 6 of
// how-to/extensibility/override-docsite-components. The factory is a
// method reference, so the helper in SiteChromeOverrides.cs owns the
// full DocSiteOptions shape and Program.cs stays short.
builder.Services.AddDocSite(SiteChromeOverrides.BuildDocSiteOptions);

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
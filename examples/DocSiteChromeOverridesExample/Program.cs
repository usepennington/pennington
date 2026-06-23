using DocSiteChromeOverridesExample;
using DocSiteChromeOverridesExample.Components;
using Mdazor;
using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// Live wiring referenced from step 6 of
// how-to/extensibility/override-docsite-components. The factory is a
// method reference, so the helper in SiteChromeOverrides.cs owns the
// full DocSiteOptions shape and Program.cs stays short.
builder.Services.AddDocSite(SiteChromeOverrides.BuildDocSiteOptions);

// The home page (Content/index.md) renders <BrandPalette /> to show off the
// active ColorScheme; register the component so Mdazor resolves the tag.
builder.Services.AddMdazorComponent<BrandPalette>();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
using DocSiteKitchenSinkExample.Components;
using Mdazor;
using Pennington.DocSite;
using Pennington.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Kitchen-sink DocSite host. The configuration surface is deliberately wide —
// two areas, two locales, a custom color scheme, font preloads, extra CSS,
// a bespoke Mdazor component, a custom footer, and a GitHub URL — so each
// how-to page can `xmldocid,bodyonly` fence into one of the small helper
// methods on `ServiceConfiguration` to show exactly the surface that how-to
// teaches.
builder.Services.AddDocSite(DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions);

// Register the custom Mdazor component used by `Content/main/ui-components-in-markdown.md`.
// `AddMdazorComponent<T>()` is the one DI line needed — Mdazor's registry
// discovers the tag at markdown render time.
builder.Services.AddMdazorComponent<FeatureCallout>();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

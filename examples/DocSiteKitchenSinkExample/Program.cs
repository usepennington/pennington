using DocSiteKitchenSinkExample.Components;
using Mdazor;
using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// Kitchen-sink DocSite host. The configuration surface is deliberately wide —
// two areas, two locales, a custom color scheme, font preloads, extra CSS,
// a bespoke Mdazor component, a custom footer, and a GitHub URL — so each
// how-to page can `xmldocid,bodyonly` fence into one of the small helper
// methods on `ServiceConfiguration` to show exactly the surface that how-to
// teaches.
builder.Services.AddDocSite(DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions);

// Register the custom Mdazor components used by `Content/main/ui-components-in-markdown.md`.
// `AddMdazorComponent<T>()` is the one DI line needed per component — Mdazor's registry
// discovers the tags at markdown render time. FeatureCallout binds its parameters from
// tag attributes; PageFacts instead reads page facts (file name, URL, front matter) from
// the ambient MdazorContext that Pennington supplies per page.
builder.Services
    .AddMdazorComponent<FeatureCallout>()
    .AddMdazorComponent<PageFacts>()
    .AddMdazorComponent<StabilityBadge>();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
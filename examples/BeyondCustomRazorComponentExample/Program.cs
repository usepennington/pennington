using BeyondCustomRazorComponentExample.Components;
using Mdazor;
using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// Same DocSite host shape as the earlier tutorials. The only new line is the
// `AddMdazorComponent<PricingCard>()` call below the `AddDocSite` block —
// everything else is already wired by DocSite (Mdazor infrastructure, the
// built-in Pennington.UI components, MonorailCSS, Blazor routing, and the
// `RunOrBuildAsync` dev/build split).
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beyond Custom Razor Component",
    SiteDescription = "Authoring a Razor component and rendering it inline from markdown.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Beyond Custom Razor Component</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
});

// Register the custom PricingCard component with the Mdazor component registry.
// `AddMdazorComponent<T>()` is an `IServiceCollection` extension shipped by the
// `Mdazor` NuGet package (already transitively referenced through Pennington.DocSite).
// With this one line, the markdown renderer will pick up `<PricingCard ... />`
// tags in any page under `Content/` and render them as real Blazor components
// with their parameters bound from the tag attributes.
builder.Services.AddMdazorComponent<PricingCard>();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
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

// Register the custom components with the Mdazor component registry.
// `AddMdazorComponent<T>()` is an `IServiceCollection` extension shipped by the
// `Mdazor` NuGet package (already transitively referenced through Pennington.DocSite).
// With this one line per component, the markdown renderer picks up `<PricingCard ... />`
// and `<PageFacts />` tags in any page under `Content/` and renders them as real Blazor
// components — PricingCard binds its parameters from tag attributes, while PageFacts
// reads page facts (file name, URL, front matter) from the ambient MdazorContext that
// Pennington supplies per page.
builder.Services
    .AddMdazorComponent<PricingCard>()
    .AddMdazorComponent<PageFacts>();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
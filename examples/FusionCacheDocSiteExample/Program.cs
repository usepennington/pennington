using Pennington.ApiMetadata.Reflection;
using Pennington.DocSite;
using Pennington.DocSite.Api;

var builder = WebApplication.CreateBuilder(args);

// Standard DocSite wiring. No Areas — single default TOC.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "FusionCache Docs",
    Description = "Demo of Pennington's reflection-based API metadata backend, documenting ZiggyCreatures.FusionCache straight from its NuGet package.",
    GitHubUrl = "https://github.com/ZiggyCreatures/FusionCache",
    HeaderContent = """<a href="/" class="font-bold text-lg">FusionCache Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">FusionCache © ZiggyCreatures. Rendered by Pennington.</footer>""",
});

// Reflection-backed API metadata sourced from the ZiggyCreatures.FusionCache
// NuGet package reference in the .csproj — no live compilation, no staged
// dll/xml, no vendored source.
builder.Services.AddApiMetadataFromCompiledAssembly(opts =>
    opts.FromPackageReference("ZiggyCreatures.FusionCache"));

// Auto-publishes /api/{slug}/ pages off the metadata provider and registers
// the <ApiSummary>, <ApiMemberTable>, <ApiParameterTable>, ... Mdazor
// components. Sits in the Guides section of the sidebar so readers drop into
// the full type index from the same nav they read the stampede/fail-safe
// guides in.
builder.Services.AddApiReference(configure: opts =>
{
    opts.RoutePrefix = "/api/";
    opts.TocSectionLabel = "Guides";
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
using Pennington.ApiMetadata.Reflection;
using Pennington.DocSite;
using Pennington.DocSite.Api;

var builder = WebApplication.CreateBuilder(args);

// Two areas — one per documented version. ContentArea slugs map to top-level
// folders under Content/, so files at Content/v1/foo.md route to /v1/foo and
// the sidebar renders an area selector that doubles as a version switcher.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Humanizer Docs",
    SiteDescription = "Side-by-side documentation for two versions of Humanizer.Core, with version-scoped content and a sidebar version selector.",
    GitHubUrl = "https://github.com/Humanizr/Humanizer",
    HeaderContent = """<a href="/" class="font-bold text-lg">Humanizer Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Humanizer © Mehdi Khalili & contributors. Rendered by Pennington.</footer>""",
    Areas =
    [
        new ContentArea("v1", "v1"),
        new ContentArea("v2", "v2"),
    ],
});

// Only one version of any given assembly can load through the default load
// context, so FromPackageReference is reserved for the active (v2) reference.
// v1 lives in the NuGet cache via <PackageDownload> in the .csproj — point
// AssemblyFiles at its cached path directly.
var nuGetPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
var humanizerV1Dll = Path.Combine(nuGetPackages, "humanizer.core", "2.8.26", "lib", "netstandard2.0", "Humanizer.dll");

builder.Services.AddApiMetadataFromCompiledAssembly("humanizer-v1", opts =>
    opts.AssemblyFiles.Add(humanizerV1Dll));
builder.Services.AddApiMetadataFromCompiledAssembly("humanizer-v2", opts =>
    opts.FromPackageReference("Humanizer"));

// One AddApiReference per named provider. RoutePrefix nests each tree inside
// its version area; pages publish under /v1/api/ and /v2/api/.
builder.Services.AddApiReference("humanizer-v1", opts =>
{
    opts.RoutePrefix = "/v1/api/";
    opts.TocTitle = "API reference";
});
builder.Services.AddApiReference("humanizer-v2", opts =>
{
    opts.RoutePrefix = "/v2/api/";
    opts.TocTitle = "API reference";
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);

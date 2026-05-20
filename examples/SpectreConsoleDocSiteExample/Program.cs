using Pennington.ApiMetadata.Reflection;
using Pennington.DocSite;
using Pennington.DocSite.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Spectre.Console Docs",
    Description = "Demo of Pennington's multi-source API metadata — one docsite that documents Spectre.Console and Spectre.Console.Cli as two separate reference trees, both resolved straight from their NuGet packages.",
    GitHubUrl = "https://github.com/spectreconsole/spectre.console",
    HeaderContent = """<a href="/" class="font-bold text-lg">Spectre.Console Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Spectre.Console © Patrik Svensson & contributors. Rendered by Pennington.</footer>""",
    Areas =
    [
        new ContentArea("Console", "console"),
        new ContentArea("Cli", "cli"),
    ],
});

// Two reference trees, two named metadata providers. Each FromPackageReference
// call resolves one of the PackageReference assemblies in the .csproj — both
// live in the same NuGet cache folder, so Spectre.Console.Cli's reflection
// context finds Spectre.Console types without any extra wiring.
builder.Services.AddApiMetadataFromCompiledAssembly("spectre-console", opts =>
    opts.FromPackageReference("Spectre.Console"));
builder.Services.AddApiMetadataFromCompiledAssembly("spectre-console-cli", opts =>
    opts.FromPackageReference("Spectre.Console.Cli"));

// Two registrations, two URL prefixes. Names match the metadata registrations.
builder.Services.AddApiReference("spectre-console", opts =>
{
    opts.RoutePrefix = "/console/api/";
    opts.TocTitle = "API reference";
});
builder.Services.AddApiReference("spectre-console-cli", opts =>
{
    opts.RoutePrefix = "/cli/api/";
    opts.TocTitle = "API reference";
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
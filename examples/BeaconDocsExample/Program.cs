using MonorailCss.Theme;
using Pennington.DocSite;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beacon",
    Description = "HTTP Monitoring for .NET",
    ContentRootPath = "Content",
    CanonicalBaseUrl = "https://beacon-docs.example.com",
    GitHubUrl = "https://github.com/example/beacon",
    HeaderIcon = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="w-6 h-6"><circle cx="12" cy="12" r="2"/><path d="M16.24 7.76a6 6 0 0 1 0 8.49m-8.48-.01a6 6 0 0 1 0-8.49m11.31-2.82a10 10 0 0 1 0 14.14m-14.14 0a10 10 0 0 1 0-14.14"/></svg>""",
    HeaderContent = "<span class='text-xs font-semibold px-2 py-0.5 rounded-full bg-primary-100 text-primary-700'>v3.2</span>",
    FooterContent = "&copy; 2026 Beacon Contributors",
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 195,
        BaseColorName = ColorNames.Zinc,
    },
});

var app = builder.Build();
app.UseDocSite();

await app.RunDocSiteAsync(args);
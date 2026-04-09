using MonorailCss.Theme;
using Pennington.DocSite;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Tempo",
    Description = "Task scheduling for .NET",
    ContentRootPath = "Content",
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 150,
        BaseColorName = ColorNames.Zinc,
    },
    GitHubUrl = "https://github.com/example/tempo",
    HeaderIcon = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="w-6 h-6"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>""",
});

var app = builder.Build();
app.UseDocSite();

await app.RunDocSiteAsync(args);

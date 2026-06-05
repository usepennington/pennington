using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Documentation built with Pennington.",
    GitHubUrl = "https://github.com/your-org/your-repo",
    HeaderContent = """<a href="/">My Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    Areas =
    [
        new ContentArea("Guides", "guides"),
        new ContentArea("Reference", "reference"),
    ],
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

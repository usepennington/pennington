using Pennington.DocSite;
using Pennington.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Single-area DocSite so the tutorial's focus stays on *authoring* the page —
// front matter, alert, tabbed code group, and the outline populated from the
// rendered headings — not on area routing (that was tutorial 1.2.10).
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Author Docs",
    Description = "Authoring a doc page with DocSiteFrontMatter, alerts, and tabbed code groups.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Author Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    Areas =
    [
        new ContentArea("Guides", "guides"),
    ],
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

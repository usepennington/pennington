using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// Single-area DocSite so the tutorial's focus stays on adding pages and
// linking between them — relative paths, absolute paths, and uid/xref —
// without area routing pulling the spotlight.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Pages & Links",
    SiteDescription = "Two content pages and a hub index demonstrating relative, absolute, and uid:-based linking.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Pages &amp; Links</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    Areas =
    [
        new ContentArea("Guides", "guides"),
    ],
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
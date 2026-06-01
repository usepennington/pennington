using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// A DocSite with a single Guides area. The blog is not wired here — dropping
// markdown into Content/blog/ is what activates it. AddDocSite detects that
// folder at startup and lights up the blog index, post pages, browse-by-tag
// pages, the /rss.xml feed, and the "Blog" header link. CanonicalBaseUrl is set
// so the RSS feed emits absolute post URLs.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Harbor Docs",
    SiteDescription = "Documentation for Harbor, with a blog for release notes and field notes.",
    CanonicalBaseUrl = "https://harbor.example.com",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Harbor Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    Areas =
    [
        new ContentArea("Guides", "guides"),
    ],
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// A DocSite with no Content/ of its own. Instead of bundling markdown, it points
// ContentRootPath at the shared Bramble corpus in examples/_shared/Bramble — a
// fixed ~100-document fixture any example can mount. Relative paths resolve
// against the project directory at runtime, so "../_shared/Bramble/Content"
// reaches the sibling folder. See examples/_shared/Bramble/README.md for the
// mount recipe.
//
// The four Diataxis folders map to areas; the corpus's Content/blog/ folder
// auto-activates the DocSite blog (index, post pages, /blog/tags/, /rss.xml).
// This makes the example a realistic "docs site at scale" — useful for exercising
// navigation, heading-level search, the sitemap, and llms.txt against real volume.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Bramble",
    Description = "Documentation for Bramble, a small scripting language — backed by the shared fixture corpus.",
    ContentRootPath = new("../_shared/Bramble/Content"),
    CanonicalBaseUrl = "https://bramble.example.com",
    GitHubUrl = "https://github.com/bramble-lang/bramble",
    HeaderContent = """<a href="/">Bramble</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite. Bramble is fictional.</footer>""",

    Areas =
    [
        new ContentArea("Tutorials", "tutorials"),
        new ContentArea("How-to", "how-to"),
        new ContentArea("Reference", "reference"),
        new ContentArea("Explanation", "explanation"),
    ],
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

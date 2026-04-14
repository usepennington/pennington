using Pennington.DocSite;
using Pennington.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Same DocSite host shape as apps #4 and #5 — the focus here is on the
// *structure* of `Content/`. Two areas, each broken into two subfolder-backed
// sections. `NavigationBuilder` (inside `MainLayout`) turns the discovered
// flat TOC list into a grouped sidebar: each subfolder under an area becomes
// a non-navigable section header, and the pages inside sort by their front
// matter `order:` (tiebreaker: title).
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Sections Docs",
    Description = "Structure Content/ into areas and sections using subfolders, section, and order front matter.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Sections Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    // Two areas bound to two top-level content folders. The sidebar renders
    // an area selector above the per-area TOC; each area's TOC is grouped
    // by subfolder (sections "Getting Started" / "Advanced" under guides,
    // "Core Api" / "Extensions" under reference).
    Areas =
    [
        new ContentArea("Guides", "guides"),
        new ContentArea("Reference", "reference"),
    ],
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

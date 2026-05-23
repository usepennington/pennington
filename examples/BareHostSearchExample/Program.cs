using BareHostSearchExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

// A bare AddPennington host — no DocSite. AddPennington already emits the search
// index at /search/{locale}/index.json (term shards + per-page fragments); the
// only thing this example adds is the Pennington.UI search modal on top of it,
// wired up in MainLayout.razor. Content is the shared Bramble corpus mounted at
// the root, with the blog subtree excluded because its date/author front matter
// is not part of DocFrontMatter.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Bramble";
    penn.ContentRootPath = "../_shared/Bramble/Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "../_shared/Bramble/Content";
        md.BasePageUrl = "/";
        md.ExcludePaths = ["blog"];
    });
});

builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorName.Emerald,
        AccentColorName = ColorName.Amber,
        BaseColorName = ColorName.Slate,
    },
});

builder.Services.AddRazorComponents();

var app = builder.Build();

app.UsePennington();
app.UseMonorailCss();
app.UseAntiforgery();

// Serve the static web assets Pennington.UI and DeweySearch.Web ship under /_content
// (scripts.js, dewey-search.js). UsePennington only mounts the content folders.
app.MapStaticAssets();
app.MapRazorComponents<App>();

await app.RunOrBuildAsync(args);

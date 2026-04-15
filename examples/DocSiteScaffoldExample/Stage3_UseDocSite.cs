namespace DocSiteScaffoldExample;

using Pennington.DocSite;
using Pennington.Infrastructure;

/// <summary>
/// Stage 3 — the final wired state. `UseDocSite` mounts the DocSite
/// middleware stack (locale routing, static files, Razor component routing,
/// MonorailCSS, SPA navigation, Pennington core) and `RunDocSiteAsync`
/// delegates to <see cref="PenningtonExtensions.RunOrBuildAsync"/> so the
/// same host serves live in dev and generates static HTML when invoked as
/// <c>dotnet run -- build &lt;baseUrl&gt; &lt;outputDir&gt;</c> (both args optional;
/// defaults <c>/</c> and <c>output</c>). Tutorial prose extracts the
/// body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is
/// never instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>The fully-wired DocSite host — identical in shape to <c>Program.cs</c>.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Scaffold Docs",
            Description = "A minimal DocSite scaffold showing AddDocSite and area routing.",
            GitHubUrl = "https://github.com/usepennington/pennington",
            HeaderContent = """<a href="/">Scaffold Docs</a>""",
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
    }
}
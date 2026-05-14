namespace BeyondRoslynExample;

using Pennington.DocSite;
using Pennington.Roslyn; // [!code ++]

/// <summary>
/// Stage 2 — adds <c>AddPenningtonRoslyn</c> pointed at the inner
/// <c>BeyondRoslynExample.slnx</c>. With this one extra service registration,
/// the markdown preprocessor for <c>:xmldocid</c> / <c>:xmldocid,bodyonly</c>
/// / <c>:xmldocid-diff</c> / <c>:path</c> fence modifiers lights up and every
/// doc page that references a Sample-library symbol starts rendering real
/// source. Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>DocSite host with Roslyn integration wired.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Beyond Roslyn",
            Description = "Pulling live code snippets into docs with xmldocid fences.",
            GitHubUrl = "https://github.com/usepennington/pennington",
            HeaderContent = """<a href="/">Beyond Roslyn</a>""",
            FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
        });

        builder.Services.AddPenningtonRoslyn(roslyn => // [!code ++]
        { // [!code ++]
            roslyn.SolutionPath = "BeyondRoslynExample.slnx"; // [!code ++]
        }); // [!code ++]

        var app = builder.Build();

        app.UseDocSite();

        await app.RunDocSiteAsync(args);
    }
}
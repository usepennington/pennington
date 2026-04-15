namespace BeyondRoslynExample;

using Pennington.DocSite;

/// <summary>
/// Stage 1 — the pre-Roslyn host. DocSite is wired, pages render, but any
/// <c>csharp:xmldocid</c> fence in markdown just renders as a literal code
/// block because no <see cref="Pennington.Roslyn.RoslynOptions"/> /
/// <see cref="Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor"/>
/// is registered in DI. Tutorial prose extracts the body of <see cref="Run"/>
/// via <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>A DocSite host with no Roslyn integration yet.</summary>
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

        var app = builder.Build();

        app.UseDocSite();

        await app.RunDocSiteAsync(args);
    }
}
namespace GettingStartedMinimalSiteExample;

using Pennington.FrontMatter;
using Pennington.Infrastructure;

/// <summary>
/// Stage 3 — add the Pennington middleware and hand control to
/// <c>RunOrBuildAsync</c> so the same host serves live in dev mode and
/// generates static HTML when invoked as <c>dotnet run -- build &lt;dir&gt;</c>.
/// Page rendering arrives in <c>Program.cs</c>. Tutorial prose extracts the
/// body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>Wire the Pennington middleware and run in dev or build mode.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPennington(penn =>
        {
            penn.SiteTitle = "My First Pennington Site";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "/";
            });
        });

        var app = builder.Build();

        app.UsePennington();

        await app.RunOrBuildAsync(args);
    }
}

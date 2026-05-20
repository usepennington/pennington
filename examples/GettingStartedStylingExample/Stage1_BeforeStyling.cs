namespace GettingStartedStylingExample;

using GettingStartedStylingExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;

/// <summary>
/// Stage 1 — the unstyled starting point. The Blazor catch-all from the
/// previous tutorial renders markdown into <c>MainLayout</c>, but no MonorailCSS
/// services have been registered yet. The browser fetches <c>/styles.css</c>
/// and gets a 404, so the utility classes in <c>MainLayout.razor</c> are inert
/// and the page renders with browser defaults. Tutorial prose extracts the
/// body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>Run the Blazor host with no MonorailCSS in the picture.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPennington(penn =>
        {
            penn.SiteTitle = "My Styled Pennington Site";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "/";
            });
        });

        builder.Services.AddRazorComponents();

        var app = builder.Build();

        app.UsePennington();
        app.UseAntiforgery();
        app.MapRazorComponents<App>();

        await app.RunOrBuildAsync(args);
    }
}
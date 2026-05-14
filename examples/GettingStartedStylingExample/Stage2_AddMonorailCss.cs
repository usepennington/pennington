namespace GettingStartedStylingExample;

using GettingStartedStylingExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

/// <summary>
/// Stage 2 — register MonorailCSS in DI with a <see cref="NamedColorScheme"/>.
/// The <c>primary</c>, <c>accent</c>, and <c>base</c> utility prefixes now
/// resolve to indigo/pink/slate, but <c>UseMonorailCss</c> is not yet wired so
/// <c>/styles.css</c> still 404s and the page is still unstyled. Tutorial
/// prose extracts the body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>.
/// This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Register MonorailCSS services. Endpoint not yet mounted.</summary>
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

        // New in this stage: register MonorailCSS. Pick which named palettes
        // back the `primary`, `accent`, and `base` utility prefixes. Any
        // ColorName constant works — swap freely.
        builder.Services.AddMonorailCss(_ => new MonorailCssOptions
        {
            ColorScheme = new NamedColorScheme
            {
                PrimaryColorName = ColorName.Indigo,
                AccentColorName = ColorName.Pink,
                BaseColorName = ColorName.Slate,
            },
        });

        builder.Services.AddRazorComponents();

        var app = builder.Build();

        app.UsePennington();
        app.UseAntiforgery();
        app.MapRazorComponents<App>();

        await app.RunOrBuildAsync(args);
    }
}

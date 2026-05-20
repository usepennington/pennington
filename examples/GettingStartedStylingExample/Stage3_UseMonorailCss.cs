namespace GettingStartedStylingExample;

using GettingStartedStylingExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

/// <summary>
/// Stage 3 — call <c>app.UseMonorailCss()</c> to mount the <c>/styles.css</c>
/// endpoint. The class collector observes every utility class in the rendered
/// HTML and emits matching CSS rules; the <c>&lt;link rel="stylesheet"&gt;</c>
/// in <c>MainLayout.razor</c> now returns a populated stylesheet and the page
/// renders styled. Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>Mount the stylesheet endpoint and watch the page light up.</summary>
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

        // New in this stage: mount /styles.css. The default path matches the // [!code ++]
        // <link> tag in MainLayout.razor. // [!code ++]
        app.UseMonorailCss(); // [!code ++]

        app.UseAntiforgery();
        app.MapRazorComponents<App>();

        await app.RunOrBuildAsync(args);
    }
}
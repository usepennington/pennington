namespace BeyondLocaleExample;

using Pennington.DocSite;
using Pennington.Localization; // [!code ++]

/// <summary>
/// Stage 2 — add a <c>ConfigureLocalization</c> action to
/// <see cref="DocSiteOptions"/> and register two locales. This is the
/// only code change required; DocSite's <c>UseDocSite</c> already calls
/// <see cref="Pennington.Infrastructure.PenningtonExtensions.UseLocaleRouting"/>,
/// which is a no-op until <see cref="Pennington.Infrastructure.LocalizationOptions.IsMultiLocale"/>
/// flips to <c>true</c>. Spanish content lives under <c>Content/es/</c>;
/// each file mirrors a default-locale file by name. Tutorial prose
/// extracts the body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>.
/// This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Register a second locale; content lands under <c>Content/es/</c>.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Beyond Locale",
            SiteDescription = "Adding a second locale to a Pennington DocSite.",
            GitHubUrl = "https://github.com/usepennington/pennington",
            HeaderContent = """<a href="/">Beyond Locale</a>""",
            FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

            ConfigureLocalization = loc => // [!code ++]
            { // [!code ++]
                loc.DefaultLocale = "en"; // [!code ++]
                loc.AddLocale("en", new LocaleInfo("English")); // [!code ++]
                loc.AddLocale("es", new LocaleInfo("Español", HtmlLang: "es")); // [!code ++]
            }, // [!code ++]
        });

        var app = builder.Build();

        app.UseDocSite();

        await app.RunDocSiteAsync(args);
    }
}
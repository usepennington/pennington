namespace BeyondLocaleExample;

using Pennington.DocSite;
using Pennington.Localization;

/// <summary>
/// Stage 3 — the fully-wired host, identical in shape to <c>Program.cs</c>.
/// The built-in <c>LanguageSwitcher</c> in DocSite's <c>MainLayout.razor</c>
/// now appears in the site header because
/// <see cref="Pennington.Infrastructure.LocalizationOptions.IsMultiLocale"/>
/// is <c>true</c>. Clicking a locale rewrites the URL — the default locale
/// stays at the root (<c>/about</c>) and every other locale gets a code
/// prefix (<c>/es/about</c>). Tutorial prose extracts the body of
/// <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>The final state — same as <c>Program.cs</c>, switcher visible.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Beyond Locale",
            Description = "Adding a second locale to a Pennington DocSite.",
            GitHubUrl = "https://github.com/usepennington/pennington",
            HeaderContent = """<a href="/">Beyond Locale</a>""",
            FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

            ConfigureLocalization = loc =>
            {
                loc.DefaultLocale = "en";
                loc.AddLocale("en", new LocaleInfo("English"));
                loc.AddLocale("es", new LocaleInfo("Español", HtmlLang: "es"));
            },
        });

        var app = builder.Build();

        app.UseDocSite();

        await app.RunDocSiteAsync(args);
    }
}
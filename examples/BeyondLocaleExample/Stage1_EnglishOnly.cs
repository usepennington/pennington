namespace BeyondLocaleExample;

using Pennington.DocSite;

/// <summary>
/// Stage 1 — the starting point. A single-locale DocSite host with three
/// English pages under <c>Content/</c>. No <c>ConfigureLocalization</c>,
/// no <c>LanguageSwitcher</c> — the switcher is hidden because
/// <see cref="Pennington.Infrastructure.LocalizationOptions.IsMultiLocale"/>
/// is false. Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>The pre-localization DocSite host — English only.</summary>
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
        });

        var app = builder.Build();

        app.UseDocSite();

        await app.RunDocSiteAsync(args);
    }
}
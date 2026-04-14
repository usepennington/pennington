using BeyondLocaleExample;
using Pennington.DocSite;
using Pennington.Localization;

var builder = WebApplication.CreateBuilder(args);

// The DocSite host from tutorial 1.2 already knows how to render localized
// content — all we need to do is hand it a `ConfigureLocalization` action
// that populates `LocalizationOptions` with the default locale and every
// additional locale we want URL-prefixed. `UseDocSite` calls
// `UsePenningtonLocaleRouting` first thing, which swaps `UseRequestLocalization`
// + `LocaleDetectionMiddleware` into the pipeline ahead of endpoint matching.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beyond Locale",
    Description = "Adding a second locale to a Pennington DocSite.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Beyond Locale</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    // Two locales. `DefaultLocale = "en"` means English lives at the URL root
    // (`/`, `/about`, `/getting-started`). Every other locale is URL-prefixed
    // with its code (`/es/`, `/es/about`, `/es/getting-started`) and its
    // translated markdown lives in a matching `Content/<locale>/` subfolder.
    // The built-in `LanguageSwitcher` in `MainLayout.razor` lights up as soon
    // as `Locales.Count > 1` — no layout changes required.
    ConfigureLocalization = loc =>
    {
        loc.DefaultLocale = "en";
        loc.AddLocale("en", new LocaleInfo("English"));
        loc.AddLocale("es", new LocaleInfo("Español", HtmlLang: "es"));
    },

    // Translations sit on PenningtonOptions, not DocSiteOptions, so they
    // are registered through the ConfigurePennington escape hatch. The
    // helper owns both TranslationOptions.Add overloads.
    ConfigurePennington = penn => TranslationRegistration.Register(penn.Translations),
});

var app = builder.Build();

// `UseDocSite` wires `UsePenningtonLocaleRouting` before `MapRazorComponents`
// so the Blazor `@page "/{*fileName:nonfile}"` route in `Pages.razor` sees a
// locale-stripped path. The raw request path is still available via
// `NavigationManager.Uri`, and `ContentResolver.GetContentByUrlAsync` uses
// that full path to resolve the right translation (or fall back to English).
app.UseDocSite();

await app.RunDocSiteAsync(args);

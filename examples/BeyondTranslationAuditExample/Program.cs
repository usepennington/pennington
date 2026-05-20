using Pennington.DocSite;
using Pennington.Localization;
using Pennington.TranslationAudit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beyond Translation Audit",
    Description = "Translation status auditor wired into the build report and dev overlay.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Beyond Translation Audit</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    // Two locales. Spanish translations under Content/es/ deliberately omit
    // `getting-started.md` so the dev overlay surfaces a "missing es translation"
    // warning when you visit the page, and `dotnet run -- build` reports the same
    // entry in its diagnostic list.
    ConfigureLocalization = loc =>
    {
        loc.DefaultLocale = "en";
        loc.AddLocale("en", new LocaleInfo("English"));
        loc.AddLocale("es", new LocaleInfo("Español", HtmlLang: "es"));
    },
});

// Repository auto-discovers from cwd. The auditor implements IBuildAuditor and
// flows through the same audit cache that the dev overlay reads — no other wiring
// required.
builder.Services.AddPenningtonTranslationAudit();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
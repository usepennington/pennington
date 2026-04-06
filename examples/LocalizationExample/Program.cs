using Penn.DocSite;
using Penn.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "The Multilingual Tavern",
    Description = "A documentation site that speaks many tongues",
    ConfigureLocalization = loc =>
    {
        loc.DefaultLocale = "en";
        loc.AddLocale("en", new LocaleInfo("English"));
        loc.AddLocale("pl", new LocaleInfo("Pig Latin"));
        loc.AddLocale("sv", new LocaleInfo("Bork Bork", HtmlLang: "sv-chef"));
        loc.AddLocale("pi", new LocaleInfo("Pirate", HtmlLang: "en-pirate"));
        loc.AddLocale("kl", new LocaleInfo("Klingon", HtmlLang: "tlh"));
    },
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);

using Pennington.DocSite;
using Pennington.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Localization Tutorial",
    Description = "A documentation site with English and Spanish",
    ContentRootPath = "Content",
    ConfigureLocalization = loc =>
    {
        loc.DefaultLocale = "en";
        loc.AddLocale("en", new LocaleInfo("English"));
        loc.AddLocale("es", new LocaleInfo("Español"));
    },
});

var app = builder.Build();
app.UseDocSite();

await app.RunDocSiteAsync(args);

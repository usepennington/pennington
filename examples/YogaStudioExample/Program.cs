using Penn.Infrastructure;
using Penn.Islands;
using Penn.Localization;
using Penn.MonorailCss;
using YogaStudioExample.Components;
using YogaStudioExample.Islands;
using YogaStudioExample.Models;
using YogaStudioExample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "937 Yoga";
    penn.SiteDescription = "Find your flow at 937 Yoga Studio";
    penn.ContentRootPath = "Content";
    penn.AdditionalRoutingAssemblies = [typeof(Program).Assembly];

    penn.AddMarkdownContent<YogaBlogFrontMatter>(md =>
    {
        md.ContentPath = "Content/blog";
        md.BasePageUrl = "/blog";
        md.Section = "Blog";
    });

    penn.AddMarkdownContent<YogaFrontMatter>(md =>
    {
        md.ContentPath = "Content/pages";
        md.BasePageUrl = "";
        md.Section = "Pages";
    });

    penn.Localization.DefaultLocale = "en";
    penn.Localization.AddLocale("en", new LocaleInfo("English"));
    penn.Localization.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));
});

builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new YogaColorScheme(),
    ExtraStyles = """
        @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400..900;1,400..900&family=JetBrains+Mono:wght@400;500&display=swap');

        :root {
            --font-display: 'Playfair Display', serif;
        }

        .font-display {
            font-family: var(--font-display);
        }
        """,
    ContentPaths = ["js/search.js"],
    CustomCssFrameworkSettings = settings => settings with
    {
        Applies = settings.Applies.AddRange(YogaComponentApplies.All()),
    },
});

// SPA navigation
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddTransient<IIslandRenderer, YogaContentIslandRenderer>();

// Custom services
builder.Services.AddTransient<ContentHelper>();
builder.Services.AddSingleton<ScheduleService>();
builder.Services.AddSingleton<InstructorService>();

var app = builder.Build();
app.UsePennLocaleRouting(); // Must be before MapRazorComponents for locale URL rewriting
app.UseAntiforgery();       // Must be after UsePennLocaleRouting (which calls UseRouting internally)
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UseSpaNavigation();
app.UsePenn();
await app.RunOrBuildAsync(args);

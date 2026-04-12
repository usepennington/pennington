using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.Localization;
using Pennington.MonorailCss;
using YogaStudioExample.Components;
using YogaStudioExample.Islands;
using YogaStudioExample.Models;
using YogaStudioExample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
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

    // Note: static pages under Content/pages/ are NOT registered as a markdown
    // content source. The Razor wrapper components (About.razor, Contact.razor,
    // etc.) each have `@page "/about"` directives, which would collide with
    // routes emitted by MarkdownContentService and race on `about/index.html`
    // during parallel static generation. ContentHelper.GetStaticPageAsync reads
    // the markdown files directly off disk.

    penn.Localization.DefaultLocale = "en";
    penn.Localization.AddLocale("en", new LocaleInfo("English"));
    penn.Localization.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));

    penn.Islands.Register<YogaContentIslandRenderer>("content");
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
    CustomCssFrameworkSettings = settings => settings with
    {
        Applies = settings.Applies.AddRange(YogaComponentApplies.All()),
    },
});

// SPA navigation
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();

// Custom services
builder.Services.AddTransient<ContentHelper>();
builder.Services.AddSingleton<ScheduleService>();
builder.Services.AddSingleton<InstructorService>();

// Register the programmatic route enumerator AFTER InstructorService/ScheduleService
// so it can be resolved with their dependencies. It emits parameterized instructor
// and class detail routes plus locale-prefixed copies of every Razor @page.
builder.Services.AddSingleton<Pennington.Content.IContentService, YogaRouteContentService>();

var app = builder.Build();
app.UsePenningtonLocaleRouting(); // Must be before MapRazorComponents for locale URL rewriting
app.UseAntiforgery();       // Must be after UsePenningtonLocaleRouting (which calls UseRouting internally)
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UseSpaNavigation();
app.UsePennington();
await app.RunOrBuildAsync(args);

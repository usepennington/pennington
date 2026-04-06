using MonorailCss.Theme;
using Penn.Content;
using Penn.DocSite;
using Penn.MonorailCss;
using SearchExample.Services;
using Random = SearchExample.Services.Random;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    // Basic site information
    SiteTitle = "Random Content Site",
    Description = "Random content site for demonstration purposes.",
    CanonicalBaseUrl = "https://mydocs.example.com",

    // Styling and branding
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 235, // Blue theme (0-360)
        BaseColorName = ColorNames.Slate // Base color palette
    },
    GitHubUrl = "https://github.com/Penn/Penn",

    // Custom header with logo and branding
    HeaderContent = """
            <span class="text-xl font-bold">Random Docs</span>
        """,
    HeaderIcon = """
                 <svg class="h-5 w-5 text-white" fill="currentColor" viewBox="0 0 20 20">
                     <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                 </svg>
                 """,
    // Custom footer
    FooterContent = """
        <div class="text-center text-sm text-base-600 dark:text-base-400">
            &copy; 2024 Random Content Site. Built with Penn.
        </div>
        """,

    // Additional HTML for head section
    AdditionalHtmlHeadContent = """
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
        <link href="https://fonts.googleapis.com/css2?family=Manrope:wght@200..800&family=Petrona:ital,wght@0,100..900;1,100..900&display=swap" rel="stylesheet">
        """,

    BodyFontFamily = "Manrope, sans-serif",
    DisplayFontFamily = "Petrona, serif",

    // Custom styles
    ExtraStyles = """
        .custom-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        """,

    AdditionalRoutingAssemblies = [typeof(Random).Assembly]
});

builder.Services.AddSingleton<RandomContentService>();

// Register as IContentService (this allows multiple IContentService implementations)
builder.Services.AddSingleton<IContentService>(provider => provider.GetRequiredService<RandomContentService>());

var app = builder.Build();
app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
    string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
app.UseDocSite();
await app.RunDocSiteAsync(args);

using MonorailCss.Theme;
using Penn.BlogSite;
using Penn.BlogSite.Components;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(_ => new BlogSiteOptions
{
    SiteTitle = "Calvin's Chewing Chronicles",
    Description = "A sophisticated publication for the serious gum enthusiast",
    CanonicalBaseUrl = Environment.GetEnvironmentVariable("CanonicalBaseHref") ?? "https://calvins-chewing-chronicles.example.com",
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 300,
        BaseColorName = ColorNames.Zinc
    },
    AdditionalRoutingAssemblies = [typeof(Program).Assembly],
    ContentRootPath = "Content",
    BlogContentPath = "Blog",
    BlogBaseUrl = "/blog",
    TagsPageUrl = "/tags",
    DisplayFontFamily = "\"Noto Sans Display\", sans-serif",
    BodyFontFamily = "\"Inter\", sans-serif",
    EnableRss = true,
    EnableSitemap = true,
    AdditionalHtmlHeadContent = """
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
        <link href="https://fonts.googleapis.com/css2?family=Inter:ital,opsz,wght@0,14..32,100..900;1,14..32,100..900&family=Noto+Sans+Display:ital,wght@0,100..900;1,100..900&display=swap" rel="stylesheet">
        """,
    HeroContent = new HeroContent(
        "Flavor profiler, endurance chewer, and part-time pterygoid philosopher",
        "I'm <strong>Calvin</strong>, a gum performance analyst and recreational mandibularist based in New York City. I'm the founder of ChewLab, where we develop equipment, training protocols, and apparel that help everyday people reach elite levels of chewing efficiency."),
    MyWork =
    [
        new Project("gum-performance-benchmark", "Test chewing metrics with controlled mastication protocols.", "https://github.com/fake-calvin/gum-performance-benchmark"),
        new Project("mandible-trainer-pro", "Interval training for serious jaw athletes.", "https://github.com/fake-calvin/mandible-trainer-pro"),
        new Project("chew-jersey-detergent-lab", "Simulate washes for technical gum apparel.", "https://github.com/fake-calvin/chew-jersey-detergent-lab"),
        new Project("bubble-capacity-visualizer", "Visualize bubble data with saliva overlays.", "https://github.com/fake-calvin/bubble-capacity-visualizer"),
        new Project("gumbot-alpha", "Robotic jaw for automated gum stress testing.", "https://github.com/fake-calvin/gumbot-alpha"),
    ],
    MainSiteLinks =
    [
        new HeaderLink("About", "/about"),
        new HeaderLink("Sponsor Me", "https://github.com/fake-sponsor-link"),
    ],
    Socials =
    [
        new SocialLink(SocialIcons.GithubIcon, "#"),
        new SocialLink(SocialIcons.LinkedInIcon, "#"),
        new SocialLink(SocialIcons.MastodonIcon, "#"),
        new SocialLink(SocialIcons.BlueskyIcon, "#"),
    ],
    AuthorName = "Calvin",
    AuthorBio = "I'm <strong>Calvin</strong>, a gum performance analyst and recreational mandibularist based in New York City.",
    SocialMediaImageUrlFactory = page => $"social-images/{page.Url.Trim('/').Replace("/", "-")}.png",
});

var app = builder.Build();
app.UseBlogSite();
app.MapStaticAssets();

await app.RunBlogSiteAsync(args);

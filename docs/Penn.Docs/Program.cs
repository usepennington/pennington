using MonorailCss.Theme;
using Penn.DocSite;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "Penn",
    Description = "A Content Engine for .NET",
    SocialImageUrl = "/social.png",
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 260,
        ColorSchemeGenerator = i => (i + 180, i - 90, i + 90),
        BaseColorName = ColorNames.Stone
    },
    GitHubUrl = "https://github.com/phil-scott-78/penn",
    CanonicalBaseUrl = "https://phil-scott-78.github.io/penn/",
    DisplayFontFamily = "Lexend, sans-serif",
    BodyFontFamily = "'Noto Sans', sans-serif",
    HeaderIcon = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="w-6 h-6"><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/></svg>""",
    ExtraStyles = """
        @font-face {
            font-family: 'Lexend';
            font-style: normal;
            font-weight: 100 900;
            font-display: swap;
            src: url(fonts/lexend.woff2) format('woff2');
            unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
        }
        @font-face {
            font-family: 'Noto Sans';
            font-style: normal;
            font-weight: 100 900;
            font-display: swap;
            src: url(fonts/noto-sans.woff2) format('woff2');
            unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
        }
    """,
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);

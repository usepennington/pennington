using Mdazor;
using Pennington.Docs;
using Pennington.Docs.Components.Reference;
using Pennington.DocSite;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Roslyn;
using Pennington.Tui;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Pennington",
    Description = "A Content Engine for .NET",
    SocialImageUrl = "/social.png",
    ColorScheme = new SnugglepussColorScheme(),
    SyntaxTheme = new SyntaxTheme
    {
        Keyword  = "accent-four",
        String   = "accent-two",
        Variable = "accent-one",
        Function = "accent-three",
        Comment  = "pewter",
    },
    GitHubUrl = "https://github.com/usepennington/pennington",
    CanonicalBaseUrl = "https://phil-scott-78.github.io/pennington/",
    DisplayFontFamily = "Lexend, sans-serif",
    BodyFontFamily = "'Noto Sans', sans-serif",
    FontPreloads =
    [
        new FontPreload("/fonts/lexend.woff2"),
        new FontPreload("/fonts/noto-sans.woff2"),
    ],
    HeaderIcon = "",
    HeaderContent = """<a href="/" class="text-primary-700 dark:text-primary-400 font-bold text-lg">Pennington</a>""",
    ExtraStyles = """
        @font-face {
            font-family: 'Lexend';
            font-style: normal;
            font-weight: 100 900;
            font-display: swap;
            src: url(/fonts/lexend.woff2) format('woff2');
            unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
        }
        @font-face {
            font-family: 'Noto Sans';
            font-style: normal;
            font-weight: 100 900;
            font-display: swap;
            src: url(/fonts/noto-sans.woff2) format('woff2');
            unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
        }
    """,
    SolutionPath = "../../Pennington.slnx",
    Areas =
    [
        new ContentArea("Getting Started", "tutorials"),
        new ContentArea("Guides", "how-to"),
        new ContentArea("Under the Hood", "explanation"),
        new ContentArea("Reference", "reference"),
    ],
});

// Roslyn integration for :xmldocid and :path code extraction
builder.Services.AddPenningtonRoslyn(roslyn =>
{
    roslyn.SolutionPath = "../../Pennington.slnx";
});

// Dev-time full-screen dashboard. No-ops when the host is launched with
// `dotnet run -- build`, so the static build path is unchanged.
builder.Services.AddPenningtonTui();

// Reference-doc Mdazor components: generate property tables, member lists, and
// xmldoc content directly from the source via Roslyn.
builder.Services
    .AddMdazorComponent<ApiMemberTable>()
    .AddMdazorComponent<ApiMemberList>()
    .AddMdazorComponent<ApiParameterTable>()
    .AddMdazorComponent<ApiSummary>()
    .AddMdazorComponent<ApiReturns>()
    .AddMdazorComponent<ApiRemarks>()
    .AddMdazorComponent<ApiSeeAlso>();

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
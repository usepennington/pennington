using Mdazor;
using Pennington.ApiMetadata.Reflection;
using Pennington.Docs;
using Pennington.Docs.ApiReference;
using Pennington.Docs.Components.Reference;
using Pennington.DocSite;
using Pennington.DocSite.Api;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.TreeSitter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Pennington",
    Description = "A Content Engine for .NET",
    SocialImageUrl = "/social.png",
    ColorScheme = new GrapeColorScheme(),
    SyntaxTheme = new SyntaxTheme
    {
        Keyword = "accent-four",
        String = "accent-two",
        Variable = "accent-one",
        Function = "accent-three",
        Comment = "base",
    },
    GitHubUrl = "https://github.com/usepennington/pennington",
    CanonicalBaseUrl = "https://usepennington.github.io/pennington/",
    DisplayFontFamily = "Lexend, sans-serif",
    BodyFontFamily = "'Noto Sans', sans-serif",
    MonoFontFamily = "'JetBrains Mono', ui-monospace, SFMono-Regular, monospace",
    FontPreloads =
    [
        new FontPreload("/fonts/lexend.woff2"),
        new FontPreload("/fonts/noto-sans.woff2"),
        new FontPreload("/fonts/jetbrains-mono.woff2"),
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
        @font-face {
            font-family: 'JetBrains Mono';
            font-style: normal;
            font-weight: 100 800;
            font-display: swap;
            src: url(/fonts/jetbrains-mono.woff2) format('woff2');
            unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
        }
        html { scroll-padding-top: 5rem; }
        body { font-feature-settings: "ss01", "cv11"; }
        .font-display { letter-spacing: -0.012em; }
    """,
    Areas =
    [
        new ContentArea("Getting Started", "tutorials"),
        new ContentArea("Guides", "how-to"),
        new ContentArea("Under the Hood", "explanation"),
        new ContentArea("Reference", "reference"),
    ],
    // Brand styling lives in BrandStyling.cs — pseudo-element @apply blocks
    // (H2 gradient bar, bullet dot) and the prose flair (animated underline,
    // primary blockquote, pre inset shadow, inline-code chip) re-applied on top
    // of MonorailCSS's neutral defaults.
    CustomCssFrameworkSettings = settings => settings with
    {
        Applies = settings.Applies.AddRange(BrandStyling.Applies),
    },
    ExtendProseCustomization = BrandStyling.Extend,
});

// Code-fragment fences (:symbol) run through tree-sitter, reading source files directly —
// no MSBuild workspace, no compilation. ContentRoot is the repo root so fence bodies resolve
// against src/ and examples/ paths.
builder.Services.AddTreeSitter(treeSitter =>
{
    treeSitter.ContentRoot = "../..";
});

// Reflection-backed API metadata: read the built Pennington.* assemblies and their companion
// xmldoc files straight from this app's output folder — no MSBuild workspace, no compilation.
// Every referenced Pennington library lands in bin/, so glob them (excluding this entry app)
// to document the full set of referenced Pennington libraries.
var entryAssembly = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
builder.Services.AddApiMetadataFromCompiledAssembly(opts =>
{
    foreach (var dll in Directory.EnumerateFiles(AppContext.BaseDirectory, "Pennington*.dll"))
    {
        if (!string.Equals(Path.GetFileNameWithoutExtension(dll), entryAssembly, StringComparison.Ordinal))
        {
            opts.AssemblyFiles.Add(dll);
        }
    }
});

// Auto-publishes /reference/api/{slug}/ pages and registers the reference
// Mdazor components (<ApiMemberTable>, <ApiSummary>, <ExtensionMethods>, ...).
builder.Services.AddApiReference();

// Pennington-specific front-matter key catalog. Coupled to
// Pennington.FrontMatter.IFrontMatter and the capability interfaces, so it
// stays in this project rather than shipping from Pennington.DocSite.Api.
builder.Services.AddSingleton<FrontMatterKeyIndex>();
builder.Services.AddMdazorComponent<FrontMatterKeys>();

// Word-break typography, folded into the shared HTML rewriting pipeline so it
// mutates the already-parsed DOM instead of re-parsing the response string.
// Flat type selectors (no descendant combinators): the rewriter splits the
// text of headings, spans, and pure-text block elements. Descendant terms like
// "h1 *" are deliberately avoided — they made AngleSharp's selector engine walk
// every element on the page (the dominant build hotspot). Text nested in a
// <span> is reached because <span> is matched directly.
builder.Services.AddWordBreak(options =>
{
    options.CssSelector = "h1, h2, h3, h4, h5, h6, p, li, dt, dd, th, td, span, .text-break";
});

// Dev-time full-screen dashboard. No-ops when the host is launched with
// `dotnet run -- build`, so the static build path is unchanged.
// builder.Services.AddTui();

var app = builder.Build();
app.UseDocSite();

await app.RunDocSiteAsync(args);
# Penn — Architecture Spec

*Formerly MyLittleContentEngine. Named for Penn Watson, printer.*

Target: **C# 15 / .NET 11** — leverages union types, improved pattern matching, and modern BCL.

Feature-complete rewrite of v1. Same content files, same Razor/Blazor rendering, same dual-mode execution — but with a unified URL model, structured build reporting, decoupled Roslyn, and first-class SPA/Islands.

---

## 1. ContentRoute — Single Source of Truth for URLs

v1's #1 architectural problem: 16+ methods across 12 files all computing URLs differently. v2 replaces all of them with one type.

### The Type

```csharp
public sealed record ContentRoute
{
    // The four representations, computed once at construction
    public required UrlPath CanonicalPath { get; init; }   // e.g. "/docs/getting-started"
    public required FilePath OutputFile { get; init; }      // e.g. "docs/getting-started/index.html"
    public FilePath? SourceFile { get; init; }              // e.g. "Content/Docs/getting-started.md" (null for programmatic)
    public string Locale { get; init; } = "";               // e.g. "fr" — empty string for default locale

    // Derived — no recomputation anywhere else in the system
    public UrlPath NavigationPath => CanonicalPath.EnsureTrailingSlash();
    public UrlPath WithBaseUrl(UrlPath baseUrl) => baseUrl / CanonicalPath;
    public UrlPath AbsoluteUrl(UrlPath canonicalBase) => canonicalBase / CanonicalPath;
    public bool IsDefaultLocale => string.IsNullOrEmpty(Locale);
}
```

### Factory

One static factory per content source type. All URL logic lives here:

```csharp
public static class ContentRouteFactory
{
    // Markdown: file path → route
    public static ContentRoute FromMarkdownFile(FilePath sourceFile, FilePath contentRoot, UrlPath basePageUrl, string locale = "");

    // Razor: @page directive → route
    public static ContentRoute FromRazorPage(string pageRoute, string locale = "");

    // Programmatic: explicit URL → route
    public static ContentRoute FromUrl(UrlPath url, string locale = "");

    // Custom: for non-markdown content services (recipes, images, etc.)
    public static ContentRoute FromCustom(UrlPath url, FilePath? sourceFile = null, string locale = "");

    // Redirect: source URL → route (output is redirect HTML)
    public static ContentRoute ForRedirect(UrlPath sourceUrl);
}
```

### What This Replaces (v1 → v2)

| v1 Method | v1 File | v2 Equivalent |
|-----------|---------|---------------|
| CreateContentUrl | ContentFilesService | `ContentRouteFactory.FromMarkdownFile` |
| CreateNavigationUrl | ContentFilesService | `route.NavigationPath` |
| GetOutputFilePath | ContentFilesService | `route.OutputFile` |
| GetPageUrl | ContentFilesService | `route.WithBaseUrl(baseUrl)` |
| FilePathToUrlPath | FileSystemUtilities | `ContentRouteFactory.FromMarkdownFile` |
| CombineUrl | FileSystemUtilities | `UrlPath./` operator (kept) |
| RewriteUrl | LinkRewriter | `route.WithBaseUrl(baseUrl)` |
| PrependBaseUrl | LinkRewriter | `route.WithBaseUrl(baseUrl)` |
| NormalizeForNavigation | NavigationUrlComparer | `route.NavigationPath` |
| NormalizeUrl | MarkdownContentProcessor | `route.CanonicalPath` |
| RewriteUrl | BaseUrlRewritingProcessor | Middleware uses `route.WithBaseUrl` |
| PrependBaseUrl | BaseUrlRewritingProcessor | Eliminated (single method) |
| EnsureTrailingSlash | BaseUrlRewritingProcessor | `route.NavigationPath` |
| NormalizeUrlForValidation | OutputGenerationService | `route.CanonicalPath` |
| BuildValidOutputPathsLookup | OutputGenerationService | Set of `route.OutputFile` |

### UrlPath (kept, refined)

The `UrlPath` value type stays. It's good. Minor refinements:
- Keep the `/` operator for combining
- Keep `EnsureLeadingSlash`, `RemoveTrailingSlash`, etc.
- Add `Matches(UrlPath other)` — tolerant comparison (trailing slash, index.html variants)
- Remove any normalization logic that duplicates what ContentRoute handles

---

## 2. Content Pipeline — Union Type Stages

Content enters as files, flows through typed stages, exits as output. Each stage is a union — the pipeline cannot produce an untyped intermediate.

### Pipeline Stages

```csharp
// --- Content pipeline stage types (C# 15 union types) ---
// Reference: https://devblogs.microsoft.com/dotnet/csharp-15-union-types/
//
// C# 15 unions wrap existing types — define case types as records first,
// then declare the union over them. The compiler generates a struct with
// implicit conversions and exhaustive pattern matching.

// Case types — each is a standalone record
public record DiscoveredItem(ContentRoute Route, ContentSource Source);
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);
public record FailedItem(ContentRoute Route, ContentError Error);

// The union — compiler enforces exhaustive matching over exactly these four types.
// Adding a 5th case (e.g. ValidatedItem) produces warnings at every incomplete switch.
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)
{
    // Every case carries a Route — expose it on the union to avoid pattern matching at every call site
    public ContentRoute Route => Value switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p     => p.Route,
        RenderedItem r   => r.Route,
        FailedItem f     => f.Route,
        null => throw new InvalidOperationException("Uninitialized ContentItem")
    };
}

// Content source case types
public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource);

/// Replaces raw Func<Task<byte[]>> — provides metadata + rendering context
public interface IProgrammaticContentGenerator
{
    /// Produce front matter and raw content for the Parse stage.
    /// For binary content (images, etc.) return BinaryProgrammaticContent.
    Task<ProgrammaticContent> GenerateAsync(ContentRoute route);
}

// Programmatic content — union eliminates nullable ambiguity between text and binary
public record TextProgrammaticContent(
    IFrontMatter? Metadata,
    string RawContent,                     // never null — it's text
    string ContentType = "text/html"
);

public record BinaryProgrammaticContent(
    Func<Task<byte[]>> ByteGenerator,      // never null — it's binary
    string ContentType
);

public union ProgrammaticContent(TextProgrammaticContent, BinaryProgrammaticContent);
```

> **C# 15 union syntax note:** Union cases are NOT nested types. You write
> `DiscoveredItem`, not `ContentItem.Discovered`. Pattern matching on a union
> automatically unwraps the `Value` property — write `RenderedItem r =>` and the
> compiler checks `item.Value` for you. `default(ContentItem)` has a null `Value`;
> handle with a `null =>` arm where the union could be uninitialized.

### Pipeline Execution

```csharp
public interface IContentPipeline
{
    // Entry: content services produce discovered items
    IAsyncEnumerable<ContentItem> DiscoverAsync();

    // Transform: each stage maps items forward
    IAsyncEnumerable<ContentItem> ParseAsync(IAsyncEnumerable<ContentItem> items);
    IAsyncEnumerable<ContentItem> RenderAsync(IAsyncEnumerable<ContentItem> items);

    // Exit: generate output files
    Task<BuildReport> GenerateAsync(IAsyncEnumerable<ContentItem> items, OutputOptions options);
}
```

**Key principle**: `FailedItem` values flow through the entire pipeline (wrapped as `ContentItem` via implicit conversion). They're never caught-and-swallowed. The `GenerateAsync` step pattern-matches them out and collects them into the build report.

### RenderedContent

```csharp
public record RenderedContent(
    string Html,
    OutlineEntry[] Outline,
    ImmutableList<Tag> Tags,
    ImmutableList<CrossReference> CrossReferences,
    SearchIndexDocument? SearchDocument,
    SocialMetadata? Social
);

public record SocialMetadata(
    string? Description,
    string? ImageUrl,
    string? Type,         // "article", "website", etc.
    DateTime? PublishedTime,
    string? Author
);
```

---

## 3. Build Report — Structured Visibility

v1 silently catches errors in 7 locations. v2 accumulates everything and presents a structured report at the end of every build.

### The Report

```csharp
// Build diagnostic case types
public record DiagnosticInfo(ContentRoute Route, string Message);
public record DiagnosticWarning(ContentRoute Route, string Message);
public record DiagnosticError(ContentRoute Route, string Message, Exception? Exception);

public union BuildDiagnostic(DiagnosticInfo, DiagnosticWarning, DiagnosticError)
{
    public ContentRoute Route => Value switch
    {
        DiagnosticInfo i    => i.Route,
        DiagnosticWarning w => w.Route,
        DiagnosticError e   => e.Route,
        null => throw new InvalidOperationException()
    };

    public string Message => Value switch
    {
        DiagnosticInfo i    => i.Message,
        DiagnosticWarning w => w.Message,
        DiagnosticError e   => e.Message,
        null => throw new InvalidOperationException()
    };
}

public sealed class BuildReport
{
    public ImmutableList<BuildDiagnostic> Diagnostics { get; }
    public ImmutableList<BrokenLink> BrokenLinks { get; }
    public ImmutableList<ContentRoute> GeneratedPages { get; }
    public ImmutableList<ContentRoute> SkippedPages { get; }  // drafts
    public ImmutableList<ContentRoute> FailedPages { get; }
    public TimeSpan Duration { get; }

    // Pattern matches on DiagnosticError, not BuildDiagnostic.Error (cases aren't nested types)
    public bool HasErrors => Diagnostics.Any(d => d is DiagnosticError) 
                          || BrokenLinks.Count > 0 
                          || FailedPages.Count > 0;

    // Structured console output
    public void WriteTo(ILogger logger);
    public void WriteTo(TextWriter writer);
}
```

### Report Output (example)

```
╔══════════════════════════════════════════════════╗
║  Build Complete — 142 pages in 3.2s              ║
╠══════════════════════════════════════════════════╣
║  ✓ 138 pages generated                           ║
║  ⊘   2 pages skipped (draft)                     ║
║  ✗   2 pages failed                              ║
║  ⚠   3 warnings                                  ║
╠══════════════════════════════════════════════════╣
║  ERRORS                                          ║
║  ✗ /docs/api/broken-page                         ║
║    HTTP 500: NullReferenceException in Render     ║
║    Source: Content/Docs/api/broken-page.md        ║
║                                                   ║
║  ✗ /blog/draft-leaked                            ║
║    Front matter parse failed: invalid YAML ln 3   ║
║    Source: Content/Blog/draft-leaked.md           ║
║                                                   ║
║  WARNINGS                                        ║
║  ⚠ /docs/old-page → redirect target not found    ║
║  ⚠ 2 broken links found:                        ║
║    /docs/setup links to /docs/install (404)       ║
║    /blog/post links to /missing-image.png (404)   ║
╚══════════════════════════════════════════════════╝
```

### Integration with Dual-Mode

- **Dev mode** (`dotnet watch`): Diagnostics logged as they occur. Errors show in browser via error page middleware.
- **Build mode** (`dotnet run -- build`): Full report printed at end. Non-zero exit code if `HasErrors`.

---

## 4. Content Service System

### IContentService (simplified)

```csharp
public interface IContentService
{
    /// Discover all content items this service is responsible for.
    /// Returns DiscoveredItem directly — the pipeline wraps into ContentItem downstream.
    IAsyncEnumerable<DiscoveredItem> DiscoverAsync();

    /// Static files to copy to output (images, downloads, etc.)
    Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync();

    /// Dynamically generated files (search index, etc.)
    Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync();

    /// Navigation entries for table of contents.
    Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync();

    /// Cross-references for xref resolution.
    Task<ImmutableList<CrossReference>> GetCrossReferencesAsync();

    string DefaultSection { get; }
    int SearchPriority { get; }
}
```

**Change from v1**: `GetPagesToGenerateAsync()` is gone. Content services now return `DiscoveredItem` via the pipeline. The pipeline wraps these into `ContentItem` (implicit conversion) and handles rendering and generation — services just discover.

### Built-in Services (same set, cleaner)

- **MarkdownContentService\<T\>** — discovers `.md` files, produces `DiscoveredItem` with `MarkdownFileSource`
- **RazorPageContentService** — discovers `@page` components, produces `DiscoveredItem` with `RazorPageSource`
- **RedirectContentService** — reads `_redirects.yml`, produces `DiscoveredItem` with `RedirectSource`
- **ApiReferenceContentService** — Roslyn-based, lives in separate package (see §8)

### Custom Content Services

Custom services (recipes, responsive images, API docs, etc.) implement `IContentService` and use `ProgrammaticSource` with `IProgrammaticContentGenerator`:

```csharp
// Example: RecipeContentService discovers .cook files
public class RecipeContentService : IContentService
{
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var file in GetRecipeFiles())
        {
            var route = ContentRouteFactory.FromCustom(
                url: new UrlPath($"/recipes/{file.Name}"),
                sourceFile: file.Path);

            // DiscoveredItem is the record type; the pipeline wraps it into ContentItem via implicit conversion
            yield return new DiscoveredItem(route,
                new ProgrammaticSource(new RecipeGenerator(file)));
        }
    }

    string IContentService.DefaultSection => "Recipes";
    int IContentService.SearchPriority => 5;
}

// Generator receives route context, produces metadata + content
public class RecipeGenerator(RecipeFile file) : IProgrammaticContentGenerator
{
    public async Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
    {
        var recipe = await CookLangParser.ParseAsync(file.Path);
        // TextProgrammaticContent → ProgrammaticContent via implicit union conversion
        return new TextProgrammaticContent(
            Metadata: new RecipeFrontMatter { Title = recipe.Title },
            RawContent: recipe.ToHtml());
    }
}
```

For **binary content** (responsive images, downloads, etc.), return `BinaryProgrammaticContent`. The pipeline pattern-matches on the union to skip Parse/Render stages and write bytes directly during output:

```csharp
public class ImageGenerator(string path, ImageSize size) : IProgrammaticContentGenerator
{
    public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
        // BinaryProgrammaticContent → ProgrammaticContent via implicit union conversion
        => Task.FromResult<ProgrammaticContent>(new BinaryProgrammaticContent(
            ByteGenerator: async () => await ResizeImageAsync(path, size),
            ContentType: "image/webp"));
}
```

The pipeline dispatches on the union — no nullable checks, no ambiguous third state:

```csharp
// Inside the pipeline — compiler enforces both cases handled
var result = programmaticContent switch
{
    TextProgrammaticContent text   => ParseAndRender(text.RawContent, text.Metadata),
    BinaryProgrammaticContent bin  => await WriteBytesAsync(bin.ByteGenerator),
};
```

---

## 5. Front Matter — Capability Interfaces

v1 forces every `IFrontMatter` implementer to have 7 members (Title, Tags, IsDraft, Uid, RedirectUrl, Section, Metadata). v2 uses a minimal base with opt-in capabilities.

### Core Contract

```csharp
/// Minimum: every content page has a title.
public interface IFrontMatter
{
    string Title { get; }
}

/// Opt-in capabilities
public interface IDraftable       { bool IsDraft { get; } }
public interface ITaggable        { string[] Tags { get; } }
public interface IRedirectable    { string? RedirectUrl { get; } }
public interface ISectionable     { string? Section { get; } }
public interface ICrossReferenceable { string? Uid { get; } }
public interface IOrderable       { int Order { get; } }
public interface IDescribable     { string? Description { get; } }
public interface IDateable        { DateTime? Date { get; } }
```

### Pipeline Checks Capabilities

```csharp
// In the pipeline — not in user code
if (frontMatter is IDraftable { IsDraft: true })
    // item is skipped (added to BuildReport.SkippedPages)

if (frontMatter is ITaggable taggable)
    tags = tagService.Extract(taggable.Tags);
```

### Convenience Base Classes

For users who don't want to pick interfaces:

```csharp
/// Covers the DocSite use case
public record DocFrontMatter : IFrontMatter, IDraftable, ITaggable, 
    ISectionable, ICrossReferenceable, IOrderable, IDescribable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Section { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
}

/// Covers the BlogSite use case
public record BlogFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }
    public string? Author { get; init; }
    public string? Series { get; init; }
    public string? Uid { get; init; }
}
```

---

## 6. Code Highlighting — Pluggable with Fallback

### Architecture

```csharp
public interface ICodeHighlighter
{
    /// Languages this highlighter handles (e.g., "csharp", "python")
    IReadOnlySet<string> SupportedLanguages { get; }

    /// Highlight code. Returns HTML with spans.
    string Highlight(string code, string language);

    /// Priority — higher wins when multiple highlighters support a language.
    int Priority { get; }
}
```

### Built-in Highlighters

| Highlighter | Package | Priority | Languages |
|------------|---------|----------|-----------|
| TextMateHighlighter | Core | 50 | All TextMate grammars |
| RoslynHighlighter | `.Roslyn` package | 100 | C#, VB.NET |
| ShellHighlighter | Core | 75 | bash, shell, sh |
| PlainTextHighlighter | Core | 0 | Fallback for any language |

### Client-Side Fallback

When server-side highlighting can't handle a language (no matching grammar), the code block is emitted with `class="language-{lang}"` and no server-side spans. Client-side highlight.js picks it up automatically.

```html
<!-- Server highlighted (has spans) -->
<pre><code class="language-csharp highlighted">
  <span class="keyword">var</span> x = <span class="number">42</span>;
</code></pre>

<!-- Client fallback (no spans, highlight.js will process) -->
<pre><code class="language-obscure-lang">
  some code here
</code></pre>
```

### Custom Grammar Registration

```csharp
builder.Services.AddPenn(options => {
    options.Highlighting.AddTextMateGrammar("mylang", grammarStream);
    options.Highlighting.AddHighlighter<MyCustomHighlighter>();
});
```

### Code Block Directives (preserved from v1)

All v1 directives carry forward unchanged — they operate on highlighted output regardless of which highlighter produced it:
- `[!code highlight]`, `[!code focus]`, `[!code ++]`, `[!code --]`
- `[!code error]`, `[!code warning]`
- `[!code include-start/end]`, `[!code exclude-start/end]`
- `[!code word:term]`, `[!code word:term|message]`
- Tabbed code blocks (adjacent blocks with same tab group)

---

## 7. SPA / Islands — First Class

v1 bolted SPA on as an afterthought. v2 makes it a core pipeline output.

### Dual Output

Every rendered page produces both:
1. **Full HTML** — complete page for static generation and initial load
2. **Island data** — JSON fragments for SPA navigation

This happens in the pipeline, not as a separate content service.

### Islands Registration

```csharp
builder.Services.AddPenn(options => {
    options.Islands.Register<NavigationIsland>("navigation");
    options.Islands.Register<ContentIsland>("content");
    options.Islands.Register<OutlineIsland>("outline");
});
```

### Island Interface

```csharp
public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}

/// Convenience base for Razor component islands
public abstract class RazorIsland<TComponent> : IIslandRenderer 
    where TComponent : IComponent
{
    public abstract string IslandName { get; }
    protected abstract ValueTask<Dictionary<string, object?>> BuildParametersAsync(
        ContentRoute route, RenderContext context);
}
```

### Static Generation

During build, for each page the pipeline writes:
- `{outputFile}` — full HTML page
- `/_spa-data/{canonical-path}.json` — island JSON envelope

```csharp
public record SpaEnvelope(
    string Title,
    string? Description,
    SocialMetadata? Social,                       // OG/Twitter meta tag data
    ImmutableDictionary<string, string> Islands    // islandName → HTML fragment
);
```

### Client Runtime

`spa-engine.js` (refined from v1):
- Intercepts internal link clicks
- Fetches `/_spa-data/{path}.json`
- Swaps island containers by `data-island="{name}"` attribute
- Updates `<title>`, meta tags, browser history
- Falls back to full navigation on error

### Client-Side Component Lifecycle

The hardest part of SPA: when islands swap, JavaScript managers must re-initialize. The `spa:commit` event coordinates this.

```
1. TEARDOWN PHASE — Before island HTML is replaced:
   - All managers abort outstanding async ops (timers, fetch, listeners)
   - Clear cached DOM references
   - Reset state flags (mermaidLoaded, searchIndexFailed, etc.)

2. SWAP PHASE — Island HTML replaced via innerHTML:
   - View Transitions API wraps the swap (if supported)
   - Per-island transition names: style.viewTransitionName = `spa-island-{name}`
   - Respects prefers-reduced-motion (disable transitions via @media query)

3. SETUP PHASE — After swap, spa:commit fires:
   - PageManager re-initializes in order:
     a) SyntaxHighlighter.init() — re-highlight code blocks
     b) TabManager.init() — re-scan tablists, re-attach ARIA
     c) MermaidManager.init() — re-render diagrams
     d) OutlineManager.init() — rebuild section map from new headings
   - If any manager fails: log error, continue others (degrade gracefully)
```

**Rules for island HTML:**
- No `<script>` tags inside islands (innerHTML does not execute scripts)
- No inline event handlers (`onclick`, etc.) — use `data-*` attributes
- All event binding via JavaScript managers that re-query DOM after `spa:commit`

### Error Recovery

```
1. Fetch failures (4xx/5xx, network timeout):
   - Do NOT commit partial island swaps
   - Fall back to full page navigation: location.href = url
   - Timeout: 10 seconds (configurable via data-spa-timeout)

2. JSON parse errors:
   - Log error, fall back to full navigation (data corruption is unrecoverable)

3. Offline:
   - Detect via navigator.onLine + fetch error
   - Show offline indicator, allow retry when online
```

### Scroll Behavior

- New page (no hash): scroll to top after View Transition completes
- Hash navigation (`#section`): scroll to element after transition
- Back/forward: restore `history.state.scrollY`
- Prefetch does not affect bfcache

### Accessibility

- ARIA live region announces `"Navigated to {title}"` on `spa:commit`
- Focus moves to first heading in main content island after swap
- Outline ARIA attributes (`aria-current`) updated by OutlineManager
- Skip-to-content link target (`#main-content`) remains stable across swaps

---

## 8. Package Structure

```
Penn/                                     # Core — zero Roslyn dependency
├── Pipeline/                             # ContentItem union, pipeline stages
├── Routing/                              # ContentRoute, ContentRouteFactory, UrlPath
├── Content/                              # IContentService, Markdown*, Razor*, Redirect*
├── FrontMatter/                          # IFrontMatter, capability interfaces
├── Highlighting/                         # ICodeHighlighter, TextMate, Shell, directives
├── Navigation/                           # TOC, breadcrumbs, prev/next
├── Islands/                              # IIslandRenderer, SpaEnvelope, client JS
├── Generation/                           # OutputGenerationService, BuildReport
├── Search/                               # SearchIndexService, SearchIndexDocument
├── Feeds/                                # SitemapService, RssService
├── Localization/                         # Multi-locale support
└── Infrastructure/                       # File watcher, middleware, link verification

Penn.Roslyn/             # Optional — API docs + C# highlighting
├── ApiReferenceContentService
├── RoslynHighlighter (ICodeHighlighter)
├── SymbolExtractionService
└── CodeFragmentExtractor

Penn.UI/                 # Razor component library
├── Components/ (Badge, Card, Steps, etc.)
├── Navigation/ (OutlineNav, TOCNav)
└── wwwroot/ (scripts.js, spa-engine.js)

Penn.MonorailCss/        # CSS integration (unchanged from v1)

Penn.DocSite/            # Ready-to-use doc site
Penn.BlogSite/           # Ready-to-use blog site
```

### What Moved Out of Core

| Feature | v1 Location | v2 Location |
|---------|-------------|-------------|
| Roslyn workspace | Core | `.Roslyn` package |
| Symbol extraction | Core | `.Roslyn` package |
| C#/VB highlighting | Core | `.Roslyn` package |
| API reference service | Core | `.Roslyn` package |
| MSBuild locator | Core | `.Roslyn` package |

Core now has **zero** Microsoft.CodeAnalysis dependencies.

---

## 9. Registration API

### Minimal Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPenn(engine => {
    engine.SiteTitle = "My Site";
    engine.SiteDescription = "A site.";
    engine.AddMarkdownContent<MyFrontMatter>(md => {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

builder.Services.AddMonorailCss();

var app = builder.Build();
app.UsePenn();
await app.RunOrBuildAsync(args);
```

### Full Setup

```csharp
builder.Services.AddPenn(engine => {
    engine.SiteTitle = "Docs";
    engine.SiteDescription = "Documentation";
    engine.CanonicalBaseUrl = "https://docs.example.com";
    engine.ContentRootPath = "Content";

    // Multiple content sources
    engine.AddMarkdownContent<DocFrontMatter>(md => {
        md.ContentPath = "Content/Docs";
        md.BasePageUrl = "/docs";
        md.Section = "Documentation";
    });
    engine.AddMarkdownContent<BlogFrontMatter>(md => {
        md.ContentPath = "Content/Blog";
        md.BasePageUrl = "/blog";
    });

    // Highlighting
    engine.Highlighting.AddTextMateGrammar("mylang", grammarStream);

    // Islands
    engine.Islands.Register<NavIsland>("nav");
    engine.Islands.Register<ContentIsland>("content");

    // Localization
    engine.Localization.DefaultLocale = "en";
    engine.Localization.AddLocale("fr", "Français");
    engine.Localization.AddLocale("ja", "日本語");
});

// Optional: Roslyn integration (separate package)
builder.Services.AddPennRoslyn(roslyn => {
    roslyn.SolutionPath = "path/to/solution.slnx";
    roslyn.IncludeNamespaces = ["MyLib.Public"];
});

builder.Services.AddMonorailCss(css => {
    css.ColorScheme = new AlgorithmicColorScheme { PrimaryHue = 220 };
});
```

### High-Level Helpers (DocSite / BlogSite)

```csharp
// One-liner doc site
builder.Services.AddDocSite(options => { ... });
app.UseDocSite();
await app.RunDocSiteAsync(args);

// One-liner blog
builder.Services.AddBlogSite(options => { ... });
app.UseBlogSite();
await app.RunBlogSiteAsync(args);
```

### DocSite Behavior

`AddDocSite()` registers a pre-configured Penn instance with:
- `DocFrontMatter` as the front matter type (all standard capabilities enabled)
- Default islands: navigation, content, outline
- Mdazor integration for Razor-in-Markdown
- Optional Roslyn API reference (if `.Roslyn` package present and configured)
- Section landing page discovery via `index.md` / `_index.metadata.yml`

Users with custom front matter must implement all capability interfaces they need — omitting `IDraftable` silently disables draft filtering, omitting `ITaggable` disables tag pages, etc.

### BlogSite Behavior

`AddBlogSite()` registers a blog-optimized Penn instance with:
- `BlogFrontMatter` as the front matter type
- Date-sorted archive: pipeline sorts `IDateable` items descending by `Date`
- Series grouping: `IBlogContentService<T>.GetPostsBySeriesAsync()` for series navigation
- RSS eligibility: posts with `IDateable` + not draft + not redirect are included automatically
- Social metadata: `SocialMetadata` populated from `IDescribable.Description`, `BlogSiteOptions.SocialMediaImageUrlFactory`, and `IDateable.Date`
- Optional author/series index page generation via `BlogSiteOptions`:

```csharp
builder.Services.AddBlogSite(options => {
    options.ContentPath = "Content/Blog";
    options.EnableRss = true;
    options.GenerateSeriesIndexes = true;   // /blog/series/{slug}/
    options.GenerateAuthorPages = false;    // opt-in
    options.SocialMediaImageUrlFactory = (route, fm) => $"/images/social/{route.CanonicalPath}.png";
});
```

---

## 10. Dual-Mode Execution (preserved)

Same pattern as v1 — the app is a real ASP.NET Core server in dev mode and a static site generator in build mode.

```csharp
// In RunOrBuildAsync:
if (args is ["build", ..])
{
    var outputOptions = OutputOptions.FromArgs(args);
    var report = await pipeline.GenerateAsync(outputOptions);
    report.WriteTo(Console.Out);
    return report.HasErrors ? 1 : 0;
}
else
{
    await app.RunAsync();
    return 0;
}
```

### Build Mode

```bash
# Basic
dotnet run -- build

# With base URL for subfolder deployment (GitHub Pages)
dotnet run -- build /my-project

# With custom output folder
dotnet run -- build /my-project ./dist

# Environment variable fallback
BASE_HREF=/my-project dotnet run -- build
```

### Dev Mode

```bash
dotnet watch    # Hot reload for content + code changes
```

File watcher system preserved from v1 with refinements:
- `IPennFileWatcher` monitors content directories
- `FileWatchDependencyFactory<T>` invalidates cached services on change
- Content changes → cache invalidation → reload on next request
- Code changes → .NET hot reload

### Dev Workflow & Cache Invalidation

Typical content edit cycle:
1. Developer edits `Content/docs/installation.md` and saves
2. FileWatcher detects change → `FileWatchDependencyFactory.InvalidateInstance()`
3. Developer refreshes browser at `/docs/installation`
4. Request pipeline rebuilds content cache (re-parses all `.md` files in service)
5. If parsing fails: `ContentErrorBoundaryMiddleware` returns error page with file path, line number, and error details
6. If parsing succeeds: page renders with new content

**Debouncing:** File changes are coalesced within a 500ms window. Rapid saves (editor auto-save, Git operations) don't trigger multiple cache rebuilds. Configurable via `FileWatchOptions.DebounceMs`.

**Cache rebuild failures:** `AsyncLazy` with `RetryOnFailure` ensures the next request retries a failed rebuild. When the developer fixes the error and refreshes, the page renders correctly.

**No auto-refresh:** The browser does not auto-reload. `dotnet watch` handles code recompilation; content changes require a manual browser refresh. This is intentional — auto-refresh during rapid editing is disruptive.

---

## 11. Navigation & Table of Contents

Same model as v1 (it works well), with ContentRoute integration.

### ContentTocItem

```csharp
public record ContentTocItem(
    string Title,
    ContentRoute Route,            // was: UrlPath Url
    int Order,
    string[] HierarchyParts,       // tree structure independent of URL
    string? Section,
    string? Locale
);
```

### NavigationInfo (unchanged shape)

```csharp
public record NavigationInfo(
    string? SectionName,
    ContentRoute? SectionRoute,
    ImmutableList<BreadcrumbItem> Breadcrumbs,
    string PageTitle,
    NavigationTreeItem? PreviousPage,
    NavigationTreeItem? NextPage
);
```

### Tree Building

Same approach: `HierarchyParts` defines tree structure independently of URL. Folder metadata via `_index.metadata.yml` sidecar files. Selection state calculated from current route's `CanonicalPath`.

### Section Landing Pages

Section index pages are discovered by naming convention:
- `Content/Docs/index.md` → section landing page at `/docs/`
- `Content/Docs/api/index.md` → subsection landing page at `/docs/api/`
- `_index.metadata.yml` in a folder provides section title and order without requiring an `index.md`

For localized section landing pages: `Content/Docs/fr/index.md` → `/fr/docs/`

### Search Priority

Each content service declares `SearchPriority` (higher = ranked first in results). Recommended defaults:
- Documentation: 10 (highest — users expect docs first)
- API Reference: 8
- Blog posts: 5
- Custom services: user-defined

---

## 12. Localization (elevated to first-class)

v1 treated localization as a bolt-on. v2 makes locale a core data type carried through the entire pipeline via `ContentRoute.Locale`.

### URL Pattern (preserved)
- Default locale: no prefix (`/docs/page/`)
- Other locales: prefixed (`/fr/docs/page/`)

### Locale on ContentRoute

Locale is an explicit field on `ContentRoute` (see §1). It is set once at construction and available to every downstream consumer without URL parsing:

```csharp
ContentRouteFactory.FromMarkdownFile(file, contentRoot, basePageUrl, locale: "fr");
// → CanonicalPath: "/fr/docs/page", Locale: "fr"

// Downstream: no URL parsing needed
if (route.Locale == "fr") { ... }
if (route.IsDefaultLocale) { ... }
```

### Content Folder Structure

```
Content/
  Docs/                    ← default locale files
    getting-started.md
    api-reference.md
  Docs/fr/                 ← French translations (subfolder per locale)
    getting-started.md     ← translated version
                           ← api-reference.md missing → falls back to default
  Docs/ja/                 ← Japanese translations
    getting-started.md
```

**Fallback:** If a locale subfolder doesn't contain a translated file, the default locale version is used automatically. The content service logs a diagnostic (`DiagnosticInfo`) for untranslated pages.

### Per-Locale Feeds and Search

```
/rss.xml                   ← default locale posts only
/fr/rss.xml                ← French posts only
/sitemap.xml               ← all locales with hreflang alternates
/search-index.json         ← all locales, each entry has "locale" field
```

`SearchIndexDocument` includes `Locale` from `ContentRoute.Locale`, enabling client-side locale-filtered search.

### RTL Support

`LocaleInfo` carries `Direction` ("ltr" or "rtl"). The middleware sets `dir="{direction}"` on the `<html>` element. Components can read `LocaleInfo.Direction` from the render context for layout decisions.

### Registration

```csharp
engine.Localization.DefaultLocale = "en";
engine.Localization.AddLocale("fr", new LocaleInfo("Français", Direction: "ltr", HtmlLang: "fr-FR"));
engine.Localization.AddLocale("ar", new LocaleInfo("العربية", Direction: "rtl", HtmlLang: "ar"));
```

`ILocalizedContentService<T>` interface preserved. `AlternateLanguagePage` discovery preserved for language switcher UI and hreflang generation.

---

## 13. Markdown Processing (preserved, decoupled)

Same Markdig pipeline with all extensions. Same code block directives. Same custom alerts. Same Mdazor integration.

**Changes:**
- `MarkdownParserService` returns a `ParsedItem` (implicitly converts to `ContentItem`) instead of raw objects
- Outline generation is a pipeline stage, not embedded in the parser
- `PreProcessMarkdown` hook becomes a pipeline stage (not a delegate on options)

### Processing Pipeline (v2)

```
Discover (ContentFilesService scans files)
  → DiscoveredItem (wrapped as ContentItem)
Parse (read file, extract YAML + body)
  → ParsedItem (wrapped as ContentItem)
PreProcess (optional transform hooks)
  → ParsedItem (transformed, re-wrapped as ContentItem)
Render (Markdig pipeline → HTML)
  → RenderedItem (wrapped as ContentItem)
  or FailedItem (with error details, wrapped as ContentItem)
```

---

## 14. Link Verification (enhanced)

Same HTML parsing approach (AngleSharp), but integrated into the build report.

```csharp
public record ValidLink(ContentRoute SourcePage, string Url);
public record BrokenLink(ContentRoute SourcePage, string Url, LinkType Type, string Reason);
public record ExternalLink(ContentRoute SourcePage, string Url);  // skipped, logged

public union LinkCheckResult(ValidLink, BrokenLink, ExternalLink);
```

Link verification runs after all pages are generated. Broken links are added to `BuildReport.BrokenLinks`. The build report decides severity (error vs warning) based on configuration.

---

## 15. Static Generation (refined)

Same approach: HTTP GET against running app → save to disk. But with structured output.

```csharp
public sealed class OutputGenerationService
{
    public async Task<BuildReport> GenerateAsync(OutputOptions options)
    {
        var report = new BuildReportBuilder();

        // 1. Discover all content
        var items = await pipeline.DiscoverAndRenderAsync();

        // 2. Copy static assets
        await CopyStaticAssetsAsync(report);

        // 3. Generate pages via HTTP (parallel)
        // Pattern match on case types directly — NOT ContentItem.Rendered (cases aren't nested)
        await Parallel.ForEachAsync(items, async (item, ct) =>
        {
            switch (item)
            {
                case RenderedItem rendered:
                    var result = await FetchAndSaveAsync(rendered.Route, ct);
                    report.Add(result);  // always recorded — success, warning, or error
                    break;

                case FailedItem failed:
                    report.AddError(failed.Route, failed.Error);
                    break;
            }
        });

        // 4. Verify links
        var linkResults = await linkVerifier.ValidateAllAsync(report.GeneratedPages);
        report.AddLinkResults(linkResults);

        // 5. Generate SPA data
        await GenerateSpaDataAsync(report);

        return report.Build();
    }
}
```

**Key difference from v1**: `FetchAndSaveAsync` never swallows exceptions. Every outcome is recorded in the report.

### Dev/Build Parity Contract

Both modes feed the same pipeline and middleware stack. The only difference is the configured base URL value:
- Dev mode: base URL is `/` (or whatever the user configures)
- Build mode: base URL is the `BASE_HREF` argument (e.g., `/my-project`)

`BaseUrlRewritingProcessor` runs identically in both modes. To verify parity:
1. Run `dotnet watch` and view a page
2. Run `dotnet run -- build` and serve the output folder
3. Both should produce semantically identical HTML (modulo base URL prefix)

### SPA Data Generation

During build, for each generated page with islands registered:
1. Fetch `/_spa-data/{canonical-path}.json` via HTTP (same as page fetch)
2. `BaseUrlRewritingProcessor` rewrites URLs in the JSON response
3. `CssClassCollectorProcessor` collects classes from island HTML fragments
4. Save to `{output}/_spa-data/{canonical-path}.json`

If island rendering fails, the page becomes a `FailedItem` — no partial SPA data files.

---

## 16. Middleware Stack

Same middleware as v1, with ContentRoute awareness and a new error boundary:

1. **ContentErrorBoundaryMiddleware** — catches content rendering failures (new in v2)
2. **PennRedirectMiddleware** — redirects from `_redirects.yml`
3. **ResponseProcessingMiddleware** — pipeline of `IResponseProcessor`
4. **BaseUrlRewritingProcessor** — rewrites URLs using `ContentRoute.WithBaseUrl`
5. **XrefResolver** — resolves `xref:uid` using cross-reference registry
6. **CssClassCollectorProcessor** — collects classes for MonorailCSS generation

### ContentErrorBoundaryMiddleware (new in v2)

v1's biggest dev-time pain: errors are silently logged and the page either doesn't render or renders empty. v2 shows errors inline.

```csharp
public class ContentErrorBoundaryMiddleware(RequestDelegate next)
{
    // Dev mode (IsDevelopment): catches content rendering exceptions and returns
    // an HTML error page showing:
    //   - Error type (ParseError, RenderError, ValidationError)
    //   - Source file path (relative to content root)
    //   - Line number (if available from YAML/Markdown parse errors)
    //   - Stack trace (full)
    //   - "Fix the file and refresh" guidance
    //
    // Build mode: does not catch — errors propagate to OutputGenerationService
    // which records them in the BuildReport as FailedItem.
}
```

### BaseUrlRewritingProcessor — Scope and Parity

`ContentRoute.WithBaseUrl()` is used **only for link verification** (§14) to normalize URLs for comparison. It is NOT used during rendering.

Base URL rewriting is handled entirely by `BaseUrlRewritingProcessor` which runs as a response processor after rendering. It rewrites:
- `href`, `src`, `srcset`, `form action` attributes in HTML
- `url()` references in inline CSS
- `data-base-url` attribute on `<body>`
- URLs inside `application/json` responses (SPA data files)

This ensures dev mode and build mode use the same code path for URL rewriting. The processor runs identically in both modes — the only difference is the configured base URL value.

---

## 17. Search, Sitemap, RSS (preserved)

Same features, same endpoints:
- `/sitemap.xml` — all pages with lastmod, hreflang
- `/rss.xml` — dated content, sorted descending
- `/search-index.json` — FlexSearch client-side index

`SearchIndexDocument` now carries `ContentRoute` instead of raw URL string. Canonical URLs derived from `route.AbsoluteUrl(canonicalBase)`.

---

## 18. UI Component Library (preserved)

Same components, same JS managers:
- Badge, Card, LinkCard, CardGrid, BigTable, Steps, Step
- OutlineNavigation, TableOfContentsNavigation
- CodeBlock, FallbackNotice, LanguageSwitcher
- ThemeManager, SearchManager, MermaidManager, TabManager, etc.

No architectural changes — the UI layer is clean in v1.

---

## 19. MonorailCSS Integration (preserved, with SPA clarifications)

Same architecture:
- `CssClassCollector` + `CssClassCollectorProcessor` scan responses
- `MonorailCssService` generates stylesheet from collected classes
- `NamedColorScheme` and `AlgorithmicColorScheme` (OKLCH-based)
- Prose styling, code block styles, alert styles

### SPA/Islands CSS Collection

With SPA islands now first-class, CSS class collection must cover both full HTML pages and island JSON fragments:

- **Static build mode:** The generator fetches both HTML pages and `/_spa-data/{path}.json` endpoints. `CssClassCollectorProcessor` processes `application/json` responses by unescaping HTML fragments and extracting classes. Island-only classes (used in an island but not the main page) are discovered this way.
- **Dev mode:** CSS classes are collected as responses are served (both SSR full pages and SPA JSON endpoints). Hot reload timing: new classes from edited components are collected on the next request, not immediately.
- **Important:** All `/_spa-data/` endpoints must be fetched during static build to ensure CSS completeness. Missing fetches = missing styles for island-only classes.

---

## 20. Error Handling Philosophy

### v1 (broken)
```
try { process(); } catch (Exception) { log.Warning(...); /* continue */ }
```

### v2 (no silent catches)
```
Every operation returns a result type. Failed items propagate through
the pipeline. The build report collects everything. Nothing is swallowed.
```

**Rules:**
1. Content services **never** throw exceptions for content problems. They return `FailedItem` (implicitly converts to `ContentItem`).
2. Infrastructure failures (disk I/O, network) propagate as exceptions — these are genuine crashes.
3. The build report distinguishes between content errors (bad markdown, missing front matter) and infrastructure errors (disk full, port in use).
4. Dev mode shows content errors inline via `ContentErrorBoundaryMiddleware` (§16) — showing file path, line number, error type, and stack trace.
5. Build mode returns non-zero exit code if any errors exist.
6. Unresolved `xref:uid` links render as visually distinct broken links (red strikethrough) in dev mode AND are recorded as `DiagnosticWarning` in the build report.

### Error Examples

```
YAML parse failure:
  Discover → file found
  Parse → FAILS: YamlException at line 4
  → new FailedItem(route, new ContentError("YAML parse error..."))
  → implicitly converts to ContentItem, flows through pipeline
  Dev: error page with file path + line number
  Build: recorded in BuildReport.FailedPages, build continues

Missing front matter title:
  Discover → file found
  Parse → front matter parsed, but Title is empty
  → new FailedItem(route, new ContentError("Title is required"))

Razor component error:
  Discover → @page component found
  Render → component throws NullReferenceException
  → new FailedItem(route, new ContentError("Render failed: NRE in About.razor:42"))

Empty site (no content files):
  Discover → 0 items
  → BuildReport with 0 GeneratedPages, new DiagnosticWarning(route, "No content discovered")
  → Exit code 0 (not an error — just empty)
```

---

## 21. v1 Feature Checklist Coverage

Every feature from v1 mapped to v2 location:

| v1 Feature | v2 Status |
|-----------|-----------|
| Markdown → HTML (Markdig) | §13 — same pipeline |
| YAML front matter (YamlDotNet) | §5 — capability interfaces |
| Custom front matter types | §5 — just implement `IFrontMatter` + desired capabilities |
| Multiple markdown services | §9 — `engine.AddMarkdownContent<T>()` multiple times |
| Razor page discovery | §4 — `RazorPageContentService` |
| URL redirects (`_redirects.yml`) | §4 — `RedirectContentService` |
| API reference (Roslyn) | §8 — separate `.Roslyn` package |
| Code syntax highlighting (TextMate) | §6 — pluggable, TextMate is default |
| Code block directives | §6 — preserved unchanged |
| Tabbed code blocks | §6 — preserved |
| Custom alerts | §13 — Markdig extension preserved |
| Mdazor components in markdown | §13 — preserved |
| Page outline (H2/H3) | §2 — pipeline stage produces `Outline` |
| Site navigation tree | §11 — same model, ContentRoute-aware |
| Breadcrumbs | §11 — preserved |
| Previous/next navigation | §11 — preserved |
| Tag system | §5 — `ITaggable` capability, same TagService |
| Multi-locale with fallback | §12 — **elevated to first-class**, locale on ContentRoute |
| Alternate language pages | §12 — preserved |
| hreflang in sitemap | §17 — preserved |
| Social media meta tags | §2 — `SocialMetadata` on `RenderedContent` + `SpaEnvelope` |
| SPA / Islands | §7 — **first-class**, with lifecycle, error recovery, a11y |
| Search index (FlexSearch) | §17 — preserved |
| Sitemap.xml | §17 — preserved |
| RSS feed | §17 — preserved |
| Link verification | §14 — enhanced, integrated into report |
| Base URL rewriting (subfolder) | §1 — `ContentRoute.WithBaseUrl` |
| Cross-reference (xref) | §16 — preserved |
| File watcher / cache invalidation | §10 — preserved |
| Hot reload (dotnet watch) | §10 — preserved |
| Static file copying | §4 — `GetContentToCopyAsync` |
| Dynamic content creation | §4 — `GetContentToCreateAsync` |
| MonorailCSS | §19 — preserved |
| Dark mode | §18 — preserved |
| Prose styling | §19 — preserved |
| DocSite package | §8, §9 — preserved |
| BlogSite package | §8, §9 — preserved |
| Series grouping (blog) | BlogSite — preserved |
| Social media meta tags (DocSite/Blog) | DocSite/BlogSite — `SocialMetadata` populated from options |
| Custom TextMate grammars | §6 — `engine.Highlighting.AddTextMateGrammar` |
| Custom Markdig pipeline | §9 — preserved |
| Custom URL creation | §1 — `ContentRouteFactory.FromCustom` |
| Custom content services (recipes, images) | §4 — `IProgrammaticContentGenerator` + worked examples |
| PreProcess markdown hooks | §13 — pipeline stage |
| Assembly routing | §9 — `AdditionalRoutingAssemblies` preserved |
| Glob pattern exclusion | §4 — preserved |

---

## 22. Testing Strategy

**Test framework:** xunit.v3.mtp-v2 with Shouldly for assertions.

v1's test infrastructure (`PennTestBuilder`, `MarkdownTestData`, `ServiceMockFactory`) is well-designed. v2 extends it to support union-type pipelines, stage-by-stage testing, and dev/build parity validation.

### PennTestBuilder 2.0

```csharp
var builder = new PennTestBuilder()
    .WithMarkdownFiles(
        ("/content/index.md", MarkdownTestData.SimplePost),
        ("/content/about.md", MarkdownTestData.RichPost))
    .WithContentOptions(opts => {
        opts.ContentPath = "/content";
        opts.BasePageUrl = "/";
    });

// Test individual pipeline stages
var discovered = await builder.BuildDiscoveryStageAsync().ToListAsync();
var parsed = await builder.BuildParsingStageAsync(discovered).ToListAsync();
var rendered = await builder.BuildRenderingStageAsync(parsed).ToListAsync();

// Or test the full pipeline at once
var report = await builder.BuildFullPipelineAsync();
report.HasErrors.ShouldBeFalse();
report.GeneratedPages.Count.ShouldBe(2);
```

### Pipeline Stage Testing

Each union type stage is independently testable:

```csharp
// Verify Failed items survive the entire pipeline
[Fact]
public async Task FailedItems_PropagateThrough_EntirePipeline()
{
    var builder = new PennTestBuilder()
        .WithMarkdownFiles(("/content/bad.md", "---\ntitle: [invalid\n---\n# Hi"));

    var report = await builder.BuildFullPipelineAsync();

    report.FailedPages.Count.ShouldBe(1);
    report.Diagnostics.OfType<DiagnosticError>()
        .ShouldContain(d => d.Message.Contains("YAML"));
}
```

### Dev/Build Parity Testing

```csharp
// Start app in dev mode, fetch page, compare to build output
[Fact]
public async Task DevMode_And_BuildMode_ProduceIdenticalOutput()
{
    var devHtml = await FetchFromDevServer("/docs/getting-started");
    var buildHtml = await ReadBuildOutput("docs/getting-started/index.html");

    // Semantic comparison (ignoring whitespace, timestamps)
    devHtml.ShouldBeSemanticallySameAs(buildHtml);
}
```

### ServiceMockFactory Extensions

```csharp
// Mock content service returning async enumerable of union types.
// Case types are constructed directly — implicit conversion wraps them into the union.
var mock = ServiceMockFactory.CreateDiscoveryService(
    ("Home", "/", new MarkdownFileSource(new FilePath("/content/index.md"))),
    ("About", "/about", new RazorPageSource("AboutPage")));

// Mock failures for error-handling tests
var failing = ServiceMockFactory.CreateFailingDiscoveryService(
    "File not found", new UrlPath("/missing"));
```

### Snapshot Testing

Use approval-based testing (e.g., Verify) for rendered HTML, SPA envelopes, and build reports:

```csharp
[Fact]
public async Task Rendering_RichPost_MatchesGoldenFile()
{
    var report = await new PennTestBuilder()
        .WithMarkdownFiles(("/content/post.md", MarkdownTestData.RichPost))
        .BuildFullPipelineAsync();

    var html = report.GeneratedPages.First().Content.Html;
    await Verify(html);
}
```

### What to Test

| Concern | Test Type | Approach |
|---------|-----------|----------|
| URL computation (ContentRoute) | Unit | Exhaustive factory tests with edge cases |
| Front matter parsing | Unit | Valid, invalid, missing, partial YAML |
| Pipeline error propagation | Integration | Failed items survive all stages |
| Draft filtering | Integration | `IDraftable` items appear in SkippedPages |
| Dev/build parity | Integration | Compare HTTP response to build output |
| SPA envelope shape | Snapshot | JSON structure and required fields |
| Link verification | Integration | Mock page set, verify broken link detection |
| Base URL rewriting | Integration | Same page with `/` and `/subpath` base URLs |
| Locale routing | Unit + Integration | Default, prefixed, fallback, RTL |

---

## 23. Migration Path

Content files (`.md`, `_redirects.yml`, `_index.metadata.yml`, YAML front matter) are **100% compatible**. Users change C# registration code, not content.

### What Users Change

1. NuGet packages: `MyLittleContentEngine` → `Penn` + optionally `Penn.Roslyn`
2. Registration: `AddContentEngineService().WithMarkdownContentService<T>()` → `AddPenn(engine => { engine.AddMarkdownContent<T>(...) })`
3. Front matter: Remove unused interface members, or switch to capability interfaces
4. If using DocSite/BlogSite helpers: nearly identical — options class updated

### What Custom Content Service Authors Change

Custom `IContentService` implementations (like RecipeExample) require moderate migration:

| v1 | v2 |
|----|-----|
| `GetPagesToGenerateAsync()` → `ImmutableList<PageToGenerate>` | `DiscoverAsync()` → `IAsyncEnumerable<DiscoveredItem>` |
| `new PageToGenerate(url, outputFile, metadata)` | `new DiscoveredItem(route, new ProgrammaticSource(generator))` |
| URL computed as string | URL via `ContentRouteFactory.FromCustom(url, sourceFile)` |
| Rendering via HTTP route handler | Rendering via `IProgrammaticContentGenerator.GenerateAsync()` |
| Error handling: try/catch → skip | Error handling: return `new FailedItem(route, error)` |

For binary content services (responsive images), return `BinaryProgrammaticContent` from `GenerateAsync()` — the pipeline pattern-matches the `ProgrammaticContent` union and skips Parse/Render for binary cases.

### What Users Don't Change

- All `.md` files
- All YAML front matter
- All `_redirects.yml` files
- All `_index.metadata.yml` files
- All Razor layout/page components (minor namespace changes)
- All `dotnet run -- build` commands
- All `dotnet watch` workflows

---

## Appendix A. C# 15 Union Types — Developer Reference

> **Why this appendix exists:** C# 15 union types ship with .NET 11. LLM training data and most developer experience predate this feature. This section is the canonical reference for Penn contributors. See also: [C# 15 Union Types — .NET Blog](https://devblogs.microsoft.com/dotnet/csharp-15-union-types/).

### Syntax

Unions wrap **existing types** — they don't declare inline cases. Define case types first, then declare the union:

```csharp
// 1. Define case types as records (or classes, structs — any type works)
public record Cat(string Name);
public record Dog(string Name);
public record Bird(string Name);

// 2. Declare the union over those types
public union Pet(Cat, Dog, Bird);
```

The compiler generates a struct with:
- A constructor for each case type enabling **implicit conversions**
- A `Value` property of type `object?` holding the underlying value

### Implicit Conversion

Assigning a case type to a union variable just works — no cast, no wrapping:

```csharp
Pet pet = new Dog("Rex");          // implicit conversion: Dog → Pet
ContentItem item = new FailedItem(route, error);  // implicit conversion: FailedItem → ContentItem
```

### Pattern Matching and Exhaustiveness

Pattern matching **automatically unwraps** the union's `Value` property. Write the case type directly:

```csharp
string description = pet switch
{
    Dog d  => $"Dog: {d.Name}",
    Cat c  => $"Cat: {c.Name}",
    Bird b => $"Bird: {b.Name}",
};
// Compiler enforces all three cases. No default/discard needed.
// If you add a fourth case type to the union, every incomplete switch gets a warning.
```

This is the primary value proposition for Penn: **the compiler enforces that every pipeline stage, every diagnostic severity, and every content source type is handled everywhere they're consumed.** Adding a new pipeline stage is a one-line union change that produces warnings at every incomplete handler.

### Cases Are NOT Nested Types

This is the most common mistake. You **cannot** write:

```csharp
ContentItem.Discovered   // ❌ NOT VALID — Discovered is not a member of ContentItem
BuildDiagnostic.Error    // ❌ NOT VALID — Error is not a member of BuildDiagnostic
```

Use the record type name directly:

```csharp
DiscoveredItem           // ✅ the record type
DiagnosticError          // ✅ the record type
```

### Null and Default

The default value of a union struct has `Value == null`:

```csharp
ContentItem item = default;  // item.Value is null
```

When a union variable could be default-initialized (e.g., array elements, out parameters), add a `null` arm:

```csharp
item switch
{
    DiscoveredItem d => ...,
    ParsedItem p     => ...,
    RenderedItem r   => ...,
    FailedItem f     => ...,
    null => throw new InvalidOperationException("Uninitialized ContentItem")
};
```

For non-nullable locals assigned at declaration, the `null` arm is not required.

### Union Members

Unions can declare methods and properties in a body. Use this for shared properties across cases:

```csharp
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)
{
    public ContentRoute Route => Value switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p     => p.Route,
        RenderedItem r   => r.Route,
        FailedItem f     => f.Route,
        null => throw new InvalidOperationException()
    };
}
```

### Implementation Detail: Boxing

Union structs store their contents as a single `object?` reference. Value types are boxed. All Penn union cases are records (reference types), so **no boxing overhead** applies in this codebase.

### Penn's Union Types at a Glance

| Union | Cases | Purpose |
|-------|-------|---------|
| `ContentItem` | `DiscoveredItem`, `ParsedItem`, `RenderedItem`, `FailedItem` | Pipeline stages — exhaustive matching prevents silent drops |
| `ContentSource` | `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource` | Content origin — dispatch to correct processor |
| `BuildDiagnostic` | `DiagnosticInfo`, `DiagnosticWarning`, `DiagnosticError` | Severity levels — structured reporting |
| `LinkCheckResult` | `ValidLink`, `BrokenLink`, `ExternalLink` | Link verification outcomes |
| `ProgrammaticContent` | `TextProgrammaticContent`, `BinaryProgrammaticContent` | Text vs binary content — eliminates nullable ambiguity |

---
title: "Optimizing CSS and JavaScript"
description: "How Penn generates CSS at runtime with MonorailCSS and options for production optimization"
uid: "penn.guides.optimizing-css-and-javascript"
order: 2300
---

Penn generates CSS at runtime by observing which utility classes appear in your rendered HTML. There is no `tailwind.config.js`, no PurgeCSS step, and no build-time CSS toolchain. MonorailCSS scans every response, accumulates the class names it finds, and generates a stylesheet containing only the styles your site actually uses.

This article covers the internals of that pipeline, how the static site build ensures the stylesheet is complete, and what production optimization looks like when the framework handles purging for you.

## How Penn Generates CSS

Three components form the CSS generation pipeline. Each has a single responsibility, and they communicate through a shared class name registry.

### CssClassCollector: The Accumulator

`CssClassCollector` is a singleton that stores every CSS class name discovered across all requests. It uses a static `HashSet<string>` protected by a `ReaderWriterLockSlim`, so multiple request threads can read the current set while a single writer adds new entries.

Classes accumulate and are never cleared at runtime. Stale entries from deleted content are harmless -- MonorailCSS ignores tokens it does not recognize as utility classes. On the next static build, the collector starts fresh because the application process restarts.

### CssClassCollectorProcessor: The Observer

`CssClassCollectorProcessor` implements `IResponseProcessor` and runs inside Penn's `ResponseProcessingMiddleware`. It reads every HTML and JSON response body, extracts class attribute values with a regex, and registers the discovered names with the collector.

The processor never modifies the response body. It observes and moves on.

Two details matter here. First, the processor's `Order` is 100, which places it after `BaseUrlRewritingProcessor` at Order 0. By the time CSS classes are extracted, URL rewriting has already happened, so the processor sees the final HTML. Second, the processor accepts `application/json` responses in addition to `text/html`. This is how SPA island content gets scanned -- more on that below.

### MonorailCssService: The Generator

`MonorailCssService` reads a snapshot of all collected class names from `CssClassCollector.GetClasses()`, builds a `CssFramework` instance from the configured theme and options, and calls `cssFramework.Process(classNames)` to produce the stylesheet. The result includes any raw CSS from `MonorailCssOptions.ExtraStyles` prepended to the generated utility classes.

The stylesheet is served at `/styles.css` by default (configurable in `UseMonorailCss()`):

```csharp
app.MapGet("/styles.css", (MonorailCssService cssService) =>
    Results.Content(cssService.GetStyleSheet(), "text/css"));
```

Every request to `/styles.css` regenerates the stylesheet from the current snapshot. During development this means the stylesheet reflects any new classes discovered since the last request. During a static build, the stylesheet is fetched exactly once, after all other pages have been crawled.

## Processing Order in the Middleware Chain

Penn's `ResponseProcessingMiddleware` captures the response body into a `MemoryStream`, then runs all registered `IResponseProcessor` implementations in ascending `Order`. The processors relevant to CSS generation are:

| Processor | Order | Role |
|---|---|---|
| `BaseUrlRewritingProcessor` | 0 | Rewrites root-relative URLs for subdirectory deployments |
| `CssClassCollectorProcessor` | 100 | Extracts CSS class names from responses |

This ordering means the collector sees rewritten HTML. If a component renders `class="bg-primary-600"`, that class name is captured regardless of whether the base URL was prepended to neighboring `href` attributes.

For JSON responses (SPA island data), the processor unescapes Unicode sequences before scanning. `JavaScriptEncoder.Default` encodes `"` as `\u0022` in JSON strings, which would prevent the `class="..."` regex from matching. The processor converts these back to literal characters first, then runs the same extraction logic.

## Startup Content Scanning

Some CSS classes never appear in server-rendered HTML. If your client-side JavaScript constructs DOM elements with utility classes -- for example, a search results component that adds `bg-primary-100` dynamically -- the response processor will never see those classes during normal page rendering.

`MonorailCssOptions.ContentPaths` solves this. It accepts an array of file paths relative to your web root:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ContentPaths = ["js/spa-engine.js", "js/search.js"]
});
```

When `UseMonorailCss()` is called, it scans each listed file before any requests are served. The scanner uses two extraction strategies:

1. **HTML class attribute regex** -- finds `class="..."` patterns, which catches classes in template literals and HTML strings embedded in JavaScript.
2. **Broad token split** -- splits the entire file on whitespace and delimiter characters, treating every resulting token as a potential class name. This catches classes stored as bare strings, like `const HIGHLIGHT = 'bg-primary-500/20';`.

False positives from the broad split are harmless. MonorailCSS processes only tokens it recognizes as valid utility classes and silently discards everything else.

## The Build Pipeline

When you run `dotnet run -- build`, Penn's `OutputGenerationService` generates a static site by making HTTP requests to itself. The order in which pages are fetched determines whether the stylesheet is complete.

The build executes nine phases. The phases relevant to CSS are:

**Phase 1-2: Discovery.** The service collects all content pages from registered `IContentService` implementations, then discovers all `MapGet` routes (including `/styles.css`) from the ASP.NET endpoint metadata.

**Phase 3-5: Preparation.** The output directory is cleared and recreated. Static assets are copied. Dynamic content files are generated.

**Phase 6: Fetch HTML pages.** All content pages are fetched in parallel using `Parallel.ForEachAsync`. Each response flows through `ResponseProcessingMiddleware`, which triggers `CssClassCollectorProcessor`. By the end of this phase, the collector contains every CSS class used in every page of the site.

**Phase 7: Fetch MapGet routes last.** Routes like `/styles.css`, `/search-index.json`, and `/sitemap.xml` are fetched after all HTML pages. When the stylesheet endpoint is hit, `MonorailCssService.GetStyleSheet()` reads the complete set of collected classes and generates a stylesheet that covers the entire site.

**Phase 8-9: Cleanup.** A 404 page is generated, and internal links are verified across all fetched HTML.

This ordering is handled automatically by `OutputGenerationService`. You do not need to configure fetch order or worry about the stylesheet being generated too early.

## SPA Navigation and CSS

Penn's SPA navigation system fetches page content as JSON envelopes rather than full HTML pages. When a user clicks a navigation link, the SPA engine requests a `/_spa-data/` endpoint that returns a `SpaEnvelopeDto` containing HTML fragments for each island on the page.

These JSON responses contain HTML with CSS classes that may not appear in any full-page render. `CssClassCollectorProcessor` handles this by accepting `application/json` content types in its `ShouldProcess` check. Before extracting classes, it unescapes JSON string encoding:

- `\u0022` is converted back to `"`
- `\"` is converted back to `"`

This allows the `class="..."` regex to match class attributes inside JSON-encoded HTML fragments.

During development, the behavior works like this: when a user navigates to a page via SPA for the first time, any new CSS classes in that page's islands are added to the collector. The next request to `/styles.css` includes them. There may be a brief flash of unstyled content on the first SPA navigation to a page with previously unseen classes. A full page refresh resolves it, and subsequent SPA navigations to the same page are styled correctly.

During a static build, this is not an issue. Phase 6 fetches all HTML pages (which triggers island rendering and CSS collection), and Phase 7 generates the stylesheet with the complete class set.

## Production Optimization

Penn's generated CSS is already "purged" by design -- only classes that appear in your content are included in the stylesheet. The main optimization opportunity is minification.

### Post-Build Minification

After running the static build, minify the output files. [tdewolff/minify](https://github.com/tdewolff/minify) is a standalone binary that handles CSS, JavaScript, and HTML:

```bash
# Build the static site
dotnet run -- build

# Minify CSS
minify -o output/styles.css output/styles.css

# Minify JavaScript files
find output -name "*.js" -exec minify -o {} {} \;

# Optionally minify HTML
find output -name "*.html" -exec minify --type html -o {} {} \;
```

In a GitHub Actions workflow:

```yaml
- name: Build static site
  run: dotnet run --project docs/MyDocs -- build

- name: Install minifier
  run: |
    curl -sL https://github.com/tdewolff/minify/releases/latest/download/minify_linux_amd64.tar.gz | tar xz
    sudo mv minify /usr/local/bin/

- name: Minify output
  run: |
    minify -r -o output/ --match "\.css$|\.js$|\.html$" output/
```

Typical reductions are 20-30% for CSS and 15-25% for JavaScript. HTML minification yields smaller gains but adds up across many pages.

### ExtraStyles and Framework Customization

If you add raw CSS through `MonorailCssOptions.ExtraStyles`, that CSS is included verbatim in the generated stylesheet. Keep it minimal -- font-face declarations, keyframe animations, and CSS custom properties are good candidates. Large blocks of hand-written CSS defeat the purpose of utility-first generation.

For component-level styling, prefer the `Applies` dictionary in `CustomCssFrameworkSettings`, which maps CSS selectors to utility class strings. MonorailCSS processes these alongside the collected classes, so they benefit from the same generation pipeline. See [MonorailCSS Configuration](xref:penn.reference.monorail-css-configuration) for the full API.

## Next Steps

- [Configure Custom Styling](xref:penn.guides.configure-custom-styling) -- theme colors, extra styles, and framework overrides
- [MonorailCSS Configuration](xref:penn.reference.monorail-css-configuration) -- full API for `MonorailCssOptions`, color schemes, and applies
- [Dev vs. Deployment Architecture](xref:penn.under-the-hood.dev-vs-deployment-architecture) -- how runtime and static-build modes differ

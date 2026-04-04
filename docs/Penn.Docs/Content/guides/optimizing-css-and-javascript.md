---
title: "Optimizing CSS and JavaScript"
description: "How Penn generates CSS at runtime with MonorailCSS, and what you can do about asset optimization"
uid: "penn.guides.optimizing-css-and-javascript"
order: 2300
---

Penn generates CSS at runtime. This sounds alarming until you understand why: the stylesheet contains only the utility classes actually used in your content. No purging step, no PurgeCSS configuration, no "why is my production build missing styles" debugging sessions. The tradeoff is that CSS generation is a runtime concern, which means understanding how it works is useful if you want to optimize for production.

> [!IMPORTANT]
> The optimization techniques in this guide are optional. Penn sites are fast out of the box. These techniques shave kilobytes, not seconds. Apply them if you care about that sort of thing.

## How Penn Generates CSS

### The Collection Phase

Penn uses MonorailCSS (a Tailwind-like utility CSS framework) with a runtime class collection pipeline:

1. **`CssClassCollectorProcessor`** is an `IResponseProcessor` that scans every HTML and JSON response for `class="..."` attributes. It extracts CSS class names and registers them with the `CssClassCollector`.

2. **`CssClassCollector`** is a thread-safe singleton that accumulates all discovered class names across requests. It uses a `ReaderWriterLockSlim` for concurrent access.

3. **`MonorailCssService`** reads the collected classes and generates a stylesheet via the MonorailCSS framework engine.

4. The stylesheet is served at `/styles.css` (configurable via `UseMonorailCss(path)`).

### Processing Order Matters

`CssClassCollectorProcessor` has `Order = 100`, which means it runs *after* `BaseUrlRewritingProcessor` (Order = 0). This is intentional -- the collector sees the final HTML after URL rewriting, ensuring it captures classes from the fully processed output.

The processor never modifies the response body. It only observes. This is a read-only observer pattern, not a transform.

### JSON Response Handling

SPA navigation returns JSON responses containing HTML strings with escaped quotes. The `CssClassCollectorProcessor` handles this by unescaping `\u0022` and `\"` sequences before scanning for class attributes. Without this step, CSS classes in SPA island content would be invisible to the collector, and your SPA pages would have unstyled content. That would be bad.

### Startup Content Scanning

MonorailCSS also supports scanning static files at startup via `MonorailCssOptions.ContentPaths`:

```csharp
services.AddMonorailCss(sp => new MonorailCssOptions
{
    ContentPaths = ["scripts/app.js"],  // Paths relative to wwwroot
});
```

This catches CSS classes that only appear in client-side JavaScript (like dynamically constructed class strings). The `MonorailServiceExtensions.ScanContentFiles()` method reads each file, extracts potential class names using two strategies:

1. **HTML class attribute extraction**: Regex matching `class="..."` patterns
2. **Broad token extraction**: Splits on delimiters and keeps everything. False positives are harmless -- MonorailCSS ignores tokens it doesn't recognize as utility classes.

### The Color System

MonorailCSS supports two color scheme approaches:

**Named colors** -- map existing Tailwind palette names to semantic roles:

```csharp
ColorScheme = new NamedColorScheme
{
    PrimaryColorName = ColorNames.Blue,
    AccentColorName = ColorNames.Purple,
    TertiaryOneColorName = ColorNames.Cyan,
    TertiaryTwoColorName = ColorNames.Pink,
    BaseColorName = ColorNames.Slate,
}
```

**Algorithmic colors** -- generate palettes from a single hue value:

```csharp
ColorScheme = new AlgorithmicColorScheme
{
    PrimaryHue = 220,
    BaseColorName = ColorNames.Gray,
}
```

The algorithmic approach derives accent and tertiary hues automatically (complementary, split-complementary). Good for rapid prototyping. The named approach gives you full control.

## The Build Pipeline

During static site generation (`dotnet run -- build`), the CSS pipeline works like this:

1. The app starts and `UseMonorailCss()` scans any configured `ContentPaths`.
2. `OutputGenerationService` crawls every discovered page via HTTP.
3. Each response passes through `ResponseProcessingMiddleware`, which invokes `CssClassCollectorProcessor`.
4. The collector accumulates classes from all pages.
5. When the build requests `/styles.css`, `MonorailCssService` generates the final stylesheet from all collected classes.
6. The stylesheet is written to the output directory.

This means the build order matters. Pages must be crawled *before* the stylesheet is fetched, or the stylesheet will be incomplete. Penn handles this automatically -- `OutputGenerationService` crawls pages first, then copies static assets.

## Production Optimization

### Post-Build Minification

After static generation, you can minify CSS and JavaScript with any tool you like. Here's an example using [tdewolff/minify](https://github.com/tdewolff/minify) in a GitHub Actions workflow:

```yaml
- name: Generate static site
  run: |
    dotnet run --project ./src/MyDocs/MyDocs.csproj \
      --configuration Release -- build "/my-project/"

- name: Install minify
  run: |
    curl -sfL https://github.com/tdewolff/minify/releases/latest/download/minify_linux_amd64.tar.gz \
      | tar -xzf - -C /tmp
    sudo mv /tmp/minify /usr/local/bin/

- name: Minify assets
  run: |
    find ./src/MyDocs/output -type f -name "*.css" -exec /usr/local/bin/minify -o {} {} \;
    find ./src/MyDocs/output -type f -name "*.js" -exec /usr/local/bin/minify -o {} {} \;
```

Typical reductions:

- **CSS**: 20-30% smaller (whitespace, comments, shorthand optimization)
- **JavaScript**: 15-25% smaller (whitespace, comments, variable shortening)

### Extra Styles

If you need CSS that MonorailCSS can't express as utility classes, use the `ExtraStyles` option:

```csharp
services.AddMonorailCss(sp => new MonorailCssOptions
{
    ExtraStyles = """
        @font-face {
            font-family: 'CustomFont';
            src: url('/fonts/custom.woff2') format('woff2');
        }

        .custom-gradient {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        """,
});
```

Extra styles are prepended to the generated stylesheet. They're not processed by MonorailCSS -- they're raw CSS.

### Custom Framework Settings

For advanced MonorailCSS customization, use `CustomCssFrameworkSettings`:

```csharp
services.AddMonorailCss(sp => new MonorailCssOptions
{
    CustomCssFrameworkSettings = settings => settings with
    {
        // Customize the framework settings here
    },
});
```

## SPA Navigation and CSS

SPA navigation creates a subtle CSS problem: when new island content is injected, it may contain CSS classes that weren't present in any previously rendered page. The `CssClassCollectorProcessor` handles this because it scans JSON responses too (the SPA data endpoint returns JSON with embedded HTML). During the build, all SPA data endpoints are crawled, so the final stylesheet includes classes from all possible island content.

During development, new classes in SPA content are collected on-the-fly. The stylesheet at `/styles.css` is regenerated per-request, so it always reflects the current set of collected classes. This means you might see a brief flash of unstyled content on the first SPA navigation to a page with new utility classes -- refreshing resolves it. In production (static output), this doesn't happen because all pages are pre-crawled.

## Summary

Penn's CSS pipeline is: collect classes from responses, generate a stylesheet from those classes, serve it. No build step, no configuration file, no `tailwind.config.js`. The MonorailCSS integration handles the runtime generation, and the response processor pipeline ensures classes from both HTML and JSON responses are captured.

For production, the generated CSS is already purged by nature (only used classes are included). Post-build minification is the main optimization lever, and it's a straightforward post-processing step you can add to any CI/CD pipeline.

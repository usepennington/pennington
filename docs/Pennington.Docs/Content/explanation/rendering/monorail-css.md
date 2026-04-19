---
title: "MonorailCSS integration"
description: "Why Pennington scans classes off the wire during the response and generates the stylesheet on demand instead of pre-building a static CSS file."
uid: explanation.rendering.monorail-css
order: 302010
sectionLabel: "Rendering and Theming"
tags: [monorail, css, theming, oklch]
---

Utility-first CSS normally needs a build step that scans source files and regenerates a stylesheet — so how does Pennington emit a correct stylesheet when there is no `npm run build` in the loop and new classes can appear the instant someone edits a markdown file?

## Context

Utility-first CSS frameworks like Tailwind and MonorailCSS ship a vast class surface and rely on a scanner to collect only the classes in use, keeping the final stylesheet small. Traditional setups solve this with a pre-build step that globs source files. That model fights a dotnet-watch content engine in two ways: markdown is rendered at runtime through Markdig extensions, so classes do not exist on disk until a request renders them, and adding a Razor component or a new page would require rerunning a separate tool.

Pennington's answer is to move class collection onto the response path. Every HTML or JSON response flows through a processor that extracts class attributes, and the `/styles.css` endpoint rebuilds the stylesheet from that accumulated set on demand. Because dev-serve and the static-build crawler run the same HTTP pipeline, the crawl naturally observes every class the site emits — no separate build integration needed.

## How it works

### Classes are collected at response time

`CssClassCollectorProcessor` is an `IResponseProcessor` registered by `AddMonorailCss` alongside the core response-processing pipeline. Its `ShouldProcess` hook matches `text/html` and `application/json` — the latter catches SPA envelope payloads, which contain HTML with escaped quotes, so the processor unescapes `\u0022` and `\"` before scanning. For every matching response it runs a regex over `class="..."` attributes, splits and HTML-decodes the tokens, and hands the result to a shared `CssClassCollector`. Critically, it never rewrites the body; it is a pure observer.

The reason this matters is that any mutation of the response body would interact badly with downstream processors and with content-length headers. Keeping the processor strictly read-only means it can sit anywhere in the chain without coordination overhead.

Three details about `CssClassCollectorProcessor` are worth holding onto. The `Order => 100` places it after the rewriting processors, so it observes final HTML rather than a pre-transform draft — if a processor upstream adds a class, this one sees it. The collector is keyed by string, so duplicates across pages cost nothing beyond a hash lookup. And the processor emits a trace log per response, which means "why is my class missing?" debugging usually ends at a log line rather than a breakpoint.

The shared `CssClassCollector` is a singleton protected by a `ReaderWriterLockSlim`, and it deliberately never clears at runtime — stale classes are harmless because MonorailCSS ignores tokens it does not recognize, and a fresh build starts from an empty set anyway.

### The stylesheet generates on demand

`UseMonorailCss` maps a `GET /styles.css` endpoint that calls `MonorailCssService.GetStyleSheet()`. Each hit snapshots the current collector, instantiates a `CssFramework` with the configured theme, and calls `Process` to emit CSS. Because this runs on every request, the stylesheet is always in sync with what has been seen. The dev-serve loop re-fetches `/styles.css` on every page load, so the browser always picks up whatever classes the rest of the app has registered. In static-build mode, the crawl visits every discovered route before it fetches `/styles.css`, so the collector is fully warm by the time the stylesheet snapshot is written to disk.

The on-demand approach has a pleasant property: there is no configuration file mapping source paths to output. The stylesheet's contents are always a direct consequence of what the HTTP pipeline emitted, which makes the system self-auditing — if a class appears in the stylesheet, it was in a response.

The edge case that `ContentPaths` exists to solve is classes that live only in client-side JavaScript — a template literal in a search-modal script, for instance. These never appear in an HTML `class=` attribute, so the response-processor pass cannot see them. `MonorailCssOptions.ContentPaths` accepts a list of static-file paths that `UseMonorailCss` scans once at startup with a broader token-extractor, seeding the collector before the first request arrives. See <xref:reference.api.monorail-css-options> for the full options surface.

`ColorScheme` on `MonorailCssOptions` is the hook that feeds the MonorailCSS theme, and it comes in two distinct flavors.

### Color schemes: named vs algorithmic

A `NamedColorScheme` maps three role names — `primary`, `accent`, `base` — onto built-in MonorailCSS palettes by name (for example, `Blue` or `Slate`). It is the default, and it suits sites where the designer thinks in terms of recognizable Tailwind-style palettes and picks by name rather than by hue.

An `AlgorithmicColorScheme` takes a single `PrimaryHue` number in degrees plus a `ColorSchemeGenerator` function — defaulting to the complementary hue — and synthesizes `primary` and `accent` from scratch. This second shape is what makes "my brand color is hue 214, derive everything from that" viable without hand-authoring a full palette table.

The two schemes are not a legacy/modern pair or a simple/advanced pair — they occupy different spots on the designer-versus-programmer axis. Named is the choice when a designer says "I want Tailwind Purple for primary"; Algorithmic is the choice when the starting point is a brand hue expressed in degrees and the palette needs to be coherent rather than cherry-picked.

Syntax-highlight colors are deliberately kept off the brand scheme. `SyntaxTheme` on `MonorailCssOptions` holds the five roles `.hljs-*` token classes consume — keyword, string, variable, function, and comment — each mapped to its own Tailwind palette. The default picks a tuned combination (Sky / Emerald / Rose / Amber / Slate) that reads well against either a light or dark code background, so a site can pick primary and accent purely for brand reasons without constraining how code renders.

### OKLCH palette generation

The algorithmic scheme delegates to `ColorPaletteGenerator.GenerateFromHue`, which produces a dictionary keyed by the familiar `50` through `950` shade names. The generator walks three tables in parallel — a chroma curve, a lightness curve, and a palette-key list — and emits one OKLCH color per shade. Two nonlinearities make the output match the hand-tuned Tailwind v4 reference: a per-hue lightness adjustment (yellows and greens are lighter in the mid-range to match perceptual brightness) and a per-hue chroma multiplier (yellows sit at 78% of base chroma because a full-chroma yellow reads as lemon highlighter rather than a design color). A hue-shift curve also rotates the darker shades slightly toward an adjacent family — blues drift toward violet, reds toward orange — because that is what Tailwind's designers do by eye.

The key property that earns OKLCH its spot here is perceptual uniformity. Stepping lightness by a fixed amount produces steps that look evenly spaced regardless of hue, which is not true of HSL — a 500-weight green at HSL lightness 40% looks brighter than a 500-weight blue at the same value. OKLCH makes the generated scheme feel visually coherent without per-hue handwork from the caller, which is what makes "give me a palette from hue 214" a reasonable thing to ask.

`ColorPaletteGenerator.GenerateFromHue` is a plain static method, so nothing in the color story depends on DI — a test or a small designer tool can call it directly to preview a palette before committing to a hue.

## Trade-offs

- **The stylesheet is stale until a page renders.** If nothing has hit `/new-page` yet, the classes on `/new-page` are not in the collector, so a client that fetches only `/styles.css` sees an incomplete file. Dev-serve hides this because the browser always loads the HTML first; the static build hides it because the crawler visits every route before it fetches the stylesheet. The invariant is real, though — a new class requires a render that observes it.
- **A pre-build scan of source files was considered and rejected.** A Tailwind-style scanner that globs `.razor` and `.md` would not reflect what the site emits, because markdown is rendered through extension pipelines at request time. The source files are not the output. Scanning rendered HTML is a superset of scanning source, and it costs nothing extra because the response is already passing through the processor chain.
- **Classes accumulate for the lifetime of a dev-serve session.** The collector is append-only across requests. Over a long `dotnet watch` session a class deleted from a page still lives in the stylesheet until the host restarts. This is an explicit trade: runtime clears would race with in-flight requests, and the penalty for keeping a stale class is a few hundred bytes of CSS rather than a correctness issue.
- **`ContentPaths` is a narrow escape valve, not a general mechanism.** The response pass cannot see classes that live only in client scripts, so that is the one place where a source-file scan remains. The tradeoff here is that it runs once at startup — so a class added to a JS file after the host starts will not be picked up until the next restart, whereas a class added to a rendered template is observed on the next request.

## Further reading

- Reference: [`MonorailCssOptions`](xref:reference.api.monorail-css-options) — the full option surface with defaults.
- How-to: [Customize MonorailCSS](xref:how-to.configuration.monorail-css) — swapping schemes, injecting `CustomCssFrameworkSettings`, and wiring `ContentPaths`.
- External: [MonorailCSS upstream documentation](https://monorailcss.com/) — TODO confirm the canonical MonorailCSS docs URL before publish; currently a placeholder.

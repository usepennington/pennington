---
title: "MonorailCSS integration"
description: "Why Pennington scans classes off the wire during the response and generates the stylesheet on demand instead of pre-building a static CSS file."
uid: explanation.rendering.monorail-css
order: 10
sectionLabel: "Rendering and Theming"
tags: [monorail, css, theming, oklch]
---

> **In this page.** _Paraphrase the Covers line: classes are collected from HTML and JSON responses by a response processor, the stylesheet endpoint generates on demand from the accumulated set, and color schemes come in two flavors — a named mapping and an algorithmic one that builds OKLCH palettes from hue. One sentence._
>
> **Not in this page.** _Paraphrase the Does-not-cover line: point readers who want to learn MonorailCSS's own utility syntax at the upstream MonorailCSS documentation — this page is about how Pennington wires it up, not what classes to type. One sentence._

## The question

_Ask the reader's question in one sentence, something like: "Utility-first CSS normally needs a build step that scans source files and regenerates a stylesheet — so how does Pennington emit a correct stylesheet when there is no `npm run build` in the loop and new classes can appear the instant someone edits a markdown file?" Do not answer yet; the rest of the page is the answer._

## Context

_Three to five sentences. The tension: utility-first CSS frameworks (Tailwind and MonorailCSS alike) ship a vast class surface and rely on a scanner to collect only the classes you actually use, so the final stylesheet stays small. Traditional setups solve this with a pre-build step that globs source files. That model fights a dotnet-watch content engine in two ways — markdown is rendered at runtime through Markdig extensions, so classes literally do not exist on disk until a request renders them, and adding a Razor component or a custom class in a new page would require rerunning a separate tool. Pennington's answer is to move class collection onto the response path: every HTML or JSON response flows through a processor that extracts class attributes, and the `/styles.css` endpoint rebuilds the stylesheet from that collector on demand. Finish the paragraph by noting that because dev-serve and the static-build crawler run the same HTTP pipeline, the crawl naturally observes every class the site emits — no separate build integration needed._

## How it works

### Classes are collected at response time

_Two paragraphs. Describe the processor: `CssClassCollectorProcessor` is an `IResponseProcessor` registered by `AddMonorailCss` alongside the core response-processing pipeline. Its `ShouldProcess` hook matches `text/html` and `application/json` (the latter catches SPA envelope payloads, which contain HTML with escaped quotes — the processor unescapes `\u0022` and `\"` before scanning). For every matching response it runs a regex over `class="..."` attributes, splits and HTML-decodes the tokens, and hands the result to a shared `CssClassCollector`. Critically, it never rewrites the body — it is a pure observer._

```csharp:xmldocid
T:Pennington.MonorailCss.CssClassCollectorProcessor
```

_After the fence, point out three details: the `Order => 100` puts it after the rewriting processors (so it observes final HTML, not the pre-transform draft), the collector itself is keyed by string so duplicates across pages cost nothing, and the processor emits a trace log per response so "why is my class missing?" debugging stays cheap._

```csharp:xmldocid
T:Pennington.MonorailCss.CssClassCollector
```

_One sentence after the fence: the collector is a singleton with a `ReaderWriterLockSlim`, and it deliberately never clears at runtime — stale classes are harmless because MonorailCSS ignores tokens it does not recognize, and a fresh build (the site's next crawl) starts from an empty set anyway._

### The stylesheet generates on demand

_Two paragraphs. `UseMonorailCss` maps a `GET /styles.css` endpoint that calls `MonorailCssService.GetStyleSheet()`. Each hit snapshots the current collector, instantiates a `CssFramework` with the configured theme and applies, and calls `Process` to emit CSS. Because this runs every request, the stylesheet is always in sync with what has been seen — and because the dev-serve loop re-fetches `/styles.css` on every page load, the browser always picks up whatever classes the rest of the app just registered. In static-build mode, the crawl visits every discovered route before it fetches `/styles.css`, so the collector is fully warm by the time the stylesheet snapshot is written to disk._

```csharp:xmldocid
M:Pennington.MonorailCss.MonorailCssService.GetStyleSheet
```

_After the fence, cover the edge case the `ContentPaths` option exists to solve: classes that live only in client-side JavaScript (for example, a template literal in a search-modal script) never appear in an HTML `class=` attribute, so the response-processor pass cannot see them. `MonorailCssOptions.ContentPaths` accepts a list of static-file paths that `UseMonorailCss` scans once at startup with a broader token-extractor, seeding the collector before the first request arrives._

```csharp:xmldocid
T:Pennington.MonorailCss.MonorailCssOptions
```

_One sentence after the fence connecting it to the next section: `ColorScheme` on this options type is the hook that feeds the MonorailCSS theme, and it comes in two flavors._

### Color schemes: named vs algorithmic

_Two short paragraphs. A `NamedColorScheme` maps five role names — `primary`, `accent`, `tertiary-one`, `tertiary-two`, `base` — onto built-in MonorailCSS palettes by name (e.g. `Blue`, `Slate`). It is the default, and it is the right choice when the site wants recognizable Tailwind-style palettes and the designer picks by name rather than by hue. An `AlgorithmicColorScheme` instead takes a single `PrimaryHue` number in degrees plus a `ColorSchemeGenerator` function (default: complement, split-complement-left, split-complement-right) and synthesizes all four non-base palettes from scratch. This second shape is what makes "my brand is hue 214" viable without hand-authoring a palette._

```csharp:xmldocid
T:Pennington.MonorailCss.NamedColorScheme
T:Pennington.MonorailCss.AlgorithmicColorScheme
```

_After the fence, make the mental-model point: the two schemes are not a legacy/modern pair — they occupy different spots on the designer-versus-programmer axis. Named is "I know I want Tailwind Purple"; Algorithmic is "my brand hue is 214, derive everything from that."_

### OKLCH palette generation

_Two paragraphs. The algorithmic scheme delegates to `ColorPaletteGenerator.GenerateFromHue`, which produces a dictionary keyed by the familiar `50` through `950` palette shade names. The generator walks three tables in parallel — a chroma curve, a lightness curve, and a palette-key list — and emits one OKLCH color per shade. Two nonlinearities make the output match the hand-tuned Tailwind v4 reference: a per-hue lightness adjustment (yellows and greens are lighter in the mid range to match perceptual brightness) and a per-hue chroma multiplier (yellows sit at 78% of base chroma because a full-chroma yellow looks like lemon highlighter). A hue-shift curve rotates the darker shades slightly toward an adjacent family — blues drift toward violet, reds toward orange — because that is what Tailwind's designers do by eye._

```csharp:xmldocid
T:Pennington.MonorailCss.ColorPaletteGenerator
```

_After the fence, note the key property that earns OKLCH its spot: it is a perceptually uniform color space, so stepping lightness by a fixed amount produces steps that look evenly spaced regardless of hue. A naive HSL palette drifts — 500-weight greens look brighter than 500-weight blues at the same L. OKLCH makes the generated scheme feel coherent without per-hue handwork from the caller._

```csharp:xmldocid
M:Pennington.MonorailCss.ColorPaletteGenerator.GenerateFromHue(System.Double)
```

_One sentence after the fence: the generator is a plain static method, so nothing in the color story depends on DI — a test or a designer-tool can call it directly to preview a palette before committing a hue._

## Trade-offs

_Three to four bullets. Name the real costs, not stylistic preferences._

- **Cost — the stylesheet is stale until a page actually renders.** _If nothing has hit `/new-page` yet, the classes on `/new-page` are not in the collector, so a hypothetical "fetch only /styles.css" client sees an incomplete file. Dev serve hides this because the browser always loads the HTML first; the static build hides it because the crawler visits every route before it fetches the stylesheet. But the invariant is real — a new class requires a render that observes it._
- **Alternative considered — pre-build scan of source files.** _A Tailwind-style scanner that globs `.razor` and `.md` was rejected because markdown is rendered through extension pipelines at request time, so the "source files" alone do not reflect what the site actually emits. Scanning rendered HTML is a superset of scanning source, and it costs nothing extra because the response is already passing through the processor chain._
- **Cost — classes accumulate forever at dev-serve time.** _The collector is append-only across requests. Over a long `dotnet watch` session a class you deleted from a page still lives in the stylesheet until the host restarts. This is an explicit trade: runtime clears would race with in-flight requests, and the penalty for keeping a stale class is a few hundred bytes of CSS, not a bug._
- **Consequence — `ContentPaths` is the safety valve for JS-only classes.** _The response pass cannot see classes that live only in client scripts, so the one piece of "source scanning" Pennington keeps is for those exact files. Treat it as a narrow escape hatch, not a preferred path — every class you can put in a rendered attribute belongs in a rendered attribute._

## Further reading

_Three cross-quadrant links, one per line. Do NOT include the sibling explanation link (highlighting) — that is auto-generated._

- Reference: [`MonorailCssOptions`](/reference/options/monorail-css-options/) — the full option surface with defaults.
- How-to: [Customize MonorailCSS](/how-to/configuration/monorail-css/) — swapping schemes, injecting `CustomCssFrameworkSettings`, and wiring `ContentPaths`.
- External: [MonorailCSS upstream documentation](https://monorailcss.com/) — TODO confirm the canonical MonorailCSS docs URL before publish; currently a placeholder.

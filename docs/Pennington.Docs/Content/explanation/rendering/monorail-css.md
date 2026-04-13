---
title: "MonorailCSS integration"
description: "How Pennington collects utility classes at response time, serves the stylesheet on demand, and builds color palettes from named or algorithmic OKLCH schemes."
section: "rendering"
order: 10
tags: []
uid: explanation.rendering.monorail-css
isDraft: true
search: false
llms: false
---

> **In this page.** How classes are collected at response time (`CssClassCollectorProcessor`), how the stylesheet is generated on demand, and the color-scheme model (named vs. algorithmic with OKLCH palette generation).
>
> **Not in this page.** MonorailCSS's own syntax — utility names, variants, arbitrary values — lives in the upstream MonorailCSS documentation.

## The question

Why does Pennington discover its CSS classes by scraping rendered responses instead of scanning source files ahead of time, and how does that choice shape the stylesheet endpoint and the color model?

## Context

- The utility-first bet: every screen is built from thousands of tiny classes; shipping all of them is a megabyte nobody pays for.
- Tailwind's answer is build-time source scanning with a configured `content` glob; that lives outside the ASP.NET request pipeline and requires a separate tool chain.
- Pennington already runs every page — dev serve and static build — through the same HTTP pipeline (`OutputGenerationService` fetches the live host; see the dev-vs-build unified-path invariant).
- That invariant made a second answer available: if every pixel passes through `ResponseProcessingMiddleware` anyway, the middleware is a perfectly good place to harvest classes. No source-file scanner, no build step, no second source of truth for what MonorailCSS should compile.
- The color model then follows from the same "one path" logic — palettes must resolve at render time, not at a static tool step.

## How it works

### Collecting classes at response time

- `CssClassCollectorProcessor` (`src/Pennington.MonorailCss/CssClassCollectorProcessor.cs`) is registered as `IResponseProcessor` with `Order => 100`, sitting after the HTML rewriters and dev-only injectors.
- `ShouldProcess` accepts `text/html` and `application/json` responses — the latter because SPA-navigation envelopes (`/_spa-data/*.json`) carry HTML fragments with escaped quotes.
- For JSON bodies it unescapes `\u0022` and `\"` before scanning so the regex can see real `class="..."` attributes.
- A compiled regex gathers every token inside every `class` attribute, HTML-decodes them, deduplicates, then hands the set to `CssClassCollector.AddClasses(url, classes)` under a write lock.
- The processor never mutates the body — it is observational. The comment at the bottom of `ProcessAsync` calls that out: "Never modify the response body — just observe."

```csharp:xmldocid
T:Pennington.MonorailCss.CssClassCollectorProcessor
```

### The collector as a process-wide set

- `CssClassCollector` (`src/Pennington.MonorailCss/CssClassCollector.cs`) holds a `static HashSet<string>` guarded by a `ReaderWriterLockSlim`. The storage is deliberately process-wide — one set per host, shared across every request.
- Classes accumulate. There is no "clear on hot reload" and no per-URL scoping (the `url` argument is informational). Stale entries are harmless because MonorailCSS silently ignores tokens that don't parse as utilities, and a fresh build process starts with an empty set.
- Trade-off noted in code: an earlier design tried per-URL tracking plus targeted invalidation, but the timing against live reload was fragile; the current design picks "always-on accumulation" over precision.

### Supplementing with content-path scans at startup

- Some classes never appear in any rendered response — e.g. strings baked into client-side JS or dynamic template literals. The runtime scraper can't see them.
- `UseMonorailCss` reads `MonorailCssOptions.ContentPaths`, loads each file from the `WebRootFileProvider`, and runs `ExtractPotentialClasses` against it (see `MonorailServiceExtensions.ScanContentFiles`).
- The extraction is intentionally greedy: a `class="..."` regex plus a token splitter on whitespace/punctuation. False positives are accepted; MonorailCSS filters them at compile time.
- This is the same role Tailwind's `content` glob plays, narrowed to files the observer truly cannot see.

### Generating the stylesheet on demand

- `UseMonorailCss(path = "/styles.css")` maps a single `GET` endpoint: `MonorailCssService.GetStyleSheet()` runs on every request to `/styles.css`.
- `GetStyleSheet` snapshots the collector, builds a fresh `CssFramework` — theme, applies dictionaries (code blocks, tabs, alerts, hljs, search modal), prose customization — and returns `options.ExtraStyles` concatenated with `cssFramework.Process(classes)`.
- No caching layer, no pre-compiled artifact. The stylesheet is always consistent with the most recent render: a class introduced by a page served 50ms ago shows up the next time `/styles.css` is requested.
- At static build time, `OutputGenerationService` fetches `/styles.css` like any other URL; because every page has already been fetched (and its classes collected), the final CSS file naturally contains the closure of everything the site actually used.

### Color schemes: named vs. algorithmic

- `IColorScheme.ApplyToTheme(Theme)` is the single seam between Pennington and the MonorailCSS palette system. Two built-ins ship.
- `NamedColorScheme` (`src/Pennington.MonorailCss/MonorailCssOptions.cs`) maps five existing Tailwind-named palettes (primary, accent, tertiary-one, tertiary-two, base) onto the theme via `MapColorPalette`. It is the zero-surprise default.
- `AlgorithmicColorScheme` takes a single `PrimaryHue` integer and derives the other hues (default: +180° accent, +90°/-90° tertiaries) before passing each through `ColorPaletteGenerator.GenerateFromHue`. It lets a site identify itself with one number.
- Consumers can supply any `IColorScheme` — examples/YogaStudioExample ships a hand-tuned `YogaColorScheme` that builds five palettes by literal value.

```csharp:xmldocid
M:YogaStudioExample.Models.YogaColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)
```

### OKLCH palette generation

- `ColorPaletteGenerator.GenerateFromHue` (`src/Pennington.MonorailCss/ColorPaletteGenerator.cs`) emits eleven shades (50, 100, 200, … 900, 950) as `oklch(L C H)` strings.
- OKLCH is chosen because lightness is perceptually uniform — stepping from shade 400 to 500 feels like the same jump across every hue, which HSL and sRGB do not deliver.
- The lightness and chroma curves are calibrated against Tailwind v4's actual palette: Gaussian-shaped chroma peaking around shades 500–600, lightness following a measured ramp from 97.1% to 25.8%.
- Per-hue corrections (`GetLightnessAdjustment`, `GetChromaMultiplier`, `CalculateHueShift`) compensate for the asymmetries OKLCH exposes — yellows are inherently lighter and less chromatic, blues need a rotation toward violet in their darkest shades to stay vivid. Without these, a flat hue-sweep produces muddy mid-tones.
- The result is that `AlgorithmicColorScheme { PrimaryHue = 25 }` yields a coherent, Tailwind-adjacent palette without hand-picking eleven shades.

## Trade-offs

- **Cost: initial-render cold classes.** The very first response that introduces a new utility is served before that class has a compiled rule — if a browser requested `/styles.css` immediately after `/new-page` and before the collector lock committed, it could miss a class. In practice dev serve and the build-time crawler both fetch the stylesheet after content, so the race is not observable.
- **Cost: process-wide state.** The static `HashSet` means a long-lived dev server accumulates classes from branches, experiments, and pages that no longer exist. This is deliberate — stale classes are harmless and flushing them correctly was worse than ignoring them — but it does mean the `/styles.css` emitted in dev can be larger than the one emitted by a fresh build.
- **Alternative considered: source-file globbing at startup.** This is the Tailwind model and was rejected because it forces a second truth-source (the glob) that drifts from the actual render pipeline — Razor components, Markdig-generated HTML, and island outputs can all introduce classes a glob misses. The content-path scan survives as a narrow escape hatch for files the observer genuinely cannot see.
- **Alternative considered: precomputed palettes.** Early iterations shipped hand-written Tailwind palettes only. Algorithmic generation was added because doc sites and blog sites benefit from a single knob — "make it orange" — without authors curating eleven OKLCH strings. OKLCH over HSL because HSL's non-uniform lightness produced palettes that looked good for red and flat for yellow.
- **Consequence: MonorailCSS must tolerate unknown tokens.** The collector's greedy regex and the startup scanner's token splitter both feed false positives into `CssFramework.Process`. The design relies on MonorailCSS silently dropping what it can't parse — a contract with the upstream library, not Pennington's own guarantee.

## Further reading

- Reference: [MonorailCssOptions](/reference/monorail-css/options) — every field, including `ContentPaths`, `CustomCssFrameworkSettings`, and `ExtraStyles`.
- How-to: [Configure an algorithmic color scheme](/how-to/styling/algorithmic-color-scheme) — pick a hue and wire it into `AddMonorailCss`.
- Explanation: [The unified dev-and-build pipeline](/explanation/core/dev-vs-build) — why every page, including `/styles.css`, flows through the same HTTP path.
- External: [OKLCH and perceptually-uniform color](https://evilmartians.com/chronicles/oklch-in-css-why-quit-rgb-hsl) — background on why this color space beats HSL for palette generation.

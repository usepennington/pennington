---
title: "MonorailCSS integration"
description: "Why Pennington discovers CSS classes by scanning compiled assemblies and watched source files instead of pre-building a static stylesheet."
uid: explanation.rendering.monorail-css
order: 302010
sectionLabel: "Rendering and Theming"
tags: [monorail, css, theming, oklch]
---

Utility-first CSS normally needs a build step that scans source files and regenerates a stylesheet — so how does Pennington emit a correct stylesheet when there is no `npm run build` in the loop and new classes can appear the instant someone edits a markdown file?

## Context

Utility-first CSS frameworks like Tailwind and MonorailCSS ship a vast class surface and rely on a scanner to collect only the classes in use, keeping the final stylesheet small. Traditional setups solve this with a pre-build step that globs source files. That model fights a dotnet-watch content engine in two ways: markdown is rendered at runtime through Markdig extensions, so classes do not exist on disk until a request renders them, and adding a Razor component or a new page would require rerunning a separate tool.

Pennington's answer is to lean on the `MonorailCss.Discovery` package. Discovery force-loads every non-BCL referenced assembly at startup, walks the IL for string literals that parse as utility candidates, and watches source files in development for live updates. The discovered set is exposed through an `IClassRegistry` whose `Version` token changes whenever a new class is observed. The `/styles.css` endpoint reads the registry and regenerates CSS only when that version moves, so repeated GETs hit a cache.

## How it works

### Classes are discovered by scanning compiled output

`AddMonorailCss` calls `services.AddMonorailClassDiscovery()`, which registers the runtime scanner. At startup the scanner enumerates every assembly the entry app references (skipping the BCL), force-loads each one if needed, and walks IL string literals through Pennington's configured `CssFramework` to keep only the candidates the framework actually recognises. That last step is wired through `IConfigureOptions<MonorailDiscoveryOptions>` in `MonorailServiceExtensions`: the same `CssFramework` instance that generates the stylesheet validates discovery candidates, so the theme is consistent across both halves of the pipeline.

Because the scan reads compiled IL rather than source text, every `class="bg-primary-500"` literal in a Razor component, every string constant in a C# helper, and every utility token in `Pennington.UI`'s shipped components participates without any per-project glob configuration. In development, Discovery also watches the source files behind the loaded assemblies and re-scans on edits, so a new utility added to a `.razor` or `.cs` file shows up on the next `/styles.css` fetch. If a `wwwroot/app.css` is present, Discovery treats it as the source CSS prefix.

### The stylesheet generates on demand and caches by version

`UseMonorailCss` maps a `GET /styles.css` endpoint that calls `MonorailCssService.GetStyleSheet()`. Each hit checks the `IClassRegistry.Version` token and returns the cached result when the registry hasn't moved; on a miss it pulls the current class set, calls `cssFramework.Process(classes)`, and prepends Pennington's content-visibility preamble plus any configured `ExtraStyles`. The cache is a process-local field guarded by a single lock, so the cost of a `/styles.css` fetch on a stable site is one comparison and one string return.

The version-keyed cache is what makes the dev-serve loop feel right. The first page load primes the registry with whatever classes that page emits; the browser then fetches `/styles.css` and gets a freshly-generated stylesheet. A subsequent navigation that introduces a new class moves the registry version, the next stylesheet fetch sees the bump, and only that fetch pays for regeneration.

### Color schemes: named vs algorithmic

`ColorScheme` on `MonorailCssOptions` is the hook that feeds the MonorailCSS theme, and it comes in two distinct flavors.

A `NamedColorScheme` maps three role names — `primary`, `accent`, `base` — onto built-in MonorailCSS palettes by name (for example, `Blue` or `Slate`). It is the default, and it suits sites where the designer thinks in terms of recognizable Tailwind-style palettes and picks by name rather than by hue.

An `AlgorithmicColorScheme` takes a single `PrimaryHue` number in degrees plus a `ColorSchemeGenerator` function — defaulting to the complementary hue — and synthesizes `primary` and `accent` from scratch. This second shape is what makes "my brand color is hue 214, derive everything from that" viable without hand-authoring a full palette table.

The two schemes are not a legacy/modern pair or a simple/advanced pair — they occupy different spots on the designer-versus-programmer axis. Named is the choice when a designer says "I want Tailwind Purple for primary"; Algorithmic is the choice when the starting point is a brand hue expressed in degrees and the palette needs to be coherent rather than cherry-picked.

Syntax-highlight colors are deliberately kept off the brand scheme. `SyntaxTheme` on `MonorailCssOptions` holds the five roles `.hljs-*` token classes consume — keyword, string, variable, function, and comment — each mapped to its own Tailwind palette. The default picks a tuned combination (Sky / Emerald / Rose / Amber / Slate) that reads well against either a light or dark code background, so a site can pick primary and accent purely for brand reasons without constraining how code renders.

### OKLCH palette generation

The algorithmic scheme delegates to `ColorPaletteGenerator.GenerateFromHue`, which produces a dictionary keyed by the familiar `50` through `950` shade names. The generator walks three tables in parallel — a chroma curve, a lightness curve, and a palette-key list — and emits one OKLCH color per shade. Two nonlinearities make the output match the hand-tuned Tailwind v4 reference: a per-hue lightness adjustment (yellows and greens are lighter in the mid-range to match perceptual brightness) and a per-hue chroma multiplier (yellows sit at 78% of base chroma because a full-chroma yellow reads as lemon highlighter rather than a design color). A hue-shift curve also rotates the darker shades slightly toward an adjacent family — blues drift toward violet, reds toward orange — because that is what Tailwind's designers do by eye.

The key property that earns OKLCH its spot here is perceptual uniformity. Stepping lightness by a fixed amount produces steps that look evenly spaced regardless of hue, which is not true of HSL — a 500-weight green at HSL lightness 40% looks brighter than a 500-weight blue at the same value. OKLCH makes the generated scheme feel visually coherent without per-hue handwork from the caller, which is what makes "give me a palette from hue 214" a reasonable thing to ask.

`ColorPaletteGenerator.GenerateFromHue` is a plain static method, so nothing in the color story depends on DI — a test or a small designer tool can call it directly to preview a palette before committing to a hue.

## Trade-offs

- **IL scanning sees only string literals.** Discovery extracts classes from constants the compiler emits, not from values built at runtime. Razor's `@Color`-style interpolation inside an inline `class=""` is invisible to the scanner because the concatenation happens during render, not at compile time. The fix is to enumerate the variants statically — `Pennington.UI.Components.CardColorClasses` does this with a per-color `switch` expression so every permutation lives in IL as a literal — or to leave the literal somewhere the scanner can see it (a Razor `@code` constant, a C# helper).
- **A pre-build scan of source files was considered and rejected.** A Tailwind-style scanner that globs `.razor` and `.md` would not catch utility classes that live in compiled `Pennington.*` assemblies the consumer never edits, and it would force every project to maintain its own glob list. Discovery's IL scan covers shipped libraries and consumer projects with the same mechanism.
- **Classes accumulate for the lifetime of the registry.** Discovery is append-only; a class deleted from a page still lives in the stylesheet until the registry is rebuilt (a host restart, or a watched-file edit triggering a rescan). This is an explicit trade — the penalty for keeping a stale class is a few hundred bytes of CSS rather than a correctness issue.

## Further reading

- Reference: [`MonorailCssOptions`](xref:reference.api.monorail-css-options) — the full option surface with defaults.
- How-to: [Customize MonorailCSS](xref:how-to.configuration.monorail-css) — swapping schemes, injecting `CustomCssFrameworkSettings`, and authoring extra styles.

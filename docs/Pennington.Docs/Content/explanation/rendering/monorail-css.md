---
title: "MonorailCSS integration"
description: "Why Pennington discovers CSS classes by scanning compiled assemblies and watched source files instead of pre-building a static stylesheet."
uid: explanation.rendering.monorail-css
order: 1
sectionLabel: "Rendering and Theming"
tags: [monorail, css, theming, oklch]
---

Utility-first CSS normally needs a build step that scans source files and regenerates a stylesheet — so how does Pennington emit a correct stylesheet when there is no `npm run build` in the loop and new classes can appear the instant someone edits a markdown file?

## Context

Utility-first CSS frameworks like Tailwind and [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) (the Tailwind-compatible .NET JIT compiler Pennington integrates) ship a vast class surface and rely on a scanner to collect only the classes in use, keeping the final stylesheet small. Traditional setups solve this with a pre-build step that globs source files. That model fights a runtime-rendering content engine in two ways: markdown is rendered at runtime through Markdig extensions, so classes do not exist on disk until a request renders them, and adding a Razor component or a new page would require rerunning a separate tool.

Pennington's answer is to lean on the `MonorailCss.Discovery` package. Discovery force-loads every non-BCL referenced assembly at startup, walks the IL for string literals that parse as utility candidates, and watches source files in development for live updates. The discovered set is exposed through an `IClassRegistry`. The `/styles.css` endpoint reads the current class set and runs it through a fresh `CssFramework` on every request — there is no Pennington-side cache, so an option change or a newly observed class shows up on the next fetch without a process restart.

## How it works

### Classes are discovered by scanning compiled output

`AddMonorailCss` calls `services.AddMonorailClassDiscovery()`, which registers the runtime scanner. At startup the scanner enumerates every assembly the entry app references (skipping the BCL), force-loads each one if needed, and walks IL string literals through Pennington's configured `CssFramework` to keep only the candidates the framework actually recognises. That last step is wired through `IConfigureOptions<MonorailDiscoveryOptions>` in `MonorailServiceExtensions`: the same `CssFramework` instance that generates the stylesheet validates discovery candidates, so the theme is consistent across both halves of the pipeline.

Because the scan reads compiled IL rather than source text, every `class="bg-primary-500"` literal in a Razor component, every string constant in a C# helper, and every utility token in `Pennington.UI`'s shipped components participates without any per-project glob configuration. In development, Discovery also watches the source files behind the loaded assemblies and re-scans on edits, so a new utility added to a `.razor` or `.cs` file shows up on the next `/styles.css` fetch. If a `wwwroot/app.css` is present, Discovery treats it as the source CSS prefix.

### The stylesheet generates on demand, every request

`UseMonorailCss` maps a `GET /styles.css` endpoint that calls `MonorailCssService.GetStyleSheet()`. Each hit builds a fresh `CssFramework` from the current `MonorailCssOptions`, runs it over `IClassRegistry.GetClasses()`, and prepends Pennington's content-visibility preamble plus any configured `ExtraStyles`. The service deliberately skips `IClassRegistry.Css` — that upstream cache is keyed against the framework baked in at startup, and the per-call rebuild is what lets hot-reload edits to `CustomCssFrameworkSettings` or theme tokens flow into the next stylesheet without restarting the process.

Pennington is a static content engine: the build is one-shot and the dev server is the only other consumer, so per-call regeneration is cheap enough to make caching unnecessary. The first page load primes the registry with whatever classes that page emits; the browser then fetches `/styles.css` and gets a freshly-generated stylesheet. A subsequent navigation that introduces a new class is reflected on the very next stylesheet fetch.

### Color schemes: named vs algorithmic

`ColorScheme` on `MonorailCssOptions` ships in two flavors that occupy different spots on the designer-versus-programmer axis. `NamedColorScheme` is the choice when a designer says "I want Tailwind Purple for primary" — it maps `primary`, `accent`, and `base` onto built-in palettes by name. `AlgorithmicColorScheme` is the choice when the starting point is a brand hue expressed in degrees and the whole palette needs to be derived coherently — it synthesizes everything from a single `PrimaryHue`. See <xref:reference.api.monorail-css-options> for the full parameter surface.

Syntax-highlight colors are deliberately kept off the brand scheme. `SyntaxTheme` on `MonorailCssOptions` holds the five roles `.hljs-*` token classes consume — keyword, string, variable, function, and comment — each mapped to its own Tailwind palette. The default picks a tuned combination (Sky / Emerald / Rose / Amber / Slate) that reads well against either a light or dark code background, so a site can pick primary and accent purely for brand reasons without constraining how code renders.

### OKLCH palette generation

The algorithmic scheme delegates to the `ApplyAlgorithmicColorScheme` extension method on `Theme`, which generates each palette as an 11-stop dictionary keyed by the familiar `50` through `950` shade names. Two curve families do the work. Foreground palettes (used for primary and accents) scale a fixed lightness curve fitted from averaged Tailwind palettes, and chroma is anchored at step 500 to the seed value — every other step is a relative fraction of that peak. Neutral palettes (used for the base) follow a flatter lightness curve and an absolute chroma curve scaled by the seed's intensity, with the dark tail blending between a tapered and a held-high shape so deep base colors keep a usable hue cast.

The key property that earns OKLCH its spot here is perceptual uniformity. OKLCH is a cylindrical coordinate system over the OK-Lab color space — lightness, chroma, and hue tuned so equal numeric steps look equal to the eye, which is famously not true of HSL (a 500-weight green at HSL lightness 40% looks brighter than a 500-weight blue at the same value). OKLCH makes the generated scheme feel visually coherent without per-hue handwork, which is what makes "give me a palette from hue 214" a reasonable thing to ask.

`ColorPaletteGenerator.GenerateForeground` and `GenerateNeutral` are plain static methods, so nothing in the color story depends on DI — a test or a small designer tool can call them directly to preview a palette before committing to a seed.

## Trade-offs

- **IL scanning sees only string literals.** Discovery extracts classes from constants the compiler emits, not from values built at runtime, and it does not read rendered HTML. Razor's `@Color`-style interpolation inside an inline `class=""` is invisible to the scan because the concatenation happens during render, not at compile time. A color-keyed component therefore has to expose every class it can emit as a compile-time literal — but *how* it does that decides whether the cost is a few hundred bytes or fifty kilobytes. The trap is enumerating the fully styled permutation per color: a `switch` whose every arm spells out that color's background, border, text, and fill utilities bakes the whole palette's worth of rules into every consumer's stylesheet to support the two or three colors a site uses. The pattern that scales is to enumerate only a single thin class per color — `Pennington.UI.Components.CardColorClasses.Tint` returns one literal `card-tint-{color}-500` per arm — and let a wildcard functional utility plus component CSS do the rest: `card-tint-*` resolves `--card-tint: var(--color-*)`, and one shared `.pn-card` rule derives tint, border, and text from that variable through `color-mix()`. The palette still lives in IL, but as one custom-property declaration per color rather than a per-color block of styled utilities.
- **A pre-build scan of source files was considered and rejected.** A Tailwind-style scanner that globs `.razor` and `.md` would not catch utility classes that live in compiled `Pennington.*` assemblies the consumer never edits, and it would force every project to maintain its own glob list. Discovery's IL scan covers shipped libraries and consumer projects with the same mechanism.
- **Classes accumulate for the lifetime of the registry.** Discovery is append-only; a class deleted from a page still lives in the stylesheet until the registry is rebuilt (a host restart, or a watched-file edit triggering a rescan). This is an explicit trade — the penalty for keeping a stale class is a few hundred bytes of CSS rather than a correctness issue.

## Further reading

- Reference: [`MonorailCssOptions`](xref:reference.api.monorail-css-options) — the full option surface with defaults.
- How-to: [Customize MonorailCSS](xref:how-to.theming.monorail-css) — swapping schemes, injecting `CustomCssFrameworkSettings`, and authoring extra styles.

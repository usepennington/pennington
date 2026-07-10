---
title: "Render an animated Beck diagram at build time"
description: "Fence Beck YAML with a `beck` block and let Pennington.Beck render it server-side to a self-animating inline SVG — no client JavaScript."
uid: how-to.rich-content.beck-diagrams
order: 3
sectionLabel: "Rich Content"
tags: [markdown, beck, diagrams, svg, build-time]
---

To draw an architecture, sequence, state, or class diagram that renders at build time — no client rendering, no CDN — fence Beck YAML with `beck` as the language. [Beck](https://usepennington.github.io/beck/) is a pure-C# diagram engine; the `Pennington.Beck` package registers a code-block preprocessor that turns each fence into a self-animating inline SVG during markdown rendering. The emitted SVG keys dark mode off the site's `data-theme` attribute and its `--beck-*` color tokens fall back to the host's `--color-*` palette, so diagrams adopt your theme with no per-diagram configuration. For a client-rendered alternative with Mermaid syntax, see <xref:how-to.rich-content.diagrams>.

## Register the renderer

Add the package and register the preprocessor. It hooks the shared code-block pipeline, so it works the same on `AddPennington`, `AddDocSite`, and `AddBlogSite` hosts:

```bash
dotnet add package Pennington.Beck
```

```csharp
builder.Services.AddPenningtonBeck();
```

That is the whole integration — the next `beck` fence in any markdown page renders as a diagram, on both the live dev server and the static build.

## Author a diagram

Every Beck document opens with a `type:`. This page covers the fence wiring, not the YAML grammar — the [Beck documentation](https://usepennington.github.io/beck/docs/) owns the node, edge, flow, and styling vocabulary.

### Architecture diagram

````markdown
```beck
type: architecture
nodes:
  - { id: gw, title: API Gateway, kind: gateway }
  - { id: orders, title: Orders }
  - { id: db, title: Postgres, kind: db }
  - { id: bus, title: Events, kind: queue }
edges:
  - { from: gw, to: orders }
  - { from: orders, to: db, label: query }
  - { from: orders, to: bus, label: publish, kind: async }
```
````

```beck
type: architecture
nodes:
  - { id: gw, title: API Gateway, kind: gateway }
  - { id: orders, title: Orders }
  - { id: db, title: Postgres, kind: db }
  - { id: bus, title: Events, kind: queue }
edges:
  - { from: gw, to: orders }
  - { from: orders, to: db, label: query }
  - { from: orders, to: bus, label: publish, kind: async }
```

### Sequence diagram

Sequence, state, and class documents use the same fence — only the `type:` and its vocabulary change:

````markdown
```beck
type: sequence
participants:
  - { id: web, title: Web App, kind: user }
  - { id: api, title: Orders API }
  - { id: db, title: Postgres, kind: db }
messages:
  - { from: web, to: api, label: POST /orders }
  - { from: api, to: db, label: INSERT }
  - { from: db, to: api, label: ok, reply: true }
  - { from: api, to: web, label: 201 Created, reply: true }
```
````

```beck
type: sequence
participants:
  - { id: web, title: Web App, kind: user }
  - { id: api, title: Orders API }
  - { id: db, title: Postgres, kind: db }
messages:
  - { from: web, to: api, label: POST /orders }
  - { from: api, to: db, label: INSERT }
  - { from: db, to: api, label: ok, reply: true }
  - { from: api, to: web, label: 201 Created, reply: true }
```

## Tune a fence with flags

A comma-separated tail after the language adjusts one fence without touching its YAML. Flags combine: `beck:symbol,static` works.

### Freeze the animation — `beck,static`

To show the fully-revealed final frame with no motion, add `static`. Useful when several diagrams share a page and only one should draw the eye.

### Drive playback from scroll — `beck,scrub`

`scrub` binds the choreography to scroll position instead of a looping timeline: the diagram plays as the reader scrolls it through the viewport.

### Override the style — `beck,style=sketch`

`style=<name>` overrides the document's own `meta.style`, so one YAML snippet can render in any of Beck's built-in looks:

```beck,style=sketch,static
type: architecture
nodes:
  - { id: gw, title: API Gateway, kind: gateway }
  - { id: orders, title: Orders }
  - { id: db, title: Postgres, kind: db }
edges:
  - { from: gw, to: orders }
  - { from: orders, to: db, label: query }
```

An unknown style name warns in the diagnostics and renders the document with its own style unchanged.

## Render a diagram from a file — `beck:symbol`

When a diagram should appear as both highlighted source and a live render, keep it in one `.beck.yaml` file and reference it from two fences — the same DRY convention as the tree-sitter `:symbol` source embeds:

````markdown
```yaml:symbol
diagrams/checkout.beck.yaml
```

```beck:symbol
diagrams/checkout.beck.yaml
```
````

The fence body is one file path per line; each file renders as its own diagram, and one malformed file shows its own error box without dropping the rest. Paths resolve against `BeckOptions.ContentRoot`, which defaults to the working directory — set it to match your tree-sitter `ContentRoot` so both `:symbol` forms address files the same way:

```csharp
builder.Services.AddPenningtonBeck(beck =>
{
    beck.ContentRoot = "../..";
});
```

## Configure the render

`BeckOptions.RenderOptions` is the base `SvgRenderOptions` applied to every fence: fonts, an exact text measurer, a site-wide default style, custom style registrations. By default Beck measures text with its embedded font-metrics tables and emits a `textLength` guard on every label, so a font mismatch compresses glyphs slightly instead of breaking layout. To size cards against your site's exact fonts, reference the optional `Beck.Skia` package and supply a measurer:

```csharp
var font = new BeckFontSpec
{
    Family = "Lexend",
    Files = new Dictionary<int, string> { [400] = "fonts/Lexend-Regular.ttf" },
};

builder.Services.AddPenningtonBeck(beck =>
{
    beck.RenderOptions = new SvgRenderOptions
    {
        Font = font,
        Measurer = new SkiaTextMeasurer(font),
    };
});
```

## Fullscreen zoom

Each rendered embed carries a zoom button — visible on hover, always visible on touch — that opens the diagram in a full-screen lightbox over a dimmed backdrop. Click anywhere or press Escape to close. The package contributes the button, the lightbox script, and their styles automatically; this is its one piece of client JavaScript, and rendering stays server-side. To emit bare SVG with no client behavior, turn it off:

```csharp
builder.Services.AddPenningtonBeck(beck =>
{
    beck.Zoom = false;
});
```

## What the renderer emits

Each fence renders as `<div class="beck-embed"><svg …></div>` — finished HTML that skips the standard code-block chrome. A malformed document fails loud instead of vanishing: the fence renders as `<div class="beck-embed beck-embed--error">` showing the offending YAML, and the failure lands in the per-request diagnostics, so it surfaces in the dev overlay and fails the static build report. Frame the wrapper with your own CSS (`DocSiteOptions.ExtraStyles` on a DocSite host); the SVG itself needs nothing.

## Verify

- Open a page with a `beck` fence. It renders as an inline SVG, not a code block, and follows the theme toggle between light and dark.
- Break a fence's YAML on purpose. The page shows a bordered box with the raw YAML, and `dotnet run -- diag warnings` reports the render failure.

## Related

- How-to: <xref:how-to.rich-content.diagrams> — the client-side Mermaid alternative
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — the full fence info-string grammar
- How-to: <xref:how-to.code-samples.focused-code-samples> — the tree-sitter `:symbol` embeds the `beck:symbol` form mirrors
- The [Beck documentation](https://usepennington.github.io/beck/) — YAML grammar, diagram types, styles, and the C# authoring API

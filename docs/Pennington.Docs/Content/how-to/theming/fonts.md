---
title: "Switch the body and heading typeface"
description: "Drop self-hosted woff2 files into wwwroot, register @font-face rules, declare preload hints, and point DisplayFontFamily and BodyFontFamily at the new faces."
uid: how-to.theming.fonts
order: 2
sectionLabel: "Theming"
tags: [fonts, typography, preload, docsite]
---

Swap a DocSite's default display and body typefaces for custom faces, and prime them with preload hints so they're ready for first paint.

## Before you begin
- A running DocSite built with `AddDocSite` / `UseDocSite`.
- A chosen font delivery strategy — self-hosted `.woff2` files or an external provider — with files or URLs ready.
- The CSS `font-family` name each face registers under.

For a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). The example does not ship font binaries — supply your own.

---

## Configure the typefaces

### Drop font files into `wwwroot/fonts/`

Place each `.woff2` file under `wwwroot/fonts/`. `UsePennington` wires `UseStaticFiles`, so each file becomes available at `/fonts/<file>.woff2`. The example references `/fonts/display.woff2` and `/fonts/body.woff2`.

### Register `@font-face` rules via `ExtraStyles`

Emit the `@font-face` declarations into the generated stylesheet by returning them from an `ExtraStyles` helper. MonorailCSS appends this content verbatim above its utility output, with each `src:` pointing at the `/fonts/...` path you exposed above.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildExtraStyles
```

### Declare preload hints with `FontPreloads`

Pass a `FontPreload[]` to `DocSiteOptions.FontPreloads`. DocSite then emits a `<link rel="preload" as="font" crossorigin>` tag for each entry in the document head, which prevents the flash of fallback text on first paint.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildFontPreloads
```

### Point `DisplayFontFamily` and `BodyFontFamily` at the new faces

Set `DisplayFontFamily` on `DocSiteOptions` to the CSS stack led by the display face, and set `BodyFontFamily` to the stack led by the body face. Include a `system-ui` or `sans-serif` fallback so pages still render gracefully if a file fails to load.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildDocSiteOptions
```

### Match MonorailCSS utilities to your stacks

When prose uses MonorailCSS utility classes such as `font-sans` or `font-display`, update the theme or `ExtraStyles` so those utilities resolve to the same `font-family` stacks; otherwise utility-styled text disagrees with the layout chrome. See <xref:how-to.theming.monorail-css> for how to pass `CustomCssFrameworkSettings`.

---

## Result

Body copy renders in the new body face and headings render in the new display face. The preload hints prime the browser cache before the stylesheet is parsed, so the first paint lands with the real faces in place — no fallback flash.

## Verify

- Run `dotnet run` and open any page. The DevTools **Network** panel shows `/fonts/display.woff2` and `/fonts/body.woff2` fetched with `rel=preload` before the first paint.
- **Computed styles** on the `<body>` resolve to the body family; a heading (`<h1>`) resolves to the display family.
- Run `dotnet run -- build`. The generated `index.html` contains a `<link rel="preload" as="font" ...>` tag per `FontPreload`, and `/fonts/*.woff2` lands in `output/fonts/`.

## Related

- Reference: [`DocSiteOptions`](xref:reference.api.doc-site-options) — the full property list including `DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, and `ExtraStyles`.
- Reference: [`FontPreload`](xref:reference.api.font-preload) — the `Href` / `Type` record shape (defaults to `font/woff2`).
- How-to: [Recolor the site](xref:how-to.theming.monorail-css) — for aligning utility-class font stacks with your new families.

---
title: "Switch the body and heading typeface"
description: "Drop self-hosted woff2 files into wwwroot, register @font-face rules, declare preload hints, and point DisplayFontFamily and BodyFontFamily at the new faces."
uid: how-to.configuration.fonts
order: 202040
sectionLabel: Configuration
tags: [fonts, typography, preload, docsite]
---

When a DocSite needs custom display and body typefaces instead of the defaults, and those faces should load without a flash of fallback text on first paint, the knobs below cover it. If no DocSite is running yet, start with [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) first.

## Assumptions

- A running DocSite built with `AddDocSite` / `UseDocSite`.
- A chosen font delivery strategy — self-hosted `.woff2` files or an external provider — with the files or URLs ready.
- The CSS `font-family` name each face registers under.

For a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample).

---

## Configure the typefaces

### Drop font files into `wwwroot/fonts/`

Place each `.woff2` file under `wwwroot/fonts/`. `UsePennington` wires `UseStaticFiles`, so each file becomes available at `/fonts/<file>.woff2`. The kitchen-sink example references `/fonts/display.woff2` and `/fonts/body.woff2`; supply your own files at those paths (the example does not ship font binaries).

### Register `@font-face` rules via `ExtraStyles`

Emit the `@font-face` declarations into the generated stylesheet by returning them from an `ExtraStyles` helper. MonorailCSS appends this content verbatim above its utility output, with each `src:` pointing at the `/fonts/...` path you exposed above.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles
```

### Declare preload hints with `FontPreloads`

Pass a `FontPreload[]` to `DocSiteOptions.FontPreloads`. DocSite then emits a `<link rel="preload" as="font" crossorigin>` tag for each entry in the document head, which prevents the flash of fallback text on first paint.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildFontPreloads
```

### Point `DisplayFontFamily` and `BodyFontFamily` at the new faces

Set `DisplayFontFamily` on `DocSiteOptions` to the CSS stack led by the display face, and set `BodyFontFamily` to the stack led by the body face. Include a `system-ui` or `sans-serif` fallback so pages still render gracefully if a file fails to load.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

### (Optional) Match MonorailCSS utilities to your stacks

When prose uses MonorailCSS utility classes such as `font-sans` or `font-display`, update the theme or `ExtraStyles` so those utilities resolve to the same `font-family` stacks; otherwise utility-styled text disagrees with the layout chrome. See [Recolor the site](xref:how-to.configuration.monorail-css) for how to pass `CustomCssFrameworkSettings`.

---

## Result

Body copy renders in the new body face and headings render in the new display face. Because the preload hints prime the browser cache before the stylesheet is parsed, the first paint lands with the real faces in place — no fallback flash, and the perceptible delay drops by ~40 ms on a cold load.

## Verify

- Run `dotnet run` and open any page. The DevTools **Network** panel shows `/fonts/display.woff2` and `/fonts/body.woff2` fetched with `rel=preload` before the first paint.
- **Computed styles** on the `<body>` resolve to the body family; a heading (`<h1>`) resolves to the display family.
- Run `dotnet run -- build`. The generated `index.html` contains a `<link rel="preload" as="font" ...>` tag per `FontPreload`, and `/fonts/*.woff2` lands in `output/fonts/`.

## Related

- Reference: [_`DocSiteOptions`_](xref:reference.api.doc-site-options) — the full property list including `DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, and `ExtraStyles`.
- Reference: [_`FontPreload`_](xref:reference.api.font-preload) — the `Href` / `Type` record shape (defaults to `font/woff2`).
- How-to: [_Recolor the site_](xref:how-to.configuration.monorail-css) — for aligning utility-class font stacks with your new families.

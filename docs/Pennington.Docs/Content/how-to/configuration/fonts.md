---
title: "Configure fonts and typography"
description: "Set display and body font families on DocSiteOptions, declare preload hints, and serve the font files from wwwroot."
uid: how-to.configuration.fonts
order: 202040
sectionLabel: Configuration
tags: [fonts, typography, preload, docsite]
---

When your DocSite needs custom display and body typefaces instead of the defaults, and you want those faces to load without a flash of fallback text on first paint, follow these steps. If you do not yet have a running DocSite, start with [Create your first Pennington site](xref:tutorials.getting-started.first-site) first.

## Assumptions

- You have a running DocSite built with `AddDocSite` / `UseDocSite`.
- You have chosen a font delivery strategy — self-hosted `.woff2` files or an external provider — and have the files or URLs ready.
- You know the CSS `font-family` name each face registers under.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample).

---

## Steps

### 1. Drop font files into `wwwroot/fonts/`

Place each `.woff2` file under `wwwroot/fonts/` — `UsePennington` wires `UseStaticFiles`, so each file becomes available at `/fonts/<file>.woff2`. The kitchen-sink example references `/fonts/display.woff2` and `/fonts/body.woff2`; supply your own files at those paths (the example does not ship font binaries).

### 2. Register `@font-face` rules via `ExtraStyles`

Emit the `@font-face` declarations into the generated stylesheet by returning them from an `ExtraStyles` helper. MonorailCSS appends this content verbatim above its utility output, with each `src:` pointing at the `/fonts/...` path you exposed in step 1.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles
```

### 3. Declare preload hints with `FontPreloads`

Pass a `FontPreload[]` to `DocSiteOptions.FontPreloads`. DocSite then emits a `<link rel="preload" as="font" crossorigin>` tag for each entry in the document head, which prevents the flash of fallback text on first paint.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildFontPreloads
```

### 4. Point `DisplayFontFamily` and `BodyFontFamily` at the new faces

Set `DisplayFontFamily` on `DocSiteOptions` to the CSS stack that leads with your display face, and set `BodyFontFamily` to the stack that leads with your body face. Include a `system-ui` or `sans-serif` fallback so pages still render gracefully if a file fails to load.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

### 5. (Optional) Match MonorailCSS utilities to your stacks

If you style prose with MonorailCSS utility classes such as `font-sans` or `font-display`, update the theme or `ExtraStyles` so those utilities resolve to the same `font-family` stacks — otherwise utility-styled text will disagree with the layout chrome. See [Customize MonorailCSS](xref:how-to.configuration.monorail-css) for how to pass `CustomCssFrameworkSettings` when you need that.

---

## Verify

- Run `dotnet run` and open any page — the DevTools **Network** panel should show `/fonts/display.woff2` and `/fonts/body.woff2` fetched with `rel=preload` before the first paint.
- **Computed styles** on the `<body>` should resolve to the body family; a heading (`<h1>`) should resolve to the display family.
- Run `dotnet run -- build` — the generated `index.html` must contain a `<link rel="preload" as="font" ...>` tag per `FontPreload`, and `/fonts/*.woff2` must land in `output/fonts/`.

## Related

- Reference: [_`DocSiteOptions`_](xref:reference.options.docsite-options) — the full property list including `DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, and `ExtraStyles`.
- Reference: [_`FontPreload`_](xref:reference.options.docsite-options) — the `Href` / `Type` record shape (defaults to `font/woff2`).
- How-to: [_Customize MonorailCSS_](xref:how-to.configuration.monorail-css) — for aligning utility-class font stacks with your new families.

---
title: "Configure fonts and typography"
description: "Set display and body font families on DocSiteOptions, declare preload hints, and serve the font files from wwwroot."
uid: how-to.configuration.fonts
order: 202040
sectionLabel: Configuration
tags: [fonts, typography, preload, docsite]
---

> **In this page.** Setting `DisplayFontFamily` and `BodyFontFamily` on `DocSiteOptions`, declaring `FontPreloads`, and serving the matching font files from `wwwroot/fonts/`.
>
> **Not in this page.** The self-hosting vs. Google Fonts trade-off is out of scope — pick a delivery approach before you land here.

## When to use this

_Two sentences. Readers arrive wanting a site that uses their chosen display and body faces instead of the DocSite defaults and wants those faces to load without a flash of unstyled text. Point readers who do not yet have a working DocSite back to the Getting Started tutorial — this recipe assumes the shell is already up._

## Assumptions

_Short bulleted list. Do not re-teach DocSite setup or MonorailCSS wiring._

- You have a running DocSite built with `AddDocSite` / `UseDocSite` (see [_Create your first Pennington site_](xref:tutorials.getting-started.first-site) if not).
- You have chosen a delivery strategy (self-hosted `.woff2` files in `wwwroot/fonts/`, or an external provider) and have the files or URLs on hand.
- You know the CSS `font-family` name each face registers under.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Drop font files into `wwwroot/fonts/`

_One sentence: put each `.woff2` under `wwwroot/fonts/` so `UseStaticFiles` (wired by `UsePennington`) serves it at `/fonts/<file>.woff2`. The kitchen-sink example references `/fonts/display.woff2` and `/fonts/body.woff2` in its preload and `@font-face` wiring — supply your own files at those paths (the example does not ship font binaries)._

### 2. Register `@font-face` rules via `ExtraStyles`

_One sentence: emit the `@font-face` declarations into the generated stylesheet by returning them from an `ExtraStyles` helper, which `MonorailCSS` appends verbatim above its utility output. Each rule points `src:` at the `/fonts/...` path you just exposed._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles
```

### 3. Declare preload hints with `FontPreloads`

_One sentence: hand `DocSiteOptions.FontPreloads` a `FontPreload[]` so DocSite emits a `<link rel="preload" as="font" crossorigin>` tag for each file in the document head — that's what avoids the flash of fallback text on first paint._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildFontPreloads
```

### 4. Point `DisplayFontFamily` and `BodyFontFamily` at the new faces

_Two sentences: set `DisplayFontFamily` on `DocSiteOptions` to the CSS stack that leads with your display face, and set `BodyFontFamily` to the stack that leads with your body face. Include a `system-ui` / `sans-serif` fallback so pages still render if a file 404s._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

### 5. (Optional) Match MonorailCSS utilities to your stacks

_One sentence: if you drive prose styling with MonorailCSS utility classes (e.g. `font-sans`, `font-display`), adjust the theme or `ExtraStyles` so those utilities resolve to the same `font-family` stacks — otherwise utility-styled text will disagree with the layout chrome. Link out to [_Customize MonorailCSS_](xref:how-to.configuration.monorail-css) for how to pass `CustomCssFrameworkSettings` to the bare-`AddPennington` host when you need that._

---

## Verify

- Run `dotnet run` and open any page — the DevTools **Network** panel should show `/fonts/display.woff2` and `/fonts/body.woff2` fetched with `rel=preload` before the first paint.
- **Computed styles** on the `<body>` should resolve to the body family; a heading (`<h1>`) should resolve to the display family.
- Run `dotnet run -- build` — the generated `index.html` must contain a `<link rel="preload" as="font" ...>` tag per `FontPreload`, and `/fonts/*.woff2` must land in `output/fonts/`.

## Related

- Reference: [_`DocSiteOptions`_](xref:reference.options.docsite-options) — the full property list including `DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, and `ExtraStyles`.
- Reference: [_`FontPreload`_](xref:reference.options.docsite-options) — the `Href` / `Type` record shape (defaults to `font/woff2`).
- How-to: [_Customize MonorailCSS_](xref:how-to.configuration.monorail-css) — for aligning utility-class font stacks with your new families.

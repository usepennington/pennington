---
title: "Switch the body and heading typeface"
description: "Drop self-hosted woff2 files into wwwroot, register @font-face rules, declare preload hints, and point DisplayFontFamily and BodyFontFamily at the new faces — or load the faces from an external provider instead."
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

## Self-host the font files

Self-hosting keeps the faces on your origin — no third-party request, no external dependency on first paint. The four steps below run in order: each one builds on the file paths and `@font-face` names the previous step established.

<Steps>
<Step StepNumber="1">

**Drop font files into `wwwroot/fonts/`**

Place each `.woff2` file under `wwwroot/fonts/`. `UsePennington` wires `UseStaticFiles`, so each file becomes available at `/fonts/<file>.woff2`. The example references `/fonts/display.woff2` and `/fonts/body.woff2`.

</Step>
<Step StepNumber="2">

**Register `@font-face` rules via `ExtraStyles`**

Emit the `@font-face` declarations into the generated stylesheet by returning them from an `ExtraStyles` helper. [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) prepends this content verbatim above its utility output, with each `src:` pointing at the `/fonts/...` path you exposed in step 1.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildExtraStyles
```

</Step>
<Step StepNumber="3">

**Declare preload hints with `FontPreloads`**

Pass a `FontPreload[]` to `DocSiteOptions.FontPreloads`. DocSite then emits a `<link rel="preload" as="font" crossorigin>` tag for each entry in the document head, which prevents the flash of fallback text on first paint.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildFontPreloads
```

</Step>
<Step StepNumber="4">

**Point `DisplayFontFamily` and `BodyFontFamily` at the new faces**

Set `DisplayFontFamily` on `DocSiteOptions` to the CSS stack led by the display face, and set `BodyFontFamily` to the stack led by the body face. The stack name must match the `font-family` declared in step 2. Include a `system-ui` or `sans-serif` fallback so pages still render gracefully if a file fails to load.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildDocSiteOptions
```

</Step>
</Steps>

## Load the faces from an external provider instead

To pull the faces from a hosted service (Google Fonts, Fontsource, a corporate CDN) rather than self-host, the `<link>` or `@import` goes in the document head through `DocSiteOptions.AdditionalHtmlHeadContent`, a raw-HTML string appended to `<head>`. This replaces steps 1 and 2 — the provider serves both the files and the `@font-face` rules.

```csharp
new DocSiteOptions
{
    AdditionalHtmlHeadContent =
        """<link rel="stylesheet" href="https://fonts.example.com/css?family=Display+Body">""",
    DisplayFontFamily = "'Display', system-ui, sans-serif",
    BodyFontFamily = "'Body', system-ui, sans-serif",
}
```

Steps 3 and 4 still apply: set `DisplayFontFamily` / `BodyFontFamily` to the family names the provider's CSS registers, and add `FontPreloads` entries pointing at the provider's `.woff2` URLs if you want the same first-paint priming. Provider-hosted preloads need the absolute font URL, not a `/fonts/...` path.

## Match MonorailCSS utilities to your stacks

`DisplayFontFamily` and `BodyFontFamily` flow into the layout's `<body>` / heading styles directly. They do not feed the MonorailCSS theme, so utility classes like `font-sans` and `font-display` still resolve to whatever theme tokens MonorailCSS was configured with. When prose uses those utilities, also update the theme via `CustomCssFrameworkSettings` (or add overrides through `ExtraStyles`) so the utility-driven text agrees with the layout chrome. See <xref:how-to.theming.monorail-css> for how to pass `CustomCssFrameworkSettings`.

---

## Result

Body copy renders in the new body face and headings render in the new display face. The preload hints prime the browser cache before the stylesheet is parsed, so the first paint lands with the real faces in place — no fallback flash.

## Verify

- Run `dotnet run` and open any page with the DevTools **Network** panel open. Filter to **Font**: `/fonts/display.woff2` and `/fonts/body.woff2` each show **Highest** in the **Priority** column and `preload` (rather than `link` or `script`) in the **Initiator** column, confirming the preload hint fired before the stylesheet pulled the face in.
- In the **Elements** panel, the **Computed** styles on the `<body>` resolve `font-family` to the body family; a heading (`<h1>`) resolves to the display family.
- Run `dotnet run -- build`. The generated `index.html` contains a `<link rel="preload" as="font" ...>` tag per `FontPreload`, and `/fonts/*.woff2` lands in `output/fonts/`.

## Related

- Reference: [`DocSiteOptions`](xref:reference.api.doc-site-options) — the full property list including `DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, `AdditionalHtmlHeadContent`, and `ExtraStyles`.
- Reference: [`FontPreload`](xref:reference.api.font-preload) — the `Href` / `Type` record shape (defaults to `font/woff2`).
- How-to: [Customize MonorailCSS colors, syntax theme, and prose styles](xref:how-to.theming.monorail-css) — for the broader `ExtraStyles` story and for aligning utility-class font stacks with your new families.

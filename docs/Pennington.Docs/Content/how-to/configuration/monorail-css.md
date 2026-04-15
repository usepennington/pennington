---
title: "Customize MonorailCSS"
description: "Swap color schemes, inject custom framework settings, append extra styles, and widen class collection on a Pennington site."
uid: how-to.configuration.monorail-css
order: 202030
sectionLabel: Configuration
tags: [monorailcss, color-scheme, styling, theming]
---

Use this guide when you have a working DocSite or BlogSite rendering through MonorailCSS and want to re-skin it — change the palette, tweak prose rules, or add site-wide CSS. Every knob here lives on `MonorailCssOptions`; `DocSiteOptions` and `BlogSiteOptions` forward `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` directly. Only `ContentPaths` and other non-CSS capabilities still require the bare-`AddPennington` + `AddMonorailCss` path — see [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning).

## Assumptions

- You have a running Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- You are on `AddDocSite` or `AddBlogSite` (which already call `AddMonorailCss` internally) — if you are wiring `AddPennington` directly, also call `AddMonorailCss` yourself
- You understand the `NamedColorScheme` defaults baked into `MonorailCssOptions` — if not, read the <xref:reference.options.monorail-css-options> first

The `ServiceConfiguration` helpers referenced below are backed by `examples/DocSiteKitchenSinkExample`.

---

## Steps

### 1. Pick `NamedColorScheme` for a Tailwind-named palette

`NamedColorScheme` maps five MonorailCSS palette slots (primary, accent, tertiary-one, tertiary-two, base) onto named palettes from `MonorailCss.Theme.ColorNames`. The simplest re-skin is changing the five `*ColorName` strings on the default options.

```csharp:xmldocid
T:Pennington.MonorailCss.NamedColorScheme
```

### 2. Pick `AlgorithmicColorScheme` for hue-driven palettes

`AlgorithmicColorScheme` synthesises primary, accent, and tertiary palettes from one `PrimaryHue` plus a `ColorSchemeGenerator` delegate, so the whole site repigments by changing a single number. The kitchen-sink helper below shows a plausible generator wired against `ColorNames.Zinc`.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildColorScheme
```

### 3. Assign the color scheme on the DocSite options

`DocSiteOptions.ColorScheme` is the forwarded knob — whatever `IColorScheme` you assign becomes the seed for the generated stylesheet.

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ColorScheme
```

### 4. Append site-wide rules with `ExtraStyles`

The `ExtraStyles` string is emitted verbatim above the generated utility stylesheet — use it for `@font-face` declarations, utility overrides, or one-off selectors that don't belong in a Razor component. The kitchen-sink helper below combines two font faces with a component-scoped tweak as a realistic reference.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles
```

Pass it through on the DocSite options:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ExtraStyles
```

### 5. Tweak prose rules with `CustomCssFrameworkSettings`

`DocSiteOptions.CustomCssFrameworkSettings` mirrors the `MonorailCssOptions` delegate — it post-processes the `CssFrameworkSettings` after the DocSite theme is applied, so use it to tweak prose rules, apply color maps, or register apply directives without leaving DocSite. When you need `ContentPaths` (the glob list scanned at startup for classes used in non-HTML files) or other capabilities outside DocSite's scope, drop to bare `AddPennington` + `AddMonorailCss` — see <xref:explanation.core.docsite-positioning> for the authoritative breakdown.

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.CustomCssFrameworkSettings
```

Backing options type for the delegate signature and the bare-host escape:

```csharp:xmldocid
T:Pennington.MonorailCss.MonorailCssOptions
```

For a bare `AddPennington` host the same knob sits on `MonorailCssOptions` directly — see the Lab's helper:

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.MonorailCssCustomization.BuildOptions
```

---

## Verify

- Run `dotnet run` and visit any page — inspect a `bg-primary-500` element; the rendered color matches the palette you set in steps 1 or 2.
- Fetch `/styles.css` and confirm the `ExtraStyles` block appears above the generated utility rules.
- If you wired `ContentPaths`, add a class that only appears in a referenced non-HTML file (such as `wwwroot/app.js`) and verify it lands in `/styles.css` on the next reload.

## Related

- Reference: <xref:reference.options.monorail-css-options>
- Background: <xref:explanation.core.docsite-positioning>
- Background: <xref:explanation.rendering.monorail-css>

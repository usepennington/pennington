---
title: "Recolor the site"
description: "Swap palettes, override syntax-highlight colors, append site-wide rules, and tweak prose through MonorailCSS without leaving DocSite or BlogSite."
uid: how-to.configuration.monorail-css
order: 202030
sectionLabel: Configuration
tags: [monorailcss, color-scheme, styling, theming]
---

When the site needs a different palette, a tweak to prose rules, or a chunk of site-wide CSS, the knobs below live on `MonorailCssOptions`. `DocSiteOptions` and `BlogSiteOptions` forward `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` directly, so most reskins do not need to leave the template. `ContentPaths` and other non-CSS capabilities still require the bare-`AddPennington` + `AddMonorailCss` path — see [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning).

## Assumptions

- A running Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- An `AddDocSite` or `AddBlogSite` host (which already calls `AddMonorailCss` internally); wiring `AddPennington` directly requires a separate `AddMonorailCss` call
- Familiarity with the `NamedColorScheme` defaults baked into `MonorailCssOptions` (read <xref:reference.api.monorail-css-options> first if needed)

The `ServiceConfiguration` helpers referenced below are backed by `examples/DocSiteKitchenSinkExample`.

---

## Options

### Pick `NamedColorScheme` for a Tailwind-named palette

`NamedColorScheme` maps three MonorailCSS palette slots (primary, accent, base) onto named palettes from `MonorailCss.Theme.ColorNames`. The simplest re-skin is changing the three `*ColorName` values on the default options.

```csharp:xmldocid
T:Pennington.MonorailCss.NamedColorScheme
```

### Pick `AlgorithmicColorScheme` for hue-driven palettes

`AlgorithmicColorScheme` synthesises primary and accent palettes from one `PrimaryHue` plus a `ColorSchemeGenerator` delegate (hue → accent hue), so the whole site repigments by changing a single number. The kitchen-sink helper below shows a plausible generator wired against `ColorName.Zinc`.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildColorScheme
```

### Assign the color scheme on the DocSite options

`DocSiteOptions.ColorScheme` is the forwarded knob — whichever `IColorScheme` is assigned becomes the seed for the generated stylesheet.

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ColorScheme
```

### Override syntax-highlight colors with `SyntaxTheme`

`MonorailCssOptions.SyntaxTheme` holds the five Tailwind palettes used by `.hljs-*` token classes (keyword, string, variable, function, comment). It is independent of the brand `ColorScheme`, so code colors can stay consistent while the site reskins, or vice versa. `SyntaxTheme.Default` ships Sky / Emerald / Rose / Amber / Slate; replace the whole record to substitute your own.

```csharp:xmldocid
T:Pennington.MonorailCss.SyntaxTheme
```

### Append site-wide rules with `ExtraStyles`

The `ExtraStyles` string is emitted verbatim above the generated utility stylesheet. It fits `@font-face` declarations, utility overrides, or one-off selectors that don't belong in a Razor component. The kitchen-sink helper below combines two font faces with a component-scoped tweak as a realistic reference.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles
```

Pass it through on the DocSite options:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ExtraStyles
```

### Tweak prose rules with `CustomCssFrameworkSettings`

`DocSiteOptions.CustomCssFrameworkSettings` mirrors the `MonorailCssOptions` delegate — it post-processes the `CssFrameworkSettings` after the DocSite theme is applied, so it fits prose tweaks, color maps, or apply directives without leaving DocSite. When `ContentPaths` (the glob list scanned at startup for classes used in non-HTML files) or other capabilities outside DocSite's scope are needed, drop to bare `AddPennington` + `AddMonorailCss`; see <xref:explanation.core.docsite-positioning> for the authoritative breakdown.

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.CustomCssFrameworkSettings
```

Backing options type for the delegate signature and the bare-host escape:

```csharp:xmldocid
T:Pennington.MonorailCss.MonorailCssOptions
```

For a bare `AddPennington` host the same knob sits on `MonorailCssOptions` directly; see the Lab's helper:

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.MonorailCssCustomization.BuildOptions
```

---

## Result

Every `bg-primary-*`, `text-accent-*`, `border-base-*` utility on the site resolves to the new palette on the next page load. Code-block tokens recolor independently when `SyntaxTheme` is set, and any rules from `ExtraStyles` appear at the top of `/styles.css` ahead of the generated utilities.

## Verify

- Run `dotnet run` and visit any page. Inspect a `bg-primary-500` element; the rendered color matches the palette set above.
- Fetch `/styles.css` and confirm the `ExtraStyles` block appears above the generated utility rules.
- When `ContentPaths` is wired, add a class that only appears in a referenced non-HTML file (such as `wwwroot/app.js`) and verify it lands in `/styles.css` on the next reload.

## Related

- Reference: <xref:reference.api.monorail-css-options>
- Background: <xref:explanation.core.docsite-positioning>
- Background: <xref:explanation.rendering.monorail-css>

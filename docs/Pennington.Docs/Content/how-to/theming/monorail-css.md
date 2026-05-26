---
title: "Customize MonorailCSS colors, syntax theme, and prose styles"
description: "Swap palettes, override syntax-highlight colors, append site-wide rules, and tweak prose through MonorailCSS without leaving DocSite or BlogSite."
uid: how-to.theming.monorail-css
order: 1
sectionLabel: "Theming"
tags: [monorailcss, color-scheme, styling, theming]
---

When the site needs a different palette, a tweak to prose rules, or a chunk of site-wide CSS, the knobs below live on `MonorailCssOptions`. `DocSiteOptions` and `BlogSiteOptions` forward `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` directly, so most reskins do not need to leave the template.

## Before you begin
- A running Pennington site (see <xref:tutorials.getting-started.first-site> if not).
- An `AddDocSite` or `AddBlogSite` host (both call `AddMonorailCss` internally); bare `AddPennington` requires a separate `AddMonorailCss` call.
- Familiarity with `MonorailCssOptions` and the named-palette defaults — see <xref:reference.api.monorail-css-options>.

The `ServiceConfiguration` helpers referenced below come from `examples/DocSiteKitchenSinkExample`.

---

## Options

### Pick `NamedColorScheme` for a Tailwind-named palette

Map the three palette slots — primary, accent, base — to named palettes from `MonorailCss.Theme.ColorNames`.

```csharp
ColorScheme = new NamedColorScheme
{
    PrimaryColorName = ColorName.Indigo,
    AccentColorName = ColorName.Pink,
    BaseColorName = ColorName.Slate,
}
```

### Pick `AlgorithmicColorScheme` for hue-driven palettes

`AlgorithmicColorScheme` synthesises primary, base, and accent palettes from one `PrimaryHue` plus a `Chroma` and a `CoordinatingScheme` enum (Complementary, SplitComplementary, Triadic, Analogous). The whole site repigments by changing one number.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildColorScheme
```

Assign whichever scheme to `DocSiteOptions.ColorScheme`.

### Override syntax-highlight colors with `SyntaxTheme`

`MonorailCssOptions.SyntaxTheme` holds the five palettes used by `.hljs-*` token classes (keyword, string, variable, function, comment). It is independent of the brand `ColorScheme`. `SyntaxTheme.Default` ships Sky / Emerald / Rose / Amber / Slate; replace the whole record to substitute your own.

### Append site-wide rules with `ExtraStyles`

`ExtraStyles` is a CSS string emitted verbatim above the generated utility stylesheet — `@font-face` declarations, utility overrides, or one-off selectors. Assign to `DocSiteOptions.ExtraStyles`.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildExtraStyles
```

### Tweak prose rules with `CustomCssFrameworkSettings`

`DocSiteOptions.CustomCssFrameworkSettings` post-processes the `CssFrameworkSettings` after the DocSite theme is applied — prose tweaks, color maps, or apply directives without leaving DocSite. For customisations outside DocSite's scope, see <xref:explanation.positioning.docsite-positioning>. On a bare `AddPennington` host, the same knob sits on `MonorailCssOptions` directly.

```csharp:symbol,bodyonly
examples/ExtensibilityLabExample/MonorailCssCustomization.cs > MonorailCssCustomization.BuildOptions
```

---

## Result

Every `bg-primary-*`, `text-accent-*`, `border-base-*` utility on the site resolves to the new palette on the next page load. Code-block tokens recolor independently when `SyntaxTheme` is set, and any rules from `ExtraStyles` appear at the top of `/styles.css` ahead of the generated utilities.

## Verify

- Run `dotnet run` and visit any page. Inspect a `bg-primary-500` element; the rendered color matches the palette set above.
- Fetch `/styles.css` and confirm the `ExtraStyles` block appears above the generated utility rules.

## Related

- Reference: <xref:reference.api.monorail-css-options>
- Background: <xref:explanation.positioning.docsite-positioning>
- Background: <xref:explanation.rendering.monorail-css>

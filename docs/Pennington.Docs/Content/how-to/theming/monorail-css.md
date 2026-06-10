---
title: "Recolor the site"
description: "Swap palettes, override syntax-highlight colors, append site-wide rules, and tweak prose through MonorailCSS without leaving DocSite or BlogSite."
uid: how-to.theming.monorail-css
order: 1
sectionLabel: "Theming"
tags: [monorailcss, color-scheme, styling, theming]
---

When the site needs a different palette, recolored code blocks, a change to prose rules, or a chunk of site-wide CSS, the options below live on `MonorailCssOptions`, the options record for Pennington's [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) integration. `DocSiteOptions` forwards `ColorScheme`, `SyntaxTheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` to it directly, so most reskins set these on `DocSiteOptions` and never leave the template.

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

`AlgorithmicColorScheme` synthesises primary, base, and accent palettes from one `PrimaryHue` plus a `Chroma` and a `Scheme` (of type `CoordinatingScheme`: Complementary, SplitComplementary, Triadic, Analogous). The whole site repigments by changing one number.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildColorScheme
```

Assign whichever scheme to `DocSiteOptions.ColorScheme`.

### Override syntax-highlight colors with `SyntaxTheme`

`SyntaxTheme` holds the five palettes used by `.hljs-*` token classes — `Keyword`, `String`, `Variable`, `Function`, and `Comment`. It is independent of the brand `ColorScheme`. `SyntaxTheme.Default` ships Sky / Emerald / Rose / Amber / Slate; build a new record to substitute your own (every slot is required). Assign it to `DocSiteOptions.SyntaxTheme`, which forwards to `MonorailCssOptions.SyntaxTheme`.

```csharp
SyntaxTheme = new SyntaxTheme
{
    Keyword = ColorName.Violet,
    String = ColorName.Teal,
    Variable = ColorName.Orange,
    Function = ColorName.Cyan,
    Comment = ColorName.Gray,
}
```

### Append site-wide rules with `ExtraStyles`

`ExtraStyles` is a CSS string emitted verbatim above the generated utility stylesheet — `@font-face` declarations, utility overrides, or one-off selectors. Assign to `DocSiteOptions.ExtraStyles`.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildExtraStyles
```

### Tweak prose rules with `CustomCssFrameworkSettings`

`DocSiteOptions.CustomCssFrameworkSettings` post-processes the `CssFrameworkSettings` after the DocSite theme is applied — prose adjustments, color maps, or `apply` directives without leaving DocSite. The delegate receives the fully baked settings and returns the ones the framework is built from; use a `with` expression and `AddRange`/`SetItem` so DocSite's defaults (scrollbar utilities, prose rules) survive — a plain assignment clobbers them.

```csharp
CustomCssFrameworkSettings = settings => settings with
{
    Applies = settings.Applies
        .SetItem(".callout", "rounded-lg border border-base-300 bg-base-50 px-4 py-3"),
}
```

For customizations DocSite does not cover, see <xref:explanation.positioning.docsite-positioning>. On a bare `AddPennington` host the same delegate sits on `MonorailCssOptions`, alongside `ColorScheme`, in the options you pass to `AddMonorailCss`:

```csharp:symbol,bodyonly
examples/ExtensibilityLabExample/MonorailCssCustomization.cs > MonorailCssCustomization.BuildOptions
```

---

## Result

Every `bg-primary-*`, `text-accent-*`, `border-base-*` utility on the site resolves to the new palette on the next page load. Code-block tokens recolor independently when `SyntaxTheme` is set, and any rules from `ExtraStyles` appear at the top of `/styles.css` ahead of the generated utilities.

## Verify

- Run `dotnet run` and visit any page. Inspect a `bg-primary-500` element; the rendered color matches the palette set above.
- Open a page with a fenced code block and inspect a keyword token (`.hljs-keyword`); its color matches the `SyntaxTheme.Keyword` palette, not the brand `ColorScheme`.
- Fetch `/styles.css` and confirm the rule added through `CustomCssFrameworkSettings` (the `.callout` selector above) is present, and that the scrollbar utilities (`scrollbar-thin`) are still there.
- Confirm the `ExtraStyles` block appears at the top of `/styles.css`, above the generated utility rules.

## Related

- Reference: <xref:reference.api.monorail-css-options>
- Background: <xref:explanation.positioning.docsite-positioning>
- Background: <xref:explanation.rendering.monorail-css>

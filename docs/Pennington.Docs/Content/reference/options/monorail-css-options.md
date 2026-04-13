---
title: MonorailCssOptions
description: Every property on MonorailCssOptions — ColorScheme, CustomCssFrameworkSettings, ExtraStyles, ContentPaths — plus the two built-in color-scheme types NamedColorScheme and AlgorithmicColorScheme.
section: options
order: 70
tags: []
uid: reference.options.monorail-css-options
isDraft: true
search: false
llms: false
---

> **In this page.** `ColorScheme`, `CustomCssFrameworkSettings`, `ExtraStyles`, `ContentPaths`, and the two built-in color-scheme types (`NamedColorScheme`, `AlgorithmicColorScheme`).
>
> **Not in this page.** The generator internals — see Explanation.

## Summary

The options class that configures the MonorailCSS integration — utility-first stylesheet generation, color palette application, and content-path scanning.
Namespace `Pennington.MonorailCss`; defined in `src/Pennington.MonorailCss/MonorailCssOptions.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.MonorailCss.MonorailCssOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ColorScheme` | `IColorScheme` | `NamedColorScheme` with `PrimaryColorName = Blue`, `AccentColorName = Purple`, `TertiaryOneColorName = Cyan`, `TertiaryTwoColorName = Pink`, `BaseColorName = Slate` | Color scheme applied to the MonorailCSS theme. |
| `ContentPaths` | `string[]` | `[]` | File paths (relative to the web root) scanned for CSS class usage at startup. |
| `CustomCssFrameworkSettings` | `Func<CssFrameworkSettings, CssFrameworkSettings>` | identity (`settings => settings`) | Transformation applied to the built-in `CssFrameworkSettings` before the framework is constructed. |
| `ExtraStyles` | `string` | `""` | Raw CSS prepended to the generated stylesheet. |

## `IColorScheme`

```csharp:xmldocid
T:Pennington.MonorailCss.IColorScheme
```

Contract implemented by color schemes.

### `ApplyToTheme`

```csharp:xmldocid
M:Pennington.MonorailCss.IColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)
```

Returns a new `Theme` with the scheme's color palettes applied.

## `NamedColorScheme`

```csharp:xmldocid
T:Pennington.MonorailCss.NamedColorScheme
```

Maps five named Tailwind color palettes onto the MonorailCSS theme slots. All properties are `required`.

| Name | Type | Default | Description |
|---|---|---|---|
| `AccentColorName` | `string` | required | Color name mapped to the `"accent"` palette. |
| `BaseColorName` | `string` | required | Color name mapped to the `"base"` palette. |
| `PrimaryColorName` | `string` | required | Color name mapped to the `"primary"` palette. |
| `TertiaryOneColorName` | `string` | required | Color name mapped to the `"tertiary-one"` palette. |
| `TertiaryTwoColorName` | `string` | required | Color name mapped to the `"tertiary-two"` palette. |

### `ApplyToTheme`

```csharp:xmldocid
M:Pennington.MonorailCss.NamedColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)
```

Calls `MapColorPalette` on the theme for each of the five slot-to-name pairs.

## `AlgorithmicColorScheme`

```csharp:xmldocid
T:Pennington.MonorailCss.AlgorithmicColorScheme
```

Generates primary, accent, and two tertiary palettes from a single hue using `ColorPaletteGenerator`.

| Name | Type | Default | Description |
|---|---|---|---|
| `BaseColorName` | `string` | `"Gray"` (`ColorNames.Gray`) | Named palette mapped to the `"base"` slot. |
| `ColorSchemeGenerator` | `Func<int, (int, int, int)>` | `primary => (primary + 180, primary + 90, primary - 90)` | Produces `(accentHue, tertiaryOneHue, tertiaryTwoHue)` from `PrimaryHue`. |
| `PrimaryHue` | `int` | required | Primary hue value in the range 0–360. |

### `ApplyToTheme`

```csharp:xmldocid
M:Pennington.MonorailCss.AlgorithmicColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)
```

Generates palettes for the primary hue and the three derived hues via `ColorPaletteGenerator.GenerateFromHue`, adds them as `"primary"`, `"accent"`, `"tertiary-one"`, `"tertiary-two"`, then maps `BaseColorName` onto `"base"`.

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Related reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options)

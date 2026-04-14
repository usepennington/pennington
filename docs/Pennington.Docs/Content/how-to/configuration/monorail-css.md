---
title: "Customize MonorailCSS"
description: "Swap color schemes, inject custom framework settings, append extra styles, and widen class collection on a Pennington site."
uid: how-to.configuration.monorail-css
order: 202030
sectionLabel: Configuration
tags: [monorailcss, color-scheme, styling, theming]
---

> **In this page.** _One sentence paraphrasing the TOC "Covers" line: swapping between `NamedColorScheme` and `AlgorithmicColorScheme`, injecting `CustomCssFrameworkSettings`, appending `ExtraStyles`, and widening class collection through `ContentPaths`. Note that `DocSiteOptions` forwards `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings`; `ContentPaths` remains a bare-`AddPennington` knob._
>
> **Not in this page.** _One sentence paraphrasing the TOC "Does not cover": the class-collector internals (Explanation) and writing a standalone `IColorScheme` implementation (advanced customization)._

## When to use this

_Two sentences. Frame the arrival state: reader already has a working DocSite or BlogSite rendering through MonorailCSS and wants to re-skin it — change the palette, tweak prose rules, or add site-wide CSS. Clarify that every knob here lives on `MonorailCssOptions` and that DocSite/BlogSite forward most of them (`ColorScheme`, `ExtraStyles`, `CustomCssFrameworkSettings`); only `ContentPaths` and other non-CSS caps still require the bare-`AddPennington` + `AddMonorailCss` path — see [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)._

## Assumptions

_Three bullets. Keep prerequisites tight — this is configuration, not authoring._

- You have a running Pennington site (see [_Get started with DocSite_](xref:tutorials.getting-started.first-site) if not)
- You are on `AddDocSite` or `AddBlogSite` (which already call `AddMonorailCss` internally) — if you are wiring `AddPennington` directly, also call `AddMonorailCss` yourself
- You understand the `NamedColorScheme` defaults baked into `MonorailCssOptions` — if not, read the [MonorailCssOptions reference](xref:reference.options.monorail-css-options) first

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — the `ServiceConfiguration` helpers below back this page end-to-end. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Five verb-first steps, one per TOC bullet plus the "drop to bare AddPennington" escape hatch at the end. Keep prose under two sentences per step and lean on `xmldocid` fences for both production symbols and the example helper methods._

### 1. Pick `NamedColorScheme` for a Tailwind-named palette

_Two sentences. `NamedColorScheme` maps five MonorailCSS palette slots (primary/accent/tertiary-one/tertiary-two/base) onto named palettes from `MonorailCss.Theme.ColorNames` — the simplest swap is to change the five `*ColorName` strings on the default options. Point readers at the type so they can see the required-init properties._

```csharp:xmldocid
T:Pennington.MonorailCss.NamedColorScheme
```

### 2. Pick `AlgorithmicColorScheme` for hue-driven palettes

_Two sentences. `AlgorithmicColorScheme` synthesises primary/accent/tertiary palettes from one `PrimaryHue` plus a `ColorSchemeGenerator` delegate, so the whole site repigments by changing one number. Show the full helper body from the kitchen-sink example so the reader sees a plausible generator wired against `ColorNames.Zinc`._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildColorScheme
```

### 3. Assign the color scheme on the DocSite options

_One to two sentences. `DocSiteOptions.ColorScheme` is the forwarded knob — whatever `IColorScheme` you hand it becomes the seed for the generated stylesheet. Reference the property symbol so the reader can see the exact type._

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ColorScheme
```

### 4. Append site-wide rules with `ExtraStyles`

_Two sentences. The `ExtraStyles` string is emitted verbatim above the generated utility stylesheet — use it for `@font-face` declarations, utility overrides, or one-off selectors that don't belong in a Razor component. The kitchen-sink helper combines two font faces with a component-scoped tweak as a realistic reference._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles
```

_Pass it through on the DocSite options:_

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ExtraStyles
```

### 5. Tweak prose rules with `CustomCssFrameworkSettings`

_Two sentences. `DocSiteOptions.CustomCssFrameworkSettings` mirrors the `MonorailCssOptions` delegate — it post-processes the `CssFrameworkSettings` after the DocSite theme is applied, so use it to tweak prose rules, apply colour maps, or register apply directives without leaving DocSite. For `ContentPaths` (the glob list scanned at startup for classes used in non-HTML files) and the other genuine caps — notably the single `AddMarkdownContent<DocSiteFrontMatter>` registration baked into `AddDocSite` — you still drop to bare `AddPennington` + `AddMonorailCss`. See [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) for the authoritative caps menu._

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.CustomCssFrameworkSettings
```

_Backing options type for the delegate signature and the bare-host escape:_

```csharp:xmldocid
T:Pennington.MonorailCss.MonorailCssOptions
```

_For a bare `AddPennington` host the same knob sits on `MonorailCssOptions` directly — see the Lab's helper:_

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.MonorailCssCustomization.BuildOptions
```

---

## Verify

_Terse. Three bullets, one per outcome so each swap can be confirmed independently._

- Run `dotnet run` and visit any page — inspect a `bg-primary-500` element; the rendered color matches the palette you picked in steps 1 or 2
- Fetch `/styles.css` and confirm the `ExtraStyles` block appears above the generated utility rules
- If you wired `ContentPaths`, add a class that only appears in a referenced non-HTML file (e.g. `wwwroot/app.js`) and verify the class lands in `/styles.css` on the next reload

## Related

_Three cross-quadrant links. Point at the options reference for the full property catalog, the bare-host Explanation page that sets up the escape-hatch trade-off, and the rendering explanation for the class-collector internals deliberately deferred here._

- Reference: [`MonorailCssOptions`](xref:reference.options.monorail-css-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
- Background: [MonorailCSS integration](xref:explanation.rendering.monorail-css)

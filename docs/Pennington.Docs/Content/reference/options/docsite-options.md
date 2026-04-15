---
title: "DocSiteOptions"
description: "The options record that configures a DocSite template host, from site chrome to areas to Pennington escape hatches."
uid: reference.options.docsite-options
order: 401020
sectionLabel: Configuration Options
tags: [docsite, options, configuration, reference]
---

`DocSiteOptions` is the options record passed to `AddDocSite` that configures the documentation-site template: site chrome, typography, color scheme, content areas, and escape-hatch callbacks for the underlying `PenningtonOptions` and `MonorailCssOptions`. Defined in namespace `Pennington.DocSite` and consumed by `DocSiteServiceExtensions.AddDocSite(IServiceCollection, Func<DocSiteOptions>)`.

## Declaration

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

A `public record` with two required `init`-only properties (`SiteTitle`, `Description`); all remaining properties are optional `init`-only and default to `null` or empty collections.

## Properties

<ApiMemberTable XmlDocId="T:Pennington.DocSite.DocSiteOptions" />

### `ContentArea`

```csharp:xmldocid
T:Pennington.DocSite.ContentArea
```

Record describing a top-level section of the documentation site. `Slug` doubles as the URL prefix and the top-level directory name under `ContentRootPath` — `new ContentArea("Guides", "guides")` maps `/guides/…` to `Content/guides/…`.

<ApiMemberTable XmlDocId="T:Pennington.DocSite.ContentArea" />

### `FontPreload`

```csharp:xmldocid
T:Pennington.Infrastructure.FontPreload
```

Record describing a font file to preload via `<link rel="preload">`, with `Href` and `Type` (defaults to `font/woff2`).

<ApiMemberTable XmlDocId="T:Pennington.Infrastructure.FontPreload" />

## Example

The `BuildDocSiteOptions` helper in `DocSiteKitchenSinkExample` configures every optional surface in one factory method, covering site metadata, color scheme, fonts, header/footer, localization, and areas.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

This factory is passed to `AddDocSite` in the kitchen-sink example's `Program.cs`.

## See also

- Related reference: [PenningtonOptions](xref:reference.options.pennington-options) — the base engine options `DocSiteOptions` configures through `ConfigurePennington`.
- Related reference: [MonorailCssOptions](xref:reference.options.monorail-css-options) — the `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` target surface.
- Related reference: [LocalizationOptions](xref:reference.options.localization-options) — the type mutated by the `ConfigureLocalization` callback.
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components) — task-oriented walkthrough of `AdditionalHtmlHeadContent`, `ExtraStyles`, slots, and `AdditionalRoutingAssemblies`.
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) — the positioning rationale and the three surfaces DocSite caps.

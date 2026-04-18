---
title: "HighlightingOptions, IslandsOptions, SearchIndexOptions, LlmsTxtOptions, OutputOptions"
description: "Catalog of the remaining nested option bags on PenningtonOptions plus the build-time OutputOptions, each with properties, defaults, and registration methods."
uid: reference.options.auxiliary-options
order: 401070
sectionLabel: Configuration Options
tags: [options, configuration, reference]
---

The four nested bags exposed on `PenningtonOptions` and one build-time record constructed by `RunOrBuildAsync` from CLI args. MonorailCSS options are documented separately at [`MonorailCssOptions`](xref:reference.options.monorail-css-options).

## `HighlightingOptions`

<ApiSummary XmlDocId="T:Pennington.Infrastructure.HighlightingOptions" />

<ApiMemberTable XmlDocId="T:Pennington.Infrastructure.HighlightingOptions" Kind="All" />

## `IslandsOptions`

<ApiSummary XmlDocId="T:Pennington.Infrastructure.IslandsOptions" />

<ApiMemberTable XmlDocId="T:Pennington.Infrastructure.IslandsOptions" Kind="All" />

## `SearchIndexOptions`

<ApiSummary XmlDocId="T:Pennington.Search.SearchIndexOptions" />

<ApiMemberTable XmlDocId="T:Pennington.Search.SearchIndexOptions" />

## `LlmsTxtOptions`

<ApiSummary XmlDocId="T:Pennington.LlmsTxt.LlmsTxtOptions" />

<ApiMemberTable XmlDocId="T:Pennington.LlmsTxt.LlmsTxtOptions" />

## `OutputOptions`

<ApiSummary XmlDocId="T:Pennington.Generation.OutputOptions" />

<ApiMemberTable XmlDocId="T:Pennington.Generation.OutputOptions" Kind="All" />

## See also

- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter)
- How-to: [Register an island renderer](xref:how-to.extensibility.island-renderer)
- How-to: [Configure search indexing](xref:how-to.configuration.search)
- How-to: [Generate an llms.txt](xref:how-to.configuration.llms-txt)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)

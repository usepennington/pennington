---
title: Highlighting service
description: A symbol page authoring the custom namespace and stability keys.
namespace: Pennington.Highlighting
stability: preview
order: 20
uid: kitchen-sink.symbols.highlighting-service
---

This page is parsed as `ApiFrontMatter`, so its `namespace` and `stability`
keys deserialize into typed properties. The badge below is rendered by the
`<StabilityBadge />` component, which reads `ApiFrontMatter.Stability` off the
page's front matter and styles it:

<StabilityBadge />

The `namespace` value is `Pennington.Highlighting`, available to any component
or layout that casts the resolved front matter to `ApiFrontMatter`.

---
title: Highlighting service
description: A symbol page authoring the custom namespace and stability keys.
namespace: Pennington.Highlighting
stability: preview
order: 20
uid: custom-front-matter.symbols.highlighting-service
---

This page is parsed as `ApiFrontMatter`, so its `namespace` and `stability`
keys deserialize into typed properties. The Razor page that resolves this URL
calls `ResolveAsync<ApiFrontMatter>` and reads the `stability` value off the
typed front matter — that is the line rendered above this body.

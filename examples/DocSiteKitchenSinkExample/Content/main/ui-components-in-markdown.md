---
title: UI components in markdown
description: Render a Razor component inline from markdown via Mdazor.
tags: [authoring, components]
sectionLabel: authoring
order: 100
uid: kitchen-sink.main.ui-components-in-markdown
---

Any Razor component registered via `AddMdazorComponent<T>()` is usable
as a tag inside markdown. Attributes bind to `[Parameter]` properties
by case-insensitive name match, and markdown between the open and
close tags becomes `ChildContent`.

## The `<FeatureCallout>` component

This site ships a custom `FeatureCallout.razor` under `Components/`:

<FeatureCallout Title="Fast" Kind="tip" Icon="bolt">
Pages render in a single SSR pass through Pennington's content pipeline.
</FeatureCallout>

<FeatureCallout Title="Theme aware" Kind="info" Icon="book">
Components participate in the **same** MonorailCSS utility pass as the
surrounding prose, so `text-primary-600` inside a component tracks the
site's color scheme.
</FeatureCallout>

<FeatureCallout Title="Heads up" Kind="warn" Icon="shield">
Only **primitive** parameter types bind from markdown attributes —
strings, numbers, booleans. Pack complex data into a delimited string
or use `ChildContent` for rich content.
</FeatureCallout>

## Built-ins

Pennington.UI ships seven components pre-registered by `AddDocSite` —
`<Badge>`, `<BigTable>`, `<Card>`, `<CardGrid>`, `<LinkCard>`,
`<Step>`, and `<Steps>`. Use them directly without writing your
own components.

<Badge>
Kitchen sink
</Badge>

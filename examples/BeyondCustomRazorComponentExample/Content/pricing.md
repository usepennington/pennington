---
title: Pricing
description: Two PricingCard components rendered from markdown with distinct parameter values.
order: 20
---

Pick a plan that fits your team. Both tiers below are rendered from a single
Razor component, `PricingCard`, authored in this example's `Components/`
folder and registered via `AddMdazorComponent<PricingCard>()` in
`Program.cs`. The markdown below consumes the component by name — Mdazor
intercepts tags that look like registered components, binds their
attributes as parameters, and hands the resulting HTML back to the Markdig
pipeline.

## Plans

<PricingCard Tier="Basic" Price="9" Features="1 project|5 GB storage|Community support" />

<PricingCard Tier="Pro" Price="49" Features="Unlimited projects|100 GB storage|Priority email support|Team seats included" Highlighted="true" />

## Why two cards?

Rendering the component twice with different attribute values proves that
Mdazor resolves `<PricingCard />` tags on every occurrence, not just the
first. The second card passes `Highlighted="true"`, which flips the
component into its emphasised visual state — a thicker accent border.

## How the wiring works

1. The component is a regular Razor component with `[Parameter]`-decorated
   properties for `Tier`, `Price`, `Features`, and `Highlighted`.
2. `services.AddMdazorComponent<PricingCard>()` adds the type to Mdazor's
   component registry.
3. When the markdown renderer encounters `<PricingCard ... />`, it looks up
   the registered type, instantiates it, assigns parameters via
   case-insensitive reflection, renders the component through Blazor's
   server-side `HtmlRenderer`, and inlines the resulting HTML into the page.

Self-closing (`<PricingCard ... />`) and open/close (`<PricingCard ...></PricingCard>`)
forms are both supported; the open/close form lets the component receive
`ChildContent` populated by any markdown between the tags.

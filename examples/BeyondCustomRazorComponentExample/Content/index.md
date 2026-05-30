---
title: Home
description: Landing page for the custom Razor component example.
order: 10
---

This example demonstrates how to author a custom Razor component inside an
example app and register it with Mdazor so it can be consumed directly from
markdown.

- The component lives at `Components/PricingCard.razor`
- It is registered in `Program.cs` via `services.AddMdazorComponent<PricingCard>()`
- The [Pricing](/pricing/) page consumes it with two different parameter sets

See the [pricing](/pricing/) page for the component rendered in context.

---
title: Home
description: Home page for the chrome-overrides example, showing off the active color theme.
order: 1
---

This site overrides its DocSite chrome end-to-end — head tags, styles, header/footer,
and an extra routed page. The [Guides](/guides/) area walks through each seam.

It also sets a brand `ColorScheme`: the `Orchid` color theme, one of the curated catalog
schemes that grows an entire palette from a single hue. The `primary` and `accent` roles
are generated algorithmically; the neutral `base` ramp is **auto-selected**. Orchid's hue
(≈325°) sits nearest MonorailCss's `mauve` neutral, so the surface grays pick up a faint
mauve tint instead of falling back to a generic gray.

<BrandPalette />

Swap the seed and the grays follow: a warm `Ember` lands on `taupe`, a cool `Azure` on
`slate`. The base always coordinates with the brand.

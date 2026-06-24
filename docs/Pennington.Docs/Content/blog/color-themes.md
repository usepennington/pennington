---
title: Color themes
description: A catalog of curated color themes: pick one and Pennington grows the brand, accent, neutral, and syntax palettes from a single hue.
author: Phil Scott
date: 2026-06-22
isDraft: false
tags:
  - theming
  - css
---

Theming a site usually means picking a handful of colors that don't fight each
other. Pennington now ships a catalog of pre-built themes.

## One hue per theme

A `ColorTheme` is one seed hue. From it, Pennington generates the `primary` and
`accent` brand palettes, a coordinating four-color syntax-highlight palette, and a
neutral base, all from the same starting point, so they hang together. Picking
one is a line:

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    ColorScheme = ColorTheme.Orchid,
    SyntaxTheme = ColorTheme.Orchid.SyntaxTheme,
});
```

There are twelve, named around the wheel: `Ember`, `Marigold`, `Citron`, `Fern`,
`Lagoon`, `Aqua`, `Azure`, `Indigo`, `Iris`, `Orchid`, `Rose`, and the near-gray
`Graphite`.

## The neutral base

The nicer detail is the neutral. Rather than a generic gray, a theme snaps its
base to the stock neutral whose undertone sits nearest the seed: `Orchid` lands
on mauve grays, a warm hue on stone, a cool one on slate. Backgrounds and borders
then match the brand instead of a stock gray. To pin it
yourself, set `BaseColorName` and the auto-pick steps aside. The [recolor
how-to](xref:how-to.theming.monorail-css) covers the lower-level color schemes
too.

On a DocSite, the generated accent shows up in the chrome (the active area
marker, the article eyebrow, the focus rings), so a theme changes the shell, not
just the body. [Overriding DocSite
components](xref:how-to.response-pipeline.override-docsite-components) has the
details.

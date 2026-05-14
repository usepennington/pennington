---
title: About
description: Used by the tutorial to demonstrate runtime stylesheet regeneration.
---

# About this site

Two markdown files, one MonorailCSS registration, and a `MainLayout.razor`
that wraps every page in the same utility-class scaffold. That's the whole
styling story at the bare-host level.

The `NamedColorScheme` picked in `Program.cs` decides which palette the
`primary`, `accent`, and `base` utility prefixes resolve to. Swap the
`PrimaryColorName` constant and the whole site re-skins on the next request.

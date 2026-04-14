---
title: Swap in a custom layout
description: Override the default DocSite layout with your own Razor component.
section: Advanced
order: 40
---

# Swap in a custom layout

DocSite ships with a sensible default layout, but every surface is a plain
Razor component. When you need a different header, footer, or body grid,
register your layout after `AddDocSite` and it takes precedence.

## Replace the main layout

Pass `AdditionalRoutingAssemblies` a reference to the assembly that holds
your `MyLayout.razor`, then mark it with the same `@layout`/`@inherits`
shape DocSite's `MainLayout` uses.

## Keep the sidebar or write your own

The easiest path is to keep `AreaNavigation` and `TableOfContentsNavigation`
inside your custom layout so sidebar grouping still works exactly as this
tutorial describes. The harder path — writing a bespoke nav — is covered
in a later how-to.

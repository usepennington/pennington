---
title: Deployment
description: Another page inside the guides section.
order: 20
---

This page also lives under `Content/guides/`, so it joins the Installation page
under the **Guides** section. Its `order:` of `20` places it second.

Open the menu and notice that the entry for the page you are on renders bold —
`NavMenu.razor` reads the `IsSelected` flag `NavigationBuilder` stamps onto the
node matching the current URL.

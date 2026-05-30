---
title: Configure the site
description: Pick a site title, set the GitHub link, and decide on a single area or multiple.
sectionLabel: Guides
order: 30
---

`DocSiteOptions` is the one options record `AddDocSite` reads. Set the fields that surface in the rendered chrome and the rest of the template falls into place.

## Fields worth setting first

- `SiteTitle` — appears in the header and the `<title>` tag.
- `Description` — meta description used in search snippets and social cards.
- `GitHubUrl` — surfaces the GitHub icon in the header.
- `HeaderContent` / `FooterContent` — raw HTML slots, useful for a logo and a copyright line.

## Areas — one or many?

`Areas` is an `IReadOnlyList<ContentArea>`. One entry is enough to ship; more entries turn on the area selector and split the sidebar by top-level folder. Stick with a single area until the content outgrows it.

## Previously

[Install Pennington](./install) covered getting the package and wiring in place.

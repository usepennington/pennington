---
title: Welcome
description: The home page of a navigable Pennington site.
order: 10
---

This site is the styled host from the previous tutorial with one thing added:
a navigation menu in the header.

The menu is not a hand-written list of links. It is built from the content
pipeline — every markdown file under `Content/` becomes an entry, ordered by
its `order:` front matter and grouped by folder. Add a page, and the menu
gains a link with no edit to the layout.

The `guides/` folder shows up as its own section because it holds pages but no
`index.md` of its own.

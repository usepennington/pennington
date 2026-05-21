---
title: Front-matter capabilities and the IFrontMatter contract
description: Why Pennington's front matter is built from mixin interfaces instead of one fat record, and what that buys you.
date: 2024-04-05
author: Jamie Rivers
tags:
  - pennington
  - front-matter
  - architecture
series: Pennington Field Notes
sectionLabel: field-notes
---

`IFrontMatter` is deliberately minimal — every page has a `Title`, full stop.
Everything else opts in through capability interfaces: `ITaggable` for tag
collections, `ISectionable` for section grouping, `IOrderable` for explicit
ordering, `IRedirectable` for aliases.

## Why mixins instead of one record

A single inheritance chain forces every front-matter type to drag along
fields it doesn't use. The capability shape lets a recipe site declare
`Cuisine`/`PrepTime` without inheriting fields meant for blog posts, and lets
the framework discover what a custom type supports by checking interface
implementations instead of probing properties by name.

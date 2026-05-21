---
title: Three xref pitfalls I keep hitting
description: Cross-references survive renames, but only when you do three small things right.
date: 2024-12-09
author: Jamie Rivers
tags:
  - pennington
  - xref
  - authoring
series: Pennington Field Notes
sectionLabel: field-notes
---

`[label](xref:page.uid)` is the canonical Pennington cross-link. It survives
file moves, slug changes, and locale switches because the resolver works off
the page's `uid:` front-matter field rather than its URL.

## Where it goes wrong

One: forgetting to add `uid:` to a page before linking to it. Two: pasting
the same `uid` into two files — the resolver picks one and silently drops
the other. Three: using `<xref:foo>` instead of `[text](xref:foo)` when you
wanted custom link text. The build-time `XrefAuditor` catches the first two
if you read the report.

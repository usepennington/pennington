---
title: Cross-references (source)
description: Link to another page by `uid:` instead of by file path.
tags: [authoring, linking]
sectionLabel: authoring
order: 110
uid: kitchen-sink.main.cross-references-a
---

Every page in this site carries a `uid:` front-matter key — a stable
identifier independent of file location. Link to another page by uid
and the engine resolves it to the canonical URL at render time, even
if the target moves.

## The `<xref:uid>` form

The shorter form reads like a one-liner pointer:

<xref:kitchen-sink.main.cross-references-b>

## The `[text](xref:uid)` form

Use the anchor-style form when you want a custom link label:

See also the [cross-reference target page](xref:kitchen-sink.main.cross-references-b)
for the other half of this pairing.

Either form is resolved by `XrefHtmlRewriter` on the shared response
pipeline — the `uid → URL` lookup happens once, and missing uids produce
a build-report diagnostic.

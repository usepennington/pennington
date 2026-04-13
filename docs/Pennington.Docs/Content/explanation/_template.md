---
title: "TEMPLATE — Explanation"
description: "Template for explanation pages. Duplicate, rename, and replace every placeholder before publishing."
section: "Template"
order: 9999
tags: []
uid: template.explanation
isDraft: true
search: false
llms: false
---

> **In this page.** _One sentence — paste from `docs-toc.md` "Covers" line, trim if needed._
>
> **Not in this page.** _One sentence pointing to the neighboring reference / how-to — paste from `docs-toc.md` "Does not cover" line._

## The question

_One sentence, phrased as the question a reader is carrying: "Why does Pennington use one code path for dev and build?" or "How does the content pipeline move an item from file to HTML?". A page that cannot be reduced to a single question belongs split into two explanation pages._

## Context

_Two to five sentences. What problem does this design solve? What did the alternatives look like? Set the stage; do not start explaining the mechanism yet._

## How it works

_The main section. Two to six subsections, each covering one part of the mechanism. Use a code fence only when a concrete type or signature makes the prose clearer — never to demonstrate usage (that belongs in How-Tos)._

_**Word budget for the whole page: 500–1,500 words.** If it runs longer, it's either two explanations that want splitting, or it's drifting into reference territory — check each paragraph for lookup-shaped sentences that belong elsewhere._

### _Mechanism part one — e.g., "Discovery"_

_A few paragraphs. Prose first; reach for a diagram or code fence only when prose alone leaves the idea fuzzy._

```csharp:xmldocid
T:Pennington._Namespace_._TypeName_
```

_Optional — pull a real type's signature to anchor the explanation. Skip if the prose stands alone._

### _Mechanism part two — e.g., "Parsing"_

_Continue. Aim for narrative continuity across subsections — each should pick up where the previous left off._

## Trade-offs

_Two to four bullets or short paragraphs. Name what this design costs, what it rules out, and which alternatives were considered and why they lost. This is the section that distinguishes an explanation from a reference page — do not skip it._

- **Cost:** _What does this design make harder?_
- **Alternative considered:** _What other shape was on the table, and why was it rejected?_
- **Consequence:** _What does a reader need to keep in mind because of this choice?_

## Further reading

_Two to four cross-quadrant links. Do not link to the next explanation in the same section — that is generated automatically. Prefer pointers outward: to reference pages that catalog the APIs, to how-tos that exercise the mechanism, and to external writing that influenced the design._

- Reference: [_API catalog_](/_path_)
- How-to: [_Task that uses this_](/_path_)
- External: [_Prior art or influence_](_url_)

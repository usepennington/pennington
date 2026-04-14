---
title: "TEMPLATE — Reference"
description: "Template for reference pages. Duplicate, rename, and replace every placeholder before publishing."
sectionLabel: "Template"
order: 9999
tags: []
uid: template.reference
isDraft: true
search: false
llms: false
---

> **In this page.** _One sentence — paste from `docs-toc.md` "Covers" line, trim if needed._
>
> **Not in this page.** _One sentence pointing to the neighboring how-to / explanation — paste from `docs-toc.md` "Does not cover" line._

## Summary

_**One sentence: what it is.** E.g., "The options class that configures a Markdown content source."_
_**One sentence: where it lives.** E.g., "Namespace `Pennington.Pipeline`, used by `AddMarkdownContent<T>`."_

_No "why" sentences on this page — rationale belongs in Explanation. No "here's how you'd use it" sentences — walkthroughs belong in How-Tos. If you cannot say what this thing is in one sentence, the page is scoped wrong._

## Declaration

```csharp:xmldocid
T:Pennington._Namespace_._TypeName_
```

_Show the declaration of the primary type / method / interface this page documents. Use the real production symbol, not an example project — reference pages describe the library, not sample usage._

## Properties / Parameters / Members

_Table format for option classes and parameter lists. Delete the sections that don't apply to this page._

_Order members alphabetically unless the API has an obvious ceremonial order (construction → lifecycle → teardown, or required → optional). Alphabetical wins by default — it makes ctrl-F reliable._

| Name | Type | Default | Description |
|---|---|---|---|
| `_PropertyName_` | `_Type_` | `_default_` | _One-sentence description. No "how to use it" — link to a how-to page instead._ |
| `_AnotherProperty_` | `_Type_` | `_default_` | _Description._ |

_For interfaces / method references, list each member with its own subheading:_

### `_MemberName_`

```csharp:xmldocid
M:Pennington._Namespace_._TypeName_._MemberName_
```

_One to three sentences describing what it does, what it returns, and any invariants. No examples here._

## Example

_One minimal example pulled from an `examples/` project via xmldocid. This is the only narrative content on the page — it exists so a reader recognizes the shape, not to teach usage._

```csharp:xmldocid,bodyonly
M:_ExampleProjectName_._Type_._Member_
```

_A single sentence of context. If you need more than one sentence, that belongs in a How-To._

## See also

_Two to four cross-quadrant links. Other reference pages, the how-to that uses this surface, and the explanation that justifies its design._

- How-to: [_Task-oriented title_](/_path_)
- Related reference: [_Adjacent reference page_](/_path_)
- Background: [_Explanation title_](/_path_)

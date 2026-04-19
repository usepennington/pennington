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

## Summary

_**One sentence: what it is.** E.g., "The options class that configures a Markdown content source."_
_**One sentence: where it lives.** E.g., "Namespace `Pennington.Pipeline`, used by `AddMarkdownContent<T>`."_

_No "why" sentences on this page — rationale belongs in Explanation. No "here's how you'd use it" sentences — walkthroughs belong in How-Tos. If you cannot say what this thing is in one sentence, the page is scoped wrong._

## Declaration

```csharp:xmldocid
T:Pennington._Namespace_._TypeName_
```

_Show the declaration of the primary type / method / interface this page documents. Use the real production symbol, not an example project — reference pages describe the library, not sample usage._

## Members / Parameters / Keys

Pick the authoring mode that matches the source of truth:

### Auto-generated (preferred when the content mirrors declared code)

_Use an existing generator component when the page's content is a 1:1 projection of types, members, or methods:_

- `<ApiMemberTable XmlDocId="T:..." Kind="Properties" />` — properties / methods / fields of a type (also covers Razor component `[Parameter]` props).
- `<ExtensionMethods Receiver="IServiceCollection" />` — every `*Extensions` method whose first (this) parameter short-name matches.
- `<FrontMatterKeys />` — the merged YAML catalog across every `IFrontMatter` implementation.

_These components pull descriptions from xmldoc on the declaring symbol; make sure the xmldoc is accurate before the doc builds. Each row renders through `ApiDefinitionList` (Stripe-style `<dl>`)._

### Hand-authored (for content that isn't a single type or member set)

_Wrap each item in a `<Field>` inside a `<FieldList>`. The `Name` is required; `Type`, `Required`, and `Default` render as metadata pills. The body is markdown — use the first line for extra metadata like "Applies to", "Package", or "Call site" when the minimal Field API does not cover it._

```markdown
<FieldList>
<Field Name="_keyOrArg_" Type="_type_" Default="_default_">
Applies to: _scope_.

_One-sentence description. No "how to use it" — link to a how-to page instead._
</Field>
</FieldList>
```

### Markdown tables (for trivial name → value mappings)

_When a block is genuinely tabular — every row is a short name with no prose — keep a compact markdown table. Don't force a `<FieldList>` when a table is shorter and reads fine._

```markdown
| Kind | Class |
|---|---|
| note | markdown-alert-note |
```

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

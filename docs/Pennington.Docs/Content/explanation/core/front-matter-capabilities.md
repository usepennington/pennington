---
title: "The front-matter capability system"
description: "How IFrontMatter distinguishes universal capabilities (as default members) from selective ones (as separate interfaces) — and why presence of a capability interface is a meaningful signal."
uid: explanation.core.front-matter-capabilities
order: 301030
sectionLabel: "Core Architecture"
tags: [front-matter, capabilities, design, interfaces]
---

Where does the line fall between "every page needs this" and "only some pages need this" when those two categories share a base interface?

## Context

A content engine asks a lot of questions about each page. Is it a draft? Does it belong in search? In the LLM index? Does it carry a cross-reference uid, a description, or a date? These questions apply to every page, even when the answer is trivially "no." A smaller set of questions — does it have tags? does it participate in ordered navigation? does it carry a section label? is it a redirect? — applies only to some content types.

Pennington models this split directly on the type system. The universal questions live on `IFrontMatter` itself, with sensible defaults, so every record answers them without having to opt in. The selective questions live on separate capability interfaces, so a content type that does not implement `ITaggable` is not tagged — and the engine can tell at compile time.

## How it works

### `IFrontMatter`: universal capabilities with defaults

`IFrontMatter` has one abstract member (`Title`) and six default-implemented ones. Every front-matter record inherits `IsDraft => false`, `Search => true`, `Llms => true`, `Uid => null`, `Description => null`, and `Date => null` without declaring them.

```csharp:symbol
src/Pennington/FrontMatter/IFrontMatter.cs > IFrontMatter
```

The contract reads as a typed baseline with opt-outs. A minimal record exposes a single required `Title` property and the engine handles drafts, search indexing, LLM indexing, cross-references, descriptions, and dates gracefully. Engine code uses the members directly — `if (page.IsDraft)` works on every `IFrontMatter` with no pattern-match ceremony.

### The four capability interfaces

Tags, order, section labels, and redirects live on separate interfaces because their adoption is genuinely selective. A blog post has tags but no meaningful order among siblings; a doc page has an order but no redirect target; a redirect stub carries a destination URL and little else. Folding these into `IFrontMatter` would force every record to carry empty tag arrays and meaningless sort keys — and, more importantly, would erase the signal that the interface's *presence* carries.

```csharp:symbol
src/Pennington/FrontMatter/Capabilities.cs > IOrderable
```

`NavigationBuilder` reads `IOrderable` the type, not the value. A content type either participates in ordered navigation or it does not; there is no "this page has no meaningful order" case to handle. The same applies to `ITaggable` (tag cloud participation), `ISectionable` (section-label breadcrumbs), and `IRedirectable` (redirect-stub semantics).

The rule of thumb the shape encodes: if adoption is universal, the member lives on `IFrontMatter` with a sensible default. If adoption is selective, it lives on a capability interface so that pattern-matching on the interface remains meaningful. Seeing `IOrderable` on a record means the content type consciously opted into ordered navigation.

### Custom front-matter records

A custom record buys typed access to extra keys (an `apiVersion` or `gitHubUrl` field becomes a strongly-typed property) plus the same capability-opt-in surface. The defaults give what the shipped records would give for free; the custom record only declares what it adds. See <xref:how-to.pages.front-matter> for the recipe.

## Trade-offs

- **Every `IFrontMatter` is draftable, searchable, and LLM-indexable by default.** Engine code cannot use `is IDraftable` as a gate — the capability is no longer selective. A content type that should never be a draft enforces that by overriding the default member to always return `false`, rather than by omitting an interface.
- **Default members are an interface feature** — they live on the interface itself, not on every implementing type's vtable. Reflection consumers must call `GetMembers()` against `IFrontMatter` (the declaring interface) rather than against the implementing type to see them; the typed defaults are invisible through reflection on the concrete record.
- **A `Dictionary<string, object>` of keys was considered and set aside.** It would flatten the type system entirely — no interface hierarchy, no pattern-matching, no boilerplate — at the cost of IntelliSense on the authoring side and every type mistake moving from compile time to runtime. The typed-contract shape is worth the one-member-per-capability declaration it asks for.
- **The four remaining interfaces carry signal, not boilerplate.** Their presence on a record means the content type opted into that behavior. A review that scans a new front-matter record for missing interfaces is reviewing an authoring decision, not chasing a missing ritual.

## Further reading

- Reference: [IFrontMatter and capability defaults](xref:reference.api.i-front-matter)
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- How-to: [Work with front matter](xref:how-to.pages.front-matter)

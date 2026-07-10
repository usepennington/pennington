---
title: "The front-matter capability system"
description: "How IFrontMatter distinguishes universal capabilities (as default members) from selective ones (as separate interfaces) — and why presence of a capability interface is a meaningful signal."
uid: explanation.core.front-matter-capabilities
order: 4
sectionLabel: "Core Architecture"
tags: [front-matter, capabilities, design, interfaces]
---

Where does the line fall between "every page needs this" and "only some pages need this" when those two categories share a base interface?

## Context

A content engine asks a lot of questions about each page. Is it a draft? Does it belong in search? In the LLM index? Does it carry a cross-reference uid, a description, or a date? These questions apply to every page, even when the answer is trivially "no." A smaller set of questions — does it have tags? does it participate in ordered navigation? does it carry a section label? is it a redirect? — applies only to some content types.

Pennington models this split directly on the type system. The universal questions live on `IFrontMatter` itself, with sensible defaults, so every record answers them without having to opt in. The selective questions live on separate capability interfaces, so a content type that does not implement `ITaggable` is not tagged — and the engine can tell at compile time.

## How it works

### `IFrontMatter`: universal capabilities with defaults

`IFrontMatter` has one abstract member (`Title`) and seven default-implemented ones. Every front-matter record inherits `IsDraft => false`, `Search => true`, `Llms => true`, `SearchOnly => false`, `Uid => null`, `Description => null`, and `Date => null` without declaring them.

```csharp:symbol
src/Pennington/FrontMatter/IFrontMatter.cs > IFrontMatter
```

The contract gives every record common defaults it can override. A minimal record exposes a single required `Title` property and the engine handles drafts, search indexing, LLM indexing, cross-references, descriptions, and dates gracefully. Engine code uses the members directly — `if (page.IsDraft)` works on every `IFrontMatter` without checking for each interface first.

### The capability interfaces

Tags, order, section labels, redirects, and Standard Site document keys live on separate interfaces because the interface's *presence* is itself a signal. Seeing `IOrderable` on a record says the content type consciously participates in ordered navigation; folding it into `IFrontMatter` would erase that distinction, since every record would then carry the member whether it meant anything or not. The selectivity is real, too: a blog post has tags but no meaningful order among siblings; a doc page has an order but no redirect target; a redirect stub carries a destination URL and little else. Folding these into `IFrontMatter` would force every record to carry empty tag arrays and meaningless sort keys.

```csharp:symbol
src/Pennington/FrontMatter/Capabilities.cs > IOrderable
```

`NavigationBuilder` keys off the `IOrderable` interface itself, not a sentinel value in the `Order` property. A content type either implements the interface and participates in ordered navigation, or it does not; there is no "this page has no meaningful order" case to handle. The same applies to `ITaggable` (tag cloud participation), `ISectionable` (section-label breadcrumbs), `IRedirectable` (redirect-stub semantics), and `IStandardSiteDocument` (the AT Protocol record key for Standard Site syndication).

The whole matrix fits in one view. The two shipped records implement different subsets — a doc page orders itself within its section, a blog post syndicates to Standard Site instead — and that difference is visible in the type declarations, not in runtime flags:

```beck
type: class
meta: { direction: TB, spacing: { rank: 64, node: 20 }  }
classes:
  - id: ifm
    name: IFrontMatter
    stereotype: interface
    accent: primary
    fields: ["Title: string", "IsDraft = false", "Search = true", "Llms = true", "SearchOnly = false", "Uid = null", "Description = null", "Date = null"]
  - { id: orderable, name: IOrderable, stereotype: interface, fields: ["Order: int"] }
  - { id: taggable, name: ITaggable, stereotype: interface, fields: ["Tags: string[]"] }
  - { id: sectionable, name: ISectionable, stereotype: interface, fields: ["SectionLabel: string?"] }
  - { id: redirectable, name: IRedirectable, stereotype: interface, fields: ["RedirectUrl: string?"] }
  - { id: standard, name: IStandardSiteDocument, stereotype: interface, fields: ["AtprotoRkey = null"] }
  - { id: doc, name: DocSiteFrontMatter, stereotype: record, accent: info, rank: -1 }
  - { id: blog, name: BlogSiteFrontMatter, stereotype: record, accent: info, rank: 1 }
relations:
  - { from: doc, to: ifm, kind: implements }
  - { from: doc, to: taggable, kind: implements }
  - { from: doc, to: sectionable, kind: implements }
  - { from: doc, to: orderable, kind: implements }
  - { from: doc, to: redirectable, kind: implements }
  - { from: blog, to: ifm, kind: implements }
  - { from: blog, to: taggable, kind: implements }
  - { from: blog, to: sectionable, kind: implements }
  - { from: blog, to: redirectable, kind: implements }
  - { from: blog, to: standard, kind: implements }
```

Both records also implement `IHasStructuredData`, which belongs to the JSON-LD subsystem rather than this capability system and is omitted here.

The rule of thumb is simple: if adoption is universal, the member lives on `IFrontMatter` with a sensible default. If adoption is selective, it lives on a capability interface so that pattern-matching on the interface remains meaningful.

### Custom front-matter records

A custom record buys typed access to extra keys (an `apiVersion` or `gitHubUrl` field becomes a strongly-typed property) plus the same set of capability interfaces to opt into. The defaults give what the shipped records would give; the custom record only declares what it adds. See <xref:how-to.pages.front-matter> for the recipe.

## Further reading

- Reference: [IFrontMatter and capability defaults](xref:reference.api.i-front-matter)
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- How-to: [Work with front matter](xref:how-to.pages.front-matter)

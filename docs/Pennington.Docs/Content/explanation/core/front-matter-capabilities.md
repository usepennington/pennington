---
title: "The front-matter capability system"
description: "Why six capability interfaces collapsed into default members on IFrontMatter, what that buys authors, and which four capabilities stayed separate."
sectionLabel: "Core Architecture"
order: 301030
tags: [front-matter, capabilities, design, interfaces]
uid: explanation.core.front-matter-capabilities
---

Where do you draw the line between "every page needs this" and "only some pages need this" when those two categories share a base interface?

## Context

Pennington originally modeled front-matter capabilities as a strict interface-per-concern taxonomy. The engine asked "does this page implement `IDraftable`?" before honoring the `isDraft` flag; a record that omitted the interface could not be a draft, by design. At its peak the roster ran to ten interfaces: `IFrontMatter`, `IDraftable`, `IDescribable`, `IDateable`, `ICrossReferenceable`, `ISearchable`, `ILlmsIndexable`, `ITaggable`, `ISectionable`, `IOrderable`, and `IRedirectable`. The intent was principled — each capability was explicit and query-able. The practice was less tidy: every shipped front-matter record implemented the first six interfaces identically, so the "capability query" was a ritual that never actually failed. A developer writing a custom front-matter type had to absorb the whole taxonomy before they could get a single page rendered, and any new record started with seven boilerplate declarations before it expressed a single domain-specific property.

## How it works

### Before: a capability zoo

A new `BlogFrontMatter` record required six or seven interface declarations and a matching property body for each before it could describe its first blog-specific field. Code reviewers checking front-matter records learned to scan for missing interfaces rather than missing properties — the discipline became "did the author remember `IDraftable`?" rather than "does the record carry the data it needs?" Engine code reflected this too: `if (page is IDraftable d && d.IsDraft)` reads like a type test, but it was really a guard against an authoring omission. The pattern treated universal and selective capabilities as having the same shape even though their adoption curves were opposite — the first six were always present, the last four were genuinely conditional.

### After: default-member consolidation

Commit `984dc7a` folded the six universally-implemented interfaces into `IFrontMatter` as default-implemented members. Every front-matter record now inherits `IsDraft => false`, `Search => true`, `Llms => true`, `Uid => null`, `Description => null`, and `Date => null` without declaring them. The interface itself says what words alone cannot quite convey:

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

One abstract member (`Title`) and six defaults — the contract now reads as a typed baseline with opt-outs rather than a taxonomy of parallel capabilities. Engine code simplified proportionally: `if (page.IsDraft)` works on every `IFrontMatter`, and there is no pattern-match ceremony to audit.

### The four interfaces that didn't consolidate

Tags, order, section labels, and redirects stayed as separate interfaces, and the reasoning is worth sitting with. A blog post has tags but no meaningful order among siblings; a doc page has an order but no redirect target; a redirect stub carries a destination URL and almost nothing else. Folding these into `IFrontMatter` would force every record to carry empty tag arrays and meaningless sort keys, and — more importantly — every consumer would lose the ability to ask "did this content type actually opt into ordering?" The interface's presence, not the property's value, is what `NavigationBuilder` reads when deciding whether a page participates in ordered navigation at all.

```csharp:xmldocid
T:Pennington.FrontMatter.IOrderable
```

A single-property interface looks like overkill until you consider what it signals. The consolidation encoded a rule of thumb: if adoption is universal, move the member to `IFrontMatter` with a sensible default; if adoption is selective, keep the capability interface so that pattern-matching on it remains meaningful. The four remaining capability interfaces are now a genuine signal rather than boilerplate — seeing `IOrderable` on a record tells you the content type consciously opted into ordered navigation, not that the author copied a template.

### Writing your own front-matter type

The shape an author lands on is: declare a `record`, implement `IFrontMatter`, add whichever capability interfaces the content type genuinely needs, and stop. Because the default members handle drafts, search indexing, LLM indexing, cross-references, descriptions, and dates, a minimal record can expose a single required `Title` property and the engine treats every other concern gracefully. Custom keys are ordinary extra properties on the record — `FrontMatterParser` picks them up through `YamlDotNet`'s `CamelCaseNamingConvention`, so adding a `stability` or `namespace` field is a one-line change with no interface ceremony attached. The kitchen-sink fixture in the test suite shows what "everything at once" looks like without needing to reproduce it here.

## Trade-offs

- **Loss of capability-query semantics.** Engine code can no longer use `is IDraftable` as a gate — every `IFrontMatter` is draftable now, so authors who want a content type that can never be a draft must enforce that by convention or by overriding the default member to always return `false`, rather than by omitting an interface.
- **Default-member version tolerance.** Default interface members are a binary-compatibility feature. Consumers that multi-target older TFMs or that access `IFrontMatter` through reflection need to be aware that these members live on the interface itself, not on every implementing type's vtable.
- **The alternative that was considered and set aside.** A `Dictionary<string, object>` of front-matter keys would have flattened the type system entirely — no interface hierarchy, no pattern-matching, no boilerplate. The tradeoff is that it loses IntelliSense on the authoring side and moves every type mistake from compile time to runtime. Default members preserved the typed-contract shape while removing the ceremony.
- **The four remaining interfaces are now a meaningful signal.** Because the universal capabilities consolidated, `ITaggable`, `IOrderable`, `ISectionable`, and `IRedirectable` carry genuine information — their presence on a record tells you the content type consciously opted into that behavior, not that the author copied a template.

## Further reading

- Reference: [IFrontMatter and capability defaults](xref:reference.front-matter.ifrontmatter)
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)

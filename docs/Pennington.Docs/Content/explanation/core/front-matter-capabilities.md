---
title: "The front-matter capability system"
description: "Why six capability interfaces collapsed into default members on IFrontMatter, what that buys authors, and which four capabilities stayed separate."
sectionLabel: "Core Architecture"
order: 30
tags: [front-matter, capabilities, design, interfaces]
uid: explanation.core.front-matter-capabilities
---

> **In this page.** Why capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) were collapsed into `IFrontMatter` default members, what that buys users, and how to extend with custom keys.
>
> **Not in this page.** Key-by-key documentation — see the [Front matter key reference](/reference/front-matter/keys).

## The question

_One sentence, framing the design question: "If every page needs a title, a draft flag, and a uid, but only doc pages need an order and only a handful of pages need a redirect target, where do you put the line between 'universal contract' and 'opt-in capability'?" Keep it a single sentence so the rest of the page has one thread to pull on._

## Context

_Three to four sentences. Explain that Pennington started with a strict capability-interface model — one interface per front-matter concern — so the engine could ask "does this page implement `IDraftable`?" before honoring the `isDraft` flag, and a page that didn't implement the interface simply could not be a draft. Note the ten-interface pre-commit-`984dc7a` roster: `IFrontMatter`, `IDraftable`, `IDescribable`, `IDateable`, `ICrossReferenceable`, `ISearchable`, `ILlmsIndexable`, `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable`. Point out that in practice every shipped front-matter record implemented the first six, so the "capability query" was a ritual that never failed — each new record had to re-declare the same six properties as the one before it, and authors writing a custom front-matter type had to know the whole taxonomy before they could put a page on disk. Do not start explaining the consolidation here; this is just the stage._

## How it works

_The mechanism section. Keep the narrative moving forward: the "zoo" framing, the default-member fix, the four capabilities that deliberately did not consolidate, and a short pointer at writing custom front matter. Prose first; drop to a code fence only where the type's shape says something words can't._

### Before: a capability zoo

_Two to three sentences. Sketch what the old world looked like: a new `BlogFrontMatter` record was seven interface declarations and seven property bodies before the first blog-specific property; reviewers had to scan for a missing interface rather than a missing property; and the engine code used `if (page is IDraftable d && d.IsDraft)` everywhere, which read as a type check but was really a "did the author remember to implement `IDraftable`?" check. End with the observation that this treated "universal" and "opt-in" capabilities as the same shape even though they had opposite adoption curves._

### After: default-member consolidation

_Three to four sentences. Explain that commit `984dc7a` merged the six universally-implemented interfaces into `IFrontMatter` as default-implemented members, so every front-matter record now inherits sensible defaults (`IsDraft => false`, `Search => true`, `Llms => true`, `Uid => null`, `Description => null`, `Date => null`) without declaring them. Point at the declaration and let it speak for itself — one required member, six defaulted members, nothing optional about which pages "participate" in drafts or indexing. Call out that engine code simplified too: `if (page.IsDraft)` works on every `IFrontMatter`, replacing the pattern-match ceremony._

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

_One sentence. Note the single abstract member (`Title`) and the six defaults — the interface now reads as a contract plus a set of opt-outs rather than a contract plus a capability taxonomy._

### The four interfaces that didn't consolidate

_Three to four sentences. Explain the judgment call: tags, order, section labels, and redirects are not universal — a blog post has tags but no order; a doc page has an order but no redirect target; a redirect stub has a redirect URL and not much else. Folding them into `IFrontMatter` would force every record to carry empty arrays and meaningless defaults, and every consumer would lose the ability to say "does this page actually want to be ordered?" Keeping them as real interfaces means `NavigationBuilder` can cast to `IOrderable` to decide whether to use the declared sort key or fall back to alphabetic order, and `BlogSiteContentService` can cast to `ITaggable` to decide whether to emit a tag-index page. End with the rule of thumb the consolidation encoded: if adoption is universal, default it on the base interface; if adoption is selective, keep the capability interface so consumers can pattern-match meaningfully._

```csharp:xmldocid
T:Pennington.FrontMatter.IOrderable
```

_One sentence. Point out that a single-property interface looks like overkill until you remember it is load-bearing — the interface's presence, not the property's value, is what `NavigationBuilder` reads when deciding whether a page opted into ordering at all._

### Writing your own front-matter type

_Three to four sentences. Describe the shape authors land on: declare a `record`, implement `IFrontMatter` for the universals, add whichever capability interfaces the content type actually needs, and stop. The default members mean a minimal record can be a single required `Title` property — the engine will happily treat every other concern as "use the default." Custom keys are just extra properties on the record, automatically picked up by `FrontMatterParser` through `YamlDotNet`'s `CamelCaseNamingConvention`, so adding a `stability` or `namespace` field is a one-line change with no interface ceremony. Reference the kitchen-sink fixture as the "everything at once" shape without reproducing it here._

## Trade-offs

_Three to four bullets. Name what the consolidation costs, what it ruled out, and what readers need to carry forward. The first bullet is the main one — you can no longer ask "is this page even draftable?" because every page now is. The second is the version-tolerance cost. The third is the author-friendliness win. Keep them bluntly phrased; this section is the difference between explanation and reference._

- **Cost: loss of capability-query semantics.** _Engine code can no longer use `is IDraftable` as a gate — every `IFrontMatter` is draftable now, so authors who want "this content type can never be a draft" must enforce it by convention (or by overriding the default member to always return `false`) rather than by omitting an interface._
- **Cost: default-member version tolerance.** _Default interface members are a binary-compat feature, not a language feature — consumers that multi-target older TFMs or that use `IFrontMatter` through reflection need to be aware that the members exist on the interface itself, not on every implementing type's vtable._
- **Alternative considered: "capability bag" dictionary.** _A `Dictionary<string, object>` of front-matter keys was on the table and rejected — it would have flattened the type system entirely, but it loses IntelliSense on the authoring side and moves every type mistake from compile time to runtime. Default members preserved the typed-contract shape while removing the boilerplate._
- **Consequence: the four remaining interfaces are now meaningful.** _Because the universal ones consolidated, the four capability interfaces that stayed separate (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) are a real signal — seeing one on a record tells you the content type genuinely opts into that behavior, not just that the author copied a template._

## Further reading

_Three cross-quadrant links. Do not link to the neighboring explanation (`dev-vs-build`, `response-processing`) — those are auto-linked. Point outward to the reference catalog, the authoring how-to, and (optionally) external writing on default interface members._

- Reference: [IFrontMatter and capability defaults](/reference/front-matter/ifrontmatter)
- Reference: [Front matter key reference](/reference/front-matter/keys)
- How-to: [Work with front matter](/how-to/content-authoring/front-matter)

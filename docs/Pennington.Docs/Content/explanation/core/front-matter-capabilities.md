---
title: "The front-matter capability system"
description: "Why Pennington collapsed six universal capability interfaces into IFrontMatter default members, what remained separate, and how to extend the model with custom keys."
section: "core"
order: 30
tags: []
uid: explanation.core.front-matter-capabilities
isDraft: true
search: false
llms: false
---

> **In this page.** Why capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) were collapsed into `IFrontMatter` default members, what that buys users, and how to extend with custom keys.
>
> **Not in this page.** Key-by-key documentation of every front-matter property — see the Reference quadrant.

## The question

Why does Pennington model front-matter capabilities the way it does — a single interface carrying most metadata as defaults, plus four orthogonal sibling interfaces — rather than one big base record or a flat dictionary?

## Context

- Every page in a Pennington site carries YAML front matter parsed into a typed record by `FrontMatterParser`.
- Consumers across the engine (sitemap, search, llms.txt, navigation, redirects) need to ask each parsed metadata object "do you carry a date? a section? a redirect?" without knowing which concrete record type produced it.
- An earlier design split ten behaviors across ten interfaces (`IFrontMatter`, `IDraftable`, `IDescribable`, `IDateable`, `ICrossReferenceable`, `ISearchable`, `ILlmsIndexable`, `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable`) so that each capability could be pattern-matched in isolation.
- In practice six of those ten were implemented by every real front-matter record, which pushed users into copy-pasting the same property list — and pushed the engine into the same `is IDraftable d ? d.IsDraft : false` ceremony at every consumption site.
- Commit `984dc7a` reshapes the model around the observation that "universal" belongs on the base contract, while genuinely optional capabilities stay as sibling interfaces.

## How it works

### One contract, seven properties, one required

`IFrontMatter` now carries the six former "universal" capabilities as C# default interface members so a minimal implementation only has to supply `Title`. Everything else has a sensible default and can be overridden by declaring a property with the same name.

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

A record that wants the defaults gets them for free. A record that wants to surface `Description` in YAML simply declares `public string? Description { get; init; }` and the YamlDotNet deserializer (via `FrontMatterParser`'s camelCase naming convention) populates it. The engine always sees the interface, so consumers never branch on "does this type implement `IDescribable`?" — they just read `metadata.Description`.

### Four sibling interfaces for genuinely optional shape

Four capabilities stayed separate because their presence meaningfully changes how the engine behaves, not just what it displays:

- `ITaggable` — the record opts into tag-based indexing and tag-page generation.
- `ISectionable` — the record participates in navigation grouping.
- `IOrderable` — the record wants explicit sort order inside a section instead of alphabetical.
- `IRedirectable` — the page is a redirect stub and must short-circuit rendering, sitemap inclusion, and llms.txt.

Keeping these as interfaces preserves the pattern-match shape consumers want: `MarkdownContentService` skips redirect records with `if (fm is IRedirectable { RedirectUrl.Length: > 0 })`; `SitemapBuilder` does the same; `RazorPageContentService` asks `entry.Metadata is IOrderable orderable` before pulling an order. Each check is local to the behavior it guards, and no record is forced to carry properties it will never populate.

### What users write now

`DocFrontMatter` picks up `ITaggable, ISectionable, IOrderable` because doc pages group into sections and order within them. `BlogFrontMatter` picks up only `ITaggable` — blog posts sort by date, not manual order, so the record doesn't implement `IOrderable`. User-defined records follow the same idiom: declare `IFrontMatter` for free universal metadata, then add the sibling interfaces that match the page's actual shape.

### Extending the system

Adding a new capability is a three-line move: define the interface (`public interface IAuthorable { string? Author { get; } }`), implement it on the records that need it, and pattern-match `metadata is IAuthorable a` wherever the new behavior belongs. Adding a new *universal* field is a single-property addition to `IFrontMatter` with a default — existing records keep compiling and the rest of the engine picks up the new member through the interface. Custom YAML keys that are not modeled on the interface still round-trip through `FrontMatterParser` if declared on the concrete record; consumers that only see `IFrontMatter` simply won't know about them, which is the correct layering.

## Trade-offs

- **Cost — default members tie the engine to a single .NET version floor.** Default interface members require a modern target (Pennington targets .NET 11 / C# 15), so projects stuck on older runtimes cannot consume the interface directly. For Pennington this is a deliberate platform choice, not a regression.
- **Alternative considered — keep all ten interfaces.** Rejected because six of them were implemented by every real type, which meant the composition story added ceremony without adding expressiveness. A capability you always implement is not a capability; it is part of the contract.
- **Alternative considered — collapse everything onto one fat base record.** Rejected because `IRedirectable` and the other four genuinely optional capabilities drive control flow: a redirect stub must skip rendering, sitemap inclusion, and llms.txt. Surfacing these as sibling interfaces lets consumers encode that branching as a type test instead of a nullable-field check, and lets records honestly declare "I don't do this."
- **Consequence — opting *out* of a universal capability is still possible but less visible.** A record that wants `Search => false` by default must override the property explicitly; there is no `ISearchable` interface to omit. Teams building search-excluded content types should set `Search = false` on the record's property, not assume absence of the interface.

## Further reading

- Reference: [Front matter key reference](/reference/front-matter/keys)
- Reference: [`IFrontMatter` and capability defaults](/reference/front-matter/ifrontmatter)
- How-to: [Work with front matter](/how-to/content-authoring/front-matter)
- External: [C# default interface methods proposal](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-8.0/default-interface-methods)

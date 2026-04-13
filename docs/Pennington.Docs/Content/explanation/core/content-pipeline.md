---
title: "The content pipeline and union types"
description: "Why ContentItem and ContentSource are union types, and how items move from DiscoveredItem to ParsedItem to RenderedItem (or FailedItem)."
section: "core"
order: 10
tags: []
uid: explanation.core.content-pipeline
isDraft: true
search: false
llms: false
---

> **In this page.** Why `ContentItem` and `ContentSource` are union types, how items move from `DiscoveredItem` through `ParsedItem` to `RenderedItem` (or `FailedItem`), and what the union affords over a class hierarchy.
>
> **Not in this page.** The specific interfaces that parsers and renderers implement — see the Reference section for `IContentParser`, `IContentRenderer`, and `IContentPipeline`.

## The question

- Framed as one question: "Why does Pennington model its pipeline as two unions — `ContentItem` and `ContentSource` — instead of a base class or a single record with optional fields?"
- Anchor the reader: they have seen the four case names in passing and want to know what the shape buys us.

## Context

- Pennington processes heterogeneous inputs (Markdown files, Razor pages, redirects, programmatic generators) and has to carry each one through several stages before writing HTML.
- The natural temptation is a `ContentItem` base class with nullable `Metadata`, nullable `RawMarkdown`, nullable `RenderedContent` — the "bag of optionals" shape every static-site generator eventually grows.
- Alternative considered: a tagged enum plus a parallel data record — the discriminator-and-payload pattern — which catches some mistakes but leaks the discriminator into every call site.
- C# 15 closed unions let us name the four legal states directly and let the compiler enforce that we handled each one. That is the lever this page is about.

## How it works

### Two unions, two axes of variation

- `ContentSource` discriminates **origin**: `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`. It answers "where did this come from and how do I load it?"
- `ContentItem` discriminates **stage**: `DiscoveredItem`, `ParsedItem`, `RenderedItem`, `FailedItem`. It answers "how far through the pipeline has this travelled?"
- Keeping the two axes separate keeps each union small and each case meaningful — a `DiscoveredItem` carries a `ContentSource` because the stage doesn't care what kind of source it is, only that one exists.
- Every case carries a `ContentRoute`, and `ContentItem` exposes it as a unified property (pattern-matching once on the union so every caller doesn't have to).

```csharp:xmldocid
T:Pennington.Pipeline.ContentItem
```

### The stage progression

- `DiscoveredItem(Route, Source)` — produced by `IContentService.DiscoverAsync`; just a route and an origin, nothing read yet.
- `ParsedItem(Route, IFrontMatter Metadata, string RawMarkdown)` — the parser has read the file, split front matter from body, and typed the metadata.
- `RenderedItem(Route, Metadata, RenderedContent)` — the renderer has produced HTML, an outline, tags, and cross-references.
- `FailedItem(Route, ContentError)` — a terminal state any stage can produce; once an item is failed it passes through subsequent stages untouched.
- This is a one-way progression: the type system makes it impossible to hand a `RenderedItem` back to the parser, and impossible to forget that a stage could fail.

### Failure as a first-class case, not an exception

- `FailedItem` sits next to the success cases rather than being signalled out-of-band. A thrown exception in `IContentParser.ParseAsync` is caught and demoted to a `FailedItem` carrying the route and a `ContentError`.
- The pipeline's `switch` over `ContentItem` is therefore total without any `try`/`catch` decoration at the call site — the failure path is just another case.
- This is how a single bad Markdown file produces a per-page error in `BuildReport` instead of aborting the whole build, and why `FailedItem`s propagate through `RenderAsync` and `GenerateAsync` unchanged.

### What the compiler buys us

- Exhaustive matching: every `switch` over `ContentItem` that omits a case fails to compile. When a fifth case ever lands, every consumer surfaces as a build error — the shape of the pipeline cannot drift silently.
- No nullable bags: `ParsedItem.RawMarkdown` is non-nullable because the type itself means "parse succeeded." A `DiscoveredItem` simply doesn't have that field to forget about.
- No runtime downcasts: pattern matching on case types replaces `as`/`is` chains. The union is the single source of truth for "which shape am I looking at."

## Trade-offs

- **Cost.** Adding a new stage means editing every `switch` — the compiler will find them, but it's still a pile of edits. The pipeline has to be stable enough to earn this. (It is: discover, parse, render, generate is the shape every static-site tool converges on.)
- **Alternative considered: single-record with optional fields.** Rejected because every consumer has to re-derive "which stage is this?" from nullability, and because a consumer that forgets the nullability silently produces garbage instead of a build error.
- **Alternative considered: class hierarchy with virtual methods.** Rejected because the methods that operate on items live in the pipeline and in the build reporter, not on the items themselves — giving each case a method breaks locality and invites the "god class" shape where `ContentItem` grows every cross-cutting concern.
- **Consequence.** `FailedItem` is always in the iterable. Consumers that want "only the pages that shipped" filter to `RenderedItem` explicitly — there is no partial-success middle ground that hides errors.

## Further reading

- Reference: the `IContentPipeline` / `IContentParser` / `IContentRenderer` surface (link once the Reference page exists).
- Reference: `RenderedContent`, `ContentError`, and the `ContentSource` cases.
- Explanation: the front-matter capability model — how `IFrontMatter` interacts with `ParsedItem.Metadata`.
- External: Scott Wlaschin, "Designing with Types: Making Illegal States Unrepresentable" — the broader idea the pipeline shape rests on.

---
title: "Why ContentSource is a union"
description: "Why the case-discriminated union — MarkdownFileSource, RazorPageSource, RedirectSource, EndpointSource, LlmsOnlySource — beats the polymorphic alternatives, and why every consumer goes through `.Value`."
uid: explanation.core.content-source
order: 2
sectionLabel: "Core Architecture"
tags: [pipeline, unions, content-source]
---

`ContentSource` is the second of Pennington's two pipeline unions. Where `ContentItem` discriminates a page's stage in the pipeline, `ContentSource` discriminates *where the page came from*. The five cases — `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `EndpointSource`, `LlmsOnlySource` — capture every origin Pennington ships. <xref:explanation.core.content-pipeline> covers why the pipeline as a whole is shaped as a union; this page is about why this *particular* shape, and why the polyfill choice matters more than it looks.

## Why a union and not polymorphism

A first instinct is to make `ContentSource` an interface with six implementations and dispatch through virtual methods. That solves the dispatch problem but introduces three drag points the union avoids:

- **Exhaustiveness disappears.** With an interface, adding a seventh implementation later compiles silently — every existing switch keeps its `default` clause and quietly stops covering the new case. With the union, the compiler complains at every switch that no longer covers every case. New cases land with a known fix-up surface.
- **The "no canonical body" cases get awkward.** `RedirectSource` carries a `TargetUrl` and nothing else; `EndpointSource` carries no payload at all; `LlmsOnlySource` carries a path that explicitly never produces HTML. Modelling them as interface implementations forces them to satisfy the same shape as `MarkdownFileSource`, which actually has a path to read and an HTML body to render. The union lets each case carry exactly its own data.
- **Pattern matching becomes the natural read.** Consumers want to *branch on the case*, not call a virtual method that wraps the branch. With a union, `source.Value switch { MarkdownFileSource md => …, RedirectSource r => … }` is exactly the read; with polymorphism, you'd either bake the consumer logic into each implementation (poor separation) or end up with a visitor.

## Why `.Value` and not the case type directly

Pennington multi-targets `net10.0;net11.0`. On `net11.0+` the C# 15 `union` keyword synthesizes a discriminated union with an inner `object? Value` field; on `net10.0` a hand-written polyfill struct provides the same shape. Going through `.Value` is the one read that compiles unchanged on both TFMs and matches what every consumer in the codebase already does.

The polyfill could have exposed a different surface — a constructor-as-pattern shortcut that pattern-matches against the case type without unwrapping. That shortcut works on `net10.0` only, and looks slightly cleaner there. It also breaks the moment a reader looks at the `net11.0` build. The design choice is to make the multi-TFM cost visible in every read site rather than hide it behind a shape that diverges silently between target frameworks.

## Why `RedirectSource`, `EndpointSource`, and `LlmsOnlySource` exclude themselves from `sitemap.xml`

All three cases name a route, none owns the canonical HTML for it. `RedirectSource` has no body at all — the response is a 30x to another URL. `EndpointSource` defers the body to a sibling `MapGet` whose response is the canonical HTML at that URL; from `SitemapService`'s perspective, the content service knows the route but does not own the page. `LlmsOnlySource` has no HTML page anywhere — it only contributes to the llms.txt index and its sidecar markdown.

The sitemap filters them out in one expression:

```csharp
if (discovered.Source.Value is RedirectSource or EndpointSource or LlmsOnlySource) continue;
```

The alternative — a per-case `IncludeInSitemap` bool — would push that exclusion rule into every consumer that builds a sitemap-shaped output (the canonical sitemap, an RSS variant, a llms.txt index). Centralising it in one `is …or …` line keeps the rule discoverable: when a future case needs the same treatment, the diff is one token.

## See also

- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service) — the recipe for constructing each case and pattern-matching it.
- Reference: [Sitemap configuration](xref:how-to.feeds.sitemap)

---
title: "Why ContentSource is a union"
description: "Why the case-discriminated union — FileSource, RazorPageSource, RedirectSource, EndpointSource, LlmsOnlySource — beats the polymorphic alternatives, and why every consumer goes through `.Value`."
uid: explanation.core.content-source
order: 2
sectionLabel: "Core Architecture"
tags: [pipeline, unions, content-source]
---

`ContentSource` is the second of Pennington's two pipeline unions. Where `ContentItem` discriminates a page's stage in the pipeline, `ContentSource` discriminates *where the page came from*. The five cases — `FileSource`, `RazorPageSource`, `RedirectSource`, `EndpointSource`, `LlmsOnlySource` — capture every origin Pennington ships. `FileSource` carries a path *and* a format key (`"markdown"`, `"cook"`, …), so one case covers every file-backed format — the key selects the parser and renderer, which is how a custom format like Cooklang flows through the same pipeline as markdown (see <xref:how-to.content-services.custom-content-format>). <xref:explanation.core.content-pipeline> covers why the pipeline as a whole is shaped as a union; this page is about why this *particular* shape, and why the polyfill choice matters more than it looks.

## Why a union and not polymorphism

A first instinct is to make `ContentSource` an interface with five implementations and dispatch through virtual methods. That solves the dispatch problem but introduces three problems the union avoids:

- **Exhaustiveness disappears.** With an interface, adding a sixth implementation later compiles silently — every existing switch keeps its `default` clause and quietly stops covering the new case. With the union, the compiler complains at every switch that no longer covers every case. A new case shows you exactly which switches need updating.
- **The "no canonical body" cases get awkward.** `RedirectSource` carries a `TargetUrl` and nothing else; `EndpointSource` carries no payload at all; `LlmsOnlySource` carries a path that explicitly never produces HTML. Modeling them as interface implementations forces them to satisfy the same shape as `FileSource`, which carries a path to read and a format key naming its parser and renderer. The union lets each case carry exactly its own data.
- **Pattern matching reads directly.** Consumers want to *branch on the case*, not call a virtual method that wraps the branch. With a union, `source.Value switch { FileSource f => …, RedirectSource r => … }` is exactly the read; with polymorphism, you'd either bake the consumer logic into each implementation (poor separation) or end up with a visitor.

## Why `.Value` and not the case type directly

Every consumer reads the case through `.Value` — the one read that compiles unchanged on both TFMs the library multi-targets, and what the whole codebase already does. Why that read is the portable one, and what the `net10.0` shim does underneath the `net11.0` `union` keyword, is <xref:explanation.positioning.sdk-and-the-union-shim>'s story to tell.

## Why `RedirectSource` and `LlmsOnlySource` exclude themselves from `sitemap.xml`

Both name a route with no canonical HTML page to advertise. `RedirectSource` has no body at all — the response is a 30x to another URL. `LlmsOnlySource` has no HTML page anywhere — it only contributes to the llms.txt index and its sidecar markdown. Neither belongs in a crawler's list of indexable pages, so the sitemap filters them out in one expression:

```csharp
if (discovered.Source.Value is RedirectSource or LlmsOnlySource) continue;
```

`EndpointSource` is the case that *looks* like it should join them but doesn't. It defers rendering to a sibling `MapGet`, but that endpoint returns real, canonical HTML at a stable URL — a custom content service's pages, an `AddTaxonomy` term page. Those are exactly what a sitemap is for, so they stay in, and the on-site search index and the sitemap stay consistent about which pages exist. Transport endpoints that happen to use `EndpointSource` — a JSON data route, a generated feed — emit a non-`.html` output file and are dropped earlier by `SitemapService`'s output-extension check, so the source filter only has to name the two cases that never produce an HTML page.

## See also

- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service) — the recipe for constructing each case and pattern-matching it.
- How-to: [Sitemap configuration](xref:how-to.feeds.sitemap)

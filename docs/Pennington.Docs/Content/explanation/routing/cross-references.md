---
title: "Cross-reference resolution"
description: "How uids are gathered during the content phase, how XrefHtmlRewriter resolves tags at request time, and how broken xrefs surface in the build report."
section: "routing"
order: 30
tags: []
uid: explanation.routing.cross-references
isDraft: true
search: false
llms: false
---

> **In this page.** How `uid`s are gathered during the content phase, how `XrefHtmlRewriter` resolves tags at request time, and how broken xrefs surface in the build report.
>
> **Not in this page.** Authoring cross-references (see How-Tos).

## The question

Why does Pennington resolve `xref:` links at request time against a lookup built from content metadata, instead of resolving them inline during the render phase?

## Context

- Cross-references let authors link by stable identifier (`uid`) rather than URL, so pages survive being moved or renamed and the engine can cross-link across content services (markdown, Razor, blog, programmatic) without hard-wiring URLs into source markdown.
- The naive design is "resolve during render": a Markdig extension walks link targets, looks each one up, and writes the final `href` into the rendered HTML. That design conflates two concerns — the uid catalog (global, cross-service, file-watched) and the render pass (per-item, stateless) — and forces every content service to own its own resolver.
- Pennington splits the problem across two phases that already existed for other reasons. The content phase gathers uids as a side-effect of discovery; the response pipeline already owns an HTML-rewriting pass that runs after rendering. Xref resolution slots in as one more rewriter on the response, sharing the same AngleSharp document as locale and base-URL rewriting.
- The payoff is that broken xrefs are *observable*: they travel the same diagnostic path as other per-request warnings, which means they surface in the dev overlay live *and* in the build report when the static build crawls pages.

## How it works

### Phase one — uids are gathered from content services

Every `IContentService` exposes `GetCrossReferencesAsync()`, which returns the `(uid, Title, ContentRoute)` triple for each page it owns that declares a `Uid` in front matter. `MarkdownContentService` walks its parsed metadata and emits a `CrossReference` for every page whose `IFrontMatter.Uid` is non-empty; other services (blog, Razor, llms.txt, SPA navigation) implement the method to match.

```csharp:xmldocid
T:Pennington.Pipeline.CrossReference
```

`XrefResolver` consumes this enumeration once, builds a case-insensitive `ImmutableDictionary<string, CrossReference>` behind an `AsyncLazy`, and hands out entries through `ResolveAsync(uid)`.

```csharp:xmldocid
T:Pennington.Infrastructure.XrefResolver
```

The resolver is registered via `AddFileWatched<XrefResolver>()`, so when a markdown file with a `uid:` front-matter key changes, the factory throws the cached instance away and the next request rebuilds the lookup. This is the same file-watching pattern `MarkdownLinkResolver`, `SearchIndexService`, and `SitemapService` use — xref catalog is conceptually content metadata, not pipeline state.

### Phase two — request-time rewriting on the shared document

The response-processor pipeline owns a single AngleSharp pass (`HtmlResponseRewritingProcessor`, order 10) that every `IHtmlResponseRewriter` participates in. `XrefHtmlRewriter` is the first rewriter (order 10) and runs in two sub-phases because xref syntax has two shapes with different parsing needs.

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

The two syntaxes:

- `<xref:some.uid>` — a self-closing pseudo-tag. Not valid HTML, so AngleSharp would either drop it or wrap it unpredictably if it reached the DOM parser. `PreParseAsync` runs first, does a regex substitution over the raw response string (`<xref:([^>]+)>`), and replaces each hit with a real `<a>` element. Only after this string-level pass does the orchestrator hand the body to AngleSharp.
- `[text](xref:some.uid)` — authors write this in markdown; Markdig renders it as `<a href="xref:some.uid">text</a>`, which *is* valid HTML. `ApplyAsync` then runs on the already-parsed document, queries `a[href^='xref:']`, and rewrites each matching anchor's `href` to the resolved canonical path. If the anchor text was itself `xref:...` (the tag-syntax case after Phase A), the rewriter also substitutes the resolved page title as the link text.

Both sub-phases delegate to `XrefResolvingService`, which looks each uid up in the file-watched `XrefResolver`. Because xref rewriting runs *before* `LocaleLinkHtmlRewriter` (order 20) and `BaseUrlHtmlRewriter` (order 30), the canonical paths it emits are what the locale and base-URL rewriters then transform — resolution, localization, and transport prefixing form a stack rather than competing passes.

### Broken xrefs — one path from rewriter to build report

When `XrefResolver.ResolveAsync(uid)` returns `null`, the rewriter does two things. It leaves a breadcrumb in the rendered HTML — the anchor keeps its `href="xref:uid"`, gains `data-xref-error="Reference not found"` and `data-xref-uid="..."` attributes, and (for the tag syntax) shows the raw uid as its text so the reader sees something unresolved rather than nothing. And it calls `diagnostics.AddWarning($"Unresolved xref: {uid}", "XrefResolver")` on the scoped `DiagnosticContext`.

That warning then travels the same path as every other per-request diagnostic:

1. `ResponseProcessingMiddleware` drains `DiagnosticContext` just before flushing and writes each diagnostic as an `X-Pennington-Diagnostic` header (`Severity|Message|Source`).
2. In dev serve, `DiagnosticOverlayProcessor` reads the same context and injects a visible overlay, so unresolved xrefs are noticed while the author is editing.
3. In build mode, `OutputGenerationService` issues real HTTP GETs against the running host, parses the `X-Pennington-Diagnostic` headers on each response, and calls `reportBuilder.AddDiagnostic(new BuildDiagnostic(severity, route, message, source))` — attaching the per-page `ContentRoute` so the warning has provenance.
4. `BuildReport.WriteTo` groups diagnostics by severity. Unresolved xrefs land in the `WARNINGS` section, tagged with the canonical path of the page that contained the broken link. A broken xref does *not* fail the build by itself; `BuildReport.HasErrors` reacts to `DiagnosticSeverity.Error`, broken outbound links, and `FailedItem`s — xref warnings count as advisory.

Because the rewriter emits the warning regardless of whether the request came from a browser or from the build crawler, the reporting path is identical in dev and build. The `X-Pennington-Diagnostic` header is the single wire format, and the only thing that changes between modes is whether the warning lands in the overlay or in the build report.

## Trade-offs

- **Cost:** Two passes instead of one. The tag-syntax regex pass runs over the full response string before any HTML parsing, and the link-syntax DOM pass walks `a[href^='xref:']` on the shared document. For pages with no xref syntax both passes short-circuit early, but the regex pass still scans the body looking for a match. In exchange, the rewriter never has to own its own parser and stays coherent with the other rewriters.
- **Alternative considered:** Resolve uids inside a Markdig extension at render time. Rejected because the resolver depends on the full uid catalog built from every `IContentService`, not just the markdown one — the content phase is where uids are *gathered*, not where they should be *consumed*. A render-time resolver would also need its own cache invalidation story parallel to the file-watching machinery the rest of the engine already uses.
- **Alternative considered:** Fail the build on unresolved xrefs. Rejected because the broken-xref warning leaves a visible breadcrumb in the HTML (`data-xref-error` plus the raw uid as text), so the page still ships something a human can see, and content authors routinely stage content across multiple edits. Treating xref warnings as errors would punish normal in-progress authoring; if a site wants hard enforcement, `BuildReport.Diagnostics` is enumerable and a CI script can escalate on its own.
- **Consequence:** A uid is only as stable as its front-matter declaration. Renaming a uid is a breaking change for every page that linked to it, and the broken link will surface only at request time — not at the moment the rename happens. The build-report warning is the backstop; without it, a stale xref would rot invisibly.

## Further reading

- Reference: [Diagnostics — request context](/reference/diagnostics/request-context)
- Reference: [Build report](/reference/diagnostics/build-report)
- How-to: [Add cross-references between pages](/how-to/content/add-cross-references)
- Explanation: [Response processing](/explanation/core/response-processing)

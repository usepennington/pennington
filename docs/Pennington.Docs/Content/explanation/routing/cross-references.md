---
title: "Cross-reference resolution"
description: "Why Pennington links pages by symbolic uid rather than filesystem path, and how the two-phase resolver turns those uids into canonical URLs without the authoring cost of hand-coded links."
uid: explanation.routing.cross-references
order: 3
sectionLabel: "Routing and Navigation"
tags: [cross-references, xref, routing, rewriters]
---

Why does Pennington resolve links through a symbolic `uid` indirection rather than letting authors write the URL of the target page directly?

## Context

The filesystem location of a markdown file is an unstable coordinate. Renames, reorganizations, and section moves all change the URL, and every hand-written link across the site then has to be found and updated. Relative links make the problem slightly smaller — they only break when the source or the target moves — but they do not eliminate it, and they fold poorly across locales where the same logical target has a different URL per language.

A `uid:` declared in front matter is a coordinate the author controls. Moving the file does not move the uid, and translated copies can share one uid across locales. The cost of this stability is indirection: at render time the engine must look the uid up and substitute the real URL. The rest of this page describes the shape of that lookup and why it runs in two passes.

## How it works

### Collection phase: uid → URL map

When the host starts, `XrefResolver` asks every registered `IContentService` for its `GetCrossReferencesAsync()` and folds the results into one case-insensitive `ImmutableDictionary<string, CrossReference>`. The lookup is built lazily on first resolve and cached via `AsyncLazy<T>`; because `XrefResolver` is registered with `AddFileWatched<T>`, any change under a watched content path tears down the cached resolver instance and the next request reconstructs it. Renaming a file or editing a front-matter `uid:` is therefore visible on the very next response without an explicit cache bust.

Uid collection is a content-service concern, not a markdown concern. `MarkdownContentService<T>` contributes uids by reading the optional `Uid` member on `IFrontMatter` for each discovered page; `RazorPageContentService` contributes none by default; a custom `IContentService` can synthesize `CrossReference` records for rows in a JSON feed or entries in a database. The resolver does not care where the uid came from — it only sees `(uid, title, route)` triples and picks the first one it encounters for each uid. That first-write-wins rule is what lets a default-locale route shadow later fallback duplicates without any special handling.

Every `XrefResolver` instance sees a single snapshot of the uid table, and replacing the instance is the only way the table changes — there is no `Refresh()` method.

### Pre-parse tag pass (`<xref:uid>`)

`XrefHtmlRewriter.PreParseAsync` runs before AngleSharp sees the response body, because `<xref:uid>` is not valid HTML. An HTML parser would try to interpret it as an unknown element, and the `:uid` portion would be lost or coerced into an attribute during error recovery. A regex substitution instead walks every match on the raw string and replaces it with a real `<a>` element whose `href` is the resolved canonical path and whose text content is the resolver's stored title. Downstream DOM-based rewriters — locale prefixing, base-URL stamping — receive ordinary HTML they can parse normally.

The tag form is the one to reach for when you want the engine to supply both the URL and the link text. `<xref:explanation.routing.url-paths>` renders as an anchor whose visible text is the target page's title with no additional markup from the author. If the uid is missing, the substitution still happens but the anchor carries `data-xref-error` and `data-xref-uid` attributes, making the broken link visible in dev-tools and stylable in CSS. A `DiagnosticContext` warning is accumulated on the request in parallel so the problem surfaces in both the dev overlay and the build report.

### DOM attribute pass (`href="xref:uid"`)

The second form is the markdown-native `[text](xref:uid)`. Markdig renders it as an ordinary `<a href="xref:uid">text</a>`, which AngleSharp parses without complaint. `XrefHtmlRewriter.ApplyAsync` selects `a[href^='xref:']` on the shared document, resolves each uid, and rewrites only the `href` — the anchor's existing text content is preserved because the author chose it. The one exception is the rare case where the text and the href are the same literal `xref:uid` string, which happens when `<xref:foo>` is used as a bare markdown link with no `[text]` wrapper; in that case the resolver substitutes the stored title so the anchor renders meaningfully.

This pass operates on the same `IDocument` that `LocaleLinkHtmlRewriter` and `BaseUrlHtmlRewriter` will mutate next — the three rewriters share one AngleSharp parse/serialize round trip owned by `HtmlResponseRewritingProcessor`. The order they run in matters: xref resolution runs at `Order => 10` so the canonical paths it emits are visible to the locale prefixer, which must see real URLs rather than symbolic ones. The two-phase split exists because the two link forms are genuinely different syntactic problems. One is not parseable HTML and has to be rewritten as a string; the other is parseable HTML and is cleaner to rewrite on the DOM. Merging them into a single pass would require parsing the document first, which would defeat the purpose of the pre-parse stage.

### Broken xrefs as diagnostics

An unresolved uid is never a hard failure. Both resolver phases emit a `Warning` to the scoped `DiagnosticContext`, and the response still renders — the anchor carries the `data-xref-error="Reference not found"` attribute so the reader sees a link that does not work rather than a crashed page. In dev serve, `DiagnosticOverlayProcessor` collects the context and renders the warning into the corner panel of the running site.

During a static build, the unified dev-and-build code path carries this through. `OutputGenerationService` crawls the live host with an `HttpClient`, and `ResponseProcessingMiddleware` serializes the accumulated `DiagnosticContext` into `X-Pennington-Diagnostic` response headers. `OutputGenerationService` reads those headers off each response and folds the warnings into the `BuildReport` alongside the broken-link, missing-trailing-slash, and render-failure entries. The result is that every unresolved uid emitted during the crawl shows up in the printed build report and, if it reaches `Error` severity, sets the process exit code — without Pennington needing a separate "verify xrefs" build step.

## Further reading

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor) — the member catalog for `IHtmlResponseRewriter`, `XrefHtmlRewriter`, and execution order.
- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — cross-reference tag and attribute syntax alongside the other non-CommonMark features.
- How-to: [Cross-reference pages by `uid`](xref:how-to.navigation.cross-references) — the authoring recipe for setting `uid:` and linking with `<xref:uid>` or `[text](xref:uid)`.
- Related explanation: [The response-processing pipeline](xref:explanation.core.response-processing) — why xref resolution lives inside the shared AngleSharp pass and runs first in the rewriter order.
- Related explanation: [URL paths and content routes](xref:explanation.routing.url-paths) — the `UrlPath`/`ContentRoute` value types that `CrossReference` carries into the resolver.

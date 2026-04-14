---
title: "Cross-reference resolution"
description: "Why Pennington links pages by symbolic uid rather than filesystem path, and how the two-phase resolver turns those uids into canonical URLs without the authoring cost of hand-coded links."
uid: explanation.routing.cross-references
order: 30
sectionLabel: "Routing and Navigation"
tags: [cross-references, xref, routing, rewriters]
---

> **In this page.** _Paraphrase the Covers line: how `uid`s are gathered from every content service during the content phase, how `XrefHtmlRewriter` resolves `<xref:uid>` tags and `href="xref:uid"` attributes at request time, and how unresolved uids surface as diagnostics in the build report. One sentence._
>
> **Not in this page.** _Paraphrase the Does-not-cover line: pointer readers who want the authoring shape — setting `uid:` in front matter and writing `[text](xref:uid)` — to the how-to at `/how-to/content-authoring/cross-references`. One sentence._

## The question

_One sentence in the reader's voice, something like: "Why does Pennington resolve links through a symbolic `uid` indirection instead of just letting authors write the URL of the target page directly?" Do not answer yet; the rest of the page is the answer._

## Context

_Three to five sentences. The filesystem location of a markdown file is an unstable coordinate: renames, reorganizations, and section moves all change the URL, and every hand-written link across the site then has to be found and fixed. Relative links make the problem slightly smaller (they only break when the source or the target moves) but they don't eliminate it, and they fold poorly across locales, where the "same" target page has a different URL per language. A `uid:` declared in front matter is a coordinate the author controls — moving the file does not move the uid, and translated copies can share one uid across locales. The cost is an indirection: at render time the engine has to look the uid up and substitute the real URL. The rest of this page describes the shape of that lookup and why it runs in two passes._

## How it works

### Collection phase: uid → URL map

_Two or three paragraphs. When the host starts, `XrefResolver` asks every registered `IContentService` for its `GetCrossReferencesAsync()` and folds the results into one case-insensitive `ImmutableDictionary<string, CrossReference>`. The lookup is built lazily on first resolve and cached via `AsyncLazy<T>`; because `XrefResolver` is registered with `AddFileWatched<T>`, any change under a watched content path tears down the cached resolver instance and the next request reconstructs it — so renaming a file or editing a front-matter `uid:` is visible on the very next response without an explicit cache bust._

_Note that uid collection is a content-service concern, not a markdown concern. `MarkdownContentService<T>` contributes uids by reading the optional `Uid` member on `IFrontMatter` for each discovered page; `RazorPageContentService` contributes none by default; a custom `IContentService` (see `ExtensibilityLabExample.ReleaseNotesContentService`) can synthesize `CrossReference` records for rows in a JSON feed or columns in a database. The resolver does not care where the uid came from — it only sees `(uid, title, route)` triples and picks the first one it encounters for each uid, which is the "first-write-wins" rule that lets a default-locale route shadow later fallback duplicates._

```csharp:xmldocid
T:Pennington.Infrastructure.XrefResolver
```

_After the fence, surface the invariant in one sentence: every resolver instance sees a single snapshot of the uid table, and replacing the instance is the only way the table changes. Call it out so readers do not reach for a `Refresh()` method that does not exist._

### Pre-parse tag pass (`<xref:uid>`)

_Two paragraphs. `XrefHtmlRewriter.PreParseAsync` runs before AngleSharp sees the response body, because `<xref:uid>` is not valid HTML — an HTML parser would try to interpret it as an unknown element with a child text node and the `:uid` portion would be lost or coerced into an attribute. A tight regex substitution walks every match and replaces it with a real `<a>` element whose `href` is the resolved canonical path and whose text content is the resolver's stored title. The rewrite happens on the raw string so downstream DOM-based rewriters (locale prefixing, base-URL stamping) receive already-materialized HTML they can parse normally._

_Point out that the tag form is the one authors reach for when they want the engine to supply both the URL and the link text — `<xref:explanation.routing.url-paths>` renders as an anchor whose visible text is the target page's title. If the uid is missing, the substitution still happens but the anchor carries `data-xref-error` and `data-xref-uid` attributes so the broken link is visible in dev-tools and stylable in CSS; a `DiagnosticContext` warning is accumulated on the request in parallel._

```csharp:xmldocid
M:Pennington.Infrastructure.XrefResolvingService.ResolveXrefTagsAsync(System.String,Pennington.Diagnostics.DiagnosticContext)
```

### DOM attribute pass (`href="xref:uid"`)

_Two paragraphs. The second form is the markdown-native `[text](xref:uid)` — Markdig renders it as an ordinary `<a href="xref:uid">text</a>`, which AngleSharp parses happily. `XrefHtmlRewriter.ApplyAsync` selects `a[href^='xref:']` on the shared document, resolves each uid, and rewrites only the `href` — the anchor's existing text content is preserved because the author chose it. The one exception is the rare case where the text and the href are the same literal `xref:uid` string (which happens if the author wrote `<xref:foo>` as a bare markdown link with no `[text]` wrapper); in that case the resolver substitutes the stored title so the anchor renders meaningfully._

_This pass operates on the same `IDocument` that `LocaleLinkHtmlRewriter` and `BaseUrlHtmlRewriter` will mutate next — the three rewriters share one AngleSharp parse/serialize round trip owned by `HtmlResponseRewritingProcessor`. Ordering is load-bearing: xref resolution runs at `Order => 10` specifically so the canonical paths it emits are visible to the locale prefixer, which must see real URLs, not symbolic ones. The two-phase split exists because the two link forms are genuinely different syntactic problems — one is not parseable HTML and has to be rewritten as a string, the other is parseable HTML and is cleaner to rewrite on the DOM._

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

### Broken xrefs as diagnostics

_Two or three paragraphs. An unresolved uid is never a hard failure. Both resolver phases emit a `Warning` to the scoped `DiagnosticContext` via `AddWarning($"Unresolved xref: {uid}", "XrefResolver")`, and the response still renders — the anchor carries the `data-xref-error="Reference not found"` attribute so the reader sees a link that does not work rather than a crashed page. In dev serve, `DiagnosticOverlayProcessor` collects the context and renders the warning into the corner panel of the running site._

_During a static build, the unified dev-and-build code path pays off. `OutputGenerationService` crawls the live host with an `HttpClient`, and `ResponseProcessingMiddleware` serializes the accumulated `DiagnosticContext` into `X-Pennington-Diagnostic` response headers. `OutputGenerationService.ParseDiagnosticHeaders` reads those headers off each response and folds the warnings into the `BuildReport` alongside the broken-link, missing-trailing-slash, and render-failure entries. The upshot is that every unresolved uid emitted during the crawl shows up in the printed build report and, if it reached `Error` severity, would set the process exit code — without Pennington needing a separate "verify xrefs" build step._

## Trade-offs

- **Cost:** _The uid indirection only helps if authors consistently set `uid:` in front matter. A page with no uid can be linked to only by URL, which is exactly the fragility the feature was meant to avoid — the discipline has to live in the authoring convention. The reference and how-to pages should lean on this, because the compiler cannot enforce "pick a uid" the way it enforces "pick a title."_
- **Cost:** _First-write-wins collection means that two content services contributing the same uid silently resolve to whichever source was registered first. This is the right call for the locale-fallback case it was designed for, but a custom `IContentService` that shadows an existing uid will do so without a warning. Authors integrating bespoke content sources should pick a uid namespace prefix (e.g. `release-1.2.0`) rather than reusing short generic strings._
- **Alternative considered:** _Hard-coded relative or absolute URLs. Rejected because the whole point of the feature is to decouple link identity from filesystem location — and because relative links compose poorly across locales, where the same logical target has a different URL per language._
- **Alternative considered:** _A single-phase DOM rewrite that parses the response first and then resolves `<xref:...>` inside the AngleSharp tree. Rejected because `<xref:uid>` is not valid HTML and an HTML parser's error-recovery behavior would eat the colon and the uid; the only robust way to handle the tag form is to rewrite it as a string before parsing. Keeping a second DOM pass for the attribute form avoids re-parsing valid HTML for no reason._
- **Consequence:** _Because resolution happens at response time rather than at parse time, a renamed target page's URL updates everywhere on the next request without rebuilding anything. But it also means broken uids are a runtime concern — the build report is the feedback loop, and skipping the build step before deploy is how stale uids reach production. The report is the safety net; use it._

## Further reading

- Reference: [Response processing interfaces](/reference/extension-points/response-processing) — the member catalog for `IHtmlResponseRewriter`, `XrefHtmlRewriter`, and execution order.
- Reference: [Markdown extensions catalog](/reference/markdown/extensions) — cross-reference tag and attribute syntax alongside the other non-CommonMark features.
- How-to: [Cross-reference pages by `uid`](/how-to/content-authoring/cross-references) — the authoring recipe for setting `uid:` and linking with `<xref:uid>` or `[text](xref:uid)`.
- Related explanation: [The response-processing pipeline](/explanation/core/response-processing) — why xref resolution lives inside the shared AngleSharp pass and runs first in the rewriter order.
- Related explanation: [URL paths and content routes](/explanation/routing/url-paths) — the `UrlPath`/`ContentRoute` value types that `CrossReference` carries into the resolver.

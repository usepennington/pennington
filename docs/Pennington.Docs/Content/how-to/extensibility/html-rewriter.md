---
title: "Write an HTML rewriter"
description: "Implement IHtmlResponseRewriter to mutate rendered HTML — pick PreParseAsync for non-HTML tokens, ApplyAsync for DOM edits, and slot your Order into the shared AngleSharp pass."
uid: how-to.extensibility.html-rewriter
order: 203050
sectionLabel: Extensibility
tags: [html-rewriting, extensibility, anglesharp, response-pipeline]
---

> **In this page.** _Paraphrase the TOC "Covers" line: implementing `IHtmlResponseRewriter`, choosing between `PreParseAsync` (raw string pass, for non-HTML tokens like `<xref:uid>`) and `ApplyAsync` (DOM pass over the shared AngleSharp document), and picking an `Order` that cooperates with the three built-in rewriters (`XrefHtmlRewriter` at 10, `LocaleLinkHtmlRewriter` at 20, `BaseUrlHtmlRewriter` at 30)._
>
> **Not in this page.** _Paraphrase the TOC "Does not cover" line: building a brand-new Markdig extension to alter markdown rendering at parse time — this recipe only covers post-render HTML mutation. For earlier-phase transforms see [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor) or author a Markdig extension directly against [MarkdownPipelineFactory]._

## When to use this

_Two sentences. Frame the goal: the reader wants to mutate HTML that has already been rendered — rewrite anchors, inject attributes, normalise URLs, strip sentinels — without reparsing the document themselves. Cross-link, do not re-teach: for non-HTML response types (JSON, plain text) or passes that must see the final byte stream after every rewriter has run, use [Write a response processor](xref:how-to.extensibility.response-processor) instead — that's the wider `IResponseProcessor` seam this rewriter plugs into._

## Assumptions

_Three bullets. Each is realistic prior state, not a tutorial step._

- You have an existing Pennington site rendering HTML pages (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- You understand that the `HtmlResponseRewritingProcessor` parses each response body with AngleSharp exactly once and invokes every registered rewriter against that shared `IDocument` — so your `ApplyAsync` mutates the same tree the xref, locale, and base-URL rewriters already touched.
- You know which phase fits your edit: a non-HTML token (something that is not valid HTML structure, like `<xref:uid>` or a sentinel comment) belongs in `PreParseAsync`; anything queryable by selectors belongs in `ApplyAsync`.

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `AnchorLowercaseRewriter.cs` exercises both halves of the contract and `Program.cs` registers it against a bare `AddPennington` host.

---

## Steps

### 1. Implement `IHtmlResponseRewriter`

_One sentence. The contract is four members: `Order`, `ShouldApply(HttpContext)`, `PreParseAsync(string, HttpContext)`, and `ApplyAsync(IDocument, HttpContext)` — the example class below demonstrates all four in one sealed type._

```csharp:xmldocid
T:ExtensibilityLabExample.AnchorLowercaseRewriter
```

### 2. Gate work with `ShouldApply`

_Two sentences. `ShouldApply` runs per-response; return `false` to skip both phases cheaply when the content-type, path, or headers mean you have nothing to do. The example narrows to `text/html` responses so non-HTML endpoints (search index JSON, llms.txt) bypass the rewriter entirely._

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.ShouldApply(Microsoft.AspNetCore.Http.HttpContext)
```

### 3. Use `PreParseAsync` for non-HTML tokens

_Two sentences. `PreParseAsync` receives the raw HTML string before AngleSharp parses it and returns the string to parse — use it only when the construct you are rewriting is not valid HTML structure (raw `<xref:uid>` tags are the canonical shipped example; the lab strips a sentinel comment). Return the input unchanged when there is nothing to do so you do not pay for an allocation on every response._

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.PreParseAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

### 4. Use `ApplyAsync` for DOM edits

_Two sentences. `ApplyAsync` receives the already-parsed `IDocument` shared by every rewriter in this pass — query with `QuerySelectorAll`, mutate attributes and text, and return; do not re-serialize or reparse. The example lowercases the text content of every `<a data-lowercase>`; more typical uses are href canonicalisation, `loading="lazy"` on images, or stamping `rel="noopener"` on external links._

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.ApplyAsync(AngleSharp.Dom.IDocument,Microsoft.AspNetCore.Http.HttpContext)
```

### 5. Pick an `Order` that cooperates with built-ins

_Two sentences. The three shipped rewriters run at 10 (`XrefHtmlRewriter`), 20 (`LocaleLinkHtmlRewriter`), and 30 (`BaseUrlHtmlRewriter`) — choose a number above 30 if you want to see already-resolved xref/locale/base hrefs, below 10 if you need to preempt xref resolution, or between the built-ins only when you deliberately want to slot into that chain. The example uses 500 so anchors are lowercased after every transport-layer transform has landed._

```csharp:xmldocid
P:ExtensibilityLabExample.AnchorLowercaseRewriter.Order
```

### 6. Register the rewriter in DI

_One sentence. `HtmlResponseRewritingProcessor` resolves every registered `IHtmlResponseRewriter` from the container and sorts by `Order`, so a single `AddSingleton<IHtmlResponseRewriter, T>()` next to your host wiring is all it takes._

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/lowercase-demo/`.
- Expect every `<a data-lowercase>` anchor text to be lowercase in the rendered HTML, and `<!--LOWERCASE-SENTINEL-->` to be absent from view-source.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/lowercase-demo/index.html` to confirm the rewriter also runs during publish.

## Related

- Reference: [Response processing interfaces](xref:reference.extension-points.response-processing)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
- Related how-to: [Write a response processor](xref:how-to.extensibility.response-processor)

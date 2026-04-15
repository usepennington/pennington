---
title: "Write an HTML rewriter"
description: "Implement IHtmlResponseRewriter to mutate rendered HTML — pick PreParseAsync for non-HTML tokens, ApplyAsync for DOM edits, and slot your Order into the shared AngleSharp pass."
uid: how-to.extensibility.html-rewriter
order: 203050
sectionLabel: Extensibility
tags: [html-rewriting, extensibility, anglesharp, response-pipeline]
---

Use this approach to mutate HTML that has already been rendered — rewrite anchors, inject attributes, normalise URLs, strip sentinels — without reparsing the document by hand. For non-HTML response types (JSON, plain text) or passes that need the final byte stream after every rewriter has run, use <xref:how-to.extensibility.response-processor> instead.

## Assumptions

- An existing Pennington site rendering HTML pages (see the <xref:tutorials.getting-started.first-site> if not).
- Awareness that `HtmlResponseRewritingProcessor` parses each response body with AngleSharp exactly once and invokes every registered rewriter against that shared `IDocument` — so `ApplyAsync` mutates the same tree the xref, locale, and base-URL rewriters already touched.
- A clear sense of which phase fits the edit: a non-HTML token (something that is not valid HTML structure, like `<xref:uid>` or a sentinel comment) belongs in `PreParseAsync`; anything queryable by selectors belongs in `ApplyAsync`.

For a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `AnchorLowercaseRewriter.cs` exercises both halves of the contract and `Program.cs` registers it against a bare `AddPennington` host.

---

## Steps

### 1. Implement `IHtmlResponseRewriter`

The contract requires four members: `Order`, `ShouldApply(HttpContext)`, `PreParseAsync(string, HttpContext)`, and `ApplyAsync(IDocument, HttpContext)` — the example class below demonstrates all four in one sealed type.

```csharp:xmldocid
T:ExtensibilityLabExample.AnchorLowercaseRewriter
```

### 2. Gate work with `ShouldApply`

`ShouldApply` runs per-response; return `false` to skip both phases when the content-type, path, or headers mean there is nothing to do. The example narrows to `text/html` responses so non-HTML endpoints (search index JSON, llms.txt) bypass the rewriter entirely.

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.ShouldApply(Microsoft.AspNetCore.Http.HttpContext)
```

### 3. Use `PreParseAsync` for non-HTML tokens

`PreParseAsync` receives the raw HTML string before AngleSharp parses it and returns the string to parse — use it only when the target construct is not valid HTML structure (raw `<xref:uid>` tags are the canonical shipped example; the lab strips a sentinel comment). Return the input unchanged when there is nothing to do, to avoid paying for an allocation on every response.

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.PreParseAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

### 4. Use `ApplyAsync` for DOM edits

`ApplyAsync` receives the already-parsed `IDocument` shared by every rewriter in this pass — query with `QuerySelectorAll`, mutate attributes and text, and return; do not re-serialize or reparse. The example lowercases the text content of every `<a data-lowercase>`; more typical uses include href canonicalisation, `loading="lazy"` on images, or stamping `rel="noopener"` on external links.

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.ApplyAsync(AngleSharp.Dom.IDocument,Microsoft.AspNetCore.Http.HttpContext)
```

### 5. Pick an `Order` that cooperates with built-ins

The three shipped rewriters run at 10 (`XrefHtmlRewriter`), 20 (`LocaleLinkHtmlRewriter`), and 30 (`BaseUrlHtmlRewriter`) — choose a number above 30 to see already-resolved xref/locale/base hrefs, below 10 to preempt xref resolution, or between the built-ins only to deliberately slot into that chain. The example uses 500 so anchors are lowercased after every transport-layer transform has landed.

```csharp:xmldocid
P:ExtensibilityLabExample.AnchorLowercaseRewriter.Order
```

### 6. Register the rewriter in DI

`HtmlResponseRewritingProcessor` resolves every registered `IHtmlResponseRewriter` from the container and sorts by `Order`, so a single `AddSingleton<IHtmlResponseRewriter, T>()` next to the host wiring is sufficient.

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

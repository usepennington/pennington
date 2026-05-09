---
title: "Rewrite HTML attributes after parsing"
description: "Implement IHtmlResponseRewriter to mutate already-parsed HTML — lowercase anchors, normalise hrefs, stamp rel=noopener — without reparsing the document by hand."
uid: how-to.response-pipeline.html-rewriter
order: 210010
sectionLabel: "Response Pipeline"
tags: [html-rewriting, extensibility, anglesharp, response-pipeline]
---

To rewrite anchors, inject attributes, normalise URLs, or strip sentinels in already-rendered HTML, implement `IHtmlResponseRewriter`. Pennington's `HtmlResponseRewritingProcessor` parses each response body with AngleSharp exactly once and invokes every registered rewriter against that shared `IDocument`, so the work composes with the built-in xref, locale, and base-URL passes. For non-HTML response types (JSON, plain text) or work that needs the final byte stream, use <xref:how-to.response-pipeline.response-processor> instead.

## Before you begin

- An existing Pennington site rendering HTML pages (see <xref:tutorials.getting-started.first-site> if not).
- A clear sense of which phase fits the edit: a non-HTML token (something not valid HTML structure, like `<xref:uid>` or a sentinel comment) belongs in `PreParseAsync`; anything queryable by selectors belongs in `ApplyAsync`.

For a working setup, see `examples/ExtensibilityLabExample` — `AnchorLowercaseRewriter.cs` exercises both halves of the contract and `Program.cs` registers it against a bare `AddPennington` host.

## Implement the rewriter

`IHtmlResponseRewriter` has four members: `Order`, `ShouldApply(HttpContext)`, `PreParseAsync(string, HttpContext)`, and `ApplyAsync(IDocument, HttpContext)`. The example at `examples/ExtensibilityLabExample/AnchorLowercaseRewriter.cs` demonstrates all four in one sealed type.

`ShouldApply` runs per-response; return `false` to skip both phases when the content-type, path, or headers mean there is nothing to do. The example narrows to `text/html` responses so non-HTML endpoints (search index JSON, llms.txt) bypass the rewriter entirely.

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.ShouldApply(Microsoft.AspNetCore.Http.HttpContext)
```

`PreParseAsync` receives the raw HTML string before AngleSharp parses it and returns the string to parse — use it only when the target construct is not valid HTML structure (raw `<xref:uid>` tags are the canonical shipped example; the lab strips a sentinel comment). Return the input unchanged when there is nothing to do, to avoid paying for an allocation on every response.

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.PreParseAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

`ApplyAsync` receives the already-parsed `IDocument` shared by every rewriter in this pass — query with `QuerySelectorAll`, mutate attributes and text, and return; do not re-serialize or reparse. The example lowercases the text content of every `<a data-lowercase>`; more typical uses include href canonicalisation, `loading="lazy"` on images, or stamping `rel="noopener"` on external links.

```csharp:xmldocid
M:ExtensibilityLabExample.AnchorLowercaseRewriter.ApplyAsync(AngleSharp.Dom.IDocument,Microsoft.AspNetCore.Http.HttpContext)
```

## Pick an Order value

The three shipped rewriters run at 10 (`XrefHtmlRewriter`), 20 (`LocaleLinkHtmlRewriter`), and 30 (`BaseUrlHtmlRewriter`) — choose a number above 30 to see already-resolved xref/locale/base hrefs, below 10 to preempt xref resolution, or between the built-ins only to deliberately slot into that chain. The example uses 500 so anchors are lowercased after every transport-layer transform has landed.

```csharp:xmldocid
P:ExtensibilityLabExample.AnchorLowercaseRewriter.Order
```

## Register the implementation

`HtmlResponseRewritingProcessor` resolves every registered `IHtmlResponseRewriter` from the container and sorts by `Order`, so a single `AddSingleton` next to the host wiring is sufficient.

```csharp
builder.Services.AddSingleton<IHtmlResponseRewriter, AnchorLowercaseRewriter>();
```

## Result

Anchors marked `data-lowercase` have their text content lowercased, and the sentinel comment is gone from view-source.

Before:

```html
<!--LOWERCASE-SENTINEL-->
<a data-lowercase href="/docs/">Read the DOCS</a>
<a data-lowercase href="/blog/">Latest POSTS</a>
```

After:

```html
<a data-lowercase href="/docs/">read the docs</a>
<a data-lowercase href="/blog/">latest posts</a>
```

Anchors without `data-lowercase` and non-HTML responses pass through unchanged.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/lowercase-demo/`.
- Expect every `<a data-lowercase>` anchor text to be lowercase in the rendered HTML, and `<!--LOWERCASE-SENTINEL-->` to be absent from view-source.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/lowercase-demo/index.html` to confirm the rewriter also runs during publish.

## Related

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
- Related how-to: [Write a response processor](xref:how-to.response-pipeline.response-processor)

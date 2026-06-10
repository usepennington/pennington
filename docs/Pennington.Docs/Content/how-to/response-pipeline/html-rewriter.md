---
title: "Rewrite HTML attributes after parsing"
description: "Implement IHtmlResponseRewriter to mutate already-parsed HTML — lowercase anchors, normalize hrefs, stamp rel=noopener — sharing the document parse with every other rewriter."
uid: how-to.response-pipeline.html-rewriter
order: 1
sectionLabel: "Response Pipeline"
tags: [html-rewriting, extensibility, anglesharp, response-pipeline]
---

To rewrite anchors, inject attributes, normalize URLs, or strip sentinels in already-rendered HTML, implement `IHtmlResponseRewriter`. Every rewriter shares one AngleSharp parse against the same `IDocument`. For non-HTML response types (JSON, plain text) or work that needs the final byte stream, use <xref:how-to.response-pipeline.response-processor> instead.

The recipe references `examples/ExtensibilityLabExample/AnchorLowercaseRewriter.cs`, which exercises both phases of the contract against a bare `AddPennington` host.

## Before you begin

- An existing Pennington site rendering HTML pages (see <xref:tutorials.getting-started.first-site> if not).
- A clear sense of which phase fits the edit: a non-HTML token (something not valid HTML structure, like `<xref:uid>` or a sentinel comment) belongs in `PreParseAsync`; anything queryable by selectors belongs in `ApplyAsync`.

## Write the rewriter

Implement <xref:reference.api.i-html-response-rewriter> as a sealed class. Three rules carry the page:

- `ShouldApply` runs per-response; return `false` to skip both phases when the content-type, path, or headers mean there is nothing to do. The example narrows to `text/html` responses so non-HTML endpoints (search index JSON, llms.txt) bypass the rewriter entirely.
- `PreParseAsync` receives the raw HTML string and returns the string to parse. Use it only when the target construct is not valid HTML structure — raw `<xref:uid>` tags are the canonical shipped example. Return the input unchanged when there is nothing to do.
- `ApplyAsync` receives the already-parsed `IDocument` shared by every rewriter — query with `QuerySelectorAll`, mutate attributes and text, and return. Do not re-serialize or reparse.

```csharp:symbol
examples/ExtensibilityLabExample/AnchorLowercaseRewriter.cs
```

## Pick an Order value

The shipped rewriters occupy `Order` values from 10 (xref resolution) through 60 (the last built-in transform); xref resolution, locale prefixing, and base-URL prefixing run in that relative order because each produces the link form the next one consumes. Pick above 60 to run after every shipped transform, below 10 to run before xref resolution, or between the built-ins only when that placement is deliberate. For the exact `Order` of each shipped rewriter, see <xref:reference.api.i-html-response-rewriter>. The example uses 500 so anchors are lowercased after every shipped transform has run.

## Register the rewriter

Every registered `IHtmlResponseRewriter` is picked up and ordered by its `Order` value, so a single registration next to the host wiring is sufficient. Use the lifetime that matches your dependencies — `AddSingleton` for stateless rewriters, `AddTransient` (or `AddFileWatched`) when the rewriter captures file-watched state.

```csharp
builder.Services.AddSingleton<IHtmlResponseRewriter, AnchorLowercaseRewriter>();
```

## Configure the shipped word-break rewriter

One shipped rewriter you configure rather than implement is the word-break rewriter. `AddWordBreak` turns it on; it inserts `<wbr>` break opportunities into long identifiers so dotted namespaces and PascalCase names wrap inside narrow columns instead of overflowing.

```csharp
builder.Services.AddWordBreak(options =>
{
    options.CssSelector = "h1, h2, h3, h4, h5, h6, span, .text-break";
    options.MinimumCharacters = 20;
});
```

A heading like `Pennington.Infrastructure.WordBreakOptions` then renders with breaks after each dot and before each interior case boundary:

Before:

```html
<h3>Pennington.Infrastructure.WordBreakOptions</h3>
```

After:

```html
<h3>Pennington.<wbr>Infrastructure.<wbr>WordBreakOptions</h3>
```

For every option and its default, see <xref:reference.api.word-break-options>.

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

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/lowercase-demo/`. Every `<a data-lowercase>` anchor text is lowercase in the rendered HTML and `<!--LOWERCASE-SENTINEL-->` is absent from view-source.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/lowercase-demo/index.html` to confirm the rewriter also runs during publish.

## Related

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Reference: [`WordBreakOptions`](xref:reference.api.word-break-options) — the shipped word-break rewriter's configuration
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
- Related how-to: [Write a response processor](xref:how-to.response-pipeline.response-processor)

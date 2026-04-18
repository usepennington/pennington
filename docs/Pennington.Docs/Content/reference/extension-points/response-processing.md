---
title: "Response processing interfaces"
description: "The two response-rewriting contracts (IResponseProcessor, IHtmlResponseRewriter) with execution order and the three built-in rewriters that ship in Pennington."
sectionLabel: "Extension Points"
order: 405030
tags: [response-processing, html-rewriting, extension-points, middleware]
uid: reference.extension-points.response-processing
---

## `IResponseProcessor`

<ApiSummary XmlDocId="T:Pennington.Infrastructure.IResponseProcessor" />

<ApiMemberTable XmlDocId="T:Pennington.Infrastructure.IResponseProcessor" Kind="All" />

## `IHtmlResponseRewriter`

<ApiSummary XmlDocId="T:Pennington.Infrastructure.IHtmlResponseRewriter" />

<ApiMemberTable XmlDocId="T:Pennington.Infrastructure.IHtmlResponseRewriter" Kind="All" />

## Built-in rewriters

One row per built-in `IHtmlResponseRewriter`, in execution order.

| Rewriter | Order | Purpose |
|---|---|---|
| `XrefHtmlRewriter` | 10 | Resolves `<xref:uid>` tags in `PreParseAsync` and `href="xref:uid"` attributes in `ApplyAsync`, delegating both phases to `XrefResolvingService`. |
| `LocaleLinkHtmlRewriter` | 20 | Prefixes internal anchor `href`s with the active locale when the request is serving a non-default locale. |
| `BaseUrlHtmlRewriter` | 30 | Prefixes root-relative `href`, `src`, and `action` attributes with the configured base URL and stamps `data-base-url` on `<body>`. |

## Example

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.AnchorLowercaseRewriter
```

## See also

- How-to: [Write a response processor](xref:how-to.extensibility.response-processor)
- How-to: [Write an HTML rewriter](xref:how-to.extensibility.html-rewriter)
- Related reference: [Routing types](xref:reference.extension-points.routing)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)

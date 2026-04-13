---
title: Write an HTML rewriter
description: Implement `IHtmlResponseRewriter`, choose between `PreParseAsync` and `ApplyAsync`, and register so every rewriter shares one AngleSharp pass.
section: extensibility
order: 50
tags: []
uid: how-to.extensibility.html-rewriter
isDraft: true
search: false
llms: false
---

> **In this page.** Implementing `IHtmlResponseRewriter`, when to use `PreParseAsync` vs `ApplyAsync`, and how rewriters share one AngleSharp pass.
>
> **Not in this page.** Creating brand-new Markdig extensions.

## When to use this

- You need to mutate the rendered HTML of every response — injecting attributes, rewriting link targets, stamping a marker on `<body>` — without re-parsing the DOM yourself.
- You want your transform to participate in the same AngleSharp parse/serialize cycle as `XrefHtmlRewriter`, `LocaleLinkHtmlRewriter`, and `BaseUrlHtmlRewriter`, not as a standalone `IResponseProcessor`.
- You are rewriting a construct that is valid HTML (use `ApplyAsync`) or one that is not valid HTML and must be substituted before parsing (use `PreParseAsync`).

## Assumptions

- You have a Pennington site wired with `AddPennington(...)` and `UsePennington()`.
- You are comfortable with `AngleSharp.Dom.IDocument` / `IElement` query and mutation APIs.
- You understand that `HtmlResponseRewritingProcessor` already owns the parse/serialize cycle — your rewriter must not parse the body itself.

No `examples/` project currently demonstrates a custom `IHtmlResponseRewriter`. The three built-in rewriters in `src/Pennington/` are the worked examples; the fences below resolve against the core library solution.

---

## Steps

### 1. Implement `IHtmlResponseRewriter`

- Reference the contract: one required `Order`, one required `ShouldApply`, default `PreParseAsync`, required `ApplyAsync`.
- Put the class wherever you keep your site's infrastructure types — naming convention is `<Purpose>HtmlRewriter`.
- Return immediately from `ShouldApply` when the rewriter is inapplicable; the orchestrator skips both phases and, if every rewriter opts out, skips DOM parsing entirely.

```csharp:xmldocid
T:Pennington.Infrastructure.IHtmlResponseRewriter
```

### 2. Pick the right phase — `PreParseAsync` vs `ApplyAsync`

- Use `ApplyAsync` for anything expressible in the DOM: attributes, element insertion, query selectors, text-node edits. This is the default; most rewriters only override `ApplyAsync`.
- Use `PreParseAsync` only when the input is **not** valid HTML and therefore cannot survive AngleSharp parsing intact. The canonical case is the raw `<xref:uid>` tag that `XrefHtmlRewriter` substitutes before parsing.
- Both phases receive the same `HttpContext`; `PreParseAsync` returns the mutated string, `ApplyAsync` mutates the shared `IDocument` in place.
- Every rewriter's `PreParseAsync` runs first (in `Order`), then every rewriter's `ApplyAsync` runs (in `Order`) against the single parsed document.

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

### 3. Set `Order` to slot into the rewriter chain

- Rewriters run in ascending `Order`; built-ins are `XrefHtmlRewriter = 10`, `LocaleLinkHtmlRewriter = 20`, `BaseUrlHtmlRewriter = 30`.
- Pick a value that expresses the dependency: run after xref resolution so you see canonical paths (>10); run before base-URL rewriting so you see root-relative paths (<30).
- Use a tidy ten-step value (e.g. `40`, `15`) rather than squeezing between existing orders with off-by-one values.
- `Order` is load-bearing — mis-ordering against `BaseUrlHtmlRewriter` means you operate on strings already prefixed with the deployment base URL.

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

### 4. Gate the rewriter with `ShouldApply`

- Return `false` when there is nothing to do — no locale prefix needed, no base URL configured, request is not under your scope. `LocaleLinkHtmlRewriter` gates on `IsMultiLocale` plus non-default-locale; `BaseUrlHtmlRewriter` gates on a non-empty base URL.
- The orchestrator calls `ShouldApply` before the `ShouldProcess` gate on `HtmlResponseRewritingProcessor`; if every rewriter returns `false`, the response body is not parsed at all.
- `ShouldApply` runs per request — keep it cheap (field comparisons, `HttpContext.Items` lookups). Do not touch the response body here.

```csharp:xmldocid
T:Pennington.Localization.LocaleLinkHtmlRewriter
```

### 5. Mutate the shared document in `ApplyAsync`

- You receive a parsed `AngleSharp.Dom.IDocument` that every other rewriter will also see. Do not re-parse, do not serialize — `HtmlResponseRewritingProcessor` serializes exactly once after the final rewriter.
- Use `document.QuerySelectorAll("[href]")` / `document.Body` / `element.SetAttribute(...)` patterns as the built-ins do.
- Resolve per-request services (such as `DiagnosticContext`) from `context.RequestServices.GetRequiredService<T>()`; add diagnostics via `DiagnosticContext.Add*` instead of throwing.

### 6. Register the rewriter in DI

- Register as `IHtmlResponseRewriter` so `HtmlResponseRewritingProcessor` picks it up automatically via its injected `IEnumerable<IHtmlResponseRewriter>`.
- Do **not** register as `IResponseProcessor` — that produces a second parse/serialize cycle and defeats the unified-pass design.
- Singleton lifetime is appropriate; request-scoped state belongs in `HttpContext` or scoped services resolved inside the methods.

```csharp
services.AddPennington(penn => { /* ... */ });

// After AddPennington so defaults are already registered:
services.AddSingleton<IHtmlResponseRewriter, MyHtmlRewriter>();
```

---

## Verify

- Run `dotnet run` and request a page that should trigger your rewriter. View source and confirm the DOM change is present.
- Confirm the built-in rewriters still work (xref links resolve, locale prefixes appear, base URL is prepended) — if one stops working, your `Order` is probably wrong.
- Temporarily force `ShouldApply` to `false` and confirm your change disappears without breaking anything else; this proves the rewriter is isolated.

## Related

- Reference: [`IHtmlResponseRewriter`](xref:reference.namespaces.infrastructure) — members, default implementations, built-in registrations.
- Background: [Response processing and rewriter ordering](xref:explanation.response-processing) — why xref/locale/base-URL are 10/20/30 and what "outermost transport layer" means.

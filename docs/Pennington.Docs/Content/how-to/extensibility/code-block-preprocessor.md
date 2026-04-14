---
title: "Register a code-block preprocessor"
description: "Intercept fenced code blocks by language modifier and return pre-highlighted HTML before Pennington's default highlighter chain runs."
uid: how-to.extensibility.code-block-preprocessor
order: 20
sectionLabel: Extensibility
tags: [extensibility, markdown, highlighting, preprocessor]
---

> **In this page.** _Paraphrase the TOC "Covers" line: implementing the `ICodeBlockPreprocessor` contract, choosing a `Priority` that slots your preprocessor into the ordered chain, returning a `CodeBlockPreprocessResult` (including when to set `SkipTransform`), and registering the singleton — with the shipped `RoslynCodeBlockPreprocessor` as the concrete model._
>
> **Not in this page.** _Paraphrase the TOC "Does not cover" line: swapping out the highlighter that runs after preprocessors pass (see the next page) and standing up a fresh example project for a custom preprocessor — this how-to reuses the existing `ExtensibilityLabExample` fence target._

## When to use this

_Two sentences. Frame the goal: the reader has a fence info string they want to claim (e.g. a custom `:modifier` or a whole language id) and they need to bypass the highlighter chain with pre-rendered HTML — picture a plaintext wrapper, a chart block, or the shipped xmldocid resolution. Call out that trailing-comment directives (see [Annotate code blocks](xref:how-to.content-authoring.code-annotations)) are the right tool when all you need is line classes._

## Assumptions

_Three bullets. Realistic prior state only — no tutorial-style setup._

- You have an existing Pennington site with markdown rendering already wired (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- You have picked the fence identifier your preprocessor will claim — either a full `languageId` (`linecount`) or a `:modifier` suffix (`csharp:xmldocid`) — and you know the other preprocessors currently registered so you can pick a `Priority`.
- You are comfortable producing HTML yourself: the preprocessor owns the rendered `<pre><code>…</code></pre>` when it returns a result, the default highlighter does not run again on that block.

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `LineCountPreprocessor` claims the `linecount` fence and is the fixture every snippet below references.

---

## Steps

_Five steps. Imperative verbs. xmldocid fences target `ExtensibilityLabExample.LineCountPreprocessor` (our concrete model) plus the shipped `RoslynCodeBlockPreprocessor` body for the second-preprocessor comparison in step 4. Do not hand-roll a new example._

### 1. Implement `ICodeBlockPreprocessor`

_One sentence. The contract is two members — a `Priority` int and a `TryProcess(code, languageId)` method that returns a `CodeBlockPreprocessResult?`. Return `null` for every fence you do not want to claim so the next preprocessor (or the default highlighter) sees it._

```csharp:xmldocid
T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor
```

### 2. Inspect the fence identifier and bail out early

_Two sentences. The full fence info string (everything after the opening backticks) reaches your preprocessor unchanged, so compare it — case-insensitively — against the language id or `:modifier` suffix you claim. Anything you do not recognise returns `null` immediately; this is what keeps the chain composable._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LineCountPreprocessor.TryProcess(System.String,System.String)
```

### 3. Return a `CodeBlockPreprocessResult`

_Two sentences. The result carries the already-highlighted HTML (wrap it yourself in `<pre><code>…</code></pre>` — see `CodeBlockPreprocessResult.HighlightedHtml`), the `BaseLanguage` string Pennington will stamp as the CSS class, and `SkipTransform` — set it to `true` when your output is final and `CodeTransformer` must not re-run the `[!code ...]` annotation pass over it. Returning `null` at this point still passes the block through to the next preprocessor._

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult
```

### 4. Pick a `Priority` that slots you correctly

_Two to three sentences. `CodeHighlightRenderer` sorts preprocessors by `Priority` descending and returns the first non-null `TryProcess` result, so a higher number wins. The shipped `RoslynCodeBlockPreprocessor` uses `100`; `LineCountPreprocessor` uses `500` so its `linecount` fence is never intercepted by a future language-modifier preprocessor. Reference the Roslyn preprocessor as the concrete "other player in the chain" you are ordering against._

```csharp:xmldocid
P:ExtensibilityLabExample.LineCountPreprocessor.Priority
```

### 5. Register the preprocessor as an `ICodeBlockPreprocessor` singleton

_Two sentences. Pennington collects every `ICodeBlockPreprocessor` out of DI — add yours with `AddSingleton<ICodeBlockPreprocessor, TPreprocessor>()` anywhere after `AddPennington`. No `PenningtonOptions` knob exists for this; the DI registration is the whole wiring step. Note that `AddPenningtonRoslyn` performs the same registration for `RoslynCodeBlockPreprocessor` on your behalf._

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

---

## Verify

_Three bullets. Observable checks, no prose._

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/line-count-demo/` — the `linecount` fence renders inside a `<figure class="linecount">` with the line-count badge while the adjacent `text` fence highlights normally through the default chain.
- View source on the rendered page and confirm the `linecount` figure carries `data-extensibility-lab="line-count-preprocessor"` — proves `LineCountPreprocessor.TryProcess` returned a result rather than the default `CodeHighlightRenderer` path rendering the block.
- Add a second preprocessor with a lower `Priority` that also claims `linecount`, reload, and confirm the higher-priority result still wins — priority ordering is descending.

## Related

- Reference: [Highlighting interfaces](xref:reference.extension-points.highlighting) — full signatures for `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Annotate code blocks](xref:how-to.content-authoring.code-annotations) — use trailing-comment directives when you just need line classes and do not need to take over rendering
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why preprocessors run before the highlighter and how `CodeTransformer` interacts with `SkipTransform`

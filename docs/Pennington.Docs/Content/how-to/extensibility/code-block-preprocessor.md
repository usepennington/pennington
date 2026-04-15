---
title: "Register a code-block preprocessor"
description: "Intercept fenced code blocks by language modifier and return pre-highlighted HTML before Pennington's default highlighter chain runs."
uid: how-to.extensibility.code-block-preprocessor
order: 203020
sectionLabel: Extensibility
tags: [extensibility, markdown, highlighting, preprocessor]
---

Use a code-block preprocessor when you want to claim a fence language or `:modifier` suffix and return pre-rendered HTML before the default highlighter chain runs — for example, a plaintext wrapper, a chart block, or the xmldocid resolver. When you only need line-level CSS classes on an otherwise normal code block, trailing-comment directives (see [Annotate code blocks](xref:how-to.content-authoring.code-annotations)) are the lighter-weight choice.

## Assumptions

- You have an existing Pennington site with markdown rendering already wired (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- You have picked the fence identifier your preprocessor will claim — either a full `languageId` (`linecount`) or a `:modifier` suffix (`csharp:xmldocid`) — and you know the other preprocessors currently registered so you can pick a `Priority`.
- You are comfortable producing HTML yourself: the preprocessor owns the rendered `<pre><code>…</code></pre>` when it returns a result, the default highlighter does not run again on that block.

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `LineCountPreprocessor` claims the `linecount` fence and is the fixture every snippet below references.

---

## Steps

### 1. Implement `ICodeBlockPreprocessor`

The contract is two members: a `Priority` int and a `TryProcess(code, languageId)` method returning `CodeBlockPreprocessResult?`. Return `null` for any fence you do not claim so the next preprocessor — or the default highlighter — can handle it.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor
```

### 2. Inspect the fence identifier and bail out early

The full fence info string reaches your preprocessor unchanged. Compare it case-insensitively against the language id or `:modifier` suffix you claim, and return `null` immediately for anything else — that is what keeps the chain composable.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LineCountPreprocessor.TryProcess(System.String,System.String)
```

### 3. Return a `CodeBlockPreprocessResult`

The result wraps your pre-rendered HTML (you own the `<pre><code>…</code></pre>` markup), the `BaseLanguage` CSS class Pennington stamps on the block, and `SkipTransform`. Set `SkipTransform` to `true` when your output is final and the `[!code ...]` annotation pass must not re-process it.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult
```

### 4. Pick a `Priority` that slots you correctly

`CodeHighlightRenderer` sorts preprocessors by `Priority` descending and returns the first non-null result, so higher wins. The shipped `RoslynCodeBlockPreprocessor` uses `100`; `LineCountPreprocessor` uses `500` so its `linecount` fence is never intercepted by a lower-priority modifier preprocessor. Check which preprocessors are already registered before choosing your value.

```csharp:xmldocid
P:ExtensibilityLabExample.LineCountPreprocessor.Priority
```

### 5. Register the preprocessor as an `ICodeBlockPreprocessor` singleton

Pennington collects every `ICodeBlockPreprocessor` from DI. Add yours with `AddSingleton<ICodeBlockPreprocessor, TPreprocessor>()` anywhere after `AddPennington` — there is no `PenningtonOptions` knob; the DI registration is the entire wiring step. (`AddPenningtonRoslyn` performs the equivalent registration for `RoslynCodeBlockPreprocessor`.)

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/line-count-demo/` — the `linecount` fence renders inside a `<figure class="linecount">` with the line-count badge while the adjacent `text` fence highlights normally through the default chain.
- View source on the rendered page and confirm the `linecount` figure carries `data-extensibility-lab="line-count-preprocessor"` — proves `LineCountPreprocessor.TryProcess` returned a result rather than the default `CodeHighlightRenderer` path rendering the block.
- Add a second preprocessor with a lower `Priority` that also claims `linecount`, reload, and confirm the higher-priority result still wins — priority ordering is descending.

## Related

- Reference: [Highlighting interfaces](xref:reference.extension-points.highlighting) — full signatures for `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Annotate code blocks](xref:how-to.content-authoring.code-annotations) — use trailing-comment directives when you need only line classes and do not need to take over rendering
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why preprocessors run before the highlighter and how `CodeTransformer` interacts with `SkipTransform`

---
title: "Register a code-block preprocessor"
description: "Intercept fenced code blocks by language modifier and return pre-highlighted HTML before Pennington's default highlighter chain runs."
uid: how-to.extensibility.code-block-preprocessor
order: 203020
sectionLabel: Extensibility
tags: [extensibility, markdown, highlighting, preprocessor]
---

A code-block preprocessor claims a fence language or `:modifier` suffix and returns pre-rendered HTML before the default highlighter chain runs — for example, a plaintext wrapper, a chart block, or the xmldocid resolver. For line-level CSS classes on an otherwise normal code block, trailing-comment directives (see [Annotate code blocks](xref:how-to.content-authoring.code-annotations)) are the lighter-weight choice.

## Assumptions

- An existing Pennington site with markdown rendering wired (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- A chosen fence identifier for the preprocessor to claim — either a full `languageId` (`linecount`) or a `:modifier` suffix (`csharp:xmldocid`) — along with awareness of the other preprocessors currently registered, so the `Priority` slots in sensibly.
- Comfort producing HTML by hand: the preprocessor owns the rendered `<pre><code>…</code></pre>` when it returns a result, and the default highlighter does not run again on that block.

For a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `LineCountPreprocessor` claims the `linecount` fence and is the fixture every snippet below references.

---

## Steps

<Steps>
<Step StepNumber="1">

**Implement `ICodeBlockPreprocessor`**

The contract has two members: a `Priority` int and a `TryProcess(code, languageId)` method returning `CodeBlockPreprocessResult?`. Return `null` for any unclaimed fence so the next preprocessor — or the default highlighter — can handle it.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor
```

</Step>
<Step StepNumber="2">

**Inspect the fence identifier and bail out early**

The full fence info string reaches the preprocessor unchanged. Compare it case-insensitively against the claimed language id or `:modifier` suffix, and return `null` immediately for anything else — that keeps the chain composable.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LineCountPreprocessor.TryProcess(System.String,System.String)
```

</Step>
<Step StepNumber="3">

**Return a `CodeBlockPreprocessResult`**

The result wraps the pre-rendered HTML (the preprocessor owns the `<pre><code>…</code></pre>` markup), the `BaseLanguage` CSS class Pennington stamps on the block, and `SkipTransform`. Set `SkipTransform` to `true` when the output is final and the `[!code ...]` annotation pass should not re-process it.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult
```

</Step>
<Step StepNumber="4">

**Pick a `Priority` that slots you correctly**

`CodeHighlightRenderer` sorts preprocessors by `Priority` descending and returns the first non-null result, so higher wins. The shipped `RoslynCodeBlockPreprocessor` uses `100`; `LineCountPreprocessor` uses `500` so its `linecount` fence is never intercepted by a lower-priority modifier preprocessor. Review the registered preprocessors before picking a value.

```csharp:xmldocid
P:ExtensibilityLabExample.LineCountPreprocessor.Priority
```

</Step>
<Step StepNumber="5">

**Register the preprocessor as an `ICodeBlockPreprocessor` singleton**

Pennington collects every `ICodeBlockPreprocessor` from DI. Register with `AddSingleton<ICodeBlockPreprocessor, TPreprocessor>()` anywhere after `AddPennington` — there is no `PenningtonOptions` knob; the DI registration is the entire wiring step. (`AddPenningtonRoslyn` performs the equivalent registration for `RoslynCodeBlockPreprocessor`.)

```csharp
builder.Services.AddSingleton<ICodeBlockPreprocessor, LineCountPreprocessor>();
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/line-count-demo/` — the `linecount` fence renders inside a `<figure class="linecount">` with the line-count badge while the adjacent `text` fence highlights normally through the default chain.
- View source on the rendered page and confirm the `linecount` figure carries `data-extensibility-lab="line-count-preprocessor"` — this shows `LineCountPreprocessor.TryProcess` returned a result rather than the default `CodeHighlightRenderer` path rendering the block.
- Add a second preprocessor with a lower `Priority` that also claims `linecount`, reload, and confirm the higher-priority result still wins — priority ordering is descending.

## Related

- Reference: [Highlighting interfaces](xref:reference.extension-points.highlighting) — full signatures for `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Annotate code blocks](xref:how-to.content-authoring.code-annotations) — trailing-comment directives when only line classes are needed and the preprocessor does not need to take over rendering
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why preprocessors run before the highlighter and how `CodeTransformer` interacts with `SkipTransform`

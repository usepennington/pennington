---
title: "Add a custom fence syntax"
description: "Implement ICodeBlockPreprocessor to claim a fence language or :modifier suffix and return pre-rendered HTML before the default highlighter chain runs."
uid: how-to.markdown-pipeline.code-block-preprocessor
order: 2
sectionLabel: "Markdown Pipeline"
tags: [extensibility, markdown, highlighting, preprocessor]
---

To intercept a fence language or `:modifier` suffix — a chart block, a plaintext wrapper, an xmldocid resolver — implement `ICodeBlockPreprocessor`. The preprocessor returns pre-rendered HTML before the default highlighter chain runs, including the rendered `<pre><code>...</code></pre>`. For line-level CSS classes on an otherwise normal code block, trailing-comment directives are the lighter-weight choice — see <xref:how-to.code-samples.code-annotations>.

The recipe references `examples/ExtensibilityLabExample/LineCountPreprocessor.cs`, which claims the `linecount` fence.

## Before you begin

- An existing Pennington site with markdown rendering wired (see <xref:tutorials.getting-started.first-site> if not).
- A chosen fence identifier — either a full `languageId` (`linecount`) or a `:modifier` suffix (`csharp:symbol`).

## Write the preprocessor

Implement <xref:reference.api.i-code-block-preprocessor> as a sealed class. `TryProcess(code, languageId)` receives the full fence info string unchanged. Compare it case-insensitively against the claimed language id or modifier, return `null` for anything else so the next preprocessor or the default highlighter can handle it, and otherwise build the wrapper HTML around the encoded source.

```csharp:symbol
examples/ExtensibilityLabExample/LineCountPreprocessor.cs
```

The returned `CodeBlockPreprocessResult` carries the pre-rendered HTML, the `BaseLanguage` CSS class Pennington stamps on the block, and `SkipTransform`. Set `SkipTransform` to `true` when the output is final and the `[!code ...]` annotation pass should not re-process it.

## Pick a Priority value

`CodeBlockRenderingService` sorts preprocessors by `Priority` descending and returns the first non-null result. `LineCountPreprocessor` uses `500` so its `linecount` fence is never intercepted by a lower-priority modifier preprocessor. Pick a value above any preprocessor you need to beat on a contested `:modifier`, or below it to fall through first.

## Register the implementation

Pennington collects every `ICodeBlockPreprocessor` from DI. Register anywhere after `AddPennington` — there is no `PenningtonOptions` knob. `AddTreeSitter` performs the equivalent registration for its `:symbol` preprocessor.

```csharp
builder.Services.AddSingleton<ICodeBlockPreprocessor, LineCountPreprocessor>();
```

## Result

A markdown fence tagged `linecount` renders inside a `<figure>` with the line-count badge instead of going through the default highlighter:

```html
<figure class="linecount" data-extensibility-lab="line-count-preprocessor">
  <figcaption>Line count: <strong>3</strong></figcaption>
  <pre><code>first line
second line
third line</code></pre>
</figure>
```

Adjacent fences with other languages (`text`, `csharp`) keep flowing through the default highlighter chain.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/line-count-demo/` — the `linecount` fence renders inside a `<figure class="linecount">` with the badge while the adjacent `text` fence highlights through the default chain.
- View source and confirm the `linecount` figure carries `data-extensibility-lab="line-count-preprocessor"` — that attribute means `TryProcess` returned a result rather than the default `CodeHighlightRenderer` path rendering the block.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter) — full signatures for `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Annotate code blocks](xref:how-to.code-samples.code-annotations) — trailing-comment directives when only line classes are needed
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why preprocessors run before the highlighter and how `CodeTransformer` interacts with `SkipTransform`

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

The returned `CodeBlockPreprocessResult` carries the pre-rendered HTML, the `BaseLanguage` CSS class Pennington stamps on the block, and two opt-out flags. Set `SkipTransform` to `true` when the output is final and the `[!code ...]` annotation pass should not re-process it. Set `SkipChrome` to `true` when the output is not a code block at all — a rendered diagram, a chart, an embedded widget — and the standard wrapper markup (the `code-highlight-wrapper` div, the language head bar, and the container divs) should not surround it; the HTML is emitted verbatim. A `SkipChrome` block also stops registering as a code fence in the per-page Markdown twins: without the wrapper's `data-language` attribute, the HTML→markdown converter treats the output as ordinary markup instead of reconstructing a fence around it.

## Pick a Priority value

`CodeBlockRenderingService` sorts preprocessors by `Priority` descending and returns the first non-null result. The only shipped preprocessor is the tree-sitter one that claims `:symbol` and `:symbol-diff`, at `100`. `LineCountPreprocessor` uses `500` so its `linecount` fence runs ahead of the tree-sitter preprocessor — relevant only if both could claim the same info string. Pick above `100` to beat the shipped `:symbol` preprocessor on a contested `:modifier`, or below it to let `:symbol` resolve first.

## Register the implementation

Pennington collects every `ICodeBlockPreprocessor` from DI. Register anywhere after `AddPennington` — there is no `PenningtonOptions` knob. `AddTreeSitter` performs the equivalent registration for its `:symbol` preprocessor.

```csharp
builder.Services.AddSingleton<ICodeBlockPreprocessor, LineCountPreprocessor>();
```

## Verify

On your own site, add a fence tagged with the language your preprocessor claims, then run `dotnet run` and view source on the page that holds it. A claimed `linecount` fence renders inside a `<figure>` with the line-count badge instead of going through the default highlighter, while adjacent fences with other languages keep flowing through the highlighter chain:

```html
<figure class="linecount" data-extensibility-lab="line-count-preprocessor">
  <figcaption>Line count: <strong>3</strong></figcaption>
  <pre><code>first line
second line
third line</code></pre>
</figure>
```

The wrapper markup proves `TryProcess` returned a result rather than the default highlighter rendering the block. Confirm too that a static build picks the preprocessor up: `dotnet run -- build output`, then grep the emitted HTML for the same wrapper.

To see the shipped example instead, run `dotnet run --project examples/ExtensibilityLabExample` and visit `/line-count-demo/` — the `linecount` fence renders the figure above while the adjacent `text` fence highlights through the default chain.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter) — full signatures for `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Annotate code blocks](xref:how-to.code-samples.code-annotations) — trailing-comment directives when only line classes are needed
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why preprocessors run before the highlighter and how `CodeTransformer` interacts with `SkipTransform`

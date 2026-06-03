---
title: "Add a custom syntax highlighter"
description: "Implement ICodeHighlighter for a fence language TextMateSharp doesn't cover and register it with HighlightingOptions.AddHighlighter."
uid: how-to.markdown-pipeline.custom-highlighter
order: 3
sectionLabel: "Markdown Pipeline"
tags: [highlighting, extensibility, textmate, code-blocks]
---

To color a fence language TextMateSharp does not cover — a DSL, config format, or domain notation — implement `ICodeHighlighter`. For line-level callouts on a language already supported, see <xref:how-to.code-samples.code-annotations>. For transforming the fence body rather than coloring its tokens, see <xref:how-to.markdown-pipeline.code-block-preprocessor>.

The recipe below references `examples/ExtensibilityLabExample/PipelineHighlighter.cs`, which stakes out a fictional `pipeline` DSL against a bare `AddPennington` host.

## Before you begin

- An existing Pennington site rendering markdown fences (see <xref:tutorials.getting-started.first-site> if not).
- A target language not already served by `TextMateHighlighter` (priority 50) or `ShellHighlighter` (priority 75) — render a fence and inspect the emitted HTML for built-in token spans to confirm. `PlainTextHighlighter` is the hardcoded final fallback inside `HighlightingService`, reached only when no registered highlighter matches; it is not on the priority chain.

## Write the highlighter

Implement <xref:reference.api.i-code-highlighter> as a sealed class. `Highlight(code, language)` returns the full HTML for the block, including the outer `<pre><code>` wrapper — the implementation owns escaping. Use `WebUtility.HtmlEncode` on every literal not wrapped in a span; anything missed becomes an injection vector.

```csharp:symbol
examples/ExtensibilityLabExample/PipelineHighlighter.cs
```

Two values shape how the highlighter slots into the chain:

- `SupportedLanguages` — every token returned here maps to a fence language (` ```pipeline `) that routes to this implementation. Use `StringComparer.OrdinalIgnoreCase` so `Pipeline` and `PIPELINE` match too.
- `Priority` — higher wins when multiple highlighters claim the same language. The shipped chain has `TextMateHighlighter` at 50 and `ShellHighlighter` at 75. Pick above 75 to beat every shipped highlighter; pick below 50 to register a highlighter that only runs when no shipped chain entry matches the language (after which `HighlightingService` reaches for its hardcoded `PlainTextHighlighter` fallback).

## Register the highlighter

`PenningtonOptions.Highlighting.AddHighlighter` inserts the instance into the priority-sorted chain resolved by `HighlightingService`. Call it inside the `AddPennington` delegate so the highlighter is active for both `dotnet run` and `dotnet run -- build output`.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.Highlighting.AddHighlighter(new PipelineHighlighter());
});
```

A markdown fence tagged with one of the strings from `SupportedLanguages` now routes to the custom highlighter instead of the fallback chain.

````markdown
```pipeline
source "orders" -> filter where=paid | transform total=sum | sink "warehouse"
```
````

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/pipeline-demo/`. Each keyword, arrow, and string literal inside the `pipeline` fence carries a `pipeline-*` CSS class; the neighboring `text` fence renders without spans through the hardcoded `PlainTextHighlighter` fallback.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep the emitted HTML for `class="pipeline-keyword"` to confirm the highlighter runs during publish.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
- Related how-to: [Register a code-block preprocessor](xref:how-to.markdown-pipeline.code-block-preprocessor)

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
- `Priority` — higher wins when two highlighters claim the same language. For a brand-new language like `pipeline` that nothing else touches, the value is irrelevant — any number routes the fence to your implementation. Priority matters only when you override a language a shipped highlighter already owns: pick above `75` to beat `ShellHighlighter` (`bash`/`shell`/`sh`), or above `50` to beat `TextMateHighlighter` (every grammar it can load). `PipelineHighlighter` uses `100` purely to make the intent — "this wins outright" — legible.

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

## Style the emitted classes

The highlighter only wraps tokens in spans — `pipeline-keyword`, `pipeline-arrow`, `pipeline-pipe`, `pipeline-string`. Until a stylesheet colors those classes the block renders in the surrounding body color, so the fence looks no different from the unstyled `text` fallback. Built-in languages look colored out of the box because the shipped theme already styles TextMate's `hljs-*` classes; your custom classes are new, so the theme says nothing about them. Add the rules to the stylesheet the site already serves:

```css
.pipeline-keyword { color: #c678dd; font-weight: 600; }
.pipeline-arrow   { color: #56b6c2; }
.pipeline-pipe    { color: #56b6c2; }
.pipeline-string  { color: #98c379; }
```

The class names are whatever `Highlight` emits — keep the CSS and the span classes in sync. Reuse the theme's existing token colors (or its CSS custom properties) so the new language matches the rest of the site instead of introducing a fourth palette.

## Verify

On your own site, render a page with a `pipeline` fence next to a `text` fence and load it in a browser:

- The `pipeline` fence shows colored keywords, arrows, and string literals; the `text` fence stays a single color. If both blocks look identical, the highlighter is running but the CSS rules above are missing or not loaded.
- View source: the `pipeline` block carries `<span class="pipeline-keyword">` tokens. If it does not, the fence never reached your highlighter — confirm the fence tag matches a string in `SupportedLanguages` and the registration runs inside the `AddPennington` delegate.
- Static build: run your build (`dotnet run -- build output`) and search the emitted HTML for `class="pipeline-keyword"` to confirm the highlighter runs during publish, not only under `dotnet run`.

For a complete worked highlighter and a demo fence that emits the `pipeline-*` spans, run `dotnet run --project examples/ExtensibilityLabExample` and visit `/pipeline-demo/`.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
- Related how-to: [Register a code-block preprocessor](xref:how-to.markdown-pipeline.code-block-preprocessor)

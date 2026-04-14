---
title: "Add a custom syntax highlighter"
description: "Implement ICodeHighlighter for a new language, claim it via SupportedLanguages, tune priority, and register with HighlightingOptions.AddHighlighter."
uid: how-to.extensibility.custom-highlighter
order: 30
sectionLabel: Extensibility
tags: [highlighting, extensibility, textmate, code-blocks]
---

> **In this page.** _Paraphrase the TOC "Covers" line: implementing `ICodeHighlighter` for a new fence language, populating `SupportedLanguages`, setting `Priority` so your highlighter outranks the built-in TextMate/shell chain for that language, and wiring the instance through `HighlightingOptions.AddHighlighter` on `PenningtonOptions.Highlighting`._
>
> **Not in this page.** _Paraphrase the TOC "Does not cover": authoring a TextMate `.tmLanguage.json` grammar from scratch to extend `TextMateHighlighter` — for that, consult the upstream [TextMateSharp](https://github.com/danipen/TextMateSharp) documentation and bundle the grammar rather than writing a new highlighter._

## When to use this

_Two sentences. Frame the reader's goal: they have fences with a language token (DSL, config format, or domain notation) that TextMateSharp does not cover, and they want coloured output without shipping a full TextMate grammar. Point out the cheaper alternatives so nobody lands here by accident — link to [Annotate code blocks](xref:how-to.content-authoring.code-annotations) for line-level callouts on existing languages, and to [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor) when the goal is transforming fence bodies (e.g. xmldocid embedding) rather than colouring tokens._

## Assumptions

_Three bullets. Each is realistic prior state, not a tutorial step._

- You have an existing Pennington site rendering markdown fences (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- The language you want to highlight is not already served by `TextMateHighlighter` (priority 50) or `ShellHighlighter` (priority 75) — confirm by rendering a fence and inspecting the emitted HTML for the built-in token spans.
- You are comfortable producing HTML for a fence body yourself — `ICodeHighlighter.Highlight` returns a raw HTML string, so your implementation owns escaping and the outer `<pre><code>` wrapper.

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `PipelineHighlighter.cs` stakes out a fictional `pipeline` DSL and `Program.cs` registers it against a bare `AddPennington` host.

---

## Steps

### 1. Implement `ICodeHighlighter`

_One sentence. The contract is three members: `SupportedLanguages`, `Priority`, and `Highlight(code, language)`. Skip to the code — the example wraps keywords, arrows, and string literals in classed `<span>` elements and HTML-encodes everything else._

```csharp:xmldocid
T:ExtensibilityLabExample.PipelineHighlighter
```

### 2. Declare `SupportedLanguages`

_One sentence of context: every language token you return here is a fence language (e.g. ```` ```pipeline ````) your highlighter will be asked to render. Use `StringComparer.OrdinalIgnoreCase` if you want `Pipeline` / `PIPELINE` to route here too._

```csharp:xmldocid
P:ExtensibilityLabExample.PipelineHighlighter.SupportedLanguages
```

### 3. Set `Priority`

_Two sentences. Higher priority wins when multiple highlighters claim the same language — `PlainTextHighlighter` sits at 0, `TextMateHighlighter` at 50, `ShellHighlighter` at 75. The example uses 100 so the `pipeline` fence routes here even if a future TextMate grammar also claims it, while leaving lower numbers for fallbacks you ship alongside._

```csharp:xmldocid
P:ExtensibilityLabExample.PipelineHighlighter.Priority
```

### 4. Produce the fence HTML in `Highlight`

_Two sentences. `Highlight` gets the raw fence body and the language token; it must return the full HTML to emit for the block, including the outer `<pre><code>` wrapper (the built-in highlighters follow the same convention). HTML-encode every byte you do not explicitly wrap in a span — the pipeline example reaches for `WebUtility.HtmlEncode` on every literal path._

```csharp:xmldocid
M:ExtensibilityLabExample.PipelineHighlighter.Highlight(System.String,System.String)
```

### 5. Register with `HighlightingOptions.AddHighlighter`

_Two sentences. `PenningtonOptions.Highlighting` exposes an `AddHighlighter` overload that appends your instance to the priority-sorted chain resolved by `HighlightingService`. The example calls it inside the `AddPennington` delegate so the highlighter is live for both `dotnet run` and `dotnet run -- build output`._

```csharp:xmldocid,bodyonly
M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter(Pennington.Highlighting.ICodeHighlighter)
```

### 6. Author a fence that targets your language

_One sentence. Any markdown fence tagged with one of the strings from `SupportedLanguages` now routes to your highlighter instead of the fallback._

```markdown
```pipeline
source "orders" -> filter where=paid | transform total=sum | sink "warehouse"
```
```

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/pipeline-demo/`.
- Expect each keyword, arrow, and string literal inside the `pipeline` fence to carry a `pipeline-*` CSS class; the neighbouring `text` fence should render with no spans (fallback `PlainTextHighlighter`).
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep the emitted HTML for `class="pipeline-keyword"` to confirm the highlighter also runs during publish.

## Related

- Reference: [Highlighting interfaces](xref:reference.extension-points.highlighting)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
- Related how-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)

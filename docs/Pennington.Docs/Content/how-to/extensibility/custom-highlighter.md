---
title: "Add a custom syntax highlighter"
description: "Implement ICodeHighlighter for a new language, claim it via SupportedLanguages, tune priority, and register with HighlightingOptions.AddHighlighter."
uid: how-to.extensibility.custom-highlighter
order: 203030
sectionLabel: Extensibility
tags: [highlighting, extensibility, textmate, code-blocks]
---

Use this approach for fences tagged with a language token — a DSL, config format, or domain notation — that TextMateSharp does not cover, when styled output is the goal but authoring a full TextMate grammar is not. For line-level callouts on an already-supported language, see <xref:how-to.content-authoring.code-annotations>. When the goal is transforming fence bodies rather than colouring tokens, see <xref:how-to.extensibility.code-block-preprocessor>.

## Assumptions

- An existing Pennington site rendering markdown fences (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- A target language not already served by `TextMateHighlighter` (priority 50) or `ShellHighlighter` (priority 75) — confirm by rendering a fence and inspecting the emitted HTML for the built-in token spans.
- Comfort producing HTML for a fence body by hand — `ICodeHighlighter.Highlight` returns a raw HTML string, so the implementation owns escaping and the outer `<pre><code>` wrapper.

For a working setup, see `examples/ExtensibilityLabExample` — `PipelineHighlighter.cs` stakes out a fictional `pipeline` DSL and `Program.cs` registers it against a bare `AddPennington` host.

---

## Steps

<Steps>
<Step StepNumber="1">

**Implement `ICodeHighlighter`**

The contract requires three members: `SupportedLanguages`, `Priority`, and `Highlight(code, language)`. The next three steps fence each of those members separately from the example `PipelineHighlighter` at `examples/ExtensibilityLabExample/PipelineHighlighter.cs`, which wraps keywords, arrows, and string literals in classed `<span>` elements and HTML-encodes everything else.

</Step>
<Step StepNumber="2">

**Declare `SupportedLanguages`**

Every language token returned here maps to a fence language (for example, ` ```pipeline `) that routes to the highlighter. Use `StringComparer.OrdinalIgnoreCase` to match `Pipeline` and `PIPELINE` as well.

```csharp:xmldocid
P:ExtensibilityLabExample.PipelineHighlighter.SupportedLanguages
```

</Step>
<Step StepNumber="3">

**Set `Priority`**

Higher priority wins when multiple highlighters claim the same language. The built-in chain places `PlainTextHighlighter` at 0, `TextMateHighlighter` at 50, and `ShellHighlighter` at 75. The example uses 100 so the `pipeline` fence routes here even if a future TextMate grammar also claims it, while leaving room below for any secondary fallbacks that ship alongside.

```csharp:xmldocid
P:ExtensibilityLabExample.PipelineHighlighter.Priority
```

</Step>
<Step StepNumber="4">

**Produce the fence HTML in `Highlight`**

`Highlight` receives the raw fence body and the language token and returns the full HTML for the block, including the outer `<pre><code>` wrapper — the same convention the built-in highlighters follow. HTML-encode every character not explicitly wrapped in a span; the pipeline example uses `WebUtility.HtmlEncode` on every literal path to prevent injection. Full implementation: `examples/ExtensibilityLabExample/PipelineHighlighter.cs`.

</Step>
<Step StepNumber="5">

**Register with `HighlightingOptions.AddHighlighter`**

`PenningtonOptions.Highlighting` exposes an `AddHighlighter` overload that inserts the instance into the priority-sorted chain resolved by `HighlightingService`. Call it inside the `AddPennington` delegate so the highlighter is active for both `dotnet run` and `dotnet run -- build output`.

```csharp:xmldocid,bodyonly
M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter(Pennington.Highlighting.ICodeHighlighter)
```

</Step>
<Step StepNumber="6">

**Author a fence that targets your language**

Any markdown fence tagged with one of the strings from `SupportedLanguages` now routes to the custom highlighter instead of the fallback chain.

```markdown
```pipeline
source "orders" -> filter where=paid | transform total=sum | sink "warehouse"
```
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/pipeline-demo/`.
- Expect each keyword, arrow, and string literal inside the `pipeline` fence to carry a `pipeline-*` CSS class; the neighbouring `text` fence should render with no spans (fallback `PlainTextHighlighter`).
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep the emitted HTML for `class="pipeline-keyword"` to confirm the highlighter also runs during publish.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
- Related how-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)

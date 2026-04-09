---
title: "Adding a Custom Code Highlighter"
description: "Implement ICodeHighlighter with priority-based language dispatch and register it via DI to extend Pennington's syntax highlighting"
uid: "penn.how-to.adding-a-custom-code-highlighter"
order: 20
---

## Beat 1: The Problem — Unrecognized Languages Render as Plain Text

Introduce the scenario: Forge's documentation includes fenced code blocks tagged with ` ```pipeline ` for their internal DSL. Currently these render as unstyled monospace text because no registered highlighter claims the `pipeline` language identifier. The `T:Pennington.Highlighting.HighlightingService` falls back to `T:Pennington.Highlighting.PlainTextHighlighter` which only HTML-encodes the text.

### What to show
- A markdown code block using ` ```pipeline ` that renders without any syntax coloring
- The dispatch flow: `M:Pennington.Highlighting.HighlightingService.Highlight(System.String,System.String)` iterates registered highlighters in priority order, falls back to `T:Pennington.Highlighting.PlainTextHighlighter` when none match

### Key points
- Pennington ships with three highlighters: `T:Pennington.Highlighting.TextMateHighlighter` (priority 50, claims `*` as wildcard), `T:Pennington.Highlighting.ShellHighlighter` (priority 75, claims `bash`/`shell`/`sh`), and `T:Pennington.Highlighting.PlainTextHighlighter` (priority 0, fallback)
- The wildcard `*` in `T:Pennington.Highlighting.TextMateHighlighter` means it handles any language it has a grammar for — but custom DSLs have no TextMate grammar
- Custom highlighters let you add first-class support for any language

## Beat 2: The ICodeHighlighter Interface

Walk through the three members of `T:Pennington.Highlighting.ICodeHighlighter` and explain the priority dispatch model.

### What to show
- `P:Pennington.Highlighting.ICodeHighlighter.SupportedLanguages` — `IReadOnlySet<string>` of language identifiers this highlighter handles (e.g., `{"pipeline", "pipe"}`). These match the language tag after ` ``` ` in fenced code blocks
- `M:Pennington.Highlighting.ICodeHighlighter.Highlight(System.String,System.String)` — takes raw code and language identifier, returns an HTML string with `<span>` elements for tokens wrapped in `<pre><code>`. Each highlighter is responsible for its own `<pre><code>` wrapping (see `T:Pennington.Highlighting.ShellHighlighter` for an example)
- `P:Pennington.Highlighting.ICodeHighlighter.Priority` — integer determining dispatch order. `T:Pennington.Highlighting.HighlightingService` sorts all registered highlighters by `Priority` descending and uses the first one whose `SupportedLanguages` contains the requested language (or `*`)

### Key points
- Higher priority wins — if two highlighters both support `csharp`, the one with the higher priority is used
- The priority chain in Pennington's default registration: ShellHighlighter (75) > TextMateHighlighter (50) > PlainTextHighlighter (0)
- A custom highlighter at priority 60 slots between Shell and TextMate — it wins for its declared languages but does not affect Shell or TextMate for theirs

## Beat 3: The Dispatch Chain Inside HighlightingService

Show how `T:Pennington.Highlighting.HighlightingService` selects a highlighter and how the fallback works.

### What to show
- The constructor of `T:Pennington.Highlighting.HighlightingService` at `:path:src/Pennington/Highlighting/HighlightingService.cs` — sorts `IEnumerable<ICodeHighlighter>` by `Priority` descending into `_highlighters`
- The private `FindHighlighter` method: iterates `_highlighters`, checks `SupportedLanguages.Contains(language)` or `SupportedLanguages.Contains("*")`, returns first match
- The `_fallback` field: a `T:Pennington.Highlighting.PlainTextHighlighter` instance used when no registered highlighter matches
- `M:Pennington.Highlighting.HighlightingService.HasHighlighter(System.String)` — utility that returns `true` if any non-fallback highlighter supports the language

### Key points
- The `*` wildcard in `SupportedLanguages` means "I handle anything" — `T:Pennington.Highlighting.TextMateHighlighter` uses this because it has grammars for hundreds of languages
- Explicit language sets (like `{"pipeline", "pipe"}`) take precedence at higher priority — the dispatch stops at the first match
- The `PlainTextHighlighter` fallback is not in the registered list — it is a private field, so it never conflicts with user-registered highlighters

## Beat 4: Create the PipelineHighlighter Class

Implement `T:Pennington.Highlighting.ICodeHighlighter` for Forge's pipeline DSL with regex-based tokenization.

### What to show
- Class declaration: `public sealed class PipelineHighlighter : ICodeHighlighter`
- `P:Pennington.Highlighting.ICodeHighlighter.SupportedLanguages` returns `new HashSet<string> { "pipeline", "pipe" }` — supporting two language tags for the same DSL
- `P:Pennington.Highlighting.ICodeHighlighter.Priority` returns `60` — above `T:Pennington.Highlighting.TextMateHighlighter` (50) so this highlighter wins for `pipeline`, but below `T:Pennington.Highlighting.ShellHighlighter` (75) which does not claim `pipeline` anyway
- `M:Pennington.Highlighting.ICodeHighlighter.Highlight(System.String,System.String)` implementation:
  - Wraps output in `<pre><code>`
  - Processes line by line
  - Keywords (`source`, `transform`, `sink`, `when`, `output`) get `<span class="hljs-keyword">`
  - Strings (double-quoted) get `<span class="hljs-string">`
  - Comments (`#` to end of line) get `<span class="hljs-comment">`
  - Everything else is HTML-encoded with `WebUtility.HtmlEncode`
- Reference `T:Pennington.Highlighting.ShellHighlighter` at `:path:src/Pennington/Highlighting/ShellHighlighter.cs` as a model — it uses the same line-by-line regex approach with `[GeneratedRegex]` attributes

### Key points
- Use `System.Net.WebUtility.HtmlEncode` for all text output to prevent XSS
- The CSS classes (`hljs-keyword`, `hljs-string`, `hljs-comment`) follow the hljs convention used by Pennington's built-in highlighters — this ensures the existing theme styles apply without extra CSS work
- ~40 lines total for a minimal but functional highlighter

## Beat 5: Register the Highlighter

Wire the highlighter into Pennington via `T:Pennington.Infrastructure.HighlightingOptions`.

### What to show
- In `Program.cs`, register the highlighter via DI: `services.AddSingleton<ICodeHighlighter, PipelineHighlighter>()`
- This registers it alongside the built-in `T:Pennington.Highlighting.TextMateHighlighter` and `T:Pennington.Highlighting.ShellHighlighter`
- `T:Pennington.Highlighting.HighlightingService` receives all `ICodeHighlighter` registrations via constructor injection and sorts them by priority

### Key points
- Custom highlighters are registered alongside built-ins, not instead of them — the priority system handles dispatch
- If the highlighter needs DI services (e.g., a configuration object), register with the appropriate DI overload: `services.AddSingleton<ICodeHighlighter>(sp => new PipelineHighlighter(sp.GetRequiredService<PipelineConfig>()))`

## Beat 6: Write a Documentation Page and Verify

Create a markdown page with a ` ```pipeline ` code block and confirm syntax highlighting renders.

### What to show
- A markdown file `Content/guides/pipeline-config.md` containing a fenced code block:
  ```
  source "events-db"
    transform filter when status == "active"
    transform map select name, timestamp
  sink "analytics-lake"
    # Write to the analytics data lake
  ```
- Run the site and navigate to the page — keywords appear colored, strings are highlighted, comments are visually distinct
- Inspect the rendered HTML to see the `<span class="hljs-keyword">`, `<span class="hljs-string">`, and `<span class="hljs-comment">` elements

### Key points
- The language tag in the markdown fence (` ```pipeline `) must match one of the strings in `SupportedLanguages`
- MonorailCSS or the site's existing stylesheet provides colors for the `hljs-*` classes — no additional CSS is needed if you use the standard class names

## Beat 7: Understand Priority Ordering and Edge Cases

Demonstrate what happens when priorities conflict and how to reason about the dispatch chain.

### What to show
- Temporarily change `Priority` to `200` (above everything). For a language like `csharp` that `T:Pennington.Highlighting.TextMateHighlighter` also supports via `*`, the pipeline highlighter would NOT win because `SupportedLanguages` only contains `pipeline` and `pipe` — the dispatch checks `SupportedLanguages.Contains(language)` first, then `SupportedLanguages.Contains("*")`
- Show what happens if you add `"*"` to `SupportedLanguages` at priority 200 — the custom highlighter would intercept every language. This is almost always wrong
- Demonstrate `M:Pennington.Highlighting.HighlightingService.HasHighlighter(System.String)` returning `true` for `"pipeline"` now that the custom highlighter is registered

### Key points
- Keep `SupportedLanguages` narrow — only claim languages your highlighter actually handles
- Priority only matters between highlighters that claim the same language
- The wildcard `*` should only be used by general-purpose highlighters like TextMate — custom highlighters should enumerate their specific languages

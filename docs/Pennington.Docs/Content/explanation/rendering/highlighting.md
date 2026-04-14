---
title: The syntax-highlighting cascade
description: The priority-ordered highlighter chain, why TextMateSharp was chosen, and where deferred Roslyn support fits in.
section: rendering
order: 20
tags: []
uid: explanation.rendering.highlighting
isDraft: true
search: false
llms: false
---

> **In this page.** The priority-ordered highlighter chain (custom -> ShellHighlighter -> TextMateHighlighter -> PlainTextHighlighter), why TextMateSharp was chosen, and where deferred Roslyn support fits in.
>
> **Not in this page.** How to add a highlighter (see How-Tos).

## The question

- Why does Pennington dispatch a fenced code block to one of several highlighters by priority rather than picking by language alone?

## Context

- Markdown fences arrive with language hints that range from precise (`csharp`, `python`) to vague (`sh`, empty).
- A single highlighter cannot cover both breadth (every TextMate grammar) and depth (semantic C# classification) without bloating the core package.
- Goal: give every fence a deterministic, non-throwing HTML rendering while still allowing specialized engines to win for specific languages.
- Constraint: the core library must remain free of a solution-workspace dependency; Roslyn is an optional layer.

## How it works

- Single entry point: `HighlightingService.Highlight(code, language)` in `src/Pennington/Highlighting/HighlightingService.cs`.
- Constructor receives every `ICodeHighlighter` registered in DI; sorts them by `Priority` descending once.
- On each call, walks the sorted list and returns the first whose `SupportedLanguages` contains the requested language or the wildcard `"*"`.
- If none match, falls back to a private `PlainTextHighlighter` instance held by the service (not part of the registered list).
- `HasHighlighter(language)` tells callers whether a real highlighter exists (ignores the fallback) so the Markdig renderer can decide whether to emit the `highlighted` class.

### The priority contract

```csharp:xmldocid
T:Pennington.Highlighting.ICodeHighlighter
```

- `Priority` is a plain `int` on `ICodeHighlighter` — higher wins.
- No negotiation, no merging, no second-chance: the first match returns.
- Rationale: a cascade is easier to reason about than a scoring system, and registration order becomes irrelevant (only `Priority` matters).

### The four tiers, by priority

| Priority | Highlighter | Scope | Project |
| --- | --- | --- | --- |
| 100 | `RoslynHighlighter` | `csharp`, `cs`, `c#`, `vb`, `vbnet` | `Pennington.Roslyn` (opt-in) |
| 75 | `ShellHighlighter` | `bash`, `shell`, `sh` | `Pennington` core |
| 50 | `TextMateHighlighter` | `*` (every TextMate grammar) | `Pennington` core |
| 0 | `PlainTextHighlighter` | `*` (fallback, HTML-encodes) | `Pennington` core |

- `RoslynHighlighter` only exists once `AddPenningtonRoslyn` registers it; the other three are always present.
- `TextMateHighlighter` advertises `"*"`, so without a higher-priority specialist it handles everything a TextMate grammar exists for; unknown languages still get wrapped in `<pre><code class="language-...">` with HTML-encoded content.
- `PlainTextHighlighter` is the private safety net inside `HighlightingService` — it is reachable only when no registered highlighter matches (not even TextMate's wildcard), which in practice means the only registered highlighters are custom ones with narrower scopes.

### Why ShellHighlighter sits above TextMate

- TextMate grammars for bash exist, but shell fences in docs are typically short, command-shaped, and benefit from a simple `command + flags + strings` treatment rather than full grammar tokenization.
- A hand-written regex pass keeps shell blocks from picking up spurious scopes (e.g., keyword highlighting on `cd` or `ls`) and avoids loading a grammar for a ten-character fence.
- Priority 75 places it above the `*`-wildcard TextMate tier but below any future specialist that might want to override it.

### Why TextMateSharp

- TextMateSharp ships the VS Code grammar set, so "does Pennington highlight language X" reduces to "does VS Code highlight language X" — an answer the ecosystem already tracks.
- Grammars are data, not code: adding a new language is a registration, not a new highlighter class.
- The trade-off is accuracy. TextMate grammars are pattern-based — they produce plausible-looking highlighting without understanding semantics (no type resolution, no cross-reference awareness). For C# specifically this gap is visible: `string` the type and `"string"` the literal are both strings to a grammar, but a semantic analyzer distinguishes them.
- Implementation detail: tokenize calls are serialized behind a shared `Lock` because the underlying `Registry` is not thread-safe, and each call has a five-second tokenize budget so a pathological grammar cannot stall a render.

### Roslyn as an opt-in upgrade

- `Pennington.Roslyn` is a separate package. It exists because semantic C#/VB highlighting requires the Roslyn compiler — a dependency large enough that most sites should not pay for it.
- `AddPenningtonRoslyn(...)` always registers `RoslynHighlighter` at priority 100, which shadows TextMate for C#/VB fences the moment the package is wired in.
- The highlighter uses an `AdhocWorkspace` and the `Classifier` API — no solution file required. A solution path is only needed for the xmldocid preprocessor and symbol extraction, which are separate features that live in the same package but are unrelated to the cascade.
- Because the cascade is priority-driven, enabling Roslyn is a one-line DI change: no Markdig re-wiring, no conditional branches in the renderer. The pipeline stays the same; the highest-priority handler for `csharp` simply changes.

## Trade-offs

- **Cost.** First-match-wins means a highlighter that claims a language claims it completely — there is no "Roslyn for semantics, TextMate for comments" hybrid. This is the price of a simple cascade.
- **Alternative considered.** A capability-scoring model where each highlighter returned a confidence number. Rejected as harder to debug (why did X win?) and slower (every highlighter runs). Priority-ordered first match is boring and predictable.
- **Alternative considered.** Forcing Roslyn into the core package to unify the C# story. Rejected because the Roslyn dependency graph is heavy and most consumers (blog sites, marketing pages) do not highlight C# at all.
- **Consequence.** Adding a highlighter is additive: register it at a priority above the incumbent and it wins for its declared languages. Removing one is equally local. The cost is that a mistakenly high priority silently hides the previous handler — diagnostics, not compile errors, catch this.

## Further reading

- Reference: [Highlighting interfaces](/reference/extension-points/highlighting)
- How-to: [Add a custom syntax highlighter](/how-to/extensibility/custom-highlighter)
- Tutorial: [Connect to a Roslyn solution for live API snippets](/tutorials/beyond-basics/connect-roslyn)
- External: [TextMateSharp](https://github.com/danipen/TextMateSharp)

---
title: "The syntax-highlighting cascade"
description: "Why Pennington dispatches code fences through a priority-ordered chain of highlighters with a guaranteed plain-text fallback instead of a single parser."
uid: explanation.rendering.highlighting
order: 2
sectionLabel: "Rendering and Theming"
tags: [highlighting, textmate, cascade]
---

A content engine that renders Markdown through Markdig could reasonably pick one syntax highlighter and ship it — so why does Pennington dispatch every fenced code block through a priority-ordered chain of highlighters that falls through to a plain-text fallback, instead of binding the pipeline to a single parser?

## Context

Pennington renders code in very different shapes: shell sessions that want command-and-flag styling but no formal grammar, and roughly eighty mainstream languages that need real tokenization. A single-parser design forces one of those shapes to lose. A shell-only build gives up nearly the full language surface the first time someone pastes a Python snippet; a TextMate-only build styles a bash command no differently from its flags. The design brief was therefore "layer, don't pick" — every shape is a highlighter, the ones that care most about a given language win, and nothing Markdig hands the service can make the pipeline throw. The fallback is not a politeness; it is the property that keeps `HighlightingService` total.

## How it works

### The priority chain

`HighlightingService` takes every registered `ICodeHighlighter` at construction, sorts once by descending `Priority`, and for each code block walks that list checking whether the language is supported (or the highlighter declared `"*"` as a catch-all). The first hit wins; if none match, the service reaches for a hardcoded `PlainTextHighlighter` fallback, which HTML-encodes the code and hands it back. The shipped chain is a 50/75 ladder: `TextMateHighlighter` at 50 with `"*"` so it claims any language it can find a grammar for, and `ShellHighlighter` at 75 for `bash`/`shell`/`sh` specifically. `PlainTextHighlighter` is the service's private fallback — it never appears in the priority chain, so it cannot be displaced by a misconfigured priority; a higher-priority highlighter slots in to claim specific languages above TextMate.

The cascade is specificity-ordered, not quality-ordered. Shell wins over TextMate for bash not because it produces better HTML in the abstract, but because it knows the one thing worth styling in a command fence — the command itself versus its flags. A new custom highlighter slots in by announcing a higher priority for the languages it cares about; it does not have to replace or remove anything that is already there. When no chain entry matches and no `"*"` catch-all is registered, the plain-text fallback runs and the service emits a once-per-language `Info` diagnostic so authors notice a missing grammar without the build failing.

The `HighlightingService` dispatcher is stateless past construction, so adding a highlighter via `HighlightingOptions.AddHighlighter` in DI is enough — no registry mutation, no re-sorting at runtime, no ordering surprise that depends on registration order. Priority is the only tiebreaker that matters.

The `ICodeHighlighter` contract is three members — `SupportedLanguages`, `Priority`, and `Highlight(code, language)` — which is the narrowest shape that still lets the dispatcher make a correct choice without asking a highlighter to render first and regret later. See <xref:reference.api.i-code-highlighter> for the interface surface.

### Why TextMateSharp

The broad middle of the chain — every language that is not bash and not C# — runs through `TextMateHighlighter`, which loads TextMate grammars through TextMateSharp and tokenizes line by line. TextMate grammars are the same regex-state-machine format VS Code uses for its default highlighting, which buys Pennington roughly the full set of languages you've heard of in a single dependency, without compiling a parser, without building an AST, and without pulling a language service per language. The highlighter keeps a scope-to-hljs-class mapping table so the emitted HTML uses the familiar `hljs-keyword` / `hljs-string` / `hljs-type` class names, meaning the same CSS theme highlights Python, Rust, Go, and JSON uniformly.

The alternatives that were considered and rejected make the choice clearer. A single-language semantic parser covers a language or two out of eighty and ships a heavy dependency for zero value on the rest. A Prism or highlight.js port would require either a JavaScript runtime at build time or a reimplementation of dozens of grammars in C#; TextMateSharp inherits VS Code's grammar corpus for free. A hand-rolled regex-per-language table scales linearly with language count and loses the "paste a new fence, it works" property the first time someone wants Kotlin. TextMate's cost is real — it is a regex state machine, so it does not know that `Foo` on line 40 refers to the `class Foo` on line 2 — but a higher-priority highlighter can be slotted in for a language that needs more, which is exactly what the cascade is shaped to allow.

The `"*"` entry in `TextMateHighlighter.SupportedLanguages` is load-bearing — it is how TextMate claims every language it can find a grammar for without having to enumerate the list at registration time, and it is what lets a new grammar added to the registry light up automatically.

### Slotting in a higher-priority highlighter

The cascade is the extension mechanism. A highlighter that wants to claim a language TextMate already handles — say a semantic C# highlighter that can tell a type name apart from a method name, resolve generic arguments, and annotate references to types in other files — registers at a priority above 50 for `csharp`/`cs`/`c#`. When it is present, the only change to the cascade is that C# rises from "TextMate handles it" to "the new highlighter handles it"; every other language keeps its previous highlighter.

Nothing in the core has to change to allow that. The base package ships the shell and TextMate tokenizers plus a plain-text fallback that together cover every site that does not need language-specific semantic treatment, and a higher-priority highlighter is purely additive — declare the relevant languages, pick a priority that beats whatever is currently handling them, register. The cascade does not know or care where a highlighter came from.

## Further reading

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter) — `ICodeHighlighter`, `HighlightingService`, `TextMateLanguageRegistry`, and `ICodeBlockPreprocessor` with full member tables.
- How-to: [Add a custom syntax highlighter](xref:how-to.markdown-pipeline.custom-highlighter) — the step-by-step for implementing `ICodeHighlighter`, picking a priority, and registering via `HighlightingOptions.AddHighlighter`.
- External: [TextMateSharp](https://github.com/danipen/TextMateSharp) — the upstream library that provides the grammar corpus; authoring new grammars follows its documentation, not Pennington's.

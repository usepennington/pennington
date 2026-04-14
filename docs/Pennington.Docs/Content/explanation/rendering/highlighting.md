---
title: "The syntax-highlighting cascade"
description: "Why Pennington dispatches code fences through a priority-ordered chain of highlighters with a guaranteed plain-text fallback instead of a single parser."
uid: explanation.rendering.highlighting
order: 302020
sectionLabel: "Rendering and Theming"
tags: [highlighting, textmate, roslyn, cascade]
---

> **In this page.** The priority-ordered highlighter chain (custom, `ShellHighlighter`, `TextMateHighlighter`, `PlainTextHighlighter`), why TextMateSharp was chosen for the broad middle, and where the deferred Roslyn highlighter fits in.
>
> **Not in this page.** The recipe for implementing `ICodeHighlighter` and registering one with `HighlightingOptions.AddHighlighter` — see the how-to on adding a custom highlighter.

## The question

_One sentence, in the reader's voice. Draft: "A content engine that renders Markdown through Markdig could reasonably pick one syntax highlighter and ship it — so why does Pennington dispatch every fenced code block through a priority-ordered chain of highlighters that fall through to a plain-text fallback, instead of binding the pipeline to a single parser?" Do not answer yet; the rest of the page is the answer._

## Context

_Three to five sentences. Pennington renders code in three very different shapes: shell sessions that want command-and-flag styling but no formal grammar, ~80 mainstream languages that need real tokenization but no semantic analysis, and C# samples pulled from a real solution via xmldocid fences that want Roslyn-grade classification — including type and member references the grammar alone can't see. A single-parser design forces one of those shapes to lose: a Roslyn-only build is a non-starter for YAML, Bash, and TOML blocks, a TextMate-only build can't tell `IReadOnlySet<string>` apart from a user-defined generic, and a bash-aware but otherwise plain build gives up 95% of the language surface the first time a reader pastes a Python snippet. The design brief was therefore "layer, don't pick" — every shape is a highlighter, the ones that care most about a given language win, and nothing Markdig hands us can make the pipeline throw. The fallback is not a politeness; it is the property that keeps `HighlightingService` total._

## How it works

### The priority chain

_Two to three paragraphs. `HighlightingService` takes every registered `ICodeHighlighter` at construction, sorts once by descending `Priority`, and for each code block walks that list asking `SupportedLanguages.Contains(language)` (with `"*"` matching anything). The first hit wins; if none match, the service returns a `PlainTextHighlighter` that HTML-encodes the code and hands it back. The priority numbers form a tidy 0/50/75/100 ladder — `PlainTextHighlighter` at 0, `TextMateHighlighter` at 50 with `"*"` support so it claims any language it can find a grammar for, `ShellHighlighter` at 75 for `bash`/`shell`/`sh` specifically, and `RoslynHighlighter` at 100 for `csharp`/`cs`/`c#`/`vb`/`vbnet` when the Roslyn package is wired._

_Make the layering point explicitly: the cascade is specificity-ordered, not quality-ordered. Shell wins over TextMate for bash not because it produces better HTML in the abstract, but because it knows the one thing the reader actually wants styled in a command fence — the command itself versus its flags. TextMate wins over plain because it has a real tokenizer. Roslyn wins over TextMate for C# because it can classify semantics the grammar can't see. A new custom highlighter slots in by announcing a higher priority for the languages it cares about; it does not have to replace anything._

```csharp:xmldocid
T:Pennington.Highlighting.HighlightingService
```

_After the fence, one sentence: the dispatcher is stateless past construction, so adding a highlighter via `HighlightingOptions.AddHighlighter` in DI is enough — no registry mutation, no re-sorting at runtime, no ordering surprise that depends on registration order (priority is the only tiebreaker that matters)._

```csharp:xmldocid
T:Pennington.Highlighting.ICodeHighlighter
```

_One sentence: the contract is three members — `SupportedLanguages`, `Priority`, and `Highlight(code, language)` — which is the narrowest shape that still lets the dispatcher make a correct choice without asking the highlighter to render first and regret later._

### Why TextMateSharp

_Two paragraphs. The broad middle of the chain — every language that is not bash and not C# — runs through `TextMateHighlighter`, which loads TextMate grammars through TextMateSharp and tokenizes line by line. TextMate grammars are the same regex-state-machine format VS Code uses for its default highlighting, which buys Pennington roughly the full "languages you've heard of" set in a single dependency, without compiling a parser, without building an AST, and without pulling a language service per language. The highlighter keeps a scope-to-hljs-class mapping table so the emitted HTML uses the familiar `hljs-keyword` / `hljs-string` / `hljs-type` class names, which means the same CSS theme highlights Python, Rust, Go, and JSON uniformly._

_Name the alternatives that lost. A Roslyn-only story covers two languages out of eighty and ships a heavy compiler dependency for zero value on the rest. A Prism or highlight.js port would require either a JavaScript runtime at build time or a reimplementation of dozens of grammars in C#; TextMateSharp inherits VS Code's grammar corpus for free. A hand-rolled regex-per-language table scales linearly with language count and would lose the "paste a new fence, it just works" property the first time a reader wants Kotlin. TextMate's cost is real — it is a regex state machine, so it does not know that `Foo` on line 40 refers to the `class Foo` on line 2 — but that cost is exactly what the Roslyn corner is shaped to redeem._

```csharp:xmldocid
T:Pennington.Highlighting.TextMateHighlighter
```

_After the fence, one sentence: the `"*"` entry in `SupportedLanguages` is load-bearing — it is how TextMate claims every language it can find a grammar for without having to enumerate the list at registration time, and it is what lets a new grammar added to the registry light up automatically._

### The Roslyn corner (deferred)

_Two paragraphs. The optional `Pennington.Roslyn` package registers `RoslynHighlighter` at priority 100, which beats TextMate at 50 for `csharp`/`cs`/`c#`/`vb`/`vbnet`. Unlike the TextMate case, Roslyn's advantage is not grammar coverage — TextMate already has a C# grammar — it is semantic classification. Roslyn's classifier can tell a type name apart from a method name apart from a local, can resolve generic arguments, and can annotate references to types that live in other files. That is the quality jump xmldocid fences need: when the Markdown preprocessor pulls a real method body out of a loaded solution via `RoslynCodeBlockPreprocessor`, the same package is already there to classify it properly._

_Frame it as "deferred" because that is the user-facing shape of the feature. Pennington core does not take a Roslyn dependency; the base package ships with the three tokenizers and a plain-text fallback that together cover every site that doesn't care about C# specifically. The Roslyn corner is opt-in via `AddPenningtonRoslyn`, and when it is opted in, the only change to the cascade is that C# rises from "TextMate handles it" to "Roslyn handles it" — every other language keeps its previous highlighter. The cascade is the extension mechanism; Roslyn just happens to be its most interesting user._

```csharp:xmldocid
T:Pennington.Roslyn.Highlighting.RoslynHighlighter
```

_After the fence, one sentence: this is the same pattern a third-party highlighter would follow — declare the languages you care about, pick a priority that beats whatever is currently handling them, register, done; the cascade does not know or care who you are._

## Trade-offs

- **Cost — priority numbers are global and implicit.** _A custom highlighter that announces priority 60 silently displaces TextMate for its chosen languages; there is no central registry warning anyone that the decision happened. That is the price of "no mutable state past construction" — anyone can slot in, and anyone can accidentally steal a language they didn't mean to. The 0/50/75/100 ladder leaves room between tiers deliberately._
- **Alternative considered — one parser chosen at build time.** _Pick Roslyn for C# sites, TextMate for everyone else, wire the choice into DI. Rejected because real sites mix languages — a docs site wants C# samples, bash install commands, YAML front-matter, and JSON configuration on the same page — and a single parser either wins one of those and loses the rest, or has to pretend to be a cascade anyway._
- **Alternative considered — no fallback, throw on unknown languages.** _Rejected for the same reason Markdig extensions do not throw: a bad language tag on a fenced block is an authoring mistake, not a site-killing error. The plain-text fallback means an unrecognized `language-foo` renders as HTML-encoded text with a `language-foo` class attribute still on it, so the reader sees their code and the author sees their typo in the dev overlay. Totality is the property we trade a tiny bit of loudness for._
- **Consequence — grammar-level highlighters cannot understand semantics.** _A TextMate highlighter sees tokens, not meanings; it cannot tell that the `Foo` on line 40 is the class declared on line 2. Sites that need that level of understanding for C# pick up `Pennington.Roslyn`; sites that do not, accept the grammar-level approximation. The cascade does not hide this trade; it names it by letting Roslyn take over for C# when it is present and leaving TextMate responsible otherwise._

## Further reading

- Reference: [Highlighting interfaces](xref:reference.extension-points.highlighting) — `ICodeHighlighter`, `HighlightingService`, `TextMateLanguageRegistry`, and `ICodeBlockPreprocessor` with full member tables.
- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter) — the step-by-step for implementing `ICodeHighlighter`, picking a priority, and registering via `HighlightingOptions.AddHighlighter`.
- External: [TextMateSharp](https://github.com/danipen/TextMateSharp) — the upstream library that provides the grammar corpus; authoring new grammars follows its documentation, not Pennington's.

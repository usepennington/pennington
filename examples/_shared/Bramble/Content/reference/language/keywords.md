---
title: "Keywords"
description: "Complete table of reserved words in Bramble and their roles in the language."
uid: bramble.reference.language.keywords
order: 120
sectionLabel: "Language"
tags: [keywords, reserved words, syntax, reference]
---

Bramble reserves the following identifiers. They cannot be used as variable, function, or type names. The table covers every keyword in the 1.2 stable release.

## Reserved words

| Keyword | Role |
|---|---|
| `fn` | Introduces a function declaration |
| `let` | Binds an immutable value to a name in the current scope |
| `mut` | Modifier on `let` that makes the binding mutable |
| `if` | Begins a conditional expression |
| `else` | Provides the alternative branch of an `if` expression |
| `while` | Begins a loop that continues as long as its condition is `true` |
| `for` | Begins an iteration loop over an iterable value |
| `in` | Separates the binding name from the iterable in a `for` loop |
| `match` | Begins an exhaustive pattern-matching expression |
| `import` | Brings a module or specific names into scope |
| `pub` | Marks a declaration as publicly accessible outside its module |
| `return` | Exits the enclosing function early with an explicit value |
| `struct` | Declares a named record type with named fields |
| `enum` | Declares a sum type with named variants |
| `trait` | Declares an interface that types can implement |
| `spawn` | Launches a lightweight concurrent task |
| `await` | Suspends the current task until an async value resolves |
| `true` | The boolean literal for the true value |
| `false` | The boolean literal for the false value |

## Notes on selected keywords

`let` without `mut` creates a binding that the compiler will reject on any reassignment attempt. Annotate with `mut` only when mutation is genuinely needed; the formatter (`sprig`) flags unnecessary `mut` bindings.

`match` arms must be exhaustive — the compiler emits [B0009](xref:bramble.reference.language.error-codes) for any pattern set that does not cover all variants. A wildcard arm (`_ => ...`) satisfies exhaustiveness.

`spawn` and `await` are reserved in 1.2 but are fully operational only in the 2.0 preview. Using `spawn` in a 1.2 build produces a warning rather than an error.

See [syntax](xref:bramble.reference.language.syntax) for the contexts in which these keywords appear, and [grammar](xref:bramble.reference.language.grammar) for their formal positions in the language grammar.

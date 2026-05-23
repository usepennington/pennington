---
title: "Compiler error codes"
description: "Reference table of Bramble compiler error codes B0001 through B0042, with short descriptions of each diagnostic."
uid: bramble.reference.language.error-codes
order: 160
sectionLabel: "Language"
tags: [errors, diagnostics, compiler, error codes]
---

The Bramble compiler emits structured diagnostics with a code in the range `B0001`–`B0999`. Codes below `B0100` are always hard errors; codes `B0100`–`B0199` are warnings; codes `B0200` and above are informational hints. Running `bramble explain B0014` prints a detailed explanation with examples for any code.

## Error codes B0001–B0042

| Code | Severity | Description |
|---|---|---|
| B0001 | Error | Unexpected token — the parser encountered a token it did not expect at this position |
| B0002 | Error | Unterminated string literal — the source file ended before the closing `"` was found |
| B0003 | Error | Cannot infer type — insufficient context to determine the type of an expression; add an explicit annotation |
| B0004 | Error | Type mismatch — the expected and actual types are incompatible at this position |
| B0005 | Error | Non-associative operator chain — comparison operators cannot be chained without explicit grouping |
| B0006 | Error | Undeclared identifier — the name has not been declared in any enclosing scope |
| B0007 | Error | Reassignment of immutable binding — the binding was declared with `let` (not `let mut`) |
| B0008 | Error | Return type mismatch — the returned expression type does not match the function's declared return type |
| B0009 | Error | Non-exhaustive match — one or more patterns are not covered; add the missing arms or a wildcard `_` |
| B0010 | Error | Duplicate field in struct literal — the same field name appears more than once |
| B0011 | Error | Missing field in struct literal — one or more required fields have no initialiser |
| B0012 | Error | Unknown field — the field name does not exist on the given type |
| B0013 | Error | Arity mismatch — the number of arguments does not match the number of parameters |
| B0014 | Error | Unused `Result` — a `Result` value was discarded without being checked; handle it or discard explicitly with `let _ = ...` |
| B0015 | Error | `?` outside fallible context — the propagation operator was used in a function that does not return `Result` or `Option` |
| B0016 | Error | Incompatible `?` types — the error type of the propagated `Result` does not match the enclosing function's error type |
| B0017 | Error | Trait not implemented — the type does not implement the required trait for this operation |
| B0018 | Error | Recursive type without indirection — a `struct` or `enum` contains itself directly; wrap in a list or another indirection |
| B0019 | Error | Duplicate variant — the same variant name appears more than once in an `enum` |
| B0020 | Error | Pattern type mismatch — the pattern cannot match a value of the scrutinee's type |
| B0021 | Error | Use after move — a value was used after ownership was transferred to another binding or call |
| B0022 | Error | Borrow while mutably borrowed — a second reference was taken while a mutable borrow is active |
| B0023 | Error | Mutable borrow of immutable binding — a mutable reference was requested for a binding declared without `mut` |
| B0024 | Error | Lifetime escaped — a reference outlives the value it points to |
| B0025 | Error | Integer overflow in literal — the numeric literal exceeds the range of `i64` |
| B0026 | Error | Invalid escape sequence — the character following `\` is not a recognised escape code |
| B0027 | Error | Invalid Unicode scalar — the codepoint in a `\u{...}` escape is not a valid Unicode scalar value |
| B0028 | Error | Break or continue outside loop — `break` or `continue` was used in a context with no enclosing `while` or `for` |
| B0029 | Error | Ambiguous method call — two or more traits in scope provide a method with this name; qualify the call |
| B0030 | Error | Import not found — the module path does not resolve to a known module in the workspace or Thicket cache |
| B0031 | Error | Import cycle — a circular chain of `import` declarations was detected; restructure the modules |
| B0032 | Error | Private item — the item is not marked `pub` and is not accessible from the current module |
| B0033 | Error | Unknown module — the module path prefix does not correspond to any package in `bramble.toml` |
| B0034 | Error | Duplicate binding in pattern — the same name is bound more than once within a single pattern |
| B0035 | Error | Or-pattern binding mismatch — the alternatives in an or-pattern `A | B` do not bind the same set of names |
| B0036 | Warning | Unused binding — the binding is never read; prefix with `_` to silence |
| B0037 | Warning | Unused import — the imported name is never referenced in this file |
| B0038 | Warning | Unreachable code — a statement or expression follows an unconditional `return` |
| B0039 | Warning | Unnecessary `mut` — the binding is declared mutable but never mutated |
| B0040 | Warning | Dead variant — the enum variant is never constructed anywhere in the codebase |
| B0041 | Warning | Float equality comparison — comparing `f64` values with `==` or `!=` may produce unexpected results due to rounding |
| B0042 | Hint | Suggest `?` — an explicit `match` on a `Result` or `Option` could be shortened using the `?` operator |

See [syntax](xref:bramble.reference.language.syntax) for the constructs that trigger many of these errors, and [types](xref:bramble.reference.language.types) for type-related background.

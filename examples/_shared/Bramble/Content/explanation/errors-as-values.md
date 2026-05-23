---
title: "Errors as values"
description: "Bramble treats errors as ordinary values using the Result type, making failure paths explicit and compiler-enforced."
uid: bramble.explanation.errors-as-values
order: 90
sectionLabel: "Explanation"
tags: [errors, result, types, reliability]
---

Bramble has no exceptions. When an operation can fail, it says so in its return type — a `Result<T, E>` — and the caller must do something with that fact. This is not a restriction so much as a design statement: failure is part of the interface, not a footnote.

## What Result looks like

A function that reads a file doesn't return `string`; it returns `Result<string, IoError>`. The compiler tracks whether you've handled the error case before you can use the inner value.

```bramble
fn read_config(path: string): Result<Config, IoError> {
    let text = fs::read_text(path)?
    Config::parse(text)
}
```

The `?` operator is the idiomatic way to propagate a failure upward. If the left-hand expression is `Err(e)`, the enclosing function returns `Err(e)` immediately; if it is `Ok(v)`, execution continues with `v` unwrapped. It is syntactic sugar over a `match`, not magic.

## Why the compiler forbids ignoring errors

In many languages you can call a function that returns an error code and throw the result away. Bramble treats `Result` as a value like any other — you cannot assign it to `_` and move on without a deliberate discard annotation. Silently ignoring a `Result` is a compile error.

```bramble
// This does not compile:
write_log(message)

// Either handle it:
let _ = write_log(message)   // explicit discard
// Or propagate:
write_log(message)?
```

The `let _ =` form is the intentional escape hatch; it is visible in code review and searchable.

## Matching vs propagating

`?` is convenient but coarse — it propagates without context. When callers need to distinguish error cases or add context, `match` is the right tool:

```bramble
match parse_port(raw) {
    Ok(port) => start_server(port),
    Err(ParseError::OutOfRange(n)) => fatal("port {n} is out of range"),
    Err(e) => fatal("invalid port: {e}"),
}
```

A common pattern is to convert specific errors into a richer type using `map_err` before propagating, which keeps context without forcing callers into a `match` they don't need.

## Tradeoffs to understand

Result types push error handling to call sites, which can feel verbose. In Bramble this verbosity is intentional: if a function fails in three ways, its callers should know. The alternative — invisible exceptions that unwind the stack — makes control flow harder to reason about and lets errors silently escape module boundaries.

The tradeoff is that error-heavy code accumulates `?` chains that can obscure the happy path. Bramble idiom addresses this with small, focused functions and descriptive error enums rather than a single catch-all error string.

> [!NOTE]
> Bramble 2.0 previews a `try` block that scopes `?`-propagation without requiring a separate function, which helps keep the happy path readable in longer procedures.

For practical patterns see [Handle errors with Result](xref:bramble.how-to.language.handle-errors-with-result). The relationship between error handling and the absence of `null` is discussed in [Why Bramble has no null](xref:bramble.explanation.why-no-null).

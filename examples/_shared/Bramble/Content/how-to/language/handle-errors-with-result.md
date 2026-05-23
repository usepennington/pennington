---
title: "Handle errors with Result"
description: "Return, consume, propagate, and transform errors using Bramble's Result type."
uid: bramble.how-to.language.handle-errors-with-result
order: 110
sectionLabel: "Language"
tags: [errors, result, pattern-matching, propagation]
---

Bramble has no exceptions. Functions that can fail return `Result<T, E>`, where `T` is the success value and `E` is the error type.

## Return a Result from a function

Wrap a success value in `Ok(...)` and a failure in `Err(...)`. The return type annotation makes the contract explicit.

```bramble
import std/io

fn parse_port(raw: str) -> Result<int, str> {
    let n = int::parse(raw) or return Err("not a number: ${raw}")
    if n < 1 or n > 65535 {
        return Err("port out of range: ${n}")
    }
    Ok(n)
}
```

## Consume a Result with match

`match` is the primary tool for handling both arms. The compiler enforces exhaustiveness — you cannot ignore the `Err` branch.

```bramble
match parse_port("8080") {
    Ok(port) => io::println("listening on ${port}"),
    Err(msg) => io::eprintln("bad port: ${msg}"),
}
```

## Propagate errors with ?

The `?` operator short-circuits the current function with the `Err` value when applied to a `Result`. The enclosing function must itself return a compatible `Result`.

```bramble
fn load_config(path: str) -> Result<Config, str> {
    let text = fs::read_to_string(path)?
    let config = Config::parse(text)?
    Ok(config)
}
```

Without `?`, each step would need its own `match`. Use it liberally in functions that chain fallible operations.

## Transform errors with map_err

When you need to convert one error type to another — for example, wrapping a low-level I/O error into a domain error — use `map_err`.

```bramble
fn read_count(path: str) -> Result<int, AppError> {
    let text = fs::read_to_string(path)
        .map_err(|e| AppError::Io(e))?

    int::parse(text.trim())
        .map_err(|e| AppError::Parse(e))
}
```

`map_err` leaves `Ok` values untouched and applies the closure only to the `Err` side.

## Provide a fallback with unwrap_or

When a sensible default exists and the error genuinely does not matter, `unwrap_or` extracts the value or returns the default.

```bramble
let port = parse_port(env::get("PORT").unwrap_or("3000")).unwrap_or(3000)
```

> [!WARNING]
> Avoid `unwrap_or` in library code — callers lose the ability to react to failures. Reserve it for top-level application entry points where the fallback is genuinely safe.

See [errors as values](xref:bramble.explanation.errors-as-values) for the reasoning behind this design, and [the type system](xref:bramble.explanation.the-type-system) for how `Result` is defined as a union type.

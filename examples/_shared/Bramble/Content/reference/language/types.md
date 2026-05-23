---
title: "Built-in types"
description: "Reference for every scalar and compound type built into Bramble, with descriptions and literal examples."
uid: bramble.reference.language.types
order: 140
sectionLabel: "Language"
tags: [types, scalars, collections, option, result]
---

Bramble's type system is static and inferred. Every value has a single concrete type known at compile time. This page lists all types provided by the language itself, prior to any standard library additions.

## Scalar types

| Type | Description | Literal examples |
|---|---|---|
| `i64` | Signed 64-bit integer | `0`, `-1`, `9_223_372_036_854_775_807` |
| `f64` | IEEE 754 double-precision float | `0.0`, `-1.5`, `3.14e8` |
| `bool` | Boolean | `true`, `false` |
| `char` | Single Unicode scalar value | `'a'`, `'\n'`, `'\u{1F33F}'` |
| `str` | Immutable UTF-8 string slice | `"hello"`, `"line\n"`, `"${expr}"` |
| `()` | Unit — the type of expressions with no meaningful value | `()` |

`str` is not a heap-allocated object; it is a reference to a byte sequence. Functions that produce owned strings return a `str` value managed by the GC. String interpolation (`"${...}"`) always produces a fresh `str`.

## Compound types

| Type | Description | Literal / construction example |
|---|---|---|
| `[T]` | Homogeneous resizable list | `[1, 2, 3]` |
| `{K: V}` | Hash map from keys of type `K` to values of type `V` | `{"x": 1.0, "y": 2.0}` |
| `{T}` | Hash set of values of type `T` | `{1, 2, 3}` |
| `(T, U, ...)` | Fixed-length tuple of heterogeneous types | `(42, "hi", true)` |
| `struct` | Named record with named fields | See below |
| `enum` | Sum type with named variants | See below |
| `Option<T>` | Value that is either `Some(v)` or `None` | `Some(5)`, `None` |
| `Result<T, E>` | Value that is either `Ok(v)` or `Err(e)` | `Ok(42)`, `Err("oops")` |
| `fn(T) -> R` | First-class function / closure type | `fn(x: i64) -> i64 { x * 2 }` |

## Struct and enum declaration

```bramble
struct Point {
    x: f64,
    y: f64,
}

let p = Point { x: 1.0, y: 2.5 }

enum Shape {
    Circle { radius: f64 },
    Rect { width: f64, height: f64 },
    Point,
}

let s = Shape.Circle { radius: 3.0 }
```

## Option and Result

`Option<T>` replaces `null`. There is no way to represent the absence of a value except through `Option`. See [why Bramble has no null](xref:bramble.explanation.why-no-null) for the rationale.

`Result<T, E>` is the standard mechanism for recoverable errors. Functions that can fail return `Result` rather than throwing. See [handle errors with Result](xref:bramble.how-to.language.handle-errors-with-result) for usage patterns.

## Type inference

The compiler infers types for `let` bindings from the right-hand-side expression. Explicit annotations are optional but accepted:

```bramble
let x = 42            // inferred: i64
let y: f64 = 42.0     // explicit annotation
let names: [str] = [] // annotation required when the list is empty
```

> **Caution:** Empty collection literals (`[]`, `{}`) require an explicit type annotation or a subsequent use that constrains the type; otherwise the compiler emits [B0003](xref:bramble.reference.language.error-codes).

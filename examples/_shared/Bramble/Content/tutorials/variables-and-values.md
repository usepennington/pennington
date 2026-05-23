---
title: "Variables and values"
description: "Declare variables with let, understand Bramble's immutability-first design, and work with scalar types and string interpolation."
uid: bramble.tutorials.variables-and-values
order: 30
sectionLabel: "Tutorials"
tags: [variables, types, immutability, strings, interpolation]
---

Bramble treats immutability as the default. When you bind a name with `let`, that binding cannot be reassigned. You must opt in to mutability explicitly with `mut`. This design makes data flow easier to reason about and helps the compiler give better error messages.

## Let bindings

Open `variables.bramble` and try these bindings.

```bramble
import std/io

fn main() {
    let name = "Rowan"
    let age  = 31
    let pi   = 3.14159

    io.println("Name: ${name}, age: ${age}, pi: ${pi}")
}
```

```text
Name: Rowan, age: 31, pi: 3.14159
```

Bramble infers the type from the right-hand side. You can write the type explicitly if you prefer, or if the compiler cannot infer it from context.

```bramble
let name: str = "Rowan"
let age: i64   = 31
let pi: f64    = 3.14159
```

## Scalar types

The core scalar types in Bramble are:

| Type | Description | Example literal |
|------|-------------|-----------------|
| `bool` | Boolean | `true`, `false` |
| `i32` | 32-bit signed integer | `42` |
| `i64` | 64-bit signed integer | `42` |
| `f32` | 32-bit float | `3.14f` |
| `f64` | 64-bit float | `3.14` |
| `str` | UTF-8 string | `"hello"` |
| `char` | Single Unicode scalar | `'a'` |

Integer literals default to `i64` and float literals to `f64`. Append a suffix (`i32`, `f32`, etc.) when you need a narrower type.

## Mutability with `mut`

When you need a variable that changes over time, declare it `mut`.

```bramble
import std/io

fn main() {
    let mut count = 0
    count = count + 1
    count = count + 1
    io.println("count is ${count}")
}
```

```text
count is 2
```

Trying to reassign a non-`mut` binding is a compile error (`B0041`). The compiler tells you exactly which line attempted the reassignment, so these errors are quick to fix.

> [!NOTE]
> `mut` marks the *binding* as mutable, not the value's type. A `mut` binding to a list lets you point the name at a different list; it does not automatically make the list's contents mutable. You will see how that works when you reach [Collections](xref:bramble.tutorials.collections).

## String interpolation

You have already seen `${}` in action. Any expression fits inside the braces.

```bramble
let x = 6
let y = 7
io.println("${x} × ${y} = ${x * y}")
```

```text
6 × 7 = 42
```

Multi-line strings use triple quotes. Indentation up to the column of the closing `"""` is stripped automatically.

```bramble
let poem = """
    Grows on you.
    Possibly invasive.
    """
io.println(poem)
```

## Shadowing

You can re-declare a name in the same scope with a new `let`. The new binding *shadows* the old one without needing `mut`.

```bramble
let result = fetch_raw()          // Result<Bytes, err>
let result = result.unwrap_or("")  // str — a different type!
io.println(result)
```

Shadowing is useful when you want to refine or transform a value while keeping the same conceptual name. It is not the same as mutation; the original binding still exists in the compiler's model until it goes out of scope.

With scalar types and bindings covered, move on to [Control flow](xref:bramble.tutorials.control-flow) to learn how to branch and loop over these values.

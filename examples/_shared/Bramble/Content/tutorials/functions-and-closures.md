---
title: "Functions and closures"
description: "Declare named functions with typed parameters and return types, capture values in closures, and pass functions as arguments."
uid: bramble.tutorials.functions-and-closures
order: 50
sectionLabel: "Tutorials"
tags: [functions, closures, higher-order, types]
---

Functions are the primary unit of reuse in Bramble. They are declared with `fn`, require explicit parameter types, and infer the return type from the last expression unless you annotate it. Closures are anonymous functions that can capture values from the surrounding scope.

## Declaring a function

```bramble
import std/io

fn greet(name: str) -> str {
    "Hello, ${name}!"
}

fn main() {
    let message = greet("Ada")
    io.println(message)
}
```

```text
Hello, Ada!
```

The return type after `->` is optional when the function returns `()` (unit), but writing it makes intent clear and is recommended by Sprig's default lint rules. The last expression in a function body is its return value — no `return` keyword required (though `return` exists for early exit).

## Multiple parameters and type inference

```bramble
fn add(a: i64, b: i64) -> i64 {
    a + b
}

fn clamp(value: f64, min: f64, max: f64) -> f64 {
    if value < min { min }
    else if value > max { max }
    else { value }
}
```

Parameters always require explicit types. Bramble does not infer parameter types — this keeps function signatures self-documenting and improves error messages.

## Closures

A closure is written with `|params| expression` or `|params| { block }`.

```bramble
import std/io

fn main() {
    let double = |x: i64| x * 2
    let greet  = |name: str| { io.println("Hey, ${name}!") }

    io.println(double(21))
    greet("Mx. Briar")
}
```

```text
42
Hey, Mx. Briar!
```

Closures capture variables from the enclosing scope by borrowing them. If you need ownership, prefix the capture list with `own`.

```bramble
let prefix = "LOG: "
let log = own |msg: str| { io.println("${prefix}${msg}") }
// `prefix` is now owned by the closure
```

## Passing functions as arguments

Any function or closure can be passed where a function type is expected. Function types are written as `fn(ParamTypes) -> ReturnType`.

```bramble
import std/io

fn apply_twice(f: fn(i64) -> i64, x: i64) -> i64 {
    f(f(x))
}

fn main() {
    let triple = |n: i64| n * 3
    io.println(apply_twice(triple, 2))   // 2 * 3 * 3 = 18
    io.println(apply_twice(|n| n + 10, 5)) // 25
}
```

```text
18
25
```

> [!NOTE]
> Named functions and closures are interchangeable at call sites. You can pass `add` (defined earlier) directly: `apply_twice(add, 4)` — this would fail to compile because `add` takes two arguments, but a single-argument named function works fine.

## Returning functions

Functions can return closures too. The return type is the function type.

```bramble
fn make_adder(n: i64) -> fn(i64) -> i64 {
    |x| x + n
}

let add5 = make_adder(5)
io.println(add5(10))  // 15
```

This pattern appears frequently in Bramble when building pipelines and configuring behaviour. You will see a practical example of it when you reach [Building a CLI tool](xref:bramble.tutorials.building-a-cli-tool).

Up next: [Collections](xref:bramble.tutorials.collections), where you will put iteration and higher-order functions to work on real data structures.

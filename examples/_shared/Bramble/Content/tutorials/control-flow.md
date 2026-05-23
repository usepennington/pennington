---
title: "Control flow"
description: "Branch with if/else, iterate with while and for-in, and get a first taste of Bramble's match expression."
uid: bramble.tutorials.control-flow
order: 40
sectionLabel: "Tutorials"
tags: [control-flow, if, for, match, loops]
---

Bramble's control-flow constructs are expressions, not statements — most of them produce a value. That means `if/else` can sit on the right-hand side of a `let` binding, and `match` arms can return data directly. This section walks through each form and shows where the value-producing behaviour pays off.

## if / else

The familiar form works as you expect.

```bramble
import std/io

fn main() {
    let temperature = 18

    if temperature < 0 {
        io.println("Below freezing")
    } else if temperature < 15 {
        io.println("Cold")
    } else {
        io.println("Comfortable")
    }
}
```

Because `if` is an expression, you can assign it directly.

```bramble
let label = if temperature < 15 { "cold" } else { "warm" }
io.println("It feels ${label} today")
```

Both branches must produce the same type. The compiler reports `B0110` if they diverge.

## while loops

`while` repeats a block as long as a condition holds.

```bramble
let mut n = 1
while n <= 5 {
    io.println("n = ${n}")
    n = n + 1
}
```

```text
n = 1
n = 2
n = 3
n = 4
n = 5
```

Bramble does not have a C-style `for(;;)` loop. Use `while true { }` when you need an explicit infinite loop, and `break` to exit it.

## for-in loops

`for-in` iterates over anything that implements the `Iterable` interface — lists, ranges, map entries, and custom types alike.

```bramble
let fruits = ["apple", "blueberry", "cherry"]

for fruit in fruits {
    io.println("I like ${fruit}")
}
```

Ranges are written with `..` (exclusive upper bound) or `..=` (inclusive).

```bramble
for i in 1..=10 {
    io.println(i)
}
```

> [!TIP]
> If you need both the index and the value, use `.enumerate()` on the iterable: `for (i, item) in fruits.enumerate() { }`. You will see this pattern often when building CLI tools.

## A first taste of match

`match` tests a value against a set of patterns. Every arm must be handled — the compiler enforces exhaustiveness.

```bramble
import std/io

fn describe(n: i64) -> str {
    match n {
        0       => "zero",
        1..=9   => "single digit",
        10..=99 => "two digits",
        _       => "large",
    }
}

fn main() {
    io.println(describe(0))
    io.println(describe(7))
    io.println(describe(42))
    io.println(describe(1000))
}
```

```text
zero
single digit
two digits
large
```

The `_` arm is a catch-all wildcard. If you remove it and the other arms are not exhaustive, the compiler emits `B0210`.

`match` becomes especially powerful with `Option` and `Result` types, which you will encounter in [Working with files](xref:bramble.tutorials.working-with-files) and beyond. For now, the range pattern form above is enough to keep you moving.

Continue to [Functions and closures](xref:bramble.tutorials.functions-and-closures) to learn how to organise logic into reusable pieces.

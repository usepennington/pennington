---
title: "Collections"
description: "Work with Bramble's built-in list, map, and set types, iterate over them, and apply common transformation operations."
uid: bramble.tutorials.collections
order: 60
sectionLabel: "Tutorials"
tags: [collections, list, map, set, iteration]
---

Bramble ships three general-purpose collection types in the standard library: `List`, `Map`, and `Set`. All three are generic, immutable by default, and implement `Iterable` — so the `for-in` loop and higher-order methods you have already seen work uniformly across them.

## Lists

A list literal uses square brackets. The type is inferred from the elements.

```bramble
import std/io

fn main() {
    let colors = ["red", "green", "blue"]

    for color in colors {
        io.println(color)
    }

    io.println("Count: ${colors.len()}")
    io.println("First: ${colors.first().unwrap_or("none")}")
}
```

```text
red
green
blue
Count: 3
First: red
```

`first()` returns `Option<str>` — Bramble has no null, so potentially-absent values are always wrapped. `.unwrap_or(default)` extracts the value or falls back to the default.

Common list methods:

| Method | Returns | Description |
|--------|---------|-------------|
| `.len()` | `i64` | Number of elements |
| `.first()` | `Option<T>` | First element |
| `.last()` | `Option<T>` | Last element |
| `.push(val)` | `List<T>` | New list with val appended |
| `.map(f)` | `List<U>` | Transform each element |
| `.filter(pred)` | `List<T>` | Keep matching elements |
| `.contains(val)` | `bool` | Membership test |

> [!NOTE]
> `push`, `map`, and `filter` return *new* lists — they do not modify the original. For a mutable list that grows in place, declare it `mut` and use `.push_mut(val)`.

## Maps

Maps associate keys with values. Both key and value types are inferred.

```bramble
import std/io

fn main() {
    let scores = {
        "Alice": 95,
        "Bob":   82,
        "Carol": 91,
    }

    for (name, score) in scores {
        io.println("${name}: ${score}")
    }

    let alice_score = scores.get("Alice").unwrap_or(0)
    io.println("Alice scored ${alice_score}")
}
```

Map literals use `{ key: value }` syntax. `.get(key)` returns `Option<V>` — if the key is missing you get `None`, never a crash.

## Sets

A set holds unique values and provides fast membership testing.

```bramble
import std/collections

let tags = Set.from(["bramble", "lang", "scripting", "bramble"])
io.println(tags.len())          // 3 — duplicate removed
io.println(tags.contains("lang")) // true
```

## Chaining transformations

Where collections become truly useful is in transformation chains.

```bramble
import std/io

fn main() {
    let numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

    let result = numbers
        .filter(|n| n % 2 == 0)
        .map(|n| n * n)
        .reduce(0, |acc, n| acc + n)

    io.println("Sum of squares of evens: ${result}")
}
```

```text
Sum of squares of evens: 220
```

Each method in the chain returns a new collection (or value) — nothing is mutated. The compiler is able to optimise many such chains into a single pass over the data.

With collections in hand, you have everything you need to handle structured data in memory. The next step is reading that data from disk: continue to [Working with files](xref:bramble.tutorials.working-with-files).

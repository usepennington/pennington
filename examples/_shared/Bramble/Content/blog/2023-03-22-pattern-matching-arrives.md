---
title: "Bramble 0.2: pattern matching arrives"
description: "Maple Okafor walks through the pattern matching system landing in Bramble 0.2, including match expressions, record patterns, and exhaustiveness checking."
date: 2023-03-22
author: Maple Okafor
tags: [release, pattern-matching, language, types]
uid: bramble.blog.pattern-matching-arrives
---

Bramble 0.2 is out, and the headline feature is one I've been quietly designing since before we shipped 0.1: first-class pattern matching. This post walks through what landed, how it works, and why exhaustiveness checking is the part I'm most proud of.

## Match expressions

The `match` expression dispatches on the shape of a value. Every arm produces a value of the same type, and the compiler verifies that every possible shape is covered.

```bramble
type Shape =
    | Circle(radius: Float)
    | Rect(width: Float, height: Float)
    | Triangle(base: Float, height: Float)

fn area(s: Shape) -> Float {
    match s {
        Circle(r)          => 3.14159 * r * r
        Rect(w, h)         => w * h
        Triangle(b, h)     => 0.5 * b * h
    }
}
```

If you add a new variant to `Shape` and forget to update `area`, the compiler tells you immediately. No silent fallthrough, no runtime surprises.

## Record patterns

0.2 also introduces record patterns, which let you destructure named fields inline:

```bramble
type Config = { host: String, port: Int, tls: Bool }

fn describe(c: Config) -> String {
    match c {
        { tls: true,  port: 443 } => "standard HTTPS"
        { tls: true,  port: p   } => "HTTPS on {p}"
        { tls: false, port: 80  } => "plain HTTP"
        { tls: false, port: p   } => "plain HTTP on {p}"
    }
}
```

Fields you don't mention are ignored. Fields you do mention must match exactly, and bindings like `p` capture the value for use on the right-hand side.

## Before and after

Before pattern matching, handling `Option` required nested `if let` chains:

```bramble
// Before 0.2
let name = if let Some(user) = find_user(id) {
    if let Some(display) = user.display_name {
        display
    } else {
        user.username
    }
} else {
    "anonymous"
}
```

After:

```bramble
// 0.2
let name = match find_user(id) {
    Some({ display_name: Some(n), .. }) => n
    Some({ username: u, ..          }) => u
    None                               => "anonymous"
}
```

The `..` wildcard ignores fields not listed. The whole expression is a single, readable shape.

## Exhaustiveness

The thing I care about most is the exhaustiveness checker. It's not just "did you write a wildcard arm". It tracks which constructors and field combinations are covered and tells you specifically what's missing:

```text
error: non-exhaustive match on `Option<User>`
  missing arm: None
```

> [!NOTE]
> The exhaustiveness checker runs on enum variants, record fields, and literal ranges. Catching gaps at compile time rather than at 2 AM is worth the upfront effort of writing complete matches.

The checker builds a coverage matrix and uses a column-splitting algorithm that handles nested patterns correctly. There's a lot of prior art here — we leaned heavily on ML family languages — but getting it right for Bramble's specific type system took several passes.

## What's next

Package management. We've been designing the registry and the `bramble.toml` manifest format in parallel with pattern matching. That work is nearly ready for a beta.

Install or upgrade with `bramble update`, or see the [install tutorial](xref:bramble.tutorials.install-bramble) if you're starting fresh.

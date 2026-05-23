---
title: "Introducing Bramble"
description: "Bram Foley announces Bramble 0.1, a small statically-typed scripting language built around no-null design, errors as values, and a sandboxed bytecode VM."
date: 2023-01-10
author: Bram Foley
tags: [release, language, announcement, null-safety, errors]
uid: bramble.blog.introducing-bramble
---

Today we're opening the doors on Bramble — a small, fast, statically-typed scripting language with a simple promise: grows on you, possibly invasive.

That tagline is half joke, half warning. Bramble is designed to embed easily in existing projects, run safely inside a sandbox, and get out of your way. But once you start writing it, you tend to want it everywhere.

## What Bramble is

Bramble is a bytecode-compiled scripting language targeting the 0.1 milestone we're shipping today. The compiler, runtime, and REPL are all bundled into one binary — the `bramble` command, sometimes called "the patch". The VM is small, the GC is generational, and the sandbox is on by default. You don't get file system or network access unless the host explicitly grants it.

The language itself sits in a middle ground: more expressive than Lua, leaner than Python, statically typed without the ceremony of a full systems language.

## Two decisions that shape everything

Two choices define Bramble's character more than anything else.

First: **no `null`**. Bramble uses `Option` instead. If a value might be absent, the type says so. If the type doesn't say so, the value is there. This isn't a novel idea, but it's one the language takes seriously from the start — not as a library add-on, but as a primitive.

Second: **errors are values**. Functions that can fail return `Result`. You handle the failure at the call site. There are no exceptions to catch or forget. The compiler sees through the whole chain, and if you ignore a `Result`, it tells you.

```bramble
fn divide(a: Float, b: Float) -> Result<Float, String> {
    if b == 0.0 {
        Err("division by zero")
    } else {
        Ok(a / b)
    }
}

let result = divide(10.0, 0.0)
match result {
    Ok(v)  => print("Got {v}")
    Err(e) => print("Failed: {e}")
}
```

Both decisions point at the same goal: making the impossible states in your program visible, rather than letting them hide until runtime. You can read more about the reasoning in [Why Bramble has no null](xref:bramble.explanation.why-no-null) and [Errors as values](xref:bramble.explanation.errors-as-values).

## Where things stand today

Bramble 0.1 is early. The core language is there — types, functions, closures, pattern matching is coming in the next release. The standard library is thin. There's no package manager yet (that's coming). The error messages are sometimes unhelpful. We know.

What we want from this release is real feedback from real programs. If you hit a wall, open an issue. If something delights you, tell us that too.

## Getting started

Install the `bramble` binary, run `bramble repl` to open the patch, or follow the [install tutorial](xref:bramble.tutorials.install-bramble) for a walkthrough. The docs are sparse in places but we're writing as fast as we can.

We've been building Bramble in the open because the best language tooling comes from people who actually use it. Come in, poke around, and let us know what you find.

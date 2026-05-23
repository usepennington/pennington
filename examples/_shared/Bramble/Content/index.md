---
title: "Bramble"
description: "A small, fast scripting language that grows on you. Possibly invasive."
uid: bramble.index
order: 0
sectionLabel: "Overview"
tags: [overview, bramble]
---

Bramble is a small, statically-typed scripting language built for the kind of
glue code that quietly runs everything: build steps, data shuffling, the script
nobody wants to own three years from now. It compiles to a compact bytecode VM,
starts fast, and refuses to let `null` into the building. Errors are values, not
surprises, and a lightweight ownership model keeps memory honest without a
ceremony-heavy type system.

The toolchain is deliberately boring in the good way. **Thicket** fetches and
publishes packages, **Trellis** runs your builds, **Sprig** formats and lints,
and **Hedge** lets you try the whole thing in a browser tab. None of them ask you
to learn a configuration language with its own emotional support group.

## Start here

- New to Bramble? Walk the [tutorials](xref:bramble.tutorials.index) from install to a running web service.
- Have a specific task? The [how-to guides](xref:bramble.how-to.index) solve one problem each.
- Looking something up? The [reference](xref:bramble.reference.index) covers the language, standard library, CLIs, and config files.
- Want the reasoning behind a decision? The [explanations](xref:bramble.explanation.index) dig into the design.

## What a Bramble program looks like

```bramble
import std/io

fn main() {
    let names = ["Maple", "Rowan", "Juniper"]
    for name in names {
        io.println("hello, ${name}")
    }
}
```

No semicolons, no `null`, no `try`/`catch`. A function that can fail returns a
`Result`, and the compiler will not let you ignore it.

## Where it fits

| You want to...                         | Reach for       |
| -------------------------------------- | --------------- |
| Automate a one-off task                | a single script |
| Share reusable code                    | a Thicket package |
| Wire several build steps together      | a `Trellisfile` |
| Keep a team's style consistent         | Sprig           |

Bramble is currently at **1.2**, with **2.0** in preview. Follow along on the
[blog](/blog).

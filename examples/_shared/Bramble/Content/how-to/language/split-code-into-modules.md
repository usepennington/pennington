---
title: "Split code into modules"
description: "Organise Bramble source into multiple files, control visibility with pub, and import across modules."
uid: bramble.how-to.language.split-code-into-modules
order: 160
sectionLabel: "Language"
tags: [modules, imports, visibility, project-structure]
---

In Bramble every source file is a module. The module's name is its path relative to the project root, with `/` as the separator and the `.br` extension dropped.

## Create a module file

Add a `.br` file anywhere under your project's source tree. Declarations in the file are private by default. Mark them `pub` to make them visible to importers.

```bramble
// src/math/stats.br

pub fn mean(values: list<float>) -> float {
    let sum = values.fold(0.0, |a, b| a + b)
    sum / values.len() as float
}

fn variance_internal(values: list<float>, m: float) -> float {
    values.map(|v| (v - m) * (v - m)).fold(0.0, |a, b| a + b) / values.len() as float
}

pub fn variance(values: list<float>) -> float {
    variance_internal(values, mean(values))
}
```

`variance_internal` is not `pub`, so it is invisible outside this file.

## Import from another module in your project

Use the module's path relative to the project root, without the `src/` prefix by convention, as the import path.

```bramble
// src/main.br

import math/stats

fn main() {
    let data = [2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0]
    io::println("mean: ${stats::mean(data)}")
    io::println("variance: ${stats::variance(data)}")
}
```

The module name after import becomes the namespace qualifier (`stats::`).

## Alias an import

Use `as` to bind the module to a shorter name, which is useful when two modules share a suffix.

```bramble
import math/stats as s
import text/stats as ts

io::println("${s::mean(data)}")
io::println("${ts::word_count(text)}")
```

## Re-export from a module index

A file named `mod.br` inside a directory acts as the directory's public face. Items it re-exports with `pub use` become part of that directory's module API.

```bramble
// src/math/mod.br

pub use math/stats::{ mean, variance }
pub use math/matrix::Matrix
```

Consumers can then import `math` instead of each sub-module.

```bramble
import math

let m = math::mean(data)
```

> [!NOTE]
> Module paths are resolved relative to the project root as declared in `bramble.toml`. If you move files, update the import paths — Bramble does not search subdirectories automatically.

For how the compiler resolves module paths at build time see [module resolution](xref:bramble.explanation.module-resolution). The `bramble.toml` `[paths]` section that controls source roots is documented in the [bramble.toml reference](xref:bramble.reference.config.bramble-toml).

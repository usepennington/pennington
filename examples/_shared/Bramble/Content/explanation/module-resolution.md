---
title: "How modules are resolved"
description: "Bramble treats every source file as a module and resolves import paths through a deterministic search order anchored to the lockfile."
uid: bramble.explanation.module-resolution
order: 100
sectionLabel: "Explanation"
tags: [modules, imports, packages, resolution]
---

Every `.bram` file is a module. There is no explicit module declaration at the top of a file — the file's path within the package is its identity. This keeps the surface area small: the file system is the namespace hierarchy, and moving a file changes its import path, which is both predictable and occasionally inconvenient.

## Import path syntax

An import path is a slash-separated string that starts with either a package name or a standard library prefix:

```bramble
import std/io
import std/collections/map
import myapp/auth/session
import http_client/request
```

Paths starting with `std/` resolve against the bundled standard library. All other paths are resolved by searching first within the current package, then within declared dependencies.

## Search order

When the compiler sees `import http_client/request`, it follows this sequence:

1. Look for `src/http_client/request.bram` inside the current package.
2. Look up `http_client` in `thicket.lock` to find the pinned version and its source root.
3. Look for `request.bram` inside that package's declared public source directory.
4. If nothing matches, the import is an error.

There is no ambient path, no environment-variable override, and no fallback to a global install directory. Resolution is fully reproducible from the lockfile alone.

## What the lockfile contributes

The lockfile does not just record versions — it records the exact content hash and source root for every dependency. This means two machines with the same `thicket.lock` will resolve the same import to the same bytes, regardless of what else is installed. The compiler refuses to proceed if an import resolves to a package not present in the lockfile, which prevents "it worked on my machine" situations caused by a stale global cache.

```text
# thicket.lock (excerpt)
[[package]]
name    = "http_client"
version = "2.1.4"
hash    = "sha256:4a7f..."
root    = "src"
```

## Package-internal vs re-exported modules

By default, a module inside a package is importable by any other module in the same package. To make a module importable by external consumers, it must appear in the `[exports]` table of `bramble.toml`. Attempting to import a non-exported module from an external package is a compile-time error, not a runtime surprise.

```toml
# bramble.toml
[exports]
public = ["request", "response", "client"]
```

This boundary means library authors can refactor internal modules freely without worrying about breaking consumers.

## Circular imports

Circular imports between modules in the same package are not allowed. The compiler detects cycles at build time and reports the full cycle so it can be broken. Because the import graph must be a DAG, the compiler can determine a valid compilation order without heuristics.

For practical steps on structuring a codebase into modules, see [Split code into modules](xref:bramble.how-to.language.split-code-into-modules). The role of `bramble.toml` in declaring exports and dependencies is covered in the [bramble.toml reference](xref:bramble.reference.config.bramble-toml).

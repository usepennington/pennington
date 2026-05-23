---
title: "Cross-compile"
description: "How to build Bramble bytecode artifacts for multiple target platforms from a single machine using Trellis."
uid: bramble.how-to.trellis.cross-compile
order: 330
sectionLabel: "Trellis"
tags: [trellis, cross-compile, targets, platforms, build]
---

Bramble's bytecode VM runs on any supported platform, but the runtime binary itself is platform-specific. Trellis provides a `--target` flag and per-target task configuration to produce platform-specific builds from one machine without manual scripting.

## Target triple format

Bramble uses the conventional `<arch>-<os>-<env>` triple:

| Triple | Platform |
|---|---|
| `x86_64-linux-gnu` | 64-bit Linux (glibc) |
| `aarch64-linux-gnu` | ARM64 Linux |
| `x86_64-windows-msvc` | 64-bit Windows |
| `aarch64-macos` | Apple Silicon macOS |
| `x86_64-macos` | Intel macOS |

Run `bramble targets` to list all triples supported by your installed toolchain.

## Build for a single target

```bash
trellis build --target aarch64-linux-gnu
```

This invokes your `build` task (or the default task if none is named `build`) with the `BRAMBLE_TARGET` variable set, and places output under `dist/<target>/`:

```text
dist/
  aarch64-linux-gnu/
    main.bvm
    runtime
```

## Define explicit per-target tasks

For more control, declare separate tasks and use the built-in `TARGET` variable:

```
var TARGETS = ["x86_64-linux-gnu", "aarch64-linux-gnu", "x86_64-windows-msvc"]

task compile {
  inputs  = ["src/**/*.br"]
  outputs = ["dist/${TARGET}/main.bvm"]
  run {
    bramble build src/main.br --target ${TARGET} --out dist/${TARGET}/main.bvm
  }
}

task release {
  foreach TARGET in ${TARGETS} {
    deps = [compile]
  }
}
```

Running `trellis run release` expands the `foreach` and creates one `compile` task instance per target. Trellis caches each instance independently, so only targets whose inputs changed are rebuilt.

> [!TIP]
> If the cross-compilation toolchain for a target is not installed, Bramble prints error `B0512` and exits cleanly. Wrap optional targets in a `when` condition to make them skippable.

## Build all targets in parallel

Cross-compilation tasks for different targets have no dependency on each other, so Trellis can run them concurrently:

```bash
trellis run release -j 4
```

See [running tasks in parallel](xref:bramble.how-to.trellis.run-tasks-in-parallel) for how to tune concurrency.

## Verify a cross-compiled artifact

After building for a different platform, check the artifact type without running it:

```bash
bramble inspect dist/aarch64-linux-gnu/main.bvm
```

```text
Bramble Bytecode Archive
  Format:   BVM/1
  Target:   aarch64-linux-gnu
  Entry:    main
  Symbols:  yes
  Size:     48 KB
```

The `Target` line confirms the artifact was compiled for the intended platform. For the full set of Trellis configuration options, see the [Trellisfile reference](xref:bramble.reference.config.trellisfile).

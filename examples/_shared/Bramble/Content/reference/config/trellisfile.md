---
title: "Trellisfile"
description: "Reference for the Trellisfile format, which declares tasks, dependencies, inputs, outputs, and environment variables for trellis."
uid: bramble.reference.config.trellisfile
order: 520
sectionLabel: "Configuration"
tags: [configuration, trellis, tasks, build, automation]
---

A `Trellisfile` sits at the project root and describes the tasks that `trellis` can execute. Each task is a named block with a shell command, optional dependencies on other tasks, and optional input/output declarations for incremental caching.

## Task keys

| Key | Type | Required | Description |
|---|---|---|---|
| `task <name>` | block header | yes | Declares a task; name must be an identifier |
| `description` | string | no | One-line summary shown by `trellis list` |
| `command` | string or array | yes | Shell command(s) to run |
| `deps` | array of strings | no | Task names that must complete before this one |
| `inputs` | array of globs | no | Files that, when changed, invalidate the cached result |
| `outputs` | array of globs | no | Files produced; used to validate cache hits and by `trellis clean` |
| `env` | key-value map | no | Environment variables injected into the task shell |
| `workdir` | string | no | Working directory for the command; defaults to project root |

## Incremental caching

When `inputs` and `outputs` are both declared, trellis computes a fingerprint over all matched input files. If the fingerprint matches the stored value and all output files exist, the task is skipped. Omitting either key disables caching for that task.

## Sample Trellisfile

```text
task codegen
  description = "Generate type bindings from schema files"
  command     = "bramble run tools/codegen.br -- schemas/ src/generated/"
  inputs      = ["schemas/**/*.bschema"]
  outputs     = ["src/generated/**/*.br"]

task build
  description = "Compile all packages to release artifacts"
  command     = "bramble build --release"
  deps        = ["codegen"]
  inputs      = ["src/**/*.br", "bramble.toml"]
  outputs     = ["dist/*.bramblec"]

task lint
  description = "Run sprig lint across source"
  command     = "sprig lint src/"
  inputs      = ["src/**/*.br", ".sprig.toml"]

task test
  description = "Run the full test suite"
  command     = "bramble test --jobs 4"
  deps        = ["build", "lint"]
  env         = { BRAMBLE_LOG = "warn" }

task docs
  description = "Generate API reference docs"
  command     = ["bramble run tools/docgen.br", "bramble run tools/index.br"]
  deps        = ["build"]
  outputs     = ["docs/api/**"]

task clean
  description = "Remove build outputs"
  command     = "rm -rf dist/ docs/api/ src/generated/"
```

## Variable substitution

Task `command` strings support a small set of built-in variables:

| Variable | Value |
|---|---|
| `${PROJECT_ROOT}` | Absolute path to the directory containing the Trellisfile |
| `${TASK_NAME}` | Name of the currently executing task |
| `${RELEASE}` | `true` when `--release` is set, otherwise `false` |

See [`trellis` CLI reference](xref:bramble.reference.cli.trellis) for execution flags and [caching build outputs](xref:bramble.how-to.trellis.cache-build-outputs) for fingerprinting guidance.

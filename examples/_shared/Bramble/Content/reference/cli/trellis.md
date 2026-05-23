---
title: "trellis"
description: "Reference for the trellis CLI, Bramble's task runner and incremental build coordinator."
uid: bramble.reference.cli.trellis
order: 430
sectionLabel: "CLI"
tags: [cli, trellis, tasks, build, automation]
---

`trellis` reads a `Trellisfile` in the current directory and executes declared tasks. It tracks input/output fingerprints to skip work that is already up to date, and can parallelize independent tasks.

## Subcommands

| Subcommand | Purpose |
|---|---|
| `run` | Execute one or more named tasks |
| `list` | Print all declared tasks and their descriptions |
| `graph` | Render the task dependency graph as DOT or plain text |
| `build` | Alias for `run` with `--release` implied; intended for CI |
| `clean` | Remove all recorded output artifacts and invalidate the cache |

## Global flags

| Flag | Default | Description |
|---|---|---|
| `--target <task>` | — | Name of the task to execute (can be repeated) |
| `-j <n>` | CPU count | Maximum parallel tasks |
| `--release` | false | Pass `release = true` to all task environments |
| `--no-cache` | false | Disable input/output fingerprinting; always re-run |
| `--dry-run` | false | Print what would run without executing anything |
| `--verbose` / `-v` | false | Log each command as it executes |
| `--config <path>` | `Trellisfile` | Override the Trellisfile path |

## trellis run

Runs the specified task and all of its transitive dependencies. If no `--target` is given, trellis runs the first task declared in the Trellisfile.

```bash
trellis run
trellis run --target test
trellis run --target lint --target test -j 4
```

Tasks whose inputs have not changed since the last successful run are skipped automatically. Pass `--no-cache` to force a full re-run.

## trellis list

```bash
trellis list
```

```text
TASK          DESCRIPTION
build         Compile all packages to release artifacts
test          Run the full test suite
lint          Run sprig lint across all source directories
docs          Generate API documentation
clean         Remove build outputs
```

## trellis graph

Emits the dependency graph. The default format is plain text; pass `--format dot` to get Graphviz DOT output suitable for rendering.

```bash
trellis graph
trellis graph --format dot | dot -Tsvg -o graph.svg
```

```text
build
  └─ codegen
       └─ proto
test
  ├─ build
  └─ lint
```

## trellis build

Equivalent to `trellis run --release`. Intended as a stable CI target.

```bash
trellis build
trellis build --target dist -j 8
```

## trellis clean

Removes output artifacts recorded by trellis and clears the fingerprint cache. Source files are never deleted.

```bash
trellis clean
trellis clean --target build   # clean outputs for a specific task only
```

> **Note:** `trellis clean` removes only files explicitly listed in a task's `outputs`. Files written to ad-hoc paths by task scripts are not tracked and will not be removed.

See the [Trellisfile reference](xref:bramble.reference.config.trellisfile) for full task-definition syntax, and [caching build outputs](xref:bramble.how-to.trellis.cache-build-outputs) for fingerprinting strategies.

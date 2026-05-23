---
title: "Define a build task"
description: "How to write a task block in a Trellisfile, declare dependencies between tasks, and run it with trellis run."
uid: bramble.how-to.trellis.define-a-build-task
order: 310
sectionLabel: "Trellis"
tags: [trellis, build, tasks, trellisfile, automation]
---

A Trellisfile describes your build as a set of named tasks with explicit dependencies. Trellis topologically sorts the dependency graph and runs tasks in the correct order, only re-running what has changed. This guide covers authoring a basic task and running it.

## Anatomy of a task block

Each task starts with the keyword `task`, followed by a name, an optional `deps` list, and a `run` block:

```
task clean {
  run {
    rm -rf dist/
  }
}

task compile {
  deps = [clean]
  run {
    bramble build src/main.br --out dist/main.bvm
  }
}

task test {
  deps = [compile]
  run {
    bramble test tests/
  }
}

task build {
  deps = [compile, test]
}
```

The `run` block is a shell command sequence. A task with only `deps` and no `run` acts as an aggregate target — useful for grouping related tasks under a single name.

## Declare task dependencies

The `deps` list names other tasks that must complete successfully before this task runs. Trellis builds a directed acyclic graph from all `deps` declarations and runs tasks in dependency order.

If a dependency fails, Trellis stops and reports the failing task. Downstream tasks are not attempted.

> [!NOTE]
> Circular dependencies are detected at startup and reported as error `T0011` before any task runs.

## Run a task

```bash
trellis run build
```

Trellis resolves the full dependency subgraph rooted at `build`, runs tasks in order, and streams output to the terminal. Pass multiple task names to run several roots:

```bash
trellis run compile test
```

## Pass variables to tasks

Declare variables at the top of the Trellisfile and reference them with `${VAR}`:

```
var OUT_DIR = "dist"

task compile {
  run {
    bramble build src/main.br --out ${OUT_DIR}/main.bvm
  }
}
```

Override at the command line:

```bash
trellis run compile OUT_DIR=build/debug
```

## List available tasks

```bash
trellis tasks
```

This prints each task name and, if declared, its `description` field:

```
task compile    Build the Bramble bytecode artifact
task test       Run the unit test suite
task build      Full build: compile + test
```

See the [Trellisfile reference](xref:bramble.reference.config.trellisfile) for the complete task block grammar, including environment variable injection and working directory overrides.

---
title: "Run tasks in parallel"
description: "How to use Trellis's -j flag and dependency graph to run independent tasks concurrently."
uid: bramble.how-to.trellis.run-tasks-in-parallel
order: 340
sectionLabel: "Trellis"
tags: [trellis, parallel, concurrency, performance, build]
---

Trellis derives parallelism from the structure of your dependency graph, not from imperative threads or async annotations. Any tasks with no dependency on each other are candidates for concurrent execution. You control the maximum concurrency with the `-j` flag.

## Enable parallel execution

```bash
trellis run build -j 4
```

`-j N` allows up to N tasks to run simultaneously. Trellis schedules work greedily: as soon as a task's dependencies complete, it is eligible to start — up to the concurrency limit.

Without `-j`, Trellis defaults to `-j 1` (sequential). Set `-j 0` to use the number of logical CPU cores detected at startup.

## The dependency graph determines ordering

Parallelism is automatic for tasks that share no dependency path. Given this Trellisfile:

```
task compile-core {
  inputs  = ["src/core/**/*.br"]
  outputs = ["dist/core.bvm"]
  run { bramble build src/core --out dist/core.bvm }
}

task compile-http {
  inputs  = ["src/http/**/*.br"]
  outputs = ["dist/http.bvm"]
  run { bramble build src/http --out dist/http.bvm }
}

task compile-cli {
  inputs  = ["src/cli/**/*.br"]
  outputs = ["dist/cli.bvm"]
  run { bramble build src/cli --out dist/cli.bvm }
}

task link {
  deps    = [compile-core, compile-http, compile-cli]
  inputs  = ["dist/core.bvm", "dist/http.bvm", "dist/cli.bvm"]
  outputs = ["dist/app.bvm"]
  run { bramble link dist/*.bvm --out dist/app.bvm }
}
```

Running `trellis run link -j 3` starts all three `compile-*` tasks simultaneously. `link` starts only after all three complete:

```text
[compile-core]  started
[compile-http]  started
[compile-cli]   started
[compile-core]  done  (1.2s)
[compile-cli]   done  (1.5s)
[compile-http]  done  (2.1s)
[link]          started
[link]          done  (0.4s)
```

## Interleaved output

When tasks run concurrently, Trellis buffers each task's output and flushes it as a block when the task completes. This keeps the log readable without prefixing every line.

To stream output in real time (useful for long-running tasks), add `--stream-output`:

```bash
trellis run build -j 4 --stream-output
```

Each line is then prefixed with the task name: `[compile-http] Parsing src/http/client.br...`

> [!WARNING]
> Avoid tasks that write to the same output file from parallel branches. Trellis does not lock output paths; concurrent writes will produce a corrupted artifact. Use `deps` to enforce sequential access to shared resources.

## Fail-fast vs. continue on error

By default Trellis stops scheduling new tasks as soon as one fails, waits for already-running tasks to complete, and exits. Use `--keep-going` to run all tasks that are not blocked by the failure:

```bash
trellis run build -j 4 --keep-going
```

This is useful in CI when you want to collect all failures from independent branches of the graph in one pass rather than fixing them one at a time.

See [caching build outputs](xref:bramble.how-to.trellis.cache-build-outputs) to ensure that cached tasks are skipped instantly during parallel runs, and the [build graph explanation](xref:bramble.explanation.the-build-graph) for a deeper look at scheduling semantics.

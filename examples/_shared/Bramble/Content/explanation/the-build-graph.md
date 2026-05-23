---
title: "How Trellis models the build graph"
description: "Trellis represents every build task as a node in a directed acyclic graph, letting caching and parallelism follow automatically from declared inputs and outputs."
uid: bramble.explanation.the-build-graph
order: 120
sectionLabel: "Explanation"
tags: [trellis, build, dag, caching, parallelism]
---

Trellis is not a task runner in the traditional sense, where you write a sequence of shell commands and hope they stay in sync. It is a build graph engine: every task declares what it reads and what it produces, and Trellis uses those declarations to decide what needs to run, in what order, and whether a previous result is still valid. The caching and parallelism that result are not features bolted on top — they are consequences of the model.

## Tasks, inputs, and outputs

A `Trellisfile` is a set of task definitions. Each task names its inputs (source files, environment values, the outputs of other tasks) and its outputs (files it will produce). Tasks that depend on another task's output declare that dependency explicitly; Trellis builds the DAG from these declarations.

```toml
[task.compile]
inputs  = ["src/**/*.bram", "bramble.toml"]
outputs = ["build/app.bc"]
run     = "bramble build src/main"

[task.test]
inputs  = ["build/app.bc", "tests/**/*.bram"]
outputs = []
run     = "bramble test"
depends = ["compile"]
```

A task with no `depends` entry can potentially run at the same time as any other independent task. Trellis schedules work up to a configurable degree of parallelism; the declared graph prevents races by construction.

## Content hashing, not timestamps

Trellis decides whether a cached output is still valid by hashing the actual content of every input, not by comparing file modification times. Timestamps are unreliable across source-control checkouts, network file systems, and build machines with clock skew. Content hashes are not.

When all of a task's inputs hash to the same values as the last successful run, Trellis skips the task and treats its outputs as already present. Changing a comment in a file that a task reads will invalidate the cache for that task; touching a file without changing it will not.

## Why the DAG matters

A directed acyclic graph has a property that makes build tools tractable: it has a topological ordering. Trellis can always find an execution sequence where every task's dependencies are complete before it starts. A cycle in the graph — task A depending on B, which depends on A — is an error, reported at load time rather than discovered mid-build.

The DAG also makes the cache semantics sound. If a task is a pure function of its declared inputs to its declared outputs, caching is correct. Undeclared inputs (environment variables read at runtime, files opened without being listed) are invisible to the graph and can cause stale cache hits. Trellis cannot enforce purity, but the model encourages it by making the costs of impurity visible.

## Remote and shared caches

Because cache validity is determined by content hashes of declared inputs, a cache entry produced on one machine is valid on another machine facing the same hashes. Trellis supports remote cache backends for teams that want to share build artifacts across CI and developer machines.

For hands-on configuration see the [Trellisfile reference](xref:bramble.reference.config.trellisfile) and [Cache build outputs](xref:bramble.how-to.trellis.cache-build-outputs).

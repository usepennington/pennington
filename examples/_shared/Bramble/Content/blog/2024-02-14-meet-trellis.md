---
title: "Meet Trellis, our build system"
description: "Theo Larsson introduces Trellis, Bramble's dedicated build system, covering the Trellisfile format, the build graph model, and artifact caching."
date: 2024-02-14
author: Theo Larsson
tags: [trellis, build, tooling, caching]
uid: bramble.blog.meet-trellis
---

Bramble 1.0 shipped last month, and right behind it comes the tool I've been building in parallel: Trellis, a build system designed specifically for Bramble projects. If your project has more than one output, benefits from reproducible builds, or runs on CI, Trellis is for you.

## The Trellisfile

Trellis is configured by a `Trellisfile` at the project root. Each entry is a named task with inputs, outputs, and the command to run:

```toml
[tasks.compile]
inputs  = ["src/**/*.br", "bramble.toml"]
outputs = ["dist/app.bvm"]
run     = "bramble build src/main.br -o dist/app.bvm"

[tasks.test]
inputs  = ["src/**/*.br", "tests/**/*.br"]
outputs = []
run     = "bramble test"
depends = ["compile"]

[tasks.docs]
inputs  = ["src/**/*.br", "docs/**/*.md"]
outputs = ["dist/docs/"]
run     = "bramble doc -o dist/docs/"
```

Tasks declare what they read and what they produce. That declaration is what lets Trellis do everything else.

## The build graph

When you run `trellis build`, Trellis reads the dependency declarations and constructs a directed acyclic graph of tasks. Independent tasks run in parallel. Tasks with declared dependencies wait for their dependencies to complete first.

You don't orchestrate this yourself. You declare relationships, and Trellis handles scheduling. On a project with a compilation step, several test suites, a doc generation step, and a packaging step, Trellis will figure out that the test suites can all run in parallel once compilation finishes, and that packaging has to wait for all of them. The [build graph explanation](xref:bramble.explanation.the-build-graph) goes into detail on how cycle detection and parallelism work under the hood.

## Caching

The part of Trellis I'm most pleased with is the cache. Every task's cache key is a hash of its declared inputs. If the inputs haven't changed since the last successful run, Trellis skips the task entirely.

```bash
$ trellis build
[compile]  ✓ cached (inputs unchanged)
[test]     ✓ cached (inputs unchanged)
[docs]     running...
[docs]     ✓ done in 1.3s
```

The cache is content-addressed, not timestamp-based. Reverting a file to a previous state reuses the cached output for that state. This makes local iteration fast and makes CI builds predictable — the same inputs always produce the same result and the same cache hits.

Remote cache sharing is on the roadmap. For now, the cache is local to the machine.

## Defining your own tasks

The built-in tasks cover the most common patterns, but you can define arbitrary tasks for anything your project needs — generating code, running migrations, uploading build artifacts. The [how-to guide for defining build tasks](xref:bramble.how-to.trellis.define-a-build-task) covers the full range of options including conditional task execution and task parameters.

The `trellis` command reference documents every flag; see [the Trellis CLI reference](xref:bramble.reference.cli.trellis) for the complete list.

## Getting Trellis

Trellis ships as a separate install:

```bash
thicket install --global trellis
```

Or, if you're already on `bramble` 1.0 and want it bundled, the `bramble toolchain` subcommand can install the full suite. We'll have more to say about toolchain management in an upcoming post.

If you've been managing builds with shell scripts or a borrowed `make` setup, give Trellis a try. The cache alone tends to pay for the migration cost within a week.

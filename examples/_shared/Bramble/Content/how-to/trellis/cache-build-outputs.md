---
title: "Cache build outputs"
description: "How to declare task inputs and outputs so Trellis skips unchanged work and reuses cached results."
uid: bramble.how-to.trellis.cache-build-outputs
order: 320
sectionLabel: "Trellis"
tags: [trellis, caching, build, performance, inputs-outputs]
---

Trellis caches task outputs based on a fingerprint of declared inputs. When inputs have not changed since the last successful run, Trellis skips the task entirely and restores outputs from cache. Declaring accurate inputs and outputs is the key to fast incremental builds.

## Declare inputs and outputs

Add `inputs` and `outputs` to any task block:

```
task compile {
  inputs  = ["src/**/*.br", "bramble.toml"]
  outputs = ["dist/main.bvm"]
  run {
    bramble build src/main.br --out dist/main.bvm
  }
}
```

`inputs` is a list of glob patterns relative to the Trellisfile. `outputs` is a list of files or directories that the task produces.

On the first run Trellis hashes all matching input files, runs the task, stores the output files in the local cache, and records the input fingerprint. On subsequent runs it recomputes the input fingerprint; if it matches, the outputs are restored from cache and the `run` block is skipped.

## Verify caching is working

```bash
trellis run compile
trellis run compile   # second run
```

The second run should print:

```text
[compile] cached (inputs unchanged)
```

If it re-runs instead, check that `outputs` lists everything the task writes. An output file that is not declared will be missing from cache on restore, causing Trellis to treat the task as stale.

## Cache glob patterns

| Pattern | Matches |
|---|---|
| `src/**/*.br` | All `.br` files under `src/`, recursively |
| `src/*.br` | Only direct children of `src/` |
| `bramble.toml` | Exact file |
| `assets/` | Entire directory tree |

> [!WARNING]
> Avoid using `**` alone as an input glob — it matches everything including `dist/`, which creates a dependency cycle where outputs invalidate inputs on every run.

## Share cache across CI runs

Trellis stores its local cache under `.trellis/cache/`. In CI, persist and restore this directory between runs to avoid rebuilding from scratch on every job:

```bash
# restore cache before build
cp -r $CI_CACHE_DIR/.trellis .trellis || true

trellis run build

# save cache after build
cp -r .trellis $CI_CACHE_DIR/.trellis
```

The cache is content-addressed, so it is safe to share across branches. A cache hit from `main` is valid for a feature branch if the inputs are identical.

## Invalidate the cache manually

Force a task to re-run regardless of cache state:

```bash
trellis run compile --no-cache
```

To clear the entire cache:

```bash
trellis cache clear
```

See [running tasks in parallel](xref:bramble.how-to.trellis.run-tasks-in-parallel) for how caching interacts with concurrent task execution, and the [build graph explanation](xref:bramble.explanation.the-build-graph) for a deeper look at how Trellis determines what to rerun.

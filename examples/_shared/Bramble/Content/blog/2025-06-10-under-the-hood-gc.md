---
title: "Under the hood: the garbage collector"
description: "A practical look at how Bramble's small generational GC works, when it runs, and what you can do to help it stay out of your way."
date: 2025-06-10
author: Dr. Hazel Mbeki
tags: [gc, internals, performance, memory]
uid: bramble.blog.under-the-hood-gc
---

The Bramble GC is described in the documentation as "small" and "generational," which is accurate but not especially enlightening if you're trying to understand why your allocation-heavy code pauses occasionally or why the `--gc-stats` flag is printing numbers that look surprising. Let me describe what the collector actually does, and when it does it.

## Two generations, one heap

The heap is split into two regions: the nursery and the old space. All allocations start in the nursery. The nursery is fixed-size (default 2 MB, configurable with `--nursery-size`). When it fills up, a minor collection runs.

A minor collection traces from the roots (stack frames, global variables, pinned values) and from any old-space objects that hold pointers into the nursery — those are tracked by the write barrier. Anything that's live gets copied to the old space. Anything that isn't gets discarded by resetting the nursery bump pointer. The whole thing typically takes under a millisecond for a 2 MB nursery.

Objects that survive two minor collections are tenured into the old space and won't be visited in minor collections again. This is the core generational hypothesis in action: most objects die young, so most of the time the GC only needs to look at a small region.

## Major collections

The old space uses a mark-and-sweep algorithm. Major collections run when the old space reaches a configurable occupancy threshold (default 75%). They're slower — typically 5–20 ms on a warm heap — because they trace the entire reachable object graph.

Bramble's ownership model helps here more than you might expect. Because ownership is tracked statically, the compiler can sometimes emit explicit drops for values whose lifetimes are provably bounded by a scope. Those values never make it to the heap at all; they're stack-allocated and freed at scope exit. The GC only sees allocations that genuinely require heap lifetime.

```bramble
fn count_words(text: Str) -> Int {
    // `parts` is a heap allocation the GC will see
    let parts = text.split(" ")
    parts.len()
    // `parts` is dropped here; the GC can reclaim it at the next minor collection
}
```

## The write barrier

When an old-space object is mutated to hold a reference to a nursery object, that old-space object needs to be treated as a root for the next minor collection — otherwise the nursery object would appear unreachable and be discarded. The write barrier records these cross-generational references in a grey-object buffer, which the minor collector drains before tracing.

The cost of the write barrier is small per write, but it's not zero. In tight loops that mutate many struct fields, you can sometimes see it in profiles. [A previous incident](xref:bramble.blog.the-halloween-segfault) involved a bug in this buffer — if you're curious about what can go wrong, that post-mortem is a good read.

## Tuning in practice

For most programs, the defaults are fine. Two situations warrant tuning:

**Many short-lived large allocations.** If you're allocating large byte buffers that die within one GC cycle, consider bumping `--nursery-size` so more of them fit without triggering a minor collection. The overhead of a minor collection is roughly proportional to the size of the live set, not the nursery, so a larger nursery with the same live set costs the same to collect and causes fewer collections.

**Long-running services with steady-state heap.** After warmup, the old space tends to stabilize. If you're seeing major collections still running at their default frequency, raising `--old-space-threshold` to 85% or 90% often reduces collection frequency with no downside on a stable heap.

The [garbage collection explanation](xref:bramble.explanation.garbage-collection) has the full reference for all GC flags and their defaults. This post aimed to give you the mental model that makes those flags legible.

---
title: "Postmortem: the Halloween segfault"
description: "A play-by-play of the segfault that took down the Hedge playground for four hours on October 31, 2024, and what we changed to prevent it happening again."
date: 2024-10-31
author: Theo Larsson
tags: [postmortem, gc, wasm, reliability]
uid: bramble.blog.the-halloween-segfault
---

At 21:47 UTC on October 31, the Hedge playground stopped returning results. Snippets would compile, the Wasm module would start executing, and then — nothing. The output pane stayed blank. By 22:03 we had confirmed a segmentation fault inside the GC's sweep phase. By 01:52 the next morning we had a patch deployed and a post-mortem drafted. This is that post-mortem.

## Timeline

```text
21:47 UTC  First "output pane stuck" report in the community Discord.
21:54 UTC  Second report; output pane confirmed blank across multiple browsers.
22:03 UTC  On-call (me) reproduces locally with a crafted snippet.
22:11 UTC  Segfault isolated to the GC sweep phase under Wasm memory pressure.
22:40 UTC  Root cause identified: off-by-one in the grey-object write barrier.
23:15 UTC  Fix authored, unit tests written, passing locally.
00:48 UTC  Wasm rebuild completes; canary deployed to 5% of Hedge traffic.
01:22 UTC  No new reports; canary traffic clean.
01:52 UTC  Full rollout. Incident closed.
```

## What happened

The small generational GC promotes objects from the nursery to the old generation during minor collections. When an old-generation object is mutated to point at a nursery object, a write barrier records that old object as "grey" so the next minor GC can trace it. The write barrier walks a fixed-size grey-object ring buffer.

The ring buffer had a capacity of 256 entries, indexed with a `u8`. When the index wrapped from 255 back to 0, the code that decided whether the buffer was full compared the write index to the read index using an unsigned subtraction. Under Wasm's memory model, the subtraction wrapped to a large positive number rather than a small negative one, causing the "not full" check to always pass. The buffer overwrote entries it had not yet processed.

On most runs this caused a minor trace to miss a pointer, which the major GC would catch later anyway — invisible in practice. On the specific input shape that showed up in user snippets that evening (a tight loop building many short-lived closures that captured outer references), the overwrite happened to corrupt the object currently being swept, producing a segfault.

> [!NOTE]
> The corruption was deterministic given that input shape. Any snippet with roughly 260+ rapid closure allocations inside a loop would trigger it reliably.

## The fix

The index type changed from `u8` to `usize`, and the full-check arithmetic was rewritten to avoid subtraction:

```bramble
// Before (broken):
let is_full = (write_idx - read_idx) >= GREY_BUF_CAPACITY

// After:
let count = if write_idx >= read_idx {
    write_idx - read_idx
} else {
    GREY_BUF_CAPACITY - read_idx + write_idx
}
let is_full = count >= GREY_BUF_CAPACITY
```

A regression test was added that drives the buffer through two full wrap-arounds with live old-to-new pointers throughout.

## What we're changing going forward

The off-by-one itself was caught by code review when the ring buffer was first written — but only in the native build. The Wasm target has different integer promotion rules and nobody ran the GC stress tests against the Wasm module before merging. Both of those gaps are now closed: the CI matrix now includes a Wasm stress-test job, and the [CI how-to](xref:bramble.how-to.tooling.run-in-ci) has been updated to reflect this.

We're also bumping the grey-object buffer to 1024 entries with an explicit overflow fallback (a heap-allocated spill list) so that even an incorrect full-check can't corrupt adjacent memory.

Apologies to everyone who was sharing Halloween snippets and got a blank pane instead of output. It was a good bug. A terrible, perfectly-timed bug.

---
title: "A 3x performance deep dive"
description: "Dr. Hazel Mbeki details the profiling work and VM-level changes that produced a three-times throughput improvement across Bramble's benchmark suite."
date: 2024-04-02
author: Dr. Hazel Mbeki
tags: [performance, vm, internals, benchmarks]
uid: bramble.blog.performance-deep-dive
---

Since 1.0 shipped in January, we've been running a focused performance sprint. The headline number from our benchmark suite: median throughput is up 3.1x against the 0.9 baseline. This post is the technical accounting of how that happened, because "we made it faster" is not useful to anyone who wants to understand the system.

## Where the time was going

Step one was measurement. We ran the benchmark corpus — about 60 programs ranging from tight numeric loops to realistic workloads pulling from Thicket packages — through a sampling profiler attached to the VM. The results were instructive, and embarrassing in a few places.

Roughly 38% of execution time was in the dispatch loop. The original VM used a switch-based dispatch on opcode bytes. Each iteration was: fetch, switch, execute, repeat. That's clean code but it leaves a lot of performance on the table on modern hardware, where indirect branch prediction matters.

Another 22% was in allocation. The GC was sound but the allocation path was doing more work than necessary on every object creation — initializing fields to sentinel values that the type system already guaranteed would be overwritten before use.

## The dispatch loop

We moved to a computed-goto dispatch table. Instead of a switch statement, the VM holds an array of code addresses, one per opcode, and jumps directly to the next handler after each instruction executes. The branch predictor can learn the pattern per call site rather than per switch.

This is a well-understood technique — the Bramble VM isn't inventing anything here. What was satisfying was measuring it: the dispatch loop share of execution time dropped from 38% to about 9% on the benchmark suite. The rest of the gain came from a related change: instruction encoding was revised to pack the most common operations into a single byte with an inline operand, reducing fetch overhead.

The [bytecode VM explanation](xref:bramble.explanation.the-bytecode-vm) has the updated instruction set documentation with notes on which opcodes use the packed encoding.

## The allocator

The GC's bump allocator was doing zero-initialization on every allocation. For the nursery generation, where objects are typically short-lived, this was wasted work for fields that get written before the object is ever read.

Bramble's type system already ensures that every field is initialized before the object is considered complete — that's a consequence of the ownership model. So the zero-initialization was defensive against a class of bugs that can't occur in valid Bramble code. We removed it.

For the old generation, we kept a lighter form of initialization only on promoted objects, which zeroes only reference-typed fields (for GC tracing correctness) and skips scalar fields.

The allocation benchmark — which creates and discards millions of short-lived records — improved by 4.1x on its own. The overall improvement is more modest because most programs don't stress allocation at that rate.

## String interning

One surprise find: a significant fraction of the symbol-lookup time in larger programs was caused by redundant hashing of string keys during method dispatch. We introduced an intern table for string literals at compile time. At runtime, identity comparison replaces equality comparison for interned strings.

This didn't show up in micro-benchmarks but made a measurable difference on the Thicket package integration tests, which do a lot of structural work with string-keyed records.

## What didn't help

We spent two weeks on a speculative optimization to inline monomorphic call sites. The benchmarks showed a 1-2% improvement in the best case and a regression in several programs that had a mix of polymorphic and monomorphic call sites. We reverted it. The complexity wasn't worth it at this stage.

## What's next

Async and tasks are landing in 1.2, and the VM needs changes to support efficient task scheduling. The performance work we did here — especially the dispatch changes — makes that integration cleaner than it would have been on the old code. We'll write up the async internals separately when 1.2 is closer to release.

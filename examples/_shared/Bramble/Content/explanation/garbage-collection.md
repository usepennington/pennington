---
title: "The garbage collector"
description: "How Bramble's generational, non-moving collector works alongside the borrow checker to manage heap memory with minimal pause times."
uid: bramble.explanation.garbage-collection
order: 60
sectionLabel: "Explanation"
tags: [garbage-collection, memory, generational, pauses, performance]
---

Bramble manages heap memory through a combination of static analysis and a runtime garbage collector. Understanding how these two mechanisms divide the work explains both the language's memory safety guarantees and its pause behavior.

## What the borrow checker handles

The borrow checker is the first line of memory management. Values whose lifetimes can be determined at compile time—local variables, function parameters, values that do not escape their declaring scope—are freed at the point the borrow checker establishes they are no longer used. This happens at zero runtime cost: no reference counting, no tracing, no pause. The compiler emits a deallocation at the determined point.

The practical effect is that a significant fraction of short-lived allocations in a typical Bramble program never reach the garbage collector at all. Intermediate string buffers, temporary records, loop-local values—these are often proven dead by the borrow checker and freed eagerly.

## What the GC handles

The garbage collector manages values that the borrow checker cannot fully reason about statically: values that escape to the heap with dynamic lifetimes, values shared across task boundaries via channels, and values that are part of data structures with cycles. These go into the GC-managed heap.

Bramble's collector is generational, dividing the heap into a young generation and an old generation. Most objects die young—a pattern that holds across nearly all allocation-heavy programs—so the collector can focus most of its effort on the small young generation, which is fast to scan and collect. Objects that survive a configurable number of young-generation collections are promoted to the old generation and collected less frequently.

## Non-moving design

The Bramble GC does not compact memory by relocating live objects. This is a deliberate choice that comes with a clear tradeoff: a moving collector can eliminate fragmentation and improve allocation locality over time, but it requires that every pointer to a moved object be updated, which complicates interaction with native extensions and requires write barriers on pointer stores.

Bramble's sandboxed-by-default model and its heavy use of the foreign function interface for capability-gated operations make a non-moving design significantly simpler to reason about. Fragmentation is managed instead through a size-class allocator that keeps objects of similar sizes together.

## Pause behavior

Young-generation collections are short—typically sub-millisecond for programs with modest allocation rates—because the young generation is small and the collector can walk it quickly. Old-generation collections are less frequent and take longer, proportional to the number of live objects in the old generation.

Bramble does not currently implement incremental or concurrent collection for the old generation. For most scripting workloads this is acceptable; the old generation tends to be small if the program does not accumulate long-lived state. Programs that do accumulate significant long-lived state should be aware that old-generation pauses scale with that state.

> **Note:** Bramble 2.0 previews an incremental old-generation collector that spreads collection work across multiple task yields. The API and behavior remain experimental.

The interaction between the GC and the ownership model is discussed further in [ownership and borrowing](xref:bramble.explanation.ownership-and-borrowing). The overall runtime architecture, including how the GC coordinates with the VM's register file, is covered in [the bytecode VM](xref:bramble.explanation.the-bytecode-vm).

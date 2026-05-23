---
title: "Performance characteristics"
description: "Bramble starts fast, runs at respectable throughput for a managed runtime, and has predictable latency — understanding where the costs actually sit helps you write faster programs and know when to reach for native code."
uid: bramble.explanation.performance-characteristics
order: 130
sectionLabel: "Explanation"
tags: [performance, vm, startup, throughput, benchmarks]
---

Bramble's runtime is a bytecode VM with a small generational GC. That sentence alone tells you a lot: startup is fast because there is no JIT compilation phase, throughput is good for most workloads but not competitive with ahead-of-time-compiled native code, and GC pauses are short because the collector focuses on short-lived allocations first. Each of these properties comes with a qualifier worth understanding.

## Startup time

Cold startup for a typical Bramble program — loading the VM, deserialising bytecode, calling `main` — is measured in single-digit milliseconds. This makes Bramble a reasonable choice for command-line tools and scripts that are invoked frequently, where JVM or CLR-style warmup costs would be noticeable.

The cost that does scale with program size is bytecode loading. A large application with many modules will spend more time on the initial load than a small script. Trellis's incremental compilation model means that in development, only changed modules are recompiled; in production, the serialised bytecode for a well-structured application loads quickly because modules are loaded lazily.

## Throughput

For CPU-bound work, Bramble's bytecode interpreter runs at roughly the speed you would expect from a well-implemented dynamic-language VM — faster than most interpreted languages, slower than native. The interpreter is register-based, which reduces dispatch overhead compared to stack-based designs, and the hot paths in the standard library are hand-optimised.

Where Bramble is genuinely fast is in workloads that are I/O-bound or spend most of their time in standard library code written against efficient native bindings. A web service that spends most of its time waiting on the network and deserialising JSON will perform well. A program that does heavy numerical computation in tight loops will not saturate a modern CPU.

## GC and allocation pressure

The generational GC is designed around the observation that most allocations are short-lived. The nursery is collected frequently and cheaply; objects that survive multiple collections are promoted to a longer-lived region collected less often. In practice this means most programs see very short, frequent pauses rather than occasional long ones — a profile that suits latency-sensitive workloads better than throughput-sensitive ones.

Allocation pressure is the main lever you have over GC behaviour. Avoiding allocations inside tight loops — reusing buffers, preferring value types for small composites — has the most impact. Bramble's ownership and borrowing system helps here: borrowed references do not contribute to GC pressure because the collector does not need to track them.

## When to reach for native

Bramble is not the right tool for every job. If a workload requires sustained floating-point throughput, bit manipulation across large arrays, or real-time guarantees that cannot tolerate any GC pause, a native-compiled language is the better choice. Bramble provides a foreign-function interface for calling into native libraries; the common pattern for performance-critical subsystems is to write the hot loop in a native library and call it from Bramble, keeping the orchestration, configuration, and glue code in Bramble.

The [sandbox and security model](xref:bramble.explanation.the-sandbox-and-security) is relevant here too: native extensions require explicit capability grants, so the performance escape hatch is also a capability-boundary crossing.

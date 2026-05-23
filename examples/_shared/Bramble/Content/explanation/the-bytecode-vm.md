---
title: "The bytecode virtual machine"
description: "How Bramble's register-based VM executes bytecode, why the design prioritizes startup speed, and how instruction dispatch works."
uid: bramble.explanation.the-bytecode-vm
order: 70
sectionLabel: "Explanation"
tags: [vm, bytecode, register-based, runtime, performance]
---

When the Bramble compiler finishes, it produces a `.brc` bytecode file. That file is what the runtime executes. The virtual machine that interprets it is register-based, compact, and designed to start quickly—a priority that reflects the scripting use cases Bramble is most often applied to.

## Register-based vs. stack-based

Virtual machines broadly fall into two families: stack-based and register-based. A stack-based VM operates by pushing and popping values on a value stack; every operation implicitly consumes its inputs from the stack and pushes its result. A register-based VM assigns values to a fixed set of numbered virtual registers, and instructions explicitly name their source and destination registers.

The tradeoff is in code density versus instruction count. Stack-based bytecode tends to be more compact because each instruction is simpler and shorter. Register-based bytecode often uses fewer instructions to perform the same computation, because intermediate values can stay in named registers rather than being pushed and popped through a stack. Bramble's emitter targets a VM with 256 virtual registers per call frame, which is enough for the functions typical in scripting workloads without requiring spilling.

## Why bytecode at all

Bramble could compile directly to native machine code. The reasons it does not are startup latency and portability. A native-code compiler for a scripting language must either do a full compilation before executing the first line (slow startup) or use a just-in-time compiler (complex, with a runtime warm-up curve). Bytecode interpreted by a VM starts immediately and produces consistent performance from the first instruction.

The VM is written once and ported to each supported platform. The bytecode format is platform-neutral, which means a `.brc` file produced on one architecture runs without modification on another. This matters for Bramble's use in build tooling—a `Trellisfile` that includes Bramble scripts should behave the same whether it runs on a developer's workstation or a CI server with a different architecture.

## Dispatch

The VM executes bytecode through an instruction dispatch loop. Bramble uses a direct-threaded dispatch strategy on platforms that support it: rather than a central switch statement that tests the opcode for every instruction, each opcode's handler jumps directly to the address of the next handler. This eliminates one branch per instruction and improves branch predictor utilization.

On platforms where computed gotos are not available, the VM falls back to a switch-based dispatch loop. The fallback is slower—typically by 10–20% in tight loops—but functionally identical.

## The call frame and the GC root

Each function call creates a call frame containing the 256-register file, a pointer to the calling frame, and metadata the GC needs to find live references. The GC scans call frames to find all references on the "stack" (conceptually; the register file is the actual storage). Because the register file has a fixed, known layout, scanning it is fast.

Bramble does not use a JIT compiler in 1.x. The interpreter loop is fast enough for the automation, tooling, and documentation-site generation workloads Bramble was designed for. Compute-intensive work is expected to be offloaded to native extensions through the capability interface. See [performance characteristics](xref:bramble.explanation.performance-characteristics) for measured guidance on where the VM's throughput lands, and [the compiler pipeline](xref:bramble.explanation.the-compiler-pipeline) for how source code becomes the bytecode the VM consumes.

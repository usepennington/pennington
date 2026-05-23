---
title: "Hedge: try Bramble in your browser"
description: "Hedge is a browser-based Bramble playground that runs a real compiled-to-Wasm patch, lets you share snippets, and requires no local install."
date: 2024-09-09
author: Rowan Vance
tags: [tooling, playground, wasm, hedge]
uid: bramble.blog.hedge-playground
---

If you've ever wanted to show someone Bramble without walking them through a local install, that friction is gone. [Hedge](https://hedge.bramble.dev) launched this week — a full Bramble environment running entirely in your browser, compiled to WebAssembly.

## What Hedge actually runs

Hedge isn't an interpreter pretending to be Bramble. The entire `bramble` patch — the compiler, the bytecode VM, and the GC — is compiled to a Wasm module that loads when the page opens. Code you write is compiled to the same bytecode that runs on your laptop, then executed inside the sandbox. The [bytecode VM](xref:bramble.explanation.the-bytecode-vm) article goes into the details, but the short version is: you're running real Bramble, not a subset.

Start latency is around 80 ms on a modern desktop and about 200 ms on a mid-range phone. After that first compile the module is cached, so reloads are instant.

## The editor

The editor has syntax highlighting, inline error squiggles, and a persistent output pane. The REPL mode evaluates top-level expressions as you confirm them, which is handy for exploratory work. Script mode compiles and runs the whole buffer.

A few preloaded examples cover the most common "wait, how does Bramble do X?" moments — `Option` unwrapping, `Result` propagation, a small recursive data structure:

```bramble
fn fib(n: Int) -> Int {
    match n {
        0 | 1 => n,
        _     => fib(n - 1) + fib(n - 2),
    }
}

let result = fib(10)
// => 55
```

## Sharing snippets

Every state of the editor is serializable. Hit **Share** and Hedge encodes the current buffer into a URL — no server, no account, just a link. Paste it in a PR comment, a chat message, a blog post. The recipient opens it and the code is already there, ready to run.

Shared URLs carry the full source and nothing else. They don't track who created them, and they don't expire.

## What Hedge can't do

Because the patch runs sandboxed inside Wasm, there's no filesystem, no network, and no ability to import packages from Thicket. If your snippet needs an external package, you still need a local environment. The [installation tutorial](xref:bramble.tutorials.install-bramble) covers that path.

I/O is also limited to the output pane — no stdin, no interactive prompts. That's a deliberate sandbox boundary, not a missing feature.

## Try it

Head to [hedge.bramble.dev](https://hedge.bramble.dev), paste some code, and send a link to someone you've been meaning to onboard. If something misbehaves, the feedback button in the top-right corner files a pre-filled issue on the Hedge tracker.

The Wasm build pipeline is [open source](https://github.com/bramble-lang/hedge) if you're curious how the plumbing works.

---
title: "How we built the borrow checker"
description: "Dr. Hazel Mbeki recounts the design journey behind Bramble's lightweight ownership and borrowing model, and why a scripting language warranted one."
date: 2023-11-11
author: Dr. Hazel Mbeki
tags: [design, ownership, borrowing, internals, memory]
uid: bramble.blog.the-borrow-checker-story
---

Building a borrow checker for a scripting language is the kind of decision that invites skepticism. Borrow checkers are for systems languages, people said. Scripts are short-lived. The GC handles it. Why are you doing this?

Here's the honest answer: we built it because Bramble runs in a sandbox, and the sandbox needs to reason about aliasing to make its guarantees hold. The ownership model fell out of that requirement, not from an ideological preference.

## Why the sandbox needed it

Bramble's sandbox isolates host resources: file handles, network sockets, capability tokens. The host grants these to scripts at startup and expects to reclaim them cleanly when the script finishes — or when something goes wrong mid-execution.

With a pure GC and unrestricted aliasing, we couldn't make strong guarantees about when a resource would be released. Two closures could hold references to the same file handle, and the GC's collection timing is non-deterministic. For capabilities, that's a problem.

Ownership gave us a static answer: a resource has exactly one owner at any given time. When the owner goes out of scope, the resource is released — deterministically, before GC gets involved.

## Designing a lightweight model

We didn't want to implement Rust's borrow checker. That system is extraordinarily powerful and also extraordinarily demanding. Bramble is a scripting language; the cognitive overhead has to stay low.

Our model has three rules, and we worked hard to keep it at three:

1. Every value has one owner.
2. You can lend a value as an immutable reference to as many places as you want, simultaneously.
3. You can lend a value as a mutable reference to exactly one place at a time, with no simultaneous immutable references.

That's it. No lifetime annotations in user-facing code for the common case. The compiler infers lifetimes within function bodies. You only annotate when a borrow crosses a function boundary and the inference can't resolve it — which, in practice, is rare for scripting workloads.

```bramble
fn process(items: &[Item]) -> Int {
    let mut count = 0
    for item in items {       // borrows each element immutably
        if item.active {
            count += 1
        }
    }
    count
}
```

The `&` sigil on the parameter says "I borrow this, I don't own it". The caller retains ownership. No copy, no move.

## Where we got stuck

The hard part was closures. A closure that captures a mutable binding and then escapes its declaring scope creates the exact aliasing situation the model is supposed to prevent. Our first implementation was too conservative — it rejected closures that were obviously safe.

We spent about six weeks on the closure escape analysis. The final approach tracks whether a closure outlives the values it captures mutably, and rejects only the cases where it genuinely does. Closures that stay within their scope are handled without annotation. Closures that escape require either a move or an explicit lifetime bound.

It's not perfect. There are cases the checker rejects that a human can see are safe. We've logged them all and we're working through them. The goal is never to be more restrictive than necessary.

## The result

The borrow checker catches a real class of bugs — use-after-free in capability handles, double-release of resources, aliasing violations in concurrent code. Most Bramble programs never see an error from it, because most programs don't do the things it prevents. When it does fire, the error message tries hard to explain what the conflict is and where both references live.

The [ownership and borrowing explanation](xref:bramble.explanation.ownership-and-borrowing) covers the formal model and the annotation syntax in more depth. The design is still evolving — 2.0 has some planned changes to how mutable borrows interact with async tasks — but the core three rules have held up well.

It turns out scripting languages have aliasing problems too. We just usually shrug and blame the GC.

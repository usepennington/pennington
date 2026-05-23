---
title: "Ownership and borrowing"
description: "How Bramble's lightweight ownership model provides memory safety without requiring explicit lifetime annotations in most programs."
uid: bramble.explanation.ownership-and-borrowing
order: 40
sectionLabel: "Explanation"
tags: [ownership, borrowing, memory, safety, borrow-checker]
---

Memory safety in Bramble is enforced by a borrow checker that tracks who owns a value and who holds references to it at any given point in the program. The model is intentionally lighter than comparable systems in lower-level languages: it solves the common cases automatically and only asks the programmer for explicit guidance in genuinely ambiguous situations.

## Ownership and moves

Every value in Bramble has exactly one owner at a time. When you assign a value to a new binding or pass it to a function, ownership transfers—the original binding is no longer usable. This is called a move.

```bramble
let config = Config.load("bramble.toml")
let server = Server.new(config)   // config is moved into Server.new
// using config here is a compile error
```

Types that are cheap to copy—integers, booleans, small fixed-size structs—implement the `Copy` trait and are duplicated automatically rather than moved. For larger types, explicit copying is available via the `Clone` trait, but it is never implicit. The reason is that implicit copying hides performance costs in ways that are difficult to audit.

## Shared and exclusive borrows

Rather than transferring ownership, you can lend a value to a function by passing a reference. Bramble distinguishes two kinds:

- **Shared borrow** (`&T`): any number of shared borrows can exist simultaneously. The value cannot be mutated through them.
- **Exclusive borrow** (`&mut T`): exactly one exclusive borrow can exist at a time, and no shared borrows can coexist with it. The value can be mutated through it.

This invariant—many readers or one writer, never both—is what allows Bramble to rule out data races at compile time. The same rule applies to concurrent code; the borrow checker does not have a separate concurrency mode.

```bramble
fn summarize(report: &Report) -> String {  // shared borrow
    report.title + ": " + report.summary
}

fn append_note(report: &mut Report, note: String) {  // exclusive borrow
    report.notes.push(note)
}
```

## Why explicit lifetimes are rarely needed

In most programs, the borrow checker can determine how long a reference is valid by examining the control flow of the function it is in. A reference returned from a function must come from one of the function's parameters, and in the common case where there is only one candidate, the checker infers the connection.

Explicit lifetime annotations become necessary when a returned reference could plausibly come from more than one source, or when a struct stores a reference and the checker needs to know which owner the struct is tied to. These situations arise in library code more often than in application code—most scripts and tools never encounter them.

The tradeoff compared to a more minimal ownership model (for example, reference counting everywhere) is compile-time complexity. When the borrow checker rejects a program, the error message must explain a relationship between values across lines of code. Bramble invests heavily in error message quality precisely because this is the most common source of initial friction.

## Relationship to the garbage collector

Bramble has a garbage collector, but the borrow checker reduces how much it needs to do. Values with statically-known lifetimes are freed at the end of their scope without GC involvement. The GC handles the residual cases: values that escape to the heap, values shared across tasks via channels, and reference-counted handles. The result is shorter GC pauses and less allocation pressure than you would see in a fully GC-managed language. See [the garbage collector](xref:bramble.explanation.garbage-collection) for details on the collection strategy.

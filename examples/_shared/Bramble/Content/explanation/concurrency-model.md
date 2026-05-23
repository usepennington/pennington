---
title: "The concurrency model"
description: "How Bramble's lightweight tasks, work-stealing scheduler, and channel-based communication compose into a concurrency model that rules out data races at compile time."
uid: bramble.explanation.concurrency-model
order: 80
sectionLabel: "Explanation"
tags: [concurrency, tasks, channels, scheduler, safety]
---

Bramble's approach to concurrency was designed around two constraints: it should be usable without a deep understanding of threading primitives, and it should be impossible to introduce a data race. These constraints interact—meeting both requires that the language, rather than a library, own the concurrency model.

## Lightweight tasks

The primitive unit of concurrency in Bramble is a task, created with `spawn`. Tasks are not OS threads; they are lightweight coroutines scheduled by the Bramble runtime on a pool of OS threads. A typical program can have thousands of live tasks without meaningful overhead.

```bramble
let handle = spawn {
    fetch_remote_config()
}
let config = await handle
```

`spawn` takes a block (a closure with no free mutable borrows—more on this shortly) and returns a handle. `await` suspends the current task until the spawned task completes and produces its value. Tasks are the only way to introduce parallel execution; there is no direct access to thread primitives.

## The work-stealing scheduler

Tasks are distributed across a pool of worker threads using a work-stealing scheduler. Each worker maintains a local queue of runnable tasks. When a worker exhausts its queue, it steals tasks from the tail of another worker's queue, which balances load without requiring a central coordinator for every scheduling decision.

The number of worker threads defaults to the number of logical CPU cores but is configurable. The scheduler is cooperative for CPU-bound tasks—a task runs until it `await`s or explicitly yields—which means a task that never blocks can starve others. Long-running CPU work should yield periodically or be broken into smaller spawned tasks.

## Channels

Tasks communicate by sending values through channels. A channel is a typed, buffered or unbuffered conduit between a sender and a receiver.

```bramble
let (tx, rx) = Channel[String].new(capacity: 8)

spawn {
    tx.send("first result")
    tx.send("second result")
    tx.close()
}

for msg in rx {
    log.info(msg)
}
```

Sending a value through a channel moves ownership of that value to the receiver. The sender no longer holds a reference. This is how Bramble avoids the classic shared-mutable-state problem: values are not shared, they are transferred.

## Why there are no data races

The borrow checker's rules apply to tasks. When you `spawn` a block, the block captures variables from the enclosing scope. The borrow checker verifies that either the value is moved into the task (the spawning scope loses ownership) or the value is `Copy` (a cheap duplicate). A mutable reference cannot be captured by a `spawn` block if any other borrow of that value exists.

This means the compiler rejects programs with data races before they run. There is no runtime race detector needed; the type system is the race detector.

```bramble
let mut counter = 0
spawn { counter += 1 }  // compile error: cannot move exclusive borrow into spawn
```

## Structured concurrency

Bramble encourages structured concurrency: tasks should be spawned and awaited within a defined scope, so their lifetimes nest cleanly inside the spawning task's lifetime. The `TaskGroup` API collects a set of spawned tasks and awaits all of them, propagating the first error if any task fails.

```bramble
let results = TaskGroup.collect {|group|
    for url in urls {
        group.spawn(fetch(url))
    }
}
```

The alternative—fire-and-forget tasks that outlive their spawning scope—is possible but discouraged. Tasks that escape their scope make resource management and error propagation harder to reason about. For the practical patterns, see [running work concurrently](xref:bramble.how-to.language.run-work-concurrently).

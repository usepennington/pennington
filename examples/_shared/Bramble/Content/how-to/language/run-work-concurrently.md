---
title: "Run work concurrently"
description: "Spawn tasks, await their results, and communicate between them using channels."
uid: bramble.how-to.language.run-work-concurrently
order: 150
sectionLabel: "Language"
tags: [concurrency, tasks, channels, async]
---

Bramble's concurrency model is built around lightweight tasks scheduled by the runtime rather than OS threads. You spawn tasks with `task::spawn`, await their results, and pass values between tasks through typed channels.

## Spawn a task and await its result

`task::spawn` takes a closure and returns a `Task<T>`, where `T` is the closure's return type. Call `.await()` to block the current task until the spawned task finishes.

```bramble
import std/task
import std/io

fn fetch_price(symbol: str) -> Result<float, str> {
    // ... network call ...
    Ok(42.0)
}

fn main() -> Result<(), str> {
    let t1 = task::spawn(|| fetch_price("BRAM"))
    let t2 = task::spawn(|| fetch_price("TREL"))

    let p1 = t1.await()?
    let p2 = t2.await()?

    io::println("BRAM: ${p1}, TREL: ${p2}")
    Ok(())
}
```

Both tasks are running concurrently between the two `spawn` calls and the two `await` calls. Awaiting one does not cancel the other.

## Await multiple tasks at once

`task::join` takes a list of tasks and returns a list of results, waiting for all of them.

```bramble
let symbols = ["BRAM", "TREL", "HEDG"]
let tasks = symbols.map(|s| task::spawn(|| fetch_price(s)))
let prices = task::join(tasks).map(|r| r.unwrap_or(0.0))
```

## Communicate with channels

A channel is a typed, bounded queue between tasks. `channel::bounded` returns a `(Sender<T>, Receiver<T>)` pair.

```bramble
import std/channel

fn main() -> Result<(), str> {
    let (tx, rx) = channel::bounded::<str>(32)

    task::spawn(|| {
        for item in ["apple", "pear", "plum"] {
            tx.send(item)?
        }
        tx.close()
        Ok(())
    })

    for msg in rx {
        io::println("received: ${msg}")
    }
    Ok(())
}
```

Iterating over a `Receiver` yields values until the sender closes the channel, at which point the loop ends.

## Handle task cancellation

Tasks can be cancelled by calling `.cancel()` on the `Task` handle. A cancelled task receives a `TaskCancelled` signal on its next yield point and can clean up before stopping.

```bramble
let t = task::spawn(|| long_running_job())
// ... later ...
t.cancel()
```

> [!IMPORTANT]
> Channels are not shared memory. Each value is moved into the channel on `send` and moved out on receive. This eliminates data races by construction — there is no way for two tasks to hold a mutable reference to the same value simultaneously.

The design choices behind this model are covered in [the concurrency model explanation](xref:bramble.explanation.concurrency-model). For building networked services that handle many concurrent connections, see the [web service tutorial](xref:bramble.tutorials.a-web-service).

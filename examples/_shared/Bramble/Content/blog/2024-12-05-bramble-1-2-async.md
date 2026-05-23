---
title: "Bramble 1.2: async tasks land"
description: "Bramble 1.2 ships a structured-concurrency model built on spawn/await, a cooperative scheduler, and typed channels — here's what changed and why."
date: 2024-12-05
author: Maple Okafor
tags: [release, async, concurrency, scheduler]
uid: bramble.blog.bramble-1-2-async
---

Async concurrency has been the single most-requested feature since 1.0, and it took us most of the year to get it right. Bramble 1.2 ships today with `spawn`, `await`, and typed channels — a structured-concurrency model that fits the rest of the language rather than bolting on a runtime afterthought.

## The shape of the model

Tasks in Bramble are not threads. The scheduler is cooperative and runs on a single OS thread by default (you can opt into a thread pool for CPU-bound work, but that's a separate flag). A task yields when it hits an `await`, and the scheduler picks up the next ready task. This keeps the ownership and borrowing model intact: only one task runs at a time per scheduler, so you don't need locks for most shared state.

```bramble
use std.task.{spawn, sleep}
use std.channel.{channel, Sender, Receiver}

fn producer(tx: Sender<Int>) -> Task<()> {
    for i in 0..10 {
        tx.send(i).await
        sleep(50ms).await
    }
}

fn consumer(rx: Receiver<Int>) -> Task<()> {
    loop {
        match rx.recv().await {
            Option.Some(val) => println("got {val}"),
            Option.None      => break,
        }
    }
}

fn main() -> Result<(), Error> {
    let (tx, rx) = channel()
    let p = spawn producer(tx)
    let c = spawn consumer(rx)
    p.await?
    c.await?
    Ok(())
}
```

`spawn` returns a `Task<T>`. Awaiting it blocks the current task until the spawned one completes and yields its value. Tasks are structured — a spawned task's lifetime is bounded by the scope of the `Task<T>` handle. If the handle is dropped, the task is cancelled at its next yield point.

## Channels

Channels are the primary coordination primitive. They're typed, bounded by default (`channel::<T>(capacity: Int)`), and backpressure is applied automatically when the buffer fills — `send` will yield until space is available rather than returning an error or blocking the scheduler. Unbounded channels exist but require an explicit `channel_unbounded()` call so the decision is visible.

Both `Sender` and `Receiver` implement `Drop` properly, so a closed sender causes a receiver's `recv` to return `Option.None` rather than blocking forever. This is the idiomatic way to signal "no more values."

## The scheduler

The scheduler is work-stealing when the thread-pool flag is on, cooperative-single-thread otherwise. The [concurrency model explanation](xref:bramble.explanation.concurrency-model) goes deeper into the tradeoffs here, but the practical upshot is: CPU-bound tasks should be structured as a pool of spawned tasks with the `--tasks=N` runtime flag; I/O-bound tasks work well with the default single-threaded scheduler and no flag required.

## Upgrading from 1.1

The `async` and `await` keywords were reserved but unimplemented in 1.0 and 1.1. They're live now. If you were using those identifiers as variable names (the `async` one especially appeared in a few packages as a local variable name — yes, really), the compiler will tell you at parse time.

No other breaking changes. `bramble update` via Thicket handles the runtime upgrade automatically.

## What's still missing

Error propagation across task boundaries is functional but verbose in 1.2. The `?` operator doesn't yet pierce an `await`, so you're spelling out the `match` by hand in some cases. That's the first thing on the list for 1.3.

Update with `thicket update bramble` or grab the installer at [bramble.dev](https://bramble.dev).

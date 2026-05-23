---
title: "std/time"
description: "Reference for the std/time module, covering instants, durations, sleep, and date/time formatting and parsing."
uid: bramble.reference.stdlib.time
order: 280
sectionLabel: "Standard library"
tags: [stdlib, time, duration, scheduling]
---

The `std/time` module provides types and functions for working with points in time, elapsed durations, sleeping, and formatting or parsing timestamp strings.

## Importing

```bramble
import std/time
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `now` | `() -> Instant` | Returns the current wall-clock time as an `Instant`. |
| `sleep` | `(d: Duration) -> ()` | Suspends the current task for the given duration. |
| `since` | `(start: Instant) -> Duration` | Returns the elapsed time between `start` and now. |
| `format` | `(t: Instant, layout: str) -> str` | Formats an `Instant` using a layout string (see below). |
| `parse` | `(s: str, layout: str) -> Result<Instant, TimeError>` | Parses a string into an `Instant` using a layout string. |
| `Duration.from_secs` | `(s: i64) -> Duration` | Constructs a `Duration` from a number of whole seconds. |
| `Duration.from_millis` | `(ms: i64) -> Duration` | Constructs a `Duration` from a number of milliseconds. |
| `Duration.secs` | `(self: Duration) -> i64` | Returns the whole-seconds part of a duration. |
| `Duration.millis` | `(self: Duration) -> i64` | Returns the total duration expressed in milliseconds. |
| `Instant.unix` | `(self: Instant) -> i64` | Returns seconds since the Unix epoch (UTC). |

## Layout strings

`format` and `parse` use a reference-date layout: each component is represented by its value in the reference time **2006-01-02T15:04:05Z**.

| Token | Meaning |
|---|---|
| `2006` | Four-digit year |
| `01` | Two-digit month |
| `02` | Two-digit day |
| `15` | Hour (24-hour) |
| `04` | Minute |
| `05` | Second |
| `Z` | UTC suffix; use `-07:00` for numeric offset |

Common layouts are available as constants: `time.RFC3339` (`"2006-01-02T15:04:05Z"`), `time.DATE` (`"2006-01-02"`), and `time.DATETIME` (`"2006-01-02 15:04:05"`).

## Example: timing an operation

```bramble
import std/time

fn timed_work() {
    let start = time.now()

    do_heavy_work()

    let elapsed = time.since(start)
    println("finished in ${elapsed.millis()} ms")
}
```

```text
finished in 143 ms
```

## Example: parsing and formatting

```bramble
import std/time

let ts = "2025-11-01"
let day = time.parse(ts, time.DATE)?
let pretty = time.format(day, "02 Jan 2006")
println(pretty)
```

```text
01 Nov 2025
```

## Concurrency and sleep

`time.sleep` suspends the calling task without blocking the runtime's thread pool. For patterns involving multiple concurrent timers, see [Run work concurrently](xref:bramble.how-to.language.run-work-concurrently) and the [concurrency model explanation](xref:bramble.explanation.concurrency-model).

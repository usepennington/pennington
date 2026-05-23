---
title: "std/random"
description: "Reference for the std/random module, covering seeded and unseeded random number generation, range sampling, and collection shuffling."
uid: bramble.reference.stdlib.random
order: 310
sectionLabel: "Standard library"
tags: [stdlib, random, sampling, deterministic]
---

The `std/random` module provides pseudo-random number generation using a fast xoshiro256** generator. When no seed is provided the generator is seeded from the platform entropy source. Supplying an explicit seed produces a fully deterministic sequence, which is useful for reproducible tests and simulations.

## Importing

```bramble
import std/random
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `seed` | `(s: i64) -> Rng` | Creates a new seeded `Rng` with a deterministic sequence. |
| `default` | `() -> Rng` | Creates a new `Rng` seeded from platform entropy. |
| `int_range` | `(rng: Rng, lo: i64, hi: i64) -> i64` | Returns a random integer in the inclusive range `[lo, hi]`. |
| `float` | `(rng: Rng) -> f64` | Returns a random `f64` in `[0.0, 1.0)`. |
| `bool` | `(rng: Rng) -> bool` | Returns `true` or `false` with equal probability. |
| `choice` | `(rng: Rng, items: [T]) -> Option<T>` | Returns a randomly selected element, or `Option.None` for an empty list. |
| `shuffle` | `(rng: Rng, items: [T]) -> [T]` | Returns a new list with elements in a random order. |

## Deterministic usage

Two `Rng` values created with the same seed produce identical sequences, regardless of platform:

```bramble
import std/random

let rng = random.seed(42)
println(random.int_range(rng, 1, 100))
println(random.int_range(rng, 1, 100))
```

```text
73
19
```

The sequence above is guaranteed to be the same on every platform running the same version of Bramble's standard library.

## Example: shuffling a list

```bramble
import std/random

let rng = random.default()
let deck = ["ace", "king", "queen", "jack", "ten"]
let shuffled = random.shuffle(rng, deck)
println(shuffled)
```

```text
["queen", "ten", "ace", "jack", "king"]
```

## Example: weighted sampling with `float`

```bramble
import std/random

fn roll_hit(rng: random.Rng, accuracy: f64) -> bool {
    random.float(rng) < accuracy
}

let rng = random.default()
println(roll_hit(rng, 0.75))
```

> [!WARNING]
> `Rng` values are mutable handles — each call to a generation function advances the internal state. Sharing one `Rng` across concurrent tasks without synchronisation will produce non-deterministic results even with a fixed seed. Create a separate `Rng` per task, or use `random.seed` with different seeds derived from a root value.

## Related

- [Run work concurrently](xref:bramble.how-to.language.run-work-concurrently) — task-local Rng patterns

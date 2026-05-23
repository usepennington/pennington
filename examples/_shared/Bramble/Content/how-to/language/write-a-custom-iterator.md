---
title: "Write a custom iterator"
description: "Implement the Iterator protocol with a next() method returning Option so your type works in for-in loops."
uid: bramble.how-to.language.write-a-custom-iterator
order: 140
sectionLabel: "Language"
tags: [iterators, protocol, collections, for-in]
---

Any type that implements the `Iterator` protocol can be used in a `for x in collection { }` loop. The protocol requires a single method: `next()` returning `Option<Item>`, where `None` signals the end of the sequence.

## Define a record that holds iteration state

The record carries whatever state the iterator needs between calls. A counter, a reference to the underlying data, a cursor — anything that lets `next` know where it is.

```bramble
record RangeIter {
    mut current: int,
    stop: int,
    step: int,
}
```

## Implement the Iterator protocol

Declare an `impl Iterator` block with the associated `Item` type and the `next` method.

```bramble
impl Iterator for RangeIter {
    type Item = int

    fn next(mut self) -> Option<int> {
        if self.current >= self.stop {
            return None
        }
        let value = self.current
        self.current = self.current + self.step
        Some(value)
    }
}
```

`next` takes `mut self` because it updates `current`. Returning `None` ends the sequence.

## Add a constructor function

A plain function is the standard way to expose a new iterator. Returning the concrete type lets callers use it directly without naming the iterator record.

```bramble
fn range(start: int, stop: int, step: int) -> RangeIter {
    RangeIter { current: start, stop, step }
}
```

## Use the iterator in a for-in loop

Once the `Iterator` protocol is implemented, `for x in` works without any extra ceremony.

```bramble
for n in range(0, 10, 2) {
    io::println("${n}")   // 0, 2, 4, 6, 8
}
```

The loop calls `next()` on each iteration and stops when it receives `None`.

## Chain standard adapters

Because `RangeIter` satisfies `Iterator`, all standard adapter methods — `map`, `filter`, `take`, `collect` — become available automatically.

```bramble
let squares = range(1, 6, 1)
    .map(|n| n * n)
    .collect::<list<int>>()
// [1, 4, 9, 16, 25]
```

> [!NOTE]
> `collect` needs a type annotation so Bramble knows which collection to build. The turbofish syntax `::<list<int>>` supplies it inline; you can also annotate the binding: `let squares: list<int> = range(...).collect()`.

For the built-in collection types and their iterator methods, see the [collections standard library reference](xref:bramble.reference.stdlib.collections). The [collections tutorial](xref:bramble.tutorials.collections) also covers iteration patterns in depth.

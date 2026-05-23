---
title: "std/collections"
description: "Reference for the std/collections module, covering list, map, and set operations in Bramble."
uid: bramble.reference.stdlib.collections
order: 220
sectionLabel: "Standard library"
tags: [stdlib, collections, data-structures]
---

The `std/collections` module provides higher-order operations on lists, maps, and sets. For basic list literals and map literals, no import is required; this module adds richer functional and structural operations.

## Importing

```bramble
import std/collections
```

## List functions

| Function | Signature | Description |
|---|---|---|
| `map` | `(list: [A], f: fn(A) -> B) -> [B]` | Returns a new list by applying `f` to each element of `list`. |
| `filter` | `(list: [A], pred: fn(A) -> bool) -> [A]` | Returns a new list containing only elements for which `pred` returns `true`. |
| `fold` | `(list: [A], init: B, f: fn(B, A) -> B) -> B` | Reduces `list` to a single value by accumulating with `f`, starting from `init`. |
| `any` | `(list: [A], pred: fn(A) -> bool) -> bool` | Returns `true` if `pred` is `true` for at least one element. |
| `all` | `(list: [A], pred: fn(A) -> bool) -> bool` | Returns `true` if `pred` is `true` for every element. |
| `find` | `(list: [A], pred: fn(A) -> bool) -> Option<A>` | Returns the first element matching `pred`, or `None`. |
| `sort` | `(list: [A]) -> [A]` | Returns a new sorted list; `A` must implement the `Ord` constraint. |
| `sort_by` | `(list: [A], key: fn(A) -> B) -> [A]` | Returns a new list sorted by the value returned by `key`. |
| `push` | `(list: mut [A], item: A) -> unit` | Appends `item` to the end of `list` in place. |
| `pop` | `(list: mut [A]) -> Option<A>` | Removes and returns the last element, or `None` if empty. |
| `reverse` | `(list: [A]) -> [A]` | Returns a new list with elements in reverse order. |
| `flatten` | `(list: [[A]]) -> [A]` | Concatenates a list of lists into a single list. |
| `zip` | `(a: [A], b: [B]) -> [(A, B)]` | Pairs elements from `a` and `b` by index; stops at the shorter list. |
| `len` | `(list: [A]) -> i64` | Returns the number of elements in `list`. |

## Map functions

| Function | Signature | Description |
|---|---|---|
| `insert` | `(m: mut {K: V}, key: K, val: V) -> unit` | Inserts or replaces the entry for `key` in `m`. |
| `get` | `(m: {K: V}, key: K) -> Option<V>` | Returns the value for `key`, or `None` if absent. |
| `remove` | `(m: mut {K: V}, key: K) -> Option<V>` | Removes the entry for `key` and returns its value, or `None`. |
| `keys` | `(m: {K: V}) -> [K]` | Returns all keys as a list in unspecified order. |
| `values` | `(m: {K: V}) -> [V]` | Returns all values as a list in unspecified order. |
| `contains_key` | `(m: {K: V}, key: K) -> bool` | Returns `true` if `m` has an entry for `key`. |
| `merge` | `(a: {K: V}, b: {K: V}) -> {K: V}` | Returns a new map combining `a` and `b`; keys in `b` win on conflict. |

## Set functions

A `Set<T>` is a distinct built-in type (not a map alias). The following functions operate on it.

| Function | Signature | Description |
|---|---|---|
| `set_new` | `(items: [T]) -> Set<T>` | Constructs a set from a list, discarding duplicates. |
| `set_add` | `(s: mut Set<T>, item: T) -> unit` | Adds `item` to `s`; no-op if already present. |
| `set_has` | `(s: Set<T>, item: T) -> bool` | Returns `true` if `item` is a member of `s`. |
| `set_remove` | `(s: mut Set<T>, item: T) -> bool` | Removes `item`; returns `true` if it was present. |
| `union` | `(a: Set<T>, b: Set<T>) -> Set<T>` | Returns the union of `a` and `b`. |
| `intersect` | `(a: Set<T>, b: Set<T>) -> Set<T>` | Returns only elements present in both `a` and `b`. |
| `difference` | `(a: Set<T>, b: Set<T>) -> Set<T>` | Returns elements in `a` that are not in `b`. |

## Example

Count word frequencies in a list of tokens.

```bramble
import std/collections

fn word_counts(words: [str]) -> {str: i64} {
    let mut counts: {str: i64} = {}
    for word in words {
        let current = collections.get(counts, word)
        match current {
            Some(n) -> collections.insert(counts, word, n + 1)
            None    -> collections.insert(counts, word, 1)
        }
    }
    counts
}

let tokens = ["bramble", "is", "fast", "bramble", "is", "fun"]
let freq = word_counts(tokens)
std/io.println("${collections.get(freq, "bramble")}")
```

```text
Some(2)
```

> [!WARNING]
> `sort` and `sort_by` return new lists and do not modify the original. If you pass the result to a function expecting the original variable to be sorted, assign the return value back explicitly.

## Related

- [Working with collections tutorial](xref:bramble.tutorials.collections)
- [std/strings](xref:bramble.reference.stdlib.strings)
- [Type reference](xref:bramble.reference.language.types)

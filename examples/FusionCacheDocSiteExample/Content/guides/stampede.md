---
title: Cache stampede protection
description: Only one factory per key runs at a time — every other concurrent caller for the same key gets the same result.
uid: guides.stampede
order: 40
sectionLabel: Guides
---

A cache stampede is what happens the moment a hot key expires: every in-flight request misses, every miss runs the factory, every factory hammers the same database row, and the load that the cache was supposed to absorb lands on the upstream all at once. It's the failure mode that makes people distrust caches.

FusionCache prevents it at the cache layer, with no configuration required.

## What happens without protection

```
T+0.000s   key "user:42" TTL expires
T+0.001s   request A arrives, misses, begins factory
T+0.002s   request B arrives, misses, begins factory
T+0.003s   request C arrives, misses, begins factory
...
```

N simultaneous requests → N simultaneous factories → one database gets N× the load it thought it signed up for. If the factory is slow because the database is overloaded, the overload feeds on itself.

## What FusionCache does

```
T+0.000s   key "user:42" TTL expires
T+0.001s   request A arrives, misses, begins factory
T+0.002s   request B arrives, misses — waits on A's factory
T+0.003s   request C arrives, misses — waits on A's factory
T+0.050s   A's factory completes, value cached
T+0.050s   A, B, C all return the same value
```

The first factory per key wins. Every other concurrent caller parks on a lightweight await, then resolves with the factory's result. One round-trip to the database, not N.

## It composes with fail-safe

If A's factory *throws*, protection still holds: every waiter sees the same exception — but if [fail-safe](xref:guides.fail-safe) is on, they instead see the previous value. The failure mode a single caller hits is the failure mode every caller hits; no stragglers keep trying.

## It works cross-async

Sync and async callers are coordinated against the same lock per key, so `GetOrSet` and `GetOrSetAsync` interoperate without doubling the factory.

## You have to opt out, not in

Stampede protection is the default. The opposite of the usual distributed-systems experience, where safety is a feature flag you forget to turn on.

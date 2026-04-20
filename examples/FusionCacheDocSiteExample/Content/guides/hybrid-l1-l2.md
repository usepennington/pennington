---
title: The L1 + L2 hybrid model
description: FusionCache always has an in-memory L1; adding a distributed L2 extends it across processes without changing any call site.
uid: guides.hybrid-l1-l2
order: 30
sectionLabel: Guides
---

The name "hybrid cache" means two tiers coordinated behind one API. L1 is always in memory, because that's the only thing that's fast. L2 is whatever `IDistributedCache` you already trust — Redis, SQL Server, the Azure cache. The caller writes `GetOrSet`; FusionCache decides which tier actually serves it.

## Why two tiers

- A single-node memory cache is fast but has no cross-process visibility. Every pod, every process, every replica pays the full miss cost on cold start and whenever its local copy evicts.
- A pure distributed cache has visibility but costs a network round-trip on every read. At steady state, a local hit is an order of magnitude faster.

L1 absorbs the steady-state load; L2 catches the cold starts, cross-process reads, and restarts. Together they give you both the latency of memory and the durability of a shared cache.

## Add L2

Once you have the memory cache registered (<xref:guides.getting-started>), plug any `IDistributedCache` and a serializer:

```csharp
builder.Services.AddStackExchangeRedisCache(opts => opts.Configuration = "...");

builder.Services.AddFusionCache()
    .WithSystemTextJsonSerializer()
    .WithRegisteredDistributedCache();
```

No call-site change is needed — the same `cache.GetOrSet("product:42", ...)` now checks L1, falls back to L2, and only runs the factory if both miss.

## What FusionCache does with errors at L2

L2 is a network dependency, so it fails. FusionCache **isolates** those errors by default: if Redis returns a timeout, the cache falls back to L1 (and to the factory when L1 is empty) instead of surfacing the exception to your handler. Logging captures the detail. The app keeps serving.

Combined with [fail-safe](xref:guides.fail-safe), the read path stays up through outages that would have blown up a more-naïve cache.

## Still one node? Add a backplane.

Two processes reading and writing the same keys through L2 will see each other's writes eventually, but not instantly. The **backplane** publishes invalidation messages on a pub/sub channel so every node evicts its stale L1 entry the moment another node writes. That's where you want to be once you have more than one replica.

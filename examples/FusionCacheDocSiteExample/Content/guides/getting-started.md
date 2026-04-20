---
title: Getting started
description: Install the package, wire the service, and cache your first value.
uid: guides.getting-started
order: 10
sectionLabel: Guides
---

FusionCache is a single NuGet package plus one DI call. Install it, register, and you have an in-memory cache. Add an `IDistributedCache` later to make it a hybrid without touching any call sites.

## Install

```powershell
Install-Package ZiggyCreatures.FusionCache
```

## Register

The builder pattern mirrors the rest of `Microsoft.Extensions.*`:

```csharp
builder.Services.AddFusionCache();
```

That's enough for a single-node cache that shares the ambient `IMemoryCache`. Everything else — durations, fail-safe, distributed tier, backplane — is additive from here.

## Cache your first value

The core workflow is `GetOrSet(key, factory, options)`. FusionCache checks L1 and L2 in turn; if both miss, it runs your factory, stores the result, and returns it:

```csharp
var product = cache.GetOrSet<Product>(
    $"product:{id}",
    _ => GetProductFromDb(id),
    options => options.SetDuration(TimeSpan.FromMinutes(1)));
```

A concurrent request for the same key while the factory is still running gets the same result — that's [stampede protection](xref:guides.stampede) doing its job, not an accident.

## What you get by default

- Single-instance L1 cache backed by whatever `IMemoryCache` the app already registered (or a fresh one).
- Stampede protection on every key.
- Full sync and async API surface.

## Next steps

- <xref:guides.fail-safe> — keep serving stale data when the upstream fails.
- <xref:guides.hybrid-l1-l2> — add a distributed tier without rewriting the call sites.

---
title: Survive upstream outages with fail-safe
description: Keep serving the last-known-good value when the factory throws or the distributed cache goes down.
uid: guides.fail-safe
order: 20
sectionLabel: Guides
---

Networks blip. Databases fall over. The default behavior of most caches — propagate the exception, tell the user something is on fire — is almost always the wrong call for a read path. FusionCache's **fail-safe** keeps the expired value and serves it while the upstream recovers.

## The problem

Your factory throws because the database is overloaded. Without fail-safe, every cache miss during the incident becomes a user-facing 500. The cache is empty, the DB is drowning, and everyone's having a bad time.

## Turn it on

Fail-safe is per-entry-options. Opt in, then set two windows:

```csharp
var opts = new FusionCacheEntryOptions(TimeSpan.FromMinutes(1))
    .SetFailSafe(
        isEnabled: true,
        maxDuration: TimeSpan.FromHours(2),
        throttleDuration: TimeSpan.FromSeconds(30));
```

- **`maxDuration`** — how long the expired value is allowed to hang around as a fallback. Two hours means "even after its 1-minute TTL expires, keep it eligible as a stale fallback for up to two hours."
- **`throttleDuration`** — after a fail-safe hit, wait this long before trying the factory again, so the next request doesn't hammer the downed dependency.

## What it looks like in practice

1. First request: factory runs, value lands in cache with a 1-minute TTL.
2. A minute later: factory throws. Instead of propagating the exception, FusionCache returns the stale value and schedules a retry after 30 seconds.
3. Database recovers. Next retry succeeds; cache is refreshed.
4. If the outage lasts longer than `maxDuration`, the stale value expires completely and the error surfaces.

## When not to use it

Fail-safe is a read-path tool. Don't use it for writes or for anything where stale is wrong (pricing during a flash sale, auth tokens, ticket availability). The throttle duration and max duration together should be smaller than the window in which staleness is tolerable.

## Related

- <xref:guides.hybrid-l1-l2> — fail-safe composes with the distributed tier: L1 can serve stale while L2 is syncing.
- [`FusionCacheEntryOptions`](/api/fusion-cache-entry-options/) — every per-call knob, including the fail-safe toggles.

---
title: FusionCache
description: "An easy to use, fast and robust hybrid cache with advanced resiliency features."
uid: home
---

[FusionCache](https://github.com/ZiggyCreatures/FusionCache) is a hybrid cache: L1 in-memory, optionally backed by an L2 distributed cache, with cache-stampede protection, fail-safe, and a backplane for multi-node coordination. It reaches 72M+ downloads and is embedded in Microsoft's Data API Builder.

This site is a **demo of Pennington's reflection-based metadata backend**. Not a word of FusionCache source lives in this repository — the API reference under [`/api/`](/api/) is rendered straight from the shipped `.dll` + `.xml` pair in `lib/net9.0/`.

## Start reading

- <xref:guides.getting-started> — install, register the service, call `GetOrSet`.
- <xref:guides.fail-safe> — let stale data keep you online when the upstream fails.
- <xref:guides.hybrid-l1-l2> — why an in-memory cache alone is not enough.
- [`/api/`](/api/) — every public type, rendered from the shipped `.dll` + `.xml` pair.

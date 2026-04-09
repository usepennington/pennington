---
title: "Configuration Pitfalls in ASP.NET"
date: 2026-02-28
author: "Mara Chen"
description: "Subtle configuration mistakes that cause production issues"
tags: ["aspnet", "configuration"]
---

Configuration in ASP.NET looks simple until it breaks in production. Here are pitfalls I've debugged more than once.

## Options Pattern Gotchas

The options pattern is great, but `IOptions<T>` captures values at startup while `IOptionsSnapshot<T>` reloads per-request. Mixing them up leads to stale configuration.

```csharp
// This never updates after startup
public class MyService(IOptions<MyOptions> options) { }

// This reloads from config on each request
public class MyService(IOptionsSnapshot<MyOptions> options) { }
```

## Environment Variable Precedence

Configuration sources are layered in order. Environment variables override `appsettings.json`, which catches people off guard in containers where leftover env vars silently override config files.

## Secret Manager vs. User Secrets

Don't commit connection strings. Use `dotnet user-secrets` for local development and a proper vault (Azure Key Vault, AWS Secrets Manager) for production.

---
title: "API Reference"
description: "Core types and extension methods"
order: 30
uid: "beacon.api-reference"
---

## Core Types

| Type | Description |
|------|-------------|
| `HttpMonitor` | Main monitoring class |
| `MonitorResult` | Result of a monitoring check |
| `AlertRule` | Defines when alerts trigger |
| `MonitorOptions` | Configuration options |

## Key Methods

```csharp
public class HttpMonitor
{
    public Task StartAsync(CancellationToken ct = default);
    public Task StopAsync(CancellationToken ct = default);
    public Task<MonitorResult> CheckAsync(CancellationToken ct = default);
}
```

For setup instructions, see [Getting Started](xref:beacon.getting-started).
For configuration, see [Configuration](xref:beacon.configuration).

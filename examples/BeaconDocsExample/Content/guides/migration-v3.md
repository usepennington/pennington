---
title: "Migrating from v2 to v3"
description: "Step-by-step guide to upgrading your monitoring"
order: 20
uid: "beacon.migration"
---

## Rename BeaconClient to HttpMonitor

The main class has been renamed:

```csharp
// Old (v2)
var client = new BeaconClient("https://api.example.com"); // [!code --]
// New (v3)
var monitor = new HttpMonitor("https://api.example.com"); // [!code ++]
```

## Update Configuration

```csharp
builder.Services.AddBeacon(options =>
{
    options.DefaultInterval = TimeSpan.FromMinutes(5);
    options.AlertThreshold = 3; // [!code highlight]
    options.TimeoutMs = 5000;   // [!code highlight]
    options.EnableMetrics = true;
});
```

## Remove Deprecated Calls

```csharp
// This method is removed in v3
monitor.PollOnce(); // [!code error]

// Use the async version instead
await monitor.CheckAsync(); // [!code warning]
```

> [!WARNING]
> The `PollOnce()` method has been removed with no synchronous replacement. All monitoring operations are now async.

> [!TIP]
> Run the Beacon migration analyzer: `dotnet tool install Beacon.Migrator` to find all deprecated API usage automatically.

> [!NOTE]
> The v2 configuration format is still supported but will be removed in v4.

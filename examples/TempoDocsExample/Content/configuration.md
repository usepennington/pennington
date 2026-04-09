---
title: "Configuration"
description: "Configure retry policies, concurrency, and persistence"
order: 20
uid: "tempo.configuration"
---

## Configuring Tempo

Tempo can be configured through code or JSON configuration files.

### Code Configuration

:::tabs
```csharp [C#]
builder.Services.AddTempo(options =>
{
    options.MaxConcurrency = 4;
    options.RetryPolicy = new ExponentialBackoff(
        maxRetries: 3,
        baseDelay: TimeSpan.FromSeconds(1));
    options.PersistenceProvider = new SqlitePersistence("tasks.db");
});
```

```json [JSON]
{
  "Tempo": {
    "MaxConcurrency": 4,
    "RetryPolicy": {
      "Type": "ExponentialBackoff",
      "MaxRetries": 3,
      "BaseDelaySeconds": 1
    },
    "Persistence": {
      "Provider": "Sqlite",
      "ConnectionString": "Data Source=tasks.db"
    }
  }
}
```
:::

> [!WARNING]
> Setting `MaxConcurrency` above the number of available CPU cores may cause thread pool starvation under heavy load.

## Retry Policies

Tempo supports several retry strategies out of the box. See the [API Reference](xref:tempo.api-reference) for the full list.

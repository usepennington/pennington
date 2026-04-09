---
title: "API Reference"
description: "Core types and extension methods"
order: 30
uid: "tempo.api-reference"
---

## Core Types

| Type | Description |
|------|-------------|
| `TaskScheduler` | Main entry point for scheduling tasks |
| `ScheduledTask` | Represents a scheduled task with its configuration |
| `RetryPolicy` | Base class for retry strategies |
| `PersistenceProvider` | Abstract provider for task state persistence |

## Key Methods

```csharp
public class TaskScheduler
{
    // Schedule a recurring task
    public TaskBuilder Every(TimeSpan interval);

    // Schedule a one-time task
    public TaskBuilder Once(DateTime at);

    // Start processing scheduled tasks
    public Task StartAsync(CancellationToken ct = default);

    // Gracefully stop all tasks
    public Task StopAsync(CancellationToken ct = default);
}
```

For configuration options, see the [Configuration](xref:tempo.configuration) guide.

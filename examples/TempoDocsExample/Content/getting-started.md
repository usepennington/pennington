---
title: "Getting Started"
description: "Install Tempo and schedule your first task"
order: 10
uid: "tempo.getting-started"
---

## Installation

Install the Tempo NuGet package:

```shell
dotnet add package Tempo
```

## Schedule Your First Task

```csharp
using Tempo;

var scheduler = new TaskScheduler();

scheduler.Every(TimeSpan.FromMinutes(5))
    .Do(() => Console.WriteLine("Hello from Tempo!"));

await scheduler.StartAsync();
```

> [!NOTE]
> Tempo requires .NET 9 or later. For .NET 8 support, use the `Tempo.LegacySupport` package.

## Next Steps

Once you have Tempo running, check out the [Configuration](xref:tempo.configuration) guide to customize retry policies and concurrency limits.

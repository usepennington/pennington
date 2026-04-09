---
title: "Installation"
description: "Detailed installation instructions"
order: 20
uid: "beacon.install"
---

## Package Manager

```shell
dotnet add package Beacon
```

## From Source

Clone the repository and build:

```shell
git clone https://github.com/example/beacon.git
cd beacon
dotnet build
```

## Verify Installation

```csharp
using Beacon;

Console.WriteLine($"Beacon version: {BeaconInfo.Version}");
```

Return to the [Getting Started](xref:beacon.getting-started) guide for next steps.

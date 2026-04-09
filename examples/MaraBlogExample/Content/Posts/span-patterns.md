---
title: "Span<T> Patterns That Actually Help"
date: 2026-03-22
author: "Mara Chen"
description: "Practical Span<T> patterns for zero-allocation string and buffer processing"
tags: ["performance", "dotnet"]
series: "Zero-Alloc .NET"
order: 20
uid: "mara.span-patterns"
---

`Span<T>` is the most important type for zero-allocation .NET code. But not every use of Span actually helps. Here are the patterns I reach for regularly.

## Slicing Instead of Substring

```csharp
// Allocates a new string
var name = fullPath.Substring(fullPath.LastIndexOf('/') + 1);

// Zero allocation
ReadOnlySpan<char> nameSpan = fullPath.AsSpan()[(fullPath.LastIndexOf('/') + 1)..];
```

## Stack-Allocated Buffers

```csharp
Span<byte> buffer = stackalloc byte[256];
int bytesRead = stream.Read(buffer);
ProcessChunk(buffer[..bytesRead]);
```

## String.Create for Formatting

```csharp
// Zero intermediate allocations
var result = string.Create(prefix.Length + 1 + id.Length, (prefix, id),
    static (span, state) =>
    {
        state.prefix.AsSpan().CopyTo(span);
        span[state.prefix.Length] = '-';
        state.id.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
    });
```

The key insight: Span shines when you can avoid materializing intermediate results. If you end up calling `.ToString()` at the end anyway, measure whether the Span path actually helps.

---
title: "5 Allocation Traps in Hot Paths"
date: 2026-03-15
author: "Mara Chen"
description: "Common heap allocations that sneak into performance-critical code"
tags: ["performance", "dotnet"]
series: "Zero-Alloc .NET"
order: 10
---

You profiled your hot path and it looks clean — no LINQ, no boxing, no string concatenation. But allocations are still showing up. Here are five traps I see repeatedly.

## 1. Params Arrays

Every `params` call allocates an array, even for zero arguments:

```csharp
// This allocates an empty array every call
logger.LogDebug("Processing item");

// Prefer the source-generated overload
[LoggerMessage(Level = LogLevel.Debug, Message = "Processing item")]
static partial void LogProcessing(ILogger logger);
```

## 2. Closure Captures

Lambdas that capture local variables allocate a display class:

```csharp
// Allocates a closure
items.Where(x => x.Id == targetId);

// Use a static lambda with state parameter when possible
items.Where(targetId, static (id, x) => x.Id == id);
```

## 3. Async State Machines

Even simple async methods allocate when they don't complete synchronously. Use `ValueTask` for hot paths.

## 4. String Interpolation in Logs

```csharp
// Allocates the interpolated string even if Debug is disabled
_logger.LogDebug($"Processed {count} items");

// Use structured logging instead
_logger.LogDebug("Processed {Count} items", count);
```

## 5. LINQ on Small Collections

For collections under ~10 items, a `foreach` loop with manual filtering is faster and allocation-free.

Next up: [Span&lt;T&gt; patterns](xref:mara.span-patterns) that actually help.

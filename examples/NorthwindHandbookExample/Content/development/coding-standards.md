---
title: "Coding Standards"
description: "C# coding conventions at Northwind"
order: 10
section: "Development"
---

## Naming Conventions

- Use `PascalCase` for public members and types
- Use `camelCase` for local variables and parameters
- Prefix interfaces with `I`
- Use descriptive names that reveal intent

## Example

```csharp
public interface IOrderProcessor
{
    Task<OrderResult> ProcessAsync(Order order, CancellationToken ct);
}

public class OrderProcessor : IOrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(ILogger<OrderProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<OrderResult> ProcessAsync(Order order, CancellationToken ct)
    {
        _logger.LogInformation("Processing order {OrderId}", order.Id);
        // Process the order...
        return new OrderResult(order.Id, Status.Completed);
    }
}
```

## Code Review Checklist

- All public APIs have XML doc comments
- No `TODO` comments in production code
- Unit tests cover happy path and key error paths

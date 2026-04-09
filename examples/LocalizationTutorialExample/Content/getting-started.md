---
title: "Getting Started"
description: "Learn how to get started"
order: 10
---

## Installation

Follow these steps to get started:

1. Install the package
2. Configure your project
3. Run the application

## Quick Example

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMyService();
var app = builder.Build();
app.Run();
```

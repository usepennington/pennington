---
title: "Roslyn Integration Demo"
description: "Demonstrating code highlighting and documentation with Roslyn integration"
date: 2025-01-15
tags:
  - roslyn
  - code
  - documentation
isDraft: false
---

This page demonstrates how Pennington integrates with Roslyn to provide enhanced code highlighting and documentation features.

## Code Highlighting

When Roslyn is connected, code blocks get enhanced syntax highlighting:

```csharp
string json = $$"""{"name": "John", "quote": "He said \"Hello!\"", "value": {{DateTime.Now.Year}}}""";
string pattern = """["\$]+""";
var (isValid, _) = ValidateJson(json);
using var stream = new MemoryStream();
Collection<string> items = ["item1", "item2"];
```

## XML Documentation ID Syntax

You can reference specific classes, methods, and properties from your solution using the `xmldocid` syntax:

### Referencing Classes

Reference the main content engine options class:

```csharp:xmldocid
T:Pennington.Infrastructure.PenningtonOptions
```

### Referencing Methods

Reference a specific method from the content service:

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})
```
### Excluding Declarations Syntax

You can also exclude declarations from the documentation by using the `xmldocid` syntax with the `bodyonly` keyword:

```csharp:xmldocid,bodyonly
M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})
```

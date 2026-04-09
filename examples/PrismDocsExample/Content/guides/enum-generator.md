---
title: "The Enum Generator"
description: "How the enum source generator works"
order: 10
uid: "prism.enum-generator"
---

## Overview

The Prism enum generator creates extension methods for your enums at compile time.

## Full Class

Here is the complete `EnumGenerator` class:

```csharp
// This would use :xmldocid T:Prism.V2.Generators.EnumGenerator in a real Roslyn setup
public class EnumGenerator
{
    private readonly List<string> _diagnostics = [];

    public void Initialize(GeneratorContext context)
    {
        context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
        _diagnostics.Clear();
    }

    public void Execute(GeneratorContext context)
    {
        if (context.SyntaxReceiver is not EnumSyntaxReceiver receiver)
            return;

        foreach (var enumDecl in receiver.Candidates)
        {
            var source = GenerateEnumHelper(enumDecl, context);
            context.AddSource($"{enumDecl.Name}_Extensions.g.cs", source);
        }
    }
}
```

## Execute Method Body

The core execution logic:

```csharp
// This would use :xmldocid,bodyonly M:Prism.V2.Generators.EnumGenerator.Execute in a real Roslyn setup
if (context.SyntaxReceiver is not EnumSyntaxReceiver receiver)
    return;

foreach (var enumDecl in receiver.Candidates)
{
    var source = GenerateEnumHelper(enumDecl, context);
    context.AddSource($"{enumDecl.Name}_Extensions.g.cs", source);
}
```

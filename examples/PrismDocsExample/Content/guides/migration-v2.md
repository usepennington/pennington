---
title: "Migrating to Prism v2"
description: "Differences between v1 and v2 of the enum generator"
order: 20
uid: "prism.migration"
---

## What Changed

Prism v2 introduces a completely new generator architecture based on syntax receivers instead of runtime reflection.

## Initialize Method

The v1 `Initialize` method was a no-op. V2 registers a syntax receiver:

```csharp
// V1 - no initialization
public void Initialize(object context)
{
    // V1 had no syntax receiver registration
}

// V2 - registers syntax receiver
public void Initialize(GeneratorContext context)
{
    context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
    _diagnostics.Clear();
}
```

## Execute Method

The execution strategy changed from runtime reflection to compile-time analysis:

```csharp
// V1 - runtime reflection
public void Execute(object context)
{
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    foreach (var assembly in assemblies)
    {
        var enumTypes = assembly.GetTypes().Where(t => t.IsEnum);
        foreach (var enumType in enumTypes)
        {
            GenerateHelper(enumType);
        }
    }
}

// V2 - compile-time syntax analysis
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
```

## Migration Steps

1. Update the Prism NuGet package to v2
2. Replace `object` parameters with `GeneratorContext`
3. Register syntax receivers in `Initialize`
4. Update `Execute` to use `SyntaxReceiver` instead of reflection

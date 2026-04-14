---
title: Code annotations
description: Trailing `[!code …]` directives for highlights, diffs, focus, and errors.
tags: [authoring, code]
sectionLabel: authoring
order: 70
uid: kitchen-sink.main.code-annotations
---

# Code annotations

Pennington post-processes highlighted code blocks, pulling `[!code …]`
directives out of trailing comments and applying them as classes on
each `.line` span.

## Highlight a single line

```csharp
public int Add(int a, int b)
{
    return a + b; // [!code highlight]
}
```

## Add / remove lines (diff)

```csharp
public int Multiply(int a, int b) // [!code ++]
{
    return a * b; // [!code ++]
}
public int OldWay(int a, int b) // [!code --]
{
    return a + b; // [!code --]
}
```

## Focus one line

```csharp
var config = new Config(); // [!code focus]
config.Apply();
config.Save();
```

## Error and warning annotations

```csharp
var path = null; // [!code error]
var length = path.Length; // [!code warning]
```

Each directive applies a class to the containing `.line` span so the
stylesheet can paint it — `highlight`, `diff-add`, `diff-remove`,
`focused`, `error`, `warning`.

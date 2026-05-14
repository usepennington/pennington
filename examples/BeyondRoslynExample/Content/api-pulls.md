---
title: API pulls
description: Live xmldocid fences pulling real source from the Sample library.
order: 20
---

# API pulls

Each fenced block below resolves an XmlDocId against the inner slnx that
`AddPenningtonRoslyn` loaded. When the Sample library source changes on disk,
hot reload re-reads it and the next request serves the new snippet.

## Whole type — `T:...`

Embed the entire `Calculator` class. Fence language is `csharp:xmldocid`;
the fence body is a single line holding the XmlDocId.

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Calculator
```

## Single method — `M:...`

One fence, one method. XmlDocIds for methods include parameter types.

```csharp:xmldocid
M:BeyondRoslynExample.Sample.Calculator.Add(System.Int32,System.Int32)
```

## Method body only — `,bodyonly`

Appending `,bodyonly` strips the declaration and returns just the block
contents (or expression-body expression).

```csharp:xmldocid,bodyonly
M:BeyondRoslynExample.Sample.Calculator.Multiply(System.Int32,System.Int32)
```

## A second type

The `Greeter` class lives in the same Sample library.

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Greeter
```

## Multiple symbols in one fence

List multiple XmlDocIds on separate lines inside one fence — they're
concatenated in the rendered output.

```csharp:xmldocid
M:BeyondRoslynExample.Sample.Greeter.Greet(System.String)
M:BeyondRoslynExample.Sample.Calculator.Mean(System.Collections.Generic.IReadOnlyList{System.Int32})
```

## Diff two symbols — `:xmldocid-diff`

The `:xmldocid-diff` fence takes exactly two XmlDocIds and renders the
unified diff between their bodies. Use it in explanation-quadrant pages to
compare a "before" and "after" implementation side by side without keeping
two prose snippets in sync. Here we diff `Add` against `Multiply` — both
are two-parameter `int` methods, so the diff narrows to the operator and
parameter names. Stack the same `,bodyonly` suffix to strip the
declarations when the change you want to surface is purely inside the
method body.

```csharp:xmldocid-diff,bodyonly
M:BeyondRoslynExample.Sample.Calculator.Add(System.Int32,System.Int32)
M:BeyondRoslynExample.Sample.Calculator.Multiply(System.Int32,System.Int32)
```


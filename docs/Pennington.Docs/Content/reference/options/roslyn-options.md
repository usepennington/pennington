---
title: "RoslynOptions"
description: "Configuration passed to AddPenningtonRoslyn — solution path and an optional project filter that scope Roslyn-backed symbol extraction and xmldocid preprocessing."
sectionLabel: "Configuration Options"
order: 401090
tags: [options, roslyn, xmldocid]
uid: reference.options.roslyn-options
---

> **In this page.** `SolutionPath` (path to `.sln`/`.slnx`), the optional `ProjectFilter` with `IncludedProjects`/`ExcludedProjects`, and how `AddPenningtonRoslyn` wires the options into the symbol-extraction and `xmldocid` preprocessing services shipped in `Pennington.Roslyn`.
>
> **Not in this page.** Authoring `xmldocid` code fences — see the Roslyn tutorial and the code-annotations how-to.

## Summary

The options class supplied to `services.AddPenningtonRoslyn(Action<RoslynOptions>)` in the optional `Pennington.Roslyn` package. Declared in namespace `Pennington.Roslyn` at `src/Pennington.Roslyn/RoslynOptions.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.Roslyn.RoslynOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ProjectFilter` | `ProjectFilter?` | `null` | Optional filter restricting which projects inside the loaded solution participate in symbol extraction. |
| `SolutionPath` | `string?` | `null` | Path to a `.sln` or `.slnx` file; when null, only basic syntax highlighting is registered and `xmldocid` fences do not resolve. |

## `ProjectFilter`

```csharp:xmldocid
T:Pennington.Roslyn.ProjectFilter
```

Record that scopes the set of projects loaded from the solution; applied as include-then-exclude against each project's `Project.Name`.

| Name | Type | Default | Description |
|---|---|---|---|
| `ExcludedProjects` | `HashSet<string>?` | `null` | Project names to exclude after the include filter runs; null or empty means no exclusion. |
| `IncludedProjects` | `HashSet<string>?` | `null` | Project names to include; null or empty means every project in the solution is considered. |

## Services registered by `AddPenningtonRoslyn`

```csharp:xmldocid
M:Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Roslyn.RoslynOptions})
```

Registers the configured `RoslynOptions` as a singleton and always registers `SyntaxHighlighter` and `ICodeHighlighter` → `RoslynHighlighter`. When `SolutionPath` is non-empty it additionally registers `ISolutionWorkspaceService` → `SolutionWorkspaceService`, `ISymbolExtractionService` → `SymbolExtractionService`, `ICodeBlockPreprocessor` → `RoslynCodeBlockPreprocessor`, `IXmlDocParser`, `IXmlDocHtmlRenderer`, and `IMemberEnumerator`.

## Example

```csharp:xmldocid,bodyonly
M:BeyondRoslynExample.Stage2.Run(System.String[])
```

Shape of a DocSite host that wires `AddPenningtonRoslyn` with a `SolutionPath` pointing at an inner `.slnx`.

## See also

- Tutorial: [Connect to a Roslyn solution for live API snippets](xref:tutorials.beyond-basics.connect-roslyn)
- How-to: [Annotate code blocks](xref:how-to.content-authoring.code-annotations)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)

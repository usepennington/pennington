---
title: "RoslynOptions"
description: "Configuration passed to AddPenningtonRoslyn — solution path and an optional project filter that scope Roslyn-backed symbol extraction and xmldocid preprocessing."
sectionLabel: "Configuration Options"
order: 401090
tags: [options, roslyn, xmldocid]
uid: reference.options.roslyn-options
---

The options class supplied to `services.AddPenningtonRoslyn(Action<RoslynOptions>)` in the optional `Pennington.Roslyn` package. Declared in namespace `Pennington.Roslyn` at `src/Pennington.Roslyn/RoslynOptions.cs`.

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ProjectFilter` | `ProjectFilter?` | `null` | Optional filter restricting which projects inside the loaded solution participate in symbol extraction. |
| `SolutionPath` | `string?` | `null` | Path to a `.sln` or `.slnx` file; when null, only basic syntax highlighting is registered and `xmldocid` fences do not resolve. |

## `ProjectFilter`

Record that scopes the set of projects loaded from the solution; applied as include-then-exclude against each project's `Project.Name`.

| Name | Type | Default | Description |
|---|---|---|---|
| `ExcludedProjects` | `HashSet<string>?` | `null` | Project names to exclude after the include filter runs; null or empty means no exclusion. |
| `IncludedProjects` | `HashSet<string>?` | `null` | Project names to include; null or empty means every project in the solution is considered. |

## Services registered by `AddPenningtonRoslyn`

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

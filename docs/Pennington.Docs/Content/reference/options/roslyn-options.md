---
title: "RoslynOptions"
description: "SolutionPath and the optional ProjectFilter — included/excluded projects — for AddPenningtonRoslyn."
section: "options"
order: 90
tags: []
uid: reference.options.roslyn-options
isDraft: false
search: false
llms: false
---

> **In this page.** `SolutionPath` (path to `.sln` / `.slnx`), the optional `ProjectFilter` with `IncludedProjects` / `ExcludedProjects`, and how `AddPenningtonRoslyn` wires the options into the symbol-extraction and `xmldocid` preprocessing services shipped in `Pennington.Roslyn`.
>
> **Not in this page.** Authoring `xmldocid` code fences (see the Roslyn tutorial) or writing a custom `ICodeBlockPreprocessor` (see the Extensibility how-to).

## Summary

The options class that configures Pennington's optional Roslyn integration — solution loading and project filtering.
Namespace `Pennington.Roslyn`; lives in `src/Pennington.Roslyn/RoslynOptions.cs` and is consumed by `AddPenningtonRoslyn` in `RoslynExtensions`.

## Declaration

```csharp:xmldocid
T:Pennington.Roslyn.RoslynOptions
```

## Properties

<ApiMemberTable XmlDocId="T:Pennington.Roslyn.RoslynOptions" Kind="Properties" />

## Supporting types

### `ProjectFilter`

```csharp:xmldocid
T:Pennington.Roslyn.ProjectFilter
```

A filter applied after solution load. Exactly one of the two sets may be populated, neither, or both — the semantics match `HashSet`-based include/exclude filtering.

<ApiMemberTable XmlDocId="T:Pennington.Roslyn.ProjectFilter" Kind="Properties" />

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options) — core options that `RoslynOptions` complements.
- Related reference: [Highlighting interfaces](/reference/extension-points/highlighting) — where `RoslynCodeBlockPreprocessor` slots in.
- How-to: [Add a custom code-block preprocessor](/how-to/extensibility/code-block-preprocessor) — implementing an `ICodeBlockPreprocessor` alongside or instead of `RoslynCodeBlockPreprocessor`.
- Tutorial: [Connect to a Roslyn solution for live API snippets](/tutorials/beyond-basics/connect-roslyn).

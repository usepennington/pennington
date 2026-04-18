---
title: "Build report fields"
description: "Field-by-field catalog of BuildReport, BuildDiagnostic, and BrokenLink — the record OutputGenerationService returns after a static build."
uid: reference.diagnostics.build-report
order: 407010
sectionLabel: Diagnostics
tags: [diagnostics, build, reference]
---

## `BuildReport`

<ApiSummary XmlDocId="T:Pennington.Generation.BuildReport" />

<ApiMemberTable XmlDocId="T:Pennington.Generation.BuildReport" Kind="All" />

## `BuildDiagnostic`

<ApiSummary XmlDocId="T:Pennington.Generation.BuildDiagnostic" />

<ApiMemberTable XmlDocId="T:Pennington.Generation.BuildDiagnostic" />

## `BrokenLink`

<ApiSummary XmlDocId="T:Pennington.Generation.BrokenLink" />

<ApiMemberTable XmlDocId="T:Pennington.Generation.BrokenLink" />

## `DiagnosticSeverity`

<ApiSummary XmlDocId="T:Pennington.Diagnostics.DiagnosticSeverity" />

<ApiMemberTable XmlDocId="T:Pennington.Diagnostics.DiagnosticSeverity" Kind="Fields" />

## Example

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

## See also

- Related reference: [Request-scoped diagnostics](xref:reference.diagnostics.request-context)
- Related reference: [CLI and build arguments](xref:reference.host.cli)
- How-to: [Build a static site](xref:how-to.deployment.static-build)

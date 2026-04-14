---
title: Beyond Roslyn
description: How this tutorial pulls live source into rendered docs.
order: 10
---

# Beyond Roslyn

This example backs tutorial §1.4.20 of the Pennington docs. It points
`Pennington.Roslyn` at a sibling library (`BeyondRoslynExample.Sample`) via
the inner `BeyondRoslynExample.slnx` and then embeds symbols from that
library directly into markdown pages with `csharp:xmldocid` fences.

See the [API pulls page](./api-pulls) for the fences in action.

## What's wired

- `AddDocSite(...)` — the same DocSite host used in tutorials 1.2.*
- `AddPenningtonRoslyn(options => options.SolutionPath = "BeyondRoslynExample.slnx")`
  — turns on `:xmldocid` / `:xmldocid,bodyonly` / `:path` code-fence modifiers
- `BeyondRoslynExample.slnx` — inner slnx that registers only the Sample
  library, scoping the MSBuild workspace to exactly the source we want to
  fence into docs

---
title: Pennington is on NuGet
description: The packages are public — full NuGet metadata, SourceLink and symbol packages, and a CI pipeline that publishes on every tagged release.
author: Phil Scott
date: 2026-04-09
isDraft: false
tags:
  - nuget
  - packaging
  - ci
---

Pennington is on NuGet. Until now, using it meant cloning the repo; now it's a
`dotnet add package` away.

## Install what you need

Pennington ships as a set of focused packages, so you reference only the parts
your site uses:

```bash
dotnet add package Pennington
dotnet add package Pennington.DocSite
```

`Pennington` is the core engine. `Pennington.DocSite` and `Pennington.BlogSite`
are the site templates, `Pennington.UI` carries the Razor components, and
`Pennington.MonorailCss` handles styling. Reference the ones you need and skip
the rest.

## SourceLink and symbol packages

Every package ships with SourceLink and a `.snupkg` symbol package. The point of
that shows up when something goes wrong: set a breakpoint, step into a
Pennington method, and your debugger pulls the exact source for the version
you're running, straight from the repo — no decompiler, no version-mismatched
source. The metadata is filled in too — icon, README, license, project URL,
tags — so the packages read properly on nuget.org.

## Released by pipeline

Publishing is a CI job. A two-stage pipeline builds, tests, and packs on pushes and pull
requests (docs-only changes are skipped), then publishes to NuGet on pushes to main and on
tagged releases. It uses OIDC
trusted publishing, so NuGet authenticates the pipeline directly — there's no
long-lived API key sitting in a secrets store.

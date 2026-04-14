---
title: Install Pennington
description: Add the Pennington package to a new or existing ASP.NET project.
sectionLabel: Getting Started
order: 10
---

# Install Pennington

The first thing every new Pennington site needs is the package itself. This
page walks through creating an empty web project and adding the Pennington
NuGet reference so the next pages can wire up content discovery and
rendering.

## Create the host project

Start from an empty ASP.NET project — Pennington plugs into the standard
`WebApplication` host, so there's nothing special to template.

```bash
dotnet new web -n MyDocs
cd MyDocs
```

## Add the package

```bash
dotnet add package Pennington
```

The next guide — *Create your first project* — picks up from this point and
wires `AddPennington` / `UsePennington` into `Program.cs`.

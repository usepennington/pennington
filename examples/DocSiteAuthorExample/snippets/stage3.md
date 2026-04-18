---
title: Authoring a doc page
description: Populate DocSiteFrontMatter, add an alert, and group code samples into tabs.
tags:
  - authoring
  - front-matter
  - markdown
sectionLabel: Guides
order: 20
---

# Authoring a doc page

## Callouts

> [!NOTE]
> Alerts render with a coloured left border and an icon matching the kind.

## Tabbed code groups

```bash tabs=true title="dotnet CLI"
dotnet add package Pennington
```

```powershell tabs=true title="PowerShell"
Install-Package Pennington
```

```xml tabs=true title="csproj"
<PackageReference Include="Pennington" Version="*" />
```

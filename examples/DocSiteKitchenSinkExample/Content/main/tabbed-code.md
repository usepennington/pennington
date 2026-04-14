---
title: Tabbed code groups
description: Adjacent code fences collapse into an ARIA tablist.
tags: [authoring, code]
section: authoring
order: 60
uid: kitchen-sink.main.tabbed-code
---

# Tabbed code groups

Mark two or more adjacent fenced code blocks with `tabs=true title="…"`
in the fence's info string and Pennington collapses them into a single
tablist. The first tab is active by default.

```bash tabs=true title="bash"
dotnet add package Pennington
```

```powershell tabs=true title="PowerShell"
Install-Package Pennington
```

```xml tabs=true title="csproj"
<PackageReference Include="Pennington" Version="1.0.0" />
```

Click any tab above to switch the visible panel. If you need the panels
to remember their last selection across pages, wire a `storageKey`
attribute onto each fence that all pages sharing state agree on.

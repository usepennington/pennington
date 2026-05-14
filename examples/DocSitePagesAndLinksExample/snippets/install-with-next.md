---
title: Install Pennington
description: Add the Pennington DocSite package and wire AddDocSite + UseDocSite into a fresh ASP.NET host.
sectionLabel: Guides
order: 20
---

# Install Pennington

Install Pennington into an ASP.NET project with one NuGet package and three lines of DI wiring.

## 1. Add the package

```text
dotnet add package Pennington.DocSite
```

## 2. Wire DocSite in `Program.cs`

Three calls — register the services, mount the middleware, hand control to the host:

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions { /* ... */ });
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

The host is now ready for content. Drop markdown files under `Content/guides/` and they appear in the sidebar on the next request.

## Next

Pick a site title and decide on areas in [Configure the site](./configure).

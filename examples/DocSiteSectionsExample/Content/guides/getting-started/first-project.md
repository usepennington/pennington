---
title: Create your first project
description: Wire AddPennington and UsePennington into Program.cs and drop in a markdown page.
section: Getting Started
order: 20
---

# Create your first project

With the package installed, the smallest useful site is two lines of
registration and one markdown file on disk.

## Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPennington();

var app = builder.Build();
app.UsePennington();
await app.RunOrBuildAsync(args);
```

## Drop in a page

Create `Content/index.md` with a `title:` front-matter key and a body. Run
`dotnet run` and the page is served from `/` with hot reload on file change.

The next guide covers the handful of options you will reach for first.

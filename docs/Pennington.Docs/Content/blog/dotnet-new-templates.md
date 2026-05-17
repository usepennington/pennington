---
title: Start a new site with dotnet new
description: Three project templates — pennington, pennington-docs, and pennington-blog — get a site running with a single command.
author: Phil Scott
date: 2026-04-18
isDraft: false
tags:
  - project-templates
  - tooling
---

Starting a new site used to be a short checklist: create a project, add the
package references, wire up `Program.cs`, drop in a layout. Now it's one command.

## Three templates, installed once

`Pennington.Templates` is a `dotnet new` template package. Install it once:

```bash
dotnet new install Pennington.Templates
```

Then create a project from any of the three templates:

```bash
dotnet new pennington-docs -o my-docs
```

- `pennington` — the bare content engine, for when you want to assemble the site
  yourself.
- `pennington-docs` — a full [DocSite](xref:tutorials.docsite.scaffold): sidebar
  navigation, a table of contents, and a starter page tree.
- `pennington-blog` — a full [BlogSite](xref:tutorials.blogsite.scaffold): home
  page, archive, tag pages, and an RSS feed.

Each one produces a project that runs immediately. `cd my-docs`, `dotnet run`,
and the site is in your browser with live reload.

## Why a template, not a checklist

A setup checklist is a place to make small mistakes: a missing package
reference, a `Program.cs` line in the wrong order, a layout file that doesn't
quite match. Starting from a template that already works means your first commit
is content instead of plumbing — and it's the starting point the tutorials
assume, since each one begins with `dotnet new`.

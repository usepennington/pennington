---
title: Getting started with Pennington
description: A walkthrough of the first hour using Pennington — scaffolding a site, wiring a front-matter block, and watching the nav assemble itself.
date: 2024-01-15
author: Jamie Rivers
tags:
  - pennington
  - dotnet
  - getting-started
series: Pennington Field Notes
repository: https://github.com/example/pennington-field-notes
sectionLabel: field-notes
---

Pennington is a content engine for .NET 11 that treats a folder of markdown
files as a first-class application. This post walks through the first hour of
using it — from `dotnet new web` to the moment a second markdown file appears
in the sidebar without touching any code.

## The minimum viable host

Three lines in `Program.cs` — `AddPennington`, `UsePennington`, and
`RunOrBuildAsync(args)` — are enough to turn any `WebApplication` into a
Pennington host. Drop a markdown file under `Content/` and navigate to its
path. That's the whole loop.

## Where the convenience stops

The bare host doesn't give you a layout. For a documentation site, reach for
`AddDocSite` instead — it wires a full Razor component chrome. For a personal
blog, `AddBlogSite` gives you the home page, an archive, tag pages, and an
RSS feed in one call. The rest of this series walks through the BlogSite
template surface piece by piece.

---
title: Hot reload over a thousand markdown files
description: What Pennington's file-watch layer does — and doesn't do — when the content tree grows past the toy size.
date: 2025-03-30
author: Jamie Rivers
tags:
  - pennington
  - file-watcher
  - performance
series: Pennington Field Notes
sectionLabel: field-notes
---

The dev-loop assumption is that you save a file and the next request sees
the change. Pennington's `IFileWatcher` listens on `Content/`, invalidates
the affected `AddFileWatched<T>` services, and lets them rebuild lazily.

## Where it starts to drag

`Directory.EnumerateFiles` on a thousand-file tree is fast on warm cache
and noticeably slower on cold. The first save after a long pause feels
heavier than subsequent ones. If your tree grows past five-digit page
counts, watch the diagnostics overlay for per-service rebuild costs and
consider splitting into multiple `AddMarkdownContent` registrations.

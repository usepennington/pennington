---
title: Watching files, rebuilding services, and the case for AsyncLazy
description: How Pennington keeps content-derived services fresh in dev without rebuilding the whole host on every keystroke.
date: 2024-06-08
author: Jamie Rivers
tags:
  - pennington
  - file-watcher
  - architecture
series: Pennington Field Notes
sectionLabel: field-notes
---

Most content-derived services in Pennington — `NavigationBuilder`,
`SearchIndexService`, `BlogContentResolver` — register as `AddFileWatched<T>`.
When the file watcher fires, the dependency factory rebuilds the instance.
Consumers see the new state on their next resolve.

## Why AsyncLazy under the hood

Each rebuilt instance defers its expensive work to an `AsyncLazy<T>` so
the cost only lands on the first consumer that actually queries it. A round
of file edits that nobody navigates to never reparses anything.

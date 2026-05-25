---
title: A dev loop that keeps up
description: Edit a markdown file and the browser refreshes itself — WebSocket live reload, debounced file watching, and .cs hot reload for embedded code samples.
author: Phil Scott
date: 2026-04-15
isDraft: false
tags:
  - hot-reload
  - dev-experience
---

Save a file, see the change — the shorter that loop, the more you actually
iterate instead of batching edits and checking later. Pennington's dev server
keeps it short: save, and the browser updates itself.

## Edit content, the browser follows

The dev server watches your content directory. Save a markdown file and it
pushes a refresh to the browser over a WebSocket — no manual reload. A 300ms
debounce coalesces rapid saves into a single reload, so a formatter-on-save
doesn't trigger a storm of refreshes. It works under a plain `dotnet run`, not
only `dotnet watch`, with a reconnect guard so the browser recovers cleanly
after a server restart.

## .cs edits, too

Code samples come from source files via `:symbol` fences, so a sample reflects the
current source every time the docs render. Edit the referenced `.cs` and the next
render re-reads it, so the embedded sample reflects what you just typed — no copy
to keep in sync. The watcher filters out `obj/`, `bin/`, and generated files, so a
rebuild burst doesn't thrash anything. The [hot reload
explanation](xref:explanation.dev-experience.hot-reload) covers how the watcher
and the WebSocket fit together.

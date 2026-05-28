---
title: A dashboard for your terminal
description: Pennington.Tui is a full-screen terminal dashboard that runs alongside the dev server — logs, requests, file changes, content inventory, and per-page diagnostics in one view.
author: Phil Scott
date: 2026-04-17
isDraft: false
tags:
  - tui
  - dev-experience
  - diagnostics
---

Run a content site locally and the feedback you get is a console log — a single
scrolling column where a request, a file change, and a stack trace all look the
same, and the line you wanted scrolled off a while ago. `Pennington.Tui` is an
alternative view.

## One screen, not one stream

`Pennington.Tui` is a full-screen terminal dashboard that runs alongside the dev
server. Instead of a flat log, you get panes: live `ILogger` output, HTTP
requests as they arrive, and file changes as you save — each in its own place,
so a 404 doesn't bury itself under a watcher event.

Two tabs go a bit further:

- **Content inventory** — every page the engine discovered, in one list. A quick
  way to check whether it picked up the file you just added.
- **Diagnostics** — the per-page warnings captured from real requests: a broken
  link, an unresolved `xref:`, a code fence in an unknown language, each tied to
  the page that produced it.

The diagnostics tab collects warnings on demand from real requests as you
navigate the dev site — the same data the browser overlay surfaces, in your
terminal. The [request-scoped diagnostics
reference](xref:reference.diagnostics.request-context) covers where it comes
from.

The dashboard is dev-only: under `dotnet run -- build` it does nothing, so CI
logs stay plain.

---
title: A build report you can read, and a dev overlay that shows it
description: Every build now ends with a real report — broken links, unresolved references, diagnostics — and the dev server surfaces the same warnings live in the browser.
author: Phil Scott
date: 2026-04-06
isDraft: false
tags:
  - diagnostics
  - build-report
  - dev-overlay
---

Pennington used to scatter its findings — a broken link, an `xref:` that points
nowhere, a content diagnostic — across logger output, where they were easy to
miss. Two changes make them easier to catch: the static build now ends with a
report, and the dev server shows the same findings while you write.

## A report at the end of every build

Run `dotnet run -- build` and the last thing you see is a structured summary:

```text
Build report
  Pages generated      142
  Broken links           2
    /guide/setup  ->  /guide/instalation  (no such route)
    /api/index    ->  /api/legacy         (no such route)
  Unresolved xrefs       1
    xref:reference.api.old-name  (guide/migration.md)
  Diagnostics            0

Build failed - 3 errors
```

When the report finds errors, the process exits with code 1, so a broken link
fails a CI pipeline instead of slipping through. The build verifies internal
links against the real route table and resolves every `xref:` against the
[cross-reference](xref:explanation.routing.cross-references) UID index as it
goes.

## The same warnings, live in the browser

Catching problems at build time is useful; catching them while you're still
typing is better. In dev mode, Pennington collects diagnostics per request — so
every warning is tied to the page that produced it — and shows them in a
floating overlay in the browser.

Edit a page, introduce a bad link, and the overlay updates on the next reload.
The diagnostics also travel in HTTP headers and the SPA navigation payload, so
they survive client-side page transitions. The [request-scoped diagnostics
reference](xref:reference.diagnostics.request-context) covers how that works.

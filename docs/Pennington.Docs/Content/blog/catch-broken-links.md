---
title: Catch broken links before your readers do
description: A new build-auditor pipeline surfaces broken links, unresolved references, and content overlap on two surfaces at once — the build report and the live dev overlay.
author: Phil Scott
date: 2026-05-01
isDraft: false
tags:
  - diagnostics
  - link-checking
  - build-report
---

A broken link works in every check you run while writing, then fails for a
reader weeks later. Pennington already caught broken links at build time; now it
catches them continuously and shows you where.

## One audit pipeline, two surfaces

The new piece is a build auditor. An auditor is a check that runs against
your content: a broken-link verifier, a content-overlap detector, a translation
completeness pass. Pennington runs them through one `AuditRunner` that primes a
cache at startup and re-runs on every file change.

The findings go two places from that single source:

- **The build report** — every finding collected, with a non-zero exit code if
  anything is an error. This is the CI gate.
- **The dev overlay** — the same finding, attached to the page that contains it,
  live in the browser as you write.

So a broken link doesn't wait for a build. Introduce one, save, and the overlay
on that page points it out. The diagnostic code behind the overlay is in the
[request-scoped diagnostics
reference](xref:reference.diagnostics.request-context).

## Two kinds of check

Auditors come in two kinds. Structural auditors — content overlap, translation
coverage, unresolved [cross-references](xref:explanation.routing.cross-references)
— work from the content model. Rendered auditors need the finished HTML:
broken-link verification has to follow links against real routes, so it runs
after the pipeline and dispatches each route through the live renderer.

That split keeps the system open. A new check is a new auditor, and it picks up
both surfaces — the report and the overlay — without extra wiring. Broken links
and content overlap are the first two; an accessibility pass is a likely next
one.

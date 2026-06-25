---
title: Pennington 0.1.0, in beta
description: The first beta. The public API froze, two hardening passes landed, and a handful of smaller features shipped: 404 pages, folder ordering, scheduled posts, and more.
author: Phil Scott
date: 2026-06-15
isDraft: false
tags:
  - release
  - beta
---

Pennington is at `0.1.0`, tagged beta. The honest meaning of that: it's usable,
the shape is settled, and the public API is meant to stop moving under you. It's
not a 1.0 stability promise: it still rides preview C# language features and a
few pre-1.0 dependencies.

## The API froze first

Reaching beta meant one last round of breaking changes, on purpose, so they'd
land before the line rather than after it. Incidental public types that nobody
binds to went internal, shrinking the surface to what you actually use. And the
satellite registration methods dropped their brand prefix (`AddTreeSitter`,
`AddTranslationAudit`, `UseLiveReload`), leaving `AddPennington`, `UsePennington`,
and `RunOrBuildAsync` as the only branded entry points. The [host extensions
reference](xref:reference.host.extensions) lists the current names. Two passes
followed: eight verified correctness fixes, and a documentation-accuracy sweep
against the source.

## Smaller things that shipped

Alongside the freeze, a handful of features worth calling out:

- A `404.md` at your content root becomes the site's not-found page, rendered to
  `output/404.html` and kept out of navigation and search. See [provide a 404
  page](xref:how-to.pages.not-found).
- A `_meta.yml` sidecar sets a folder's title and sort order without renaming
  anything, so ordering stays folder-local. See the [folder sidecar
  reference](xref:reference.front-matter.folder-sidecar).
- A post with a future `date:` stays out of the build until the date passes, then
  the next build publishes it. The archive paginates, too. See [drafts, tags, and
  ordering](xref:how-to.pages.drafts-tags-ordering).
- Running two versions of a docs set side by side is now a documented pattern
  with a worked example rather than something to reverse-engineer. See [version a
  DocSite](xref:how-to.versioning.docsite). It builds on `Areas`; it isn't a
  separate feature.
- A new [head subsystem](xref:explanation.core.head-subsystem) replaced the
  several mechanisms that used to fight over the `<head>`. Favicons and an
  experimental AT Protocol "Standard Site" integration plug into it.

Right after the tag, this docs site itself moved to Cloudflare Pages, with a
preview deploy on every pull request.

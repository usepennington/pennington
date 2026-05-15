---
title: Front matter that catches your typos
description: Misspell a front-matter key and the build now tells you — strict-mode parsing, an unknown-key diagnostic, and a warning when a code fence names a language Pennington doesn't know.
author: Phil Scott
date: 2026-05-07
isDraft: false
tags:
  - announcements
  - diagnostics
---

Type `tite:` instead of `title:` and YAML accepts it without complaint — a valid
key, but not one Pennington reads. The page builds, the title falls back to a
default, and you notice when the page looks wrong. Pennington now flags that
earlier.

## Unknown keys get flagged

Every front-matter parse pre-scans for keys it doesn't recognize. A misspelled or
stray key raises a diagnostic with the key, the file, and the line:

```text
content/guide/setup.md:3  unknown front-matter key 'tite'
```

Under `serve` that's a warning in the dev overlay. Under `build`, strict mode is
on by default and the warning becomes a build failure — [dev mode and build
mode](xref:explanation.core.dev-vs-build) stay permissive while you write and
strict before you ship, so a typo can't reach a published site. The keys
Pennington recognizes are listed in the [front matter
reference](xref:reference.front-matter.keys).

## Code fences, too

The same check reaches into code blocks. Tag a fence with a language Pennington
can't highlight — a typo like `cshrap`, or something genuinely unsupported — and
it emits an Info diagnostic. It fires once per unknown language, so a site with
fifty `cshrap` fences gives you one note, not fifty.

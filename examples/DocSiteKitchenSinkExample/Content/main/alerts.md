---
title: Alerts and callouts
description: GitHub-style alert blocks with five built-in flavours.
tags: [authoring, alerts]
sectionLabel: authoring
order: 80
uid: kitchen-sink.main.alerts
---

# Alerts and callouts

Pennington recognises the GitHub-flavoured alert syntax: a blockquote
whose first non-whitespace line is `[!KIND]` becomes a styled alert.

> [!NOTE]
> Notes carry side information the reader should glance at before
> continuing — a small fact, a link to related material, a caveat about
> versioning.

> [!TIP]
> Tips point at a smart default, a keyboard shortcut, or a pattern that
> keeps the common case simple.

> [!IMPORTANT]
> Important callouts flag content that is load-bearing — reading past
> one you disagreed with will leave you with a broken mental model.

> [!WARNING]
> Warnings surface something that will produce an incorrect result if
> ignored. They signal a problem the reader can still avoid.

> [!CAUTION]
> Cautions surface something that will go badly and cannot be undone —
> destructive CLI flags, wire-format-breaking changes, security
> footguns.

Five kinds, five colour schemes. Only the first line of the blockquote
is parsed for the directive; the rest is rendered as normal markdown.

---
title: Markdown extensions
description: The Markdig extensions Pennington ships with — alerts, tabbed code, highlighting.
sectionLabel: Extensions
order: 30
---

# Markdown extensions

Pennington configures Markdig with a curated set of extensions that light
up the authoring syntax the tutorials lean on.

## What ships in the box

- **Alerts** — GitHub-flavoured block quotes (`> [!NOTE]`, `TIP`,
  `IMPORTANT`, `WARNING`, `CAUTION`).
- **Tabbed code groups** — two or more adjacent fenced blocks with
  `tabs=true title="…"`.
- **Syntax highlighting** — TextMate grammars and ANSI shell output.
- **Code annotations** — trailing-comment `[!code highlight]` markers.

Registering your own extension is covered in the *Hook into the response
pipeline* guide's companion how-to.

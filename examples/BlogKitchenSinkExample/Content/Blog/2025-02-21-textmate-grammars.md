---
title: Server-side code highlighting with TextMate grammars
description: Why Pennington runs TextMateSharp at build time instead of shipping a JavaScript highlighter to every reader.
date: 2025-02-21
author: Jamie Rivers
tags:
  - pennington
  - highlighting
  - performance
series: Pennington Field Notes
sectionLabel: field-notes
---

Pennington highlights code at render time using TextMateSharp — the same
grammar engine VS Code uses. The output is pre-decorated HTML with class
names a MonorailCSS layer styles.

## The cost trade

Server-side highlighting moves CPU from the reader's browser to your build
step. A 50-post blog with five code blocks per post adds a couple of
seconds to a full rebuild and zero milliseconds to the reader's first
paint. The reader trade is almost always the one to make.

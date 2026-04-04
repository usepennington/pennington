---
title: "TextPath Widget"
description: "Display file paths with intelligent truncation and styling"
date: 2025-08-05
tags: ["widgets", "path", "file-path", "truncation"]
section: "Console"
uid: "console-widget-text-path"
order: 5320
---

The TextPath widget displays file paths with smart truncation that preserves important parts (typically the beginning and end) while fitting within available console width. It's designed specifically for showing file system paths in a readable way even when space is limited.

**Key Topics Covered:**

* **Path display** - Creating TextPath widgets from file path strings
* **Intelligent truncation** - How TextPath decides which parts of the path to show/hide when space is limited
* **Stem and leaf** - Understanding how root and filename are preserved during truncation
* **Styling** - Applying colors to different path components (root, separator, filename, etc.)
* **Root display** - Options for showing or abbreviating root directories
* **Separator handling** - Cross-platform path separator rendering
* **Use cases** - File operation output, build logs, search results, directory listings

Examples show displaying compilation output with file paths, showing search results from multiple directories, listing files being processed by a tool, truncating very long paths elegantly, styling path components differently (e.g., highlighting filenames), and building file browsers in console applications. The guide discusses when to use TextPath versus plain string paths and how to handle edge cases like very short console widths.

---
title: "Best Practices for Console Applications"
description: "General guidance and recommended practices when using Spectre.Console"
date: 2025-08-05
tags: ["explanation", "best-practices", "guidance", "recommendations", "patterns"]
section: "Console"
uid: "console-best-practices"
order: 3070
---

General guidance and recommended practices when using Spectre.Console, distilled from the library authors' experience. This explanation includes:

* **Output best practices**: Test in multiple terminal environments, avoid hard-coding unicode characters or emoji without fallbacks, and consider users' terminal background colors (don't assume a black background; use the default 16 colors when possible for theming).
* **Live rendering best practices**: Use a single thread for rendering updates, do not run two live animations (e.g. Progress and Status) at once, and keep the UI responsive by doing heavy work on background threads but rendering on the main thread.
* **Prompting and input**: Suggest injecting an `IAnsiConsole` into business logic so that it can be mocked for testing, and avoiding calling `AnsiConsole` statically inside commands (as shown in the unit testing example).
  This section reads as a set of guidelines and rationales, helping developers avoid common pitfalls and write robust console apps.
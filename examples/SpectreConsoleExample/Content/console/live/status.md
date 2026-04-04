---
title: "Status Display"
description: "Show animated status indicators with spinners for ongoing operations"
date: 2025-08-05
tags: ["live", "status", "spinner", "loading"]
section: "Console"
uid: "console-live-status"
order: 6020
---

The Status display shows an animated spinner with a status message, perfect for operations where you want to indicate work is happening but don't have progress information. It provides visual feedback that the application is responsive during long-running tasks.

**Key Topics Covered:**

* **Creating status displays** - Using `AnsiConsole.Status()` to show status with a spinner
* **Status messages** - Setting and updating the displayed status text during operations
* **Spinner styles** - Choosing from various built-in spinner animations (dots, line, star, etc.)
* **Synchronous operations** - Wrapping synchronous long-running work with status display
* **Async operations** - Using status display with async/await patterns
* **Context actions** - Executing work within the status context while spinner animates
* **Auto-refresh rate** - Controlling how frequently the spinner updates
* **Styling** - Customizing spinner and text colors

Examples show displaying status during API calls, showing "thinking" indicators for AI operations, wrapping file system operations, indicating network activity, displaying compilation or build status, and creating responsive UIs for batch operations. The guide discusses when to use Status vs. Progress displays and best practices for status messaging that keeps users informed without overwhelming them.

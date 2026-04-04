---
title: "Advanced Console Control (Clear, Alternate Screen, Record/Export)"
description: "How to use Spectre.Console for advanced console operations"
date: 2025-08-05
tags: ["how-to", "advanced", "console-control", "clear", "alternate-screen", "export"]
section: "Console"
uid: "console-advanced-control"
order: 2220
---

How to use Spectre.Console for advanced console operations. This guide is a grab-bag of utilities:

* **Clearing the Console**: Using `AnsiConsole.Clear()` to clear the screen.
* **Alternate Screen Buffer**: Using `AnsiConsole.AlternateScreen` to switch to an alternate console buffer for full-screen like applications, and disposing it to return to the main screen (useful for temporary UI like text editors in console).
* **Recording and Exporting Output**: Using `AnsiConsole.Record()` to capture console output programmatically, then `AnsiConsole.ExportText()` or `ExportHtml()` to get the output as a string or HTML. This can be useful for logging or creating reports of console output.
* **Resetting Styles**: Using `AnsiConsole.ResetColors()` and `ResetDecoration()` to restore console to default state (useful after applying many style changes).
  This guide provides short examples for each operation, such as clearing the screen between steps of a demo, or exporting console output to an HTML file for viewing results in a browser. It helps developers take advantage of Spectre.Console's integration with the console's lower-level capabilities.
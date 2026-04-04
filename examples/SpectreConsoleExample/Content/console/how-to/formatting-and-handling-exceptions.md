---
title: "Formatting and Handling Exceptions"
description: "How to output exceptions in a readable, color-highlighted format"
date: 2025-08-05
tags: ["how-to", "exceptions", "formatting", "error-handling", "debugging"]
section: "Console"
uid: "console-exception-handling"
order: 2170
---

How to output exceptions in a readable, color-highlighted format. This guide covers using `AnsiConsole.WriteException` to render an `Exception` object with Spectre.Console's default exception style (which highlights stack trace, message, and inner exceptions in colors). It explains the options via `ExceptionFormats` (such as shortening paths, skipping method info, etc.) to tailor the output. The guide also touches on best practices for handling exceptions in console apps – for example, using `AnsiConsole.WriteException(ex)` inside catch blocks to ensure consistent formatting. An example shows a try/catch where an exception is intentionally thrown and caught to demonstrate the console output. This guide ensures developers can make debugging and error messages user-friendly, aligning with Spectre.Console's capabilities.
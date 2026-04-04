---
title: "Running Tasks with an Async Spinner"
description: "How to run asynchronous tasks with a spinner animation using Spectre.Console's async extensions"
date: 2025-08-05
tags: ["how-to", "async", "spinner", "tasks", "animation"]
section: "Console"
uid: "console-async-spinner"
order: 2190
---

How to run asynchronous tasks with a spinner animation using Spectre.Console's async extensions. This guide shows how to call the extension methods like `.Spinner()` on a `Task` or `Task<T>` to automatically display a spinner while the task runs. It covers customizing the spinner type (choosing one of the built-in spinner presets or defining a custom sequence) and styling the spinner (color, style). The guide also notes limitations: the inline spinner is not thread-safe to use alongside other interactive elements, so it should be used for standalone tasks. An example is provided where a long-running computation (`Task.Delay` or a data fetch) is run with a spinner indicator, making it easy to give feedback during async operations with minimal code.
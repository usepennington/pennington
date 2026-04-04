---
title: "Showing Progress Bars and Spinners"
description: "How to track and display progress of long-running tasks in the console"
date: 2025-08-05
tags: ["how-to", "progress", "spinners", "tasks", "status"]
section: "Console"
uid: "console-progress-bars"
order: 2140
---

How to track and display progress of long-running tasks in the console. This guide demonstrates using `AnsiConsole.Progress` to create a **Progress Bar** with multiple tasks, updating their completion percentage or status messages. It explains how to start the progress, update task progress in code, and finish gracefully, leveraging the fluent API to configure appearance (e.g. auto refresh rate, finished behavior). The guide also covers using the **Status** widget to show a single live spinner with a status message (for example, "Processing…" with an animated spinner). Configuration of spinners is discussed (choosing a spinner style from the built-in set, or customizing the spinner frames). Examples include a single long task (using `Status.Start`) and multiple parallel tasks (with `Progress().Start` and tasks). The guide notes that multiple live-updating elements should not run concurrently to avoid flicker.
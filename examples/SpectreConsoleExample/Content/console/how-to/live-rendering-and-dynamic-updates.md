---
title: "Live Rendering and Dynamic Updates"
description: "How to use Spectre.Console's live rendering features to continuously update console output"
date: 2025-08-05
tags: ["how-to", "live-rendering", "dynamic", "updates", "real-time"]
section: "Console"
uid: "console-live-rendering"
order: 2200
---

How to use Spectre.Console's live rendering features to continuously update console output. This guide focuses on the **LiveDisplay** mechanism (`AnsiConsole.Live`) which allows re-rendering a widget in-place as data changes. It shows how to wrap an `IRenderable` (like a Table or Chart) in a live context and periodically update its content within a loop. For example, a live table that adds new rows every second, or updating a progress chart in real-time. The guide provides an example of a ticking dashboard: perhaps CPU usage chart that updates, or a list of tasks that refresh statuses. It emphasizes best practices for live rendering (e.g. keep updates on a single thread, don't mix multiple live renderers simultaneously) to avoid conflicts. By following this, users can create dynamic console interfaces that update in real time.
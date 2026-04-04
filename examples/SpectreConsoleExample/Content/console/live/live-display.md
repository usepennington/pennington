---
title: "Live Display"
description: "Update and refresh any renderable content dynamically in real-time"
date: 2025-08-05
tags: ["live", "display", "update", "refresh"]
section: "Console"
uid: "console-live-display"
order: 6030
---

The Live Display system allows you to render any widget or content and then update it in place, creating dynamic console interfaces that refresh without scrolling. It's the foundation for building dashboards, real-time monitors, and interactive displays.

**Key Topics Covered:**

* **Creating live displays** - Using `AnsiConsole.Live()` to create an updateable rendering context
* **Starting display** - Initiating live rendering with `Start()` and initial content
* **Updating content** - Changing the displayed renderable with `UpdateTarget()` to refresh the display
* **Supported renderables** - Any widget (tables, panels, charts, etc.) can be used in live displays
* **Auto-refresh** - Controlling update frequency and animation smoothness
* **Overflow handling** - Managing content that exceeds console height
* **Combining with other widgets** - Building complex live dashboards with multiple components
* **Performance** - Efficiently updating large or complex renderables

Examples demonstrate creating live dashboards showing system metrics, building real-time log viewers, displaying updating tables as data changes, creating terminal-based monitoring tools, showing live test results during test execution, and building animated visualizations. The guide covers best practices for live rendering performance, when to use Live Display vs. Progress/Status, and handling terminal resizing during live display.

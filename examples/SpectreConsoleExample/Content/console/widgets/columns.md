---
title: "Columns Widget"
description: "Display content side-by-side in columns with automatic width distribution"
date: 2025-08-05
tags: ["widgets", "columns", "layout", "side-by-side"]
section: "Console"
uid: "console-widget-columns"
order: 5040
---

The Columns widget simplifies creating side-by-side layouts by automatically distributing available console width among multiple columns. It's perfect for showing related information in parallel or creating multi-column text layouts.

**Key Topics Covered:**

* **Basic usage** - Creating columns by passing an array or collection of renderables to the Columns constructor
* **Width distribution** - How Columns automatically calculates and distributes available width equally among columns
* **Column content** - Using any renderable (text, panels, tables, etc.) as column content
* **Padding and spacing** - Controlling the gap between columns
* **Expand behavior** - Making columns fill or fit content width
* **Alignment within columns** - Setting vertical and horizontal alignment for content within each column
* **Responsive behavior** - How columns adapt when console width changes

Examples show creating two-column layouts for comparing information (before/after, old/new), three-column layouts for displaying metrics or statistics, mixing different renderables in columns (table + panel + text), and building complex dashboard-style interfaces. The guide also discusses when to use Columns vs. Grid vs. manual layout composition.

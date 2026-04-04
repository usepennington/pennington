---
title: "Grid Widget"
description: "Arrange content in rows and columns without visible borders for flexible layouts"
date: 2025-08-05
tags: ["widgets", "grid", "layout", "columns"]
section: "Console"
uid: "console-widget-grid"
order: 5030
---

The Grid widget provides a flexible layout system for arranging content in rows and columns without the visual weight of table borders. It's ideal for creating custom layouts, forms, and structured presentations where borders would be distracting.

**Key Topics Covered:**

* **Grid structure** - Understanding rows and columns in Grid vs. Table (no borders, different performance characteristics)
* **Adding columns** - Using `AddColumn()` with width specifications (fixed, proportional, auto)
* **Adding rows** - Populating grid rows with `AddRow()`, passing renderables or markup strings for each cell
* **Column sizing** - Controlling column widths with fixed sizes, ratios, or content-based auto-sizing
* **Alignment** - Setting per-column alignment and per-cell content positioning
* **Expansion behavior** - Configuring whether the grid expands to fill available width
* **Use cases** - When to choose Grid over Table, Columns, or custom layout compositions

Examples demonstrate building form-style layouts (label: value pairs), multi-column text layouts similar to newspaper columns, dashboard grids showing multiple metrics, and responsive layouts that adapt to console width. The guide compares Grid with similar widgets and helps readers choose the right layout tool for their needs.

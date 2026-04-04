---
title: "Rows Widget"
description: "Stack multiple renderables vertically with consistent spacing"
date: 2025-08-05
tags: ["widgets", "rows", "layout", "vertical"]
section: "Console"
uid: "console-widget-rows"
order: 5050
---

The Rows widget provides a convenient way to stack multiple renderables vertically with consistent spacing, acting as a container that manages vertical layout automatically. While you can render items sequentially without Rows, this widget is useful when composing complex layouts or when rows need to be treated as a single renderable unit.

**Key Topics Covered:**

* **Basic usage** - Creating rows by passing a collection of renderables to stack vertically
* **Spacing control** - Configuring the vertical spacing between stacked items
* **Expand behavior** - Whether rows should expand to fill width or fit content
* **Nested layouts** - Combining Rows with Columns and other layout widgets for complex arrangements
* **Use as composition root** - Using Rows to bundle multiple items into a single renderable for methods that accept one item
* **Comparison with sequential rendering** - When to use Rows vs. simply calling `AnsiConsole.Write()` multiple times

Examples demonstrate stacking panels vertically for a dashboard, combining rows with columns for grid-like layouts without tables, building card-style UIs, and using rows to organize form elements. The guide helps readers understand when the Rows widget adds value versus manual sequential rendering.

---
title: "Panel Widget"
description: "Create bordered boxes around content with customizable headers, padding, and styles"
date: 2025-08-05
tags: ["widgets", "panel", "layout", "borders"]
section: "Console"
uid: "console-widget-panel"
order: 5020
---

The Panel widget wraps content in a decorative border with optional header and styling, perfect for highlighting important information or creating visual hierarchy in console applications. This page explains all aspects of using panels effectively.

**Key Topics Covered:**

* **Basic panel creation** - Wrapping text or other renderables in a bordered panel using `new Panel(content)`
* **Headers** - Adding panel headers with `SetHeader()`, including positioning (left, right, center) and styling
* **Border styles** - Selecting from built-in border sets (same options as Table) and controlling border colors independently from content
* **Padding and spacing** - Configuring internal padding with `Padding()` to control space between content and borders
* **Panel expansion** - Using `Expand()` to control whether panels fill available width or fit content size
* **Nested panels** - Creating panels within panels for complex layouts and visual depth
* **Content alignment** - Aligning panel content (left, right, center) within the bordered area

Examples show creating information boxes, highlighting warnings or errors with colored borders, building dashboard-style layouts with multiple panels, and using panels as containers for complex multi-widget compositions. The guide also covers when to use panels vs. tables or other layout primitives.

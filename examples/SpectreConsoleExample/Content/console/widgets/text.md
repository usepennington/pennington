---
title: "Text Widget"
description: "Render styled text with precise control over formatting and overflow"
date: 2025-08-05
tags: ["widgets", "text", "rendering", "styling"]
section: "Console"
uid: "console-widget-text"
order: 5330
---

The Text widget provides a renderable wrapper for text content with explicit control over styling, justification, and overflow behavior. While markup strings are often sufficient, the Text widget offers programmatic styling control and advanced text handling features.

**Key Topics Covered:**

* **Creating Text widgets** - Using `new Text(content)` vs. markup strings
* **Styling** - Applying colors, bold, italic, underline, and other styles programmatically with `Style` objects
* **Justification** - Left, center, right, or full justification of text content
* **Overflow behavior** - Controlling how text behaves when it exceeds available width (truncate, wrap, ellipsis)
* **Line breaks** - Handling multi-line text and explicit line breaks
* **When to use Text** - Choosing between Text widgets, markup strings, and simple string output
* **Composition** - Using Text as content for other widgets like panels and tables

Examples demonstrate building styled text dynamically from variables, creating justified text blocks, handling text overflow in constrained layouts, styling large text content programmatically without markup, and using Text widgets for precise control in complex layouts. The guide clarifies when the Text widget adds value over simpler alternatives and when markup is more appropriate.

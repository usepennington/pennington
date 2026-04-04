---
title: "Canvas Widget"
description: "Draw pixel-level graphics and patterns using Braille characters"
date: 2025-08-05
tags: ["widgets", "canvas", "graphics", "drawing"]
section: "Console"
uid: "console-widget-canvas"
order: 5130
---

The Canvas widget enables pixel-level drawing in the console using Unicode Braille characters, where each character represents an 8-pixel (2×4) grid. This allows for surprisingly detailed graphics, patterns, and simple visualizations in text mode.

**Key Topics Covered:**

* **Canvas basics** - Understanding the Braille character grid and how pixels map to console characters
* **Drawing pixels** - Setting individual pixels with `SetPixel(x, y, color)` to create patterns and shapes
* **Canvas dimensions** - Working with canvas width and height, understanding resolution limitations
* **Colors** - Applying colors to pixels for creating colorful graphics
* **Drawing primitives** - Creating lines, rectangles, circles, and other shapes (if available)
* **Performance considerations** - Handling large canvases and many pixel operations efficiently
* **Use cases** - Creating sparklines, simple graphs, QR codes, ASCII art, and decorative patterns

Examples demonstrate drawing simple shapes and patterns, creating data visualizations like sparklines and bar plots at character resolution, rendering logos and icons, building terminal-based mini-games or animations, and generating decorative borders. The guide discusses the limitations of canvas-based graphics and when to use Canvas vs. other visualization approaches.

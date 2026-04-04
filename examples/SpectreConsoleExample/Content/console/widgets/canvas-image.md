---
title: "Canvas Image Widget"
description: "Display images in the console using Canvas rendering"
date: 2025-08-05
tags: ["widgets", "canvas", "image", "graphics"]
section: "Console"
uid: "console-widget-canvas-image"
order: 5140
---

The CanvasImage widget extends the Canvas widget to load and display actual image files in the console using Braille character rendering. It automatically converts images to pixel data suitable for canvas display, enabling console applications to show logos, icons, and simple graphics.

**Key Topics Covered:**

* **Loading images** - Creating CanvasImage from file paths, streams, or byte arrays
* **Supported formats** - Image file formats that can be loaded and displayed
* **Image scaling** - Resizing images to fit console dimensions while maintaining aspect ratio
* **Color handling** - How images are converted to console colors and dithering techniques
* **Resolution and quality** - Understanding quality limitations due to character-based rendering
* **Max width/height** - Setting size constraints for displayed images
* **Use cases** - Displaying app logos, showing QR codes, rendering simple diagrams

Examples show loading and displaying a company logo, rendering generated QR codes for URLs, showing product thumbnails in console catalogs, creating image-based progress indicators, and building simple image viewers. The guide also discusses when image display is practical in console applications and alternatives for more complex graphics needs.

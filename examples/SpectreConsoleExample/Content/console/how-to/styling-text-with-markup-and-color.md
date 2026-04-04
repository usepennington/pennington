---
title: "Styling Text with Markup and Color"
description: "How to output text with rich styles and colors using Spectre.Console's markup language"
date: 2025-08-05
tags: ["how-to", "styling", "markup", "color", "text"]
section: "Console"
uid: "console-styling-text"
order: 2100
---

How to output text with rich styles and colors using Spectre.Console's markup language. This guide explains the markup syntax (e.g. `[red]text[/]` for colored text, `[bold]` for bold) and lists available style names (bold, dim, italic, etc.). It shows how to combine styles and colors in text, how to escape markup when needed (using `Markup.Escape` to avoid errors with `[` characters), and how to use the `AnsiConsole.MarkupLine` convenience method. Readers will learn best practices like not assuming background color and using safe color contrasts. Examples include coloring portions of text, making ASCII banners with **Figlet** fonts, and drawing horizontal rules with **Rule** for separators.
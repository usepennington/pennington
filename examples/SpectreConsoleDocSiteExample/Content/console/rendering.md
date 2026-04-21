---
title: Rendering with Spectre.Console
description: Tables, trees, progress, prompts — the components that make Spectre.Console feel like a UI toolkit for the terminal.
uid: guides.rendering
order: 10
sectionLabel: Console
---

Spectre.Console treats the terminal like a rendering surface. Where `Console.WriteLine` thinks in strings, Spectre thinks in `IRenderable` nodes laid out by a renderer that knows the cell width, the ANSI capabilities of the host, and the Unicode width of every glyph.

The landing type is `AnsiConsole` — a global entry point mirrored by `IAnsiConsole` for DI. Most calls are on `AnsiConsole.MarkupLine("Hello [yellow]world[/]!")` or `AnsiConsole.Write(new Table().AddColumn("Name").AddColumn("Email"))`.

See the <xref:reference.core-types> reference for quick-look summaries of these types.

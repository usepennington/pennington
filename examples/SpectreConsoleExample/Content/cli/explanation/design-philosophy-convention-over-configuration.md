---
title: "Design Philosophy: Convention over Configuration"
description: "An explanation of the guiding philosophy behind Spectre.Console.Cli"
date: 2025-08-05
tags: ["explanation", "philosophy", "design", "convention", "configuration"]
section: "Cli"
uid: "cli-design-philosophy"
order: 3010
---

An explanation of the guiding philosophy behind Spectre.Console.Cli. This section discusses how the library is **opinionated** in following established CLI conventions – for example, the way options are named (single `-` for short, `--` for long), the automatic help generation, and the enforcement of a structured command pattern. It explains why the library uses the .NET type system (attributes and generic classes) to define commands rather than manual parsing: to catch errors at compile-time and provide a clear separation of concerns. The concept of composition is highlighted: commands and settings are separate, which encourages reuse and cleaner code, as demonstrated by the "add" command example. This narrative may reference how this approach leads to easier testing and maintenance. Essentially, this is a behind-the-scenes rationale that helps users understand the "why" of the design, not just the "how."
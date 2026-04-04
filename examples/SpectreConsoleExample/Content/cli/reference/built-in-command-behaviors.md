---
title: "Built-in Command Behaviors"
description: "A reference describing Spectre.Console.Cli's built-in behaviors and conventions for completeness"
date: 2025-08-05
tags: ["reference", "behaviors", "conventions", "help", "parsing"]
section: "Cli"
uid: "cli-built-in-behaviors"
order: 4040
---

A reference describing Spectre.Console.Cli's built-in behaviors and conventions for completeness. This could include:

* The default **Help** option (`-h/--help`) that's automatically available and how it triggers help output.
* The **--version** option if one exists by default (not sure if Spectre.Console.Cli provides a default version flag; if so, document it).
* How unrecognized commands or arguments are handled (perhaps throwing a `CommandParseException`).
* The default parsing rules (e.g., `--` to stop parsing options, handling of quotes in arguments which is mostly done by the shell, etc.).
* Mention of **CommandContext** – that each command's Execute gets a `CommandContext` object which contains the raw arguments, remaining args, and parent commands (if any).
  This section is more for advanced users to know the framework's default behaviors and how it conforms to typical CLI standards (for example, how it deals with case sensitivity, or combining short options).
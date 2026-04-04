---
title: "Tutorial: Building a Multi-Command CLI Tool"
description: "An intermediate tutorial that expands to multiple commands and subcommands, illustrating Spectre.Console.Cli's support for complex CLI structures"
date: 2025-08-05
tags: ["tutorial", "intermediate", "multi-command", "subcommands", "cli"]
section: "Cli"
uid: "cli-multi-command-tutorial"
order: 1020
---

An intermediate tutorial that expands to multiple commands and subcommands, illustrating Spectre.Console.Cli's support for complex CLI structures. Using a real-world scenario (e.g. a simple version control CLI or a file utility), it guides the user to create several commands with a shared theme. The tutorial covers:

* Defining multiple `CommandSettings` classes (some possibly inheriting from a common base for shared options).
* Creating corresponding `Command` classes for each (e.g. `AddCommand`, `CommitCommand`, etc.) with their `Execute` logic.
* Composing the commands into a hierarchy using `app.Configure(config => config.AddBranch(...)...)` for subcommands (for example, a top-level "add" command with subcommands "package" and "reference").
* Running and testing the CLI with various arguments (`app.exe add package --version 1.0`).
  This tutorial emphasizes how to structure the code cleanly via composition and shows the automatically generated help output for the commands. By completing it, the reader will understand how to build and organize a CLI with multiple verbs and nested commands.
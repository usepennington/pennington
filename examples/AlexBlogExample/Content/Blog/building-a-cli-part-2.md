---
title: "Building a CLI Tool, Part 2: Output Formatting"
date: 2026-03-22
author: "Alex Chen"
description: "Add color, tables, and progress bars to your CLI"
tags: ["dotnet", "cli"]
series: "Building a CLI"
---

In [Part 1](/blog/2026/03/building-a-cli-part-1), we set up argument parsing. Now let's make the output look great using Spectre.Console.

## Adding Spectre.Console

```shell
dotnet add package Spectre.Console
```

## Rich Output

```csharp
using Spectre.Console;

AnsiConsole.MarkupLine("[bold green]Success![/] Your task has been scheduled.");

var table = new Table();
table.AddColumn("Task");
table.AddColumn("Schedule");
table.AddColumn("Status");

table.AddRow("Backup DB", "Every 6h", "[green]Active[/]");
table.AddRow("Send Report", "Daily 9am", "[yellow]Pending[/]");

AnsiConsole.Write(table);
```

## Progress Bars

Spectre.Console also has great progress bar support for long-running operations. Combined with System.CommandLine from Part 1, you get a professional CLI experience.

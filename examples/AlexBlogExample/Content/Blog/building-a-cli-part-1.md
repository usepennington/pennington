---
title: "Building a CLI Tool, Part 1: Parsing Arguments"
date: 2026-03-15
author: "Alex Chen"
description: "Start a CLI tool from scratch with System.CommandLine"
tags: ["dotnet", "cli"]
series: "Building a CLI"
---

I've been wanting to build a proper CLI tool for a while. Not a quick script — a real tool with argument parsing, help text, and tab completion. Here's how I started with `System.CommandLine`.

## Setting Up

The first step is creating a new console project and adding the `System.CommandLine` package:

```shell
dotnet new console -n MyCli
dotnet add package System.CommandLine
```

## Defining Commands

```csharp
using System.CommandLine;

var rootCommand = new RootCommand("My awesome CLI tool");

var nameOption = new Option<string>(
    name: "--name",
    description: "The name to greet");

var greetCommand = new Command("greet", "Greet someone")
{
    nameOption
};

greetCommand.SetHandler((name) =>
{
    Console.WriteLine($"Hello, {name}!");
}, nameOption);

rootCommand.Add(greetCommand);
await rootCommand.InvokeAsync(args);
```

In the next post, I'll add output formatting with colors and tables.

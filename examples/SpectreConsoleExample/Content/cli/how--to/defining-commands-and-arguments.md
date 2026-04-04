---
title: "Defining Commands and Arguments"
description: "How to declare command-line parameters (arguments and options) using Spectre.Console.Cli's attributes and settings classes"
date: 2025-08-05
tags: ["how-to", "commands", "arguments", "options", "attributes"]
section: "Cli"
uid: "cli-commands-arguments"
order: 2010
---

How to declare command-line parameters (arguments and options) using Spectre.Console.Cli's attributes and settings classes. This guide covers creating a class that inherits `CommandSettings` and using:

* `[CommandArgument]` for positional arguments, with examples of required (`<angle brackets>`) vs optional (`[square brackets]`) syntax in the attribute name. It explains the significance of the position index and how only one argument can gather multiple values via arrays (the argument vector).
* `[CommandOption]` for named options (flags/switches), demonstrating short vs long form (`-c|--count`). It also shows boolean flags (which don't require a value; specifying the flag sets true) and how to hide an option from help (`IsHidden = true`).
* Using .NET's `[Description]` attribute on properties to provide help text, and `[DefaultValue]` to supply a default if the option is not provided.
  Through a clear example (perhaps a "greet" command that takes an optional name and a repeat count), the guide illustrates how to define robust command inputs. It also notes that Spectre.Console.Cli will automatically generate help and usage messages from these definitions.
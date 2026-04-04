---
title: "Configuring CommandApp and Commands"
description: "How to register commands with the CommandApp and configure global settings"
date: 2025-08-05
tags: ["how-to", "configuration", "commandapp", "aliases", "descriptions"]
section: "Cli"
uid: "cli-app-configuration"
order: 2030
---

How to register commands with the `CommandApp` and configure global settings. This guide covers the use of `CommandApp.Configure(...)` to add commands to the application. It shows basic registration with `config.AddCommand<T>("name")` for each command, and describes how to add multiple commands (e.g., a list of top-level commands like "add", "commit", "push" for a git-like CLI). It then details the fluent configuration options:

* **Aliases**: using `.WithAlias("alias")` to add alternate names for a command.
* **Descriptions**: `.WithDescription("text")` to set the help text summary for the command.
* **Examples**: `.WithExample(new[] {...})` to provide usage examples that will appear in help.
  The guide also touches on global settings via `config.Settings`: for instance, enabling exception propagation or validation of examples in DEBUG builds. It might mention `config.SetApplicationName` or other metadata if available. By going through this, readers can properly wire up their commands into the app and fine-tune how they appear and behave in the CLI.
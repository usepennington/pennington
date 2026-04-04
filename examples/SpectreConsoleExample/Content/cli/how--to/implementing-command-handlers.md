---
title: "Implementing Command Handlers"
description: "How to create the logic for commands by implementing the Command classes"
date: 2025-08-05
tags: ["how-to", "command-handlers", "execute", "validation", "dependency-injection"]
section: "Cli"
uid: "cli-command-handlers"
order: 2020
---

How to create the logic for commands by implementing the `Command` classes. This guide explains the difference between inheriting from `Command<TSettings>` vs `AsyncCommand<TSettings>`, and when to use each (sync vs async execution). It covers writing the `Execute(CommandContext, TSettings)` method where the business logic goes, and returning an exit code (0 for success, non-zero for error). A simple example is given (e.g. a `HelloCommand` that reads `settings.Name` and prints a greeting). The guide also discusses:

* **Dependency Injection in commands**: how Spectre.Console.Cli supports constructor injection for commands if a DI container/registrar is configured. For instance, showing a command that takes an `ILoggingService` via its constructor.
* **Validation**: using the `Validate` method override in a Command to perform pre-execution validation of settings. An example demonstrates checking that a file path provided as an argument exists, returning `ValidationResult.Error` with a message if not. This prevents execution if inputs are invalid.
  By following this guide, developers will learn to flesh out command classes with the needed logic and safety checks, leveraging base class features like validation and DI.
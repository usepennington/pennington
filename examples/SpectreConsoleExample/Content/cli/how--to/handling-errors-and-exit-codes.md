---
title: "Handling Errors and Exit Codes"
description: "How Spectre.Console.Cli deals with exceptions and how to customize error handling"
date: 2025-08-05
tags: ["how-to", "error-handling", "exit-codes", "exceptions", "propagation"]
section: "Cli"
uid: "cli-error-handling"
order: 2050
---

How Spectre.Console.Cli deals with exceptions and how to customize error handling. This guide explains the default behavior: any unhandled exception in a command results in an error message to the console and an exit code of -1. It then shows ways to override this:

* **PropagateExceptions**: Setting `config.PropagateExceptions()` to let exceptions bubble up to your `Main` method. The guide demonstrates wrapping `app.Run(args)` in a try-catch in Program.Main, catching exceptions and using `AnsiConsole.WriteException` (from Spectre.Console) to print them, then returning a custom exit code.
* **Custom Exception Handler**: Using `config.SetExceptionHandler(...)` to intercept exceptions. It shows both overloads: one where you return an int (to set a specific exit code), and one where you don't (using a default exit code). An example is given where the handler prints the exception in a formatted way and returns, say, -99 as the exit code.
  The guide also notes that these handlers catch exceptions thrown during command parsing or execution, and that using them can centralize error reporting. By following this, readers can implement robust error handling strategies for their CLI tools.
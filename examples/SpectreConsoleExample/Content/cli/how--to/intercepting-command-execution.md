---
title: "Intercepting Command Execution"
description: "How to use command interceptors to run logic before or after any command executes"
date: 2025-08-05
tags: ["how-to", "interceptors", "command-execution", "lifecycle", "cross-cutting"]
section: "Cli"
uid: "cli-command-interception"
order: 2080
---

How to use command interceptors to run logic before or after any command executes. This guide explains the `ICommandInterceptor` interface and how to register an interceptor via the DI container or by using `config.SetInterceptor(...)` on the CommandApp. It describes the two methods:

* `Intercept(CommandContext, CommandSettings)` – called *before* the command's `Execute`, where you can modify settings or perform setup (e.g. configure logging, initialize a database, etc.).
* `InterceptResult(CommandContext, CommandSettings, int exitCode)` – called *after* the command execution, where you can inspect or alter the result (exit code) and do teardown (e.g. flush logs, dispose resources).
  The guide provides an example scenario: an interceptor that starts a logging scope in `Intercept` and closes it or adjusts exit code in `InterceptResult`. It references the documentation's example of using an interceptor for logging with Serilog. It also notes that interceptors run around every command invocation, making them suitable for cross-cutting concerns (like timing, logging, or setting up global state) without cluttering individual command code.
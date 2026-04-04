---
title: "Dependency Injection in CLI Commands"
description: "How to integrate a DI container with Spectre.Console.Cli for injecting services into commands"
date: 2025-08-05
tags: ["how-to", "dependency-injection", "di", "services", "ioc"]
section: "Cli"
uid: "cli-dependency-injection"
order: 2060
---

How to integrate a DI container with Spectre.Console.Cli for injecting services into commands. This guide walks through setting up an `ITypeRegistrar` for your preferred DI library. It provides an example using **Microsoft Extensions DI** (via `ServiceCollection`):

* Register services (e.g. add a singleton for an interface).
* Implement `ITypeRegistrar` and `ITypeResolver` or use the provided base classes to adapt the container.
* Pass the registrar into the `CommandApp` constructor (`new CommandApp(registrar)` or `CommandApp<DefaultCommand>(registrar)`).
  The guide references the available example in the docs where a custom `MyTypeRegistrar` is used to hook into Microsoft DI. It then shows how a command can declare a dependency in its constructor (like a database or logger service) and Spectre.Console.Cli will resolve it when running the command. Tips on testing the TypeRegistrar using `TypeRegistrarBaseTests.RunAllTests()` from Spectre.Console.Testing are mentioned to ensure the DI integration is correct. By using this guide, developers can compose their CLI app using dependency injection for cleaner, testable command code.
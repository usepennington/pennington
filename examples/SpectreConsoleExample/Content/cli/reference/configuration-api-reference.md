---
title: "Configuration API Reference"
description: "A reference page enumerating the methods and properties available on the CommandApp configuration object and related classes"
date: 2025-08-05
tags: ["reference", "configuration", "api", "commandapp", "settings"]
section: "Cli"
uid: "cli-configuration-api"
order: 4030
---

A reference page enumerating the methods and properties available on the `CommandApp` configuration object and related classes:

* **CommandApp.Configure** – mention how to call it and that inside the lambda you can use `AddCommand`, `AddBranch`, etc.
* **Configurator (IConfigurator)** – list methods like `AddCommand<T>`, `AddBranch<T>`, and extension methods like `.WithAlias`, `.WithDescription`, `.WithExample`, `.IsHidden` with brief descriptions.
* **Config.Settings** – list relevant properties in `CommandAppSettings` such as `CaseSensitivity`, `StrictParsing`, `ValidateExamples`, `PropagateExceptions`, `ApplicationName`, `HelpProviderStyles`, etc., and what they control.
* **SetInterceptor**, **SetExceptionHandler**, **SetHelpProvider** – summarize these configuration hooks and reference to where in docs they are explained.
  This reference acts as an index of the fluent configuration API for those who want to see all options at a glance.
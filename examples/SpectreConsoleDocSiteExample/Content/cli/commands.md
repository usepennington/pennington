---
title: Commands with Spectre.Console.Cli
description: Type-safe commands, sub-commands, and settings driven by your DI container.
uid: guides.cli
order: 20
sectionLabel: Cli
---

Spectre.Console.Cli ships a mini-framework for building multi-command console apps in the shape of `git` or `dotnet`. The entry type is `CommandApp` — build one, register commands and settings classes, call `Run(args)`.

Typed `CommandSettings` replace manually parsing `string[] args`. The framework:

- Binds flags, switches, and positional arguments onto strongly-typed settings classes.
- Supports commands, sub-commands, and command branches.
- Integrates with Microsoft.Extensions.DependencyInjection so commands resolve their dependencies the same way ASP.NET Core services do.

See the <xref:reference.core-types> reference for the types you touch first.

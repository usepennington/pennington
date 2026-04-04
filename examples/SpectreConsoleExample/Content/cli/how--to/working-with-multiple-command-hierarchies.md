---
title: "Working with Multiple Command Hierarchies"
description: "How to create hierarchical (nested) commands using branching"
date: 2025-08-05
tags: ["how-to", "hierarchies", "nested-commands", "branching", "subcommands"]
section: "Cli"
uid: "cli-command-hierarchies"
order: 2070
---

How to create hierarchical (nested) commands using branching. This guide specifically addresses scenarios where commands have subcommands (like `git add` having subcommands in future, or `dotnet tool install/uninstall`). It explains using `AddBranch<TSettings>("name", branch => { ... })` to create a grouping command that isn't directly executable but routes to subcommands. The example from the documentation is used: a top-level "add" command that shares common settings, and two subcommands "package" and "reference" each with their own specific settings and command classes. The guide shows how the `AddBranch` takes a base settings type (for shared options like maybe a "project" in the example) and how subcommands inherit those settings. It also covers that you can nest branches further for deeper hierarchies. After reading this, users will know how to structure complex CLI command trees and understand that the type system (inheritance of settings classes) helps reuse common parameters across subcommands.
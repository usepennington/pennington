---
title: "Spectre.Console.Cli vs. Spectre.Cli (Migration Guide)"
description: "Context for those familiar with the older Spectre.Cli library"
date: 2025-08-05
tags: ["explanation", "migration", "spectre-cli", "changes", "namespace"]
section: "Cli"
uid: "cli-migration-guide"
order: 3040
---

While a step-by-step migration is in the How-To or Reference, this explanation provides context for those familiar with the older Spectre.Cli library. It outlines what changed conceptually when the functionality moved into Spectre.Console.Cli. Key points include:

* The merging of libraries (Spectre.Cli is no longer updated; Spectre.Console.Cli is the path forward).
* **Namespace changes**: everything now lives under `Spectre.Console.Cli` instead of `Spectre.Cli`.
* Minor breaking changes: for instance, exceptions namespace moved (no more `Spectre.Cli.Exceptions`), and possibly any class or API renames.
* Improvements in the new library (if any) – e.g., better help text styling or new features like interceptors that may not have existed in Spectre.Cli.
  This section doesn't just list the steps to migrate (the how-to does that), but explains why the split was made (perhaps to unify development under one umbrella) and reassures that the new CLI library aligns with Spectre.Console's patterns. It's useful for context and for convincing teams to upgrade.
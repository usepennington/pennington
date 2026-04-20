---
title: Spectre.Console
description: "A .NET library that makes it easier to create beautiful console applications."
uid: home
---

[Spectre.Console](https://github.com/spectreconsole/spectre.console) is two sibling packages:

- **Spectre.Console** — rendering primitives: tables, trees, progress bars, prompts, markup, ANSI colors.
- **Spectre.Console.Cli** — a command-line argument parser that reads commands, sub-commands, and typed settings off your DI container.

This docsite documents both as **separate API reference trees** on one site, driven by two `AddApiMetadataFromCompiledAssembly` calls paired with two `AddApiReference` calls. It's a demo of Pennington's multi-source metadata backend — no Spectre.Console source is vendored; the reference pages reflect over the shipped `.dll` + `.xml` pairs committed under `lib/net9.0/`.

## Browse

- <xref:reference.core-types> — the landmark types in both packages.
- [`/api/spectre/`](/api/spectre/) — full Spectre.Console reference.
- [`/api/spectre-cli/`](/api/spectre-cli/) — full Spectre.Console.Cli reference.

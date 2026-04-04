---
title: "Extensibility Points"
description: "A reference page on extending or integrating with Spectre.Console.Cli beyond the basics"
date: 2025-08-05
tags: ["reference", "extensibility", "ityperegistrar", "icommandinterceptor", "custom"]
section: "Cli"
uid: "cli-extensibility"
order: 4050
---

A reference page on extending or integrating with Spectre.Console.Cli beyond the basics. It could list:

* **ITypeRegistrar / ITypeResolver** – brief description of these interfaces for DI integration and that you can implement them or use provided helpers.
* **ICommandInterceptor** – recap of how to create one and that multiple can be registered via DI.
* **IRemainingArguments (if exists)** – some CLI frameworks have ways to capture all unmatched tail arguments; if Spectre.Console.Cli supports something like this or a special attribute for remaining args, document it.
* **Custom Parsing** – if advanced scenarios require manual parsing, note if Spectre.Console.Cli allows hooking or if one should pre-process args.
  This reference ensures that if developers need to extend functionality (like using a custom help provider or integrating with a config file), they know what extension interfaces are available.
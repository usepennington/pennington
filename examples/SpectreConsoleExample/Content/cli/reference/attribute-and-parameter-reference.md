---
title: "Attribute and Parameter Reference"
description: "A summary of all attributes and parameter-related features in Spectre.Console.Cli"
date: 2025-08-05
tags: ["reference", "attributes", "parameters", "commandargument", "commandoption"]
section: "Cli"
uid: "cli-attributes-parameters"
order: 4020
---

A summary of all attributes and parameter-related features in Spectre.Console.Cli. This reference lists:

* **CommandArgumentAttribute** – its constructor (position, name) and how angle vs square bracket notation works to denote required/optional.
* **CommandOptionAttribute** – its constructor (aliases string) and properties like `IsHidden`. Includes notes on boolean flags and how default values are handled.
* **DefaultValueAttribute** (from System.ComponentModel) – mention that Spectre.Console.Cli honors it for options/arguments defaults.
* **DescriptionAttribute** (System.ComponentModel) – used for help text.
* **TypeConverter support** – note that custom types can be bound by providing a TypeConverter (e.g. converting a string to a complex object or enum).
* Possibly **CommandSettings** features – e.g., if `CommandSettings` has methods like `Validate` that can be overridden (though typically one would override in Command, not settings).
  This page serves as a quick lookup for how to decorate command properties and what each attribute does, without going into usage scenarios (which are in how-to guides).
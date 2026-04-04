---
title: "Customizing Help Text and Usage"
description: "How to tailor the automatically generated help output of Spectre.Console.Cli"
date: 2025-08-05
tags: ["how-to", "help", "usage", "styling", "customization"]
section: "Cli"
uid: "cli-help-customization"
order: 2040
---

How to tailor the automatically generated help output of Spectre.Console.Cli. This guide covers:

* Providing a **High-level App Description** and examples: using `config.SetApplicationName` (if available) and ensuring top-level examples are set via `.WithExample` on default command registration, so that running `app.exe --help` shows a summary and usage.
* **Styling the help**: adjusting `HelpProviderStyle` via `config.Settings.HelpProviderStyles`. It gives an example of changing the style of the description header to bold or even setting `HelpProviderStyles = null` to remove all styling for plain output – helpful for accessibility or plain text needs.
* Hiding commands or options from help: reminding that `.IsHidden()` on a command or `IsHidden=true` on an option will omit them from the help listing (for advanced or internal commands).
* **Custom Help Provider**: if needed, how to replace the help system entirely by implementing `IHelpProvider` and calling `config.SetHelpProvider(new CustomHelpProvider())`. The guide likely references the existence of an example on GitHub for a custom help provider.
  By using this guide, users can fine-tune how their CLI's help and usage information is presented, ensuring end-users get clear instructions.
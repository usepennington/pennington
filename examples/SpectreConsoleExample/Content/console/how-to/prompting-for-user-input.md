---
title: "Prompting for User Input"
description: "How to interactively prompt the user for input using Spectre.Console"
date: 2025-08-05
tags: ["how-to", "prompts", "user-input", "interactive", "selection"]
section: "Console"
uid: "console-user-input"
order: 2150
---

How to interactively prompt the user for input using Spectre.Console. This guide covers the various prompt utilities:

* **Ask<T>**: Asking for a free-form input of a specific type (e.g. string, int) with optional default. It shows how to prompt for a value and handle the returned typed result.
* **Confirm**: Presenting a yes/no question to the user, with examples of confirmation dialogs and default values (yes/no).
* **SelectionPrompt**: Creating a menu of options for the user to select one (arrow-key navigation).
* **MultiSelectionPrompt**: Allowing the user to choose multiple items from a list (with checkboxes).
* **TextPrompt**: (If distinct from Ask) Configuring input with validation or masking (for passwords).
  This guide will include code snippets to illustrate each type of prompt, such as asking for the user's name, confirming an action, or choosing from a list of items. It also provides tips on customizing prompt appearance and ensuring prompts do not conflict with other live output.
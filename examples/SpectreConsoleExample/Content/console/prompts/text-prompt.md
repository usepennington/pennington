---
title: "TextPrompt"
description: "Prompt users for text input with validation and default values"
date: 2025-08-05
tags: ["prompts", "input", "text", "validation"]
section: "Console"
uid: "console-prompt-text"
order: 7010
---

The TextPrompt widget prompts users to enter text input with support for validation, default values, secret input masking, and custom styling. It's the foundation for gathering user input in interactive console applications.

**Key Topics Covered:**

* **Basic text prompts** - Using `AnsiConsole.Ask<T>()` or `new TextPrompt<T>()` to prompt for input
* **Type conversion** - Automatically converting input to target types (int, decimal, custom types, etc.)
* **Default values** - Providing defaults with `DefaultValue()` that users can accept by pressing Enter
* **Validation** - Adding validation rules with `Validate()` to ensure input meets requirements
* **Custom validation messages** - Providing clear error messages when validation fails
* **Optional vs required** - Making prompts optional with `AllowEmpty()` for nullable types
* **Secret input** - Masking sensitive input like passwords with `Secret()` or custom mask characters
* **Prompt styling** - Customizing colors and formatting of prompts and user input
* **Validation function** - Writing custom validation logic for complex requirements
* **Choices** - Restricting input to specific allowed values

Examples show prompting for usernames and passwords, asking for numeric input with range validation, collecting email addresses with format validation, building configuration wizards with validated input, creating interactive setup scripts, and handling complex multi-step input flows. The guide covers user experience best practices for prompts and validation messaging.

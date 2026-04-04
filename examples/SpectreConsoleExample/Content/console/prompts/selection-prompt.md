---
title: "SelectionPrompt"
description: "Let users select a single option from a list with keyboard navigation"
date: 2025-08-05
tags: ["prompts", "selection", "menu", "interactive"]
section: "Console"
uid: "console-prompt-selection"
order: 7020
---

The SelectionPrompt widget creates interactive menus where users can select one option from a list using arrow keys or typing to search. It's perfect for building menus, option pickers, and any scenario where users need to choose from predefined options.

**Key Topics Covered:**

* **Creating selection prompts** - Using `new SelectionPrompt<T>()` to build interactive choice lists
* **Adding choices** - Using `AddChoices()` to populate the selection list
* **Title and prompts** - Setting descriptive titles and instructions for users
* **Default selection** - Pre-selecting an option with highlighting
* **Paging** - Handling long lists with automatic paging and configurable page size
* **Search/filter** - Enabling type-to-search functionality to filter large lists
* **Custom converters** - Displaying complex objects with custom formatting
* **More choices indicator** - Showing hints when there are more choices above/below viewport
* **Highlighting** - Customizing the visual appearance of selected items
* **Return values** - Getting the selected item value or custom mapped result

Examples demonstrate creating main menus for applications, building environment/configuration selectors (dev/staging/prod), letting users choose from data records, creating action menus (what to do next), implementing drill-down navigation, and building wizards with sequential selections. The guide discusses UX considerations for choice lists and when to use SelectionPrompt vs. simple yes/no confirmations.

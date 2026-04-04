---
title: "MultiSelectionPrompt"
description: "Allow users to select multiple options from a list"
date: 2025-08-05
tags: ["prompts", "multi-select", "checkbox", "interactive"]
section: "Console"
uid: "console-prompt-multi-selection"
order: 7030
---

The MultiSelectionPrompt widget creates interactive checkbox-style lists where users can select multiple options using the spacebar, arrow keys for navigation, and Enter to confirm. It's ideal for selecting features, choosing multiple items, or any scenario requiring multi-selection.

**Key Topics Covered:**

* **Creating multi-selection prompts** - Using `new MultiSelectionPrompt<T>()` for multiple choice selection
* **Adding choices** - Populating the list with `AddChoices()` and organizing items
* **Required selections** - Enforcing minimum selection counts (at least one, at least N, etc.)
* **Default selections** - Pre-selecting some items as checked by default
* **Instructions** - Guiding users on how to interact (spacebar to toggle, enter to submit)
* **Selection modes** - Leaf-only selection vs. enabling selection at any level in hierarchies
* **Paging** - Managing long lists with scrolling and page size configuration
* **Highlighting and indicators** - Visual feedback for selected vs. unselected items
* **Hierarchical choices** - Organizing choices in tree structures with parent-child relationships
* **Custom formatting** - Displaying complex objects with custom converters

Examples show selecting features during application setup, choosing multiple files for processing, picking tags or categories for items, selecting build targets or configurations, creating hierarchical multi-selects (select all children when parent is selected), and building complex configuration wizards. The guide covers best practices for multi-selection UX and when to use multi-select vs. multiple single selections.

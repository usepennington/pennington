---
title: "Tree Widget"
description: "Display hierarchical data structures with expandable tree views"
date: 2025-08-05
tags: ["widgets", "tree", "hierarchy", "structure"]
section: "Console"
uid: "console-widget-tree"
order: 5120
---

The Tree widget visualizes hierarchical data structures with parent-child relationships using Unicode tree characters (├─, └─, │). It's ideal for displaying file systems, organizational charts, dependency graphs, and any nested data structures.

**Key Topics Covered:**

* **Tree structure** - Creating root nodes and building hierarchical relationships with `AddNode()`
* **Node content** - Using plain text, markup, or any renderable as node content
* **Tree styles** - Selecting from various tree line styles (ASCII, Unicode, rounded, etc.)
* **Expanding/collapsing** - Controlling which nodes are expanded by default (all trees are static, not interactive)
* **Styling nodes** - Applying colors and styles to individual nodes or entire branches
* **Guide lines** - Customizing the vertical and horizontal lines connecting tree nodes
* **Recursive structures** - Building deeply nested trees from hierarchical data sources

Examples show visualizing directory structures, displaying package dependency trees, rendering organizational hierarchies, showing JSON/XML structure, creating mind maps, and building syntax trees. The guide also covers best practices for tree depth limits, handling wide nodes, and when to use Tree vs. nested lists or indented text.

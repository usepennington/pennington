---
title: "BreakdownChart Widget"
description: "Show proportional data as a visual breakdown with colored segments"
date: 2025-08-05
tags: ["widgets", "chart", "breakdown", "proportions"]
section: "Console"
uid: "console-widget-breakdown-chart"
order: 5220
---

The BreakdownChart widget displays data as proportional colored segments in a horizontal bar, similar to a progress bar but showing multiple values that sum to a whole. It's perfect for visualizing percentages, resource allocation, categorical distributions, and "parts of a whole" data.

**Key Topics Covered:**

* **Adding items** - Using `AddItem()` to add labeled values that will be displayed proportionally
* **Proportional rendering** - How values are automatically converted to proportional segments
* **Colors and labels** - Assigning distinct colors to each segment and showing descriptive labels
* **Width control** - Setting the total width of the breakdown chart
* **Tag display** - Showing segment labels with values or percentages
* **Tag formatting** - Customizing how segment information is displayed
* **Use cases** - Disk space usage, memory allocation, survey results, budget breakdown, test results

Examples show creating disk usage breakdowns (used/free space), visualizing memory allocation across categories, displaying poll results as proportions, showing project time allocation, breaking down budget categories, and building resource utilization dashboards. The guide helps readers choose between BreakdownChart and other visualization widgets like BarChart or progress indicators based on the nature of their data.

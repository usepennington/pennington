---
title: "Lifecycle Events"
description: "JavaScript events during SPA navigation"
order: 30
---

## Available Events

Penn dispatches custom events during SPA navigation:

- **`spa:before-navigate`** — fired before navigation starts
- **`spa:commit`** — fired after new content is rendered

## Example

```javascript
document.addEventListener('spa:commit', () => {
    // Re-initialize interactive widgets after content swap
    initializeTooltips();
    highlightActiveNavItem();
});
```

These events let you hook into the navigation lifecycle to reinitialize JavaScript widgets or track analytics.

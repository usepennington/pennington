---
title: Page one
---

This is the article body of page one. It lives inside the `data-spa-region="content"` element with the red border. Click a link in the persistent sidebar (blue border) and watch the event log: the engine fires `spa:before-navigate`, then `spa:commit` after the swap.

The header and this article both swap on navigation. The sidebar keeps its DOM — the "Navigations observed" counter increments on each commit and survives because nothing replaces the nav element.

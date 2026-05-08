---
title: Page three
---

Open DevTools and inspect the `<header>` and `<main>` elements before clicking another link. Click the link, then re-inspect. The DOM nodes inside both have been replaced — same `data-spa-region` element, new contents.

Now inspect `<nav>`. After several navigations the same nav element is still there, with the same JavaScript-set counter value, and any text selection or focus you gave it is preserved.

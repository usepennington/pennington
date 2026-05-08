---
title: Page two
---

You arrived here without a full page reload. Open the network tab — the request for `/page-two/` returned the canonical HTML of this page (the same response a fresh-tab visit would get). The engine parsed it, extracted the elements with matching `data-spa-region` names, and replaced the live regions' `innerHTML`.

Scroll the article, click another link, scroll back. Notice the sidebar holds whatever scroll position you give it — the engine never touches it because it isn't marked as a region.

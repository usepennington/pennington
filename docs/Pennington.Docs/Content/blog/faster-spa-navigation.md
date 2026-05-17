---
title: Faster, smoother page navigation
description: The SPA engine got a refresh — a persistent sidebar and header that survive navigation, a progress bar for slow loads, and a synchronous swap that replaced the cross-fade.
author: Phil Scott
date: 2026-05-13
isDraft: false
tags:
  - spa
  - navigation
---

Clicking around a documentation site shouldn't feel heavy. Pennington's SPA
navigation got a round of polish — and a small design refresh, pill-style area
and table-of-contents navigation — aimed at making a click feel quick.

## The frame stays put

The biggest change is what *doesn't* move. The sidebar and the header now live
outside the region the SPA swaps. Navigate, and they don't reload — they keep
their scroll position, an open search box stays open, an expanded section stays
expanded. Only the server-driven part, like which page is selected, gets patched
on commit. Navigation reads as the content changing, rather than the whole page
reloading — which is the truth, since that's all that changed. The mechanics are
in [SPA navigation through region swaps](xref:explanation.spa.islands).

## Feedback only when it's needed

Two changes handle the timing of a click. Slow responses get a progress bar
across the top of the viewport, but with a delay and a trickle effect, so a fast
navigation stays silent — you see the bar only when there's something to wait
for.

And the old cross-fade is gone. The fade and the loading shimmer added more
visible motion than they hid. Now the old content stays on screen during the
fetch, and the swap — DOM, scroll position, and `<head>` — happens in one
synchronous block the browser paints as a single frame: no transition, no
flicker, no flash of empty page. The attributes for tuning this are in the
[SPA reference](xref:reference.spa.attributes).

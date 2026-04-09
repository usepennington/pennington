---
title: "Introduction"
description: "What is SPA navigation?"
order: 10
---

## What is SPA Navigation?

SPA (Single Page Application) navigation intercepts link clicks and fetches only the data needed to update the page, rather than performing a full page reload.

In Pennington, SPA navigation works through **islands** — named regions of the page that get swapped during navigation. The sidebar stays put while the content area updates.

## How It Works

1. User clicks a navigation link
2. Pennington intercepts the click
3. A lightweight JSON payload is fetched from `/_spa-data/{slug}.json`
4. Only the island regions are updated
5. The URL bar updates to reflect the new page

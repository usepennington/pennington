---
title: Welcome
description: A cookbook demonstrating SPA slot navigation
order: 1
tags: []
---

# My Recipe Book

Welcome to the recipe book! This site demonstrates **SPA slot navigation** — click any recipe in the sidebar and watch the content and recipe info update instantly without a full page reload.

## How it works

Each page defines named **slots** in the layout:

- `content` — the main article area (this text)
- `recipe-info` — the sidebar card with prep time, servings, etc.

On the first visit, you get full static HTML. On subsequent clicks, the SPA engine fetches a small JSON file and swaps just the slot contents.

## Try it out

Click a recipe in the navigation to see the SPA transition in action.

---
title: MonorailCSS and the case against utility-class sprawl
description: Why Pennington bakes MonorailCSS in and how the semantic palette keeps class lists from rotting.
date: 2024-08-22
author: Jamie Rivers
tags:
  - pennington
  - monorailcss
  - styling
series: Pennington Field Notes
sectionLabel: field-notes
---

Tailwind-style utility classes scale right up until your codebase has six
different shades of "almost the same grey." Pennington's MonorailCSS layer
exposes a semantic palette — `primary`, `accent`, `base` — instead of raw
color names.

## Re-skin in one line

Swap `ColorScheme` on the template options and every `text-primary-600`
class repoints. No find-and-replace across components, no per-page
overrides. The cost: you can't reach for `bg-rose-500` on a whim. The
payoff: a year later your blog still looks coherent.

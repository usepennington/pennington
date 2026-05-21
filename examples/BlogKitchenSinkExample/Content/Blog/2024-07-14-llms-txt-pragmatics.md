---
title: What llms.txt is really for
description: A short defense of shipping llms.txt even on a small blog — and the one knob that matters.
date: 2024-07-14
author: Jamie Rivers
tags:
  - pennington
  - llms-txt
  - publishing
series: Pennington Field Notes
sectionLabel: field-notes
---

`llms.txt` is the LLM-facing index of your site. Pennington ships it at
`/llms.txt` and per-page sidecars at `/<slug>/llms.md` — the second is
just the page's markdown with link rewrites applied, so a model that
fetches it gets the canonical body without HTML noise.

## The opt-out matters

Set `llms: false` on a page's front matter and it disappears from the index
and the sidecar disappears too. Use it for redirect stubs, build artifacts,
and anything else you'd rather not see quoted back at you in a chat
transcript.

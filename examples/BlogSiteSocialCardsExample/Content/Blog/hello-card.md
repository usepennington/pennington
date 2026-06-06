---
title: A post with a generated social card
description: When shared, this post links to an OpenGraph image rendered on demand by the host's Render hook.
date: 2026-05-01
author: Author Name
tags:
  - social-cards
---

This post's `og:image` and `twitter:image` point at
`/social-cards/blog/hello-card.png`, a card rendered on demand by the
`SocialCardOptions.Render` hook wired in `Program.cs`. The static build bakes one
such PNG per content page; the dev server renders them live on request.

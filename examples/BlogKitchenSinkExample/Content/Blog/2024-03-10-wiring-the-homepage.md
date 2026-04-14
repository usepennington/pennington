---
title: Wiring the homepage — hero, projects, socials
description: Turning the default BlogSite home page into something that feels like a personal site.
date: 2024-03-10
author: Jamie Rivers
tags:
  - pennington
  - blogsite
  - homepage
section: field-notes
---

`BlogSiteOptions` exposes four homepage surfaces — `HeroContent`, `MyWork`,
`Socials`, and `MainSiteLinks` — and together they turn the stock BlogSite
template into something that reads like a personal site instead of a stack
of posts.

## Hero and projects

`HeroContent(Title, Description)` is a two-field record rendered at the top
of `/`. `MyWork` is a `Project[]`, each a `Title` / `Description` / `Url`
triple — rendered as the "My Work" sidebar card. Keep the project list short
enough to scan; a kitchen-sink demo ships five for illustration, but three
or four reads more honestly on a real site.

## Icons and header

`Socials` is a `SocialLink[]` that pairs a `RenderFragment` icon with a URL.
The four built-in icon fragments — `SocialIcons.GithubIcon`, `BlueskyIcon`,
`LinkedInIcon`, `MastodonIcon` — cover the common set; authoring a new
fragment is a Razor-component topic. `MainSiteLinks` is a `HeaderLink[]`
for the top-nav and footer; three to five entries is the usual range.

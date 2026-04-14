---
title: Authoring posts with BlogSiteFrontMatter
description: Every field on BlogSiteFrontMatter and how it lights up a different surface on the rendered blog.
date: 2024-02-20
author: Jamie Rivers
tags:
  - pennington
  - authoring
  - front-matter
series: Pennington Field Notes
section: field-notes
---

The `BlogSiteFrontMatter` record is the contract between a markdown file and
everything the BlogSite template renders about it. Every key is optional
except by convention — but each populated key turns on one more surface.

## The fields you'll touch every post

`title`, `description`, and `date` drive every listing surface: the home page
card, the archive page, the RSS item, and the tag listings. `author` flows
into the per-post byline and the RSS `<author>` element. `tags` build the
`/tags/<tag>/` index pages.

## The fields that thread posts together

`series` threads several posts under a shared banner. `repository` renders a
"Source Code" link on the post chrome. `section` groups the post in the
archive. `redirectUrl` supports gentle URL migrations when a post's home
has moved elsewhere. Populate them once on your first post and the rest of
the blog's chrome follows for free.

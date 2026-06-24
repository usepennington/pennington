---
title: Social cards, generated per page
description: Pennington generates a per-page OpenGraph image and the og:image tags for it. You write the function that draws the image; Pennington does the discovery, serving, baking, and tagging.
author: Phil Scott
date: 2026-06-10
isDraft: false
tags:
  - social-cards
  - seo
---

Pennington now generates an OpenGraph image for every page and emits the
`og:image` and `twitter:image` tags that point at it.

## What you write

You write one function, `Render`, that takes a page's title, description, date,
and front matter and returns the image bytes (return null to skip a page). Around
that, Pennington discovers a `/social-cards/{page}.png` route per page, serves
each one in dev, bakes them to PNGs in the static build, and emits the `og:image`,
`twitter:image`, and `twitter:card` tags.

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    CanonicalBaseUrl = "https://example.com",
    SocialCards = new SocialCardOptions
    {
        Render = (request, services, ct) =>
            Task.FromResult<byte[]?>(Paint(request.Title, request.Width, request.Height)),
    },
});
```

You pick the image library: ImageSharp, SkiaSharp, or a headless browser. This
docs site uses [Ashcroft](https://www.nuget.org/packages/Ashcroft) to lay text
over a background image.

## A few details

The card routes produce no content records, so they never show up in search, the
sitemap, or llms.txt. A page that sets its own `og:image` keeps it; the generated
card only fills the gap, through the same [head
subsystem](xref:explanation.core.head-subsystem) everything else in the `<head>`
goes through.

One thing you have to set: OpenGraph wants an absolute image URL. Set
`CanonicalBaseUrl` or the tags come out root-relative, which works in dev but
won't unfurl when shared. The [social cards
how-to](xref:how-to.feeds.social-cards) has a full painter.

---
title: "Generate social card images"
description: "Configure SocialCards so every page ships a generated OpenGraph/Twitter image: Pennington discovers, serves, bakes, and meta-tags one card per page; you supply the drawing code."
uid: how-to.feeds.social-cards
order: 6
sectionLabel: "Feeds & Indexes"
tags: [social-cards, opengraph, twitter, discovery, configuration]
---

When a page is shared on a social network or chat app, the preview image comes from its `og:image` / `twitter:image` meta tags. Setting `SocialCards` turns on generated cards: Pennington discovers one card route per page (`/social-cards/{page-path}.png`), renders each on demand during development, bakes them all in the static build, and points every page's meta tags at its own card. You own only the drawing, through a single `Render` hook — bring whatever image library fits (ImageSharp, SkiaSharp, or a headless browser screenshotting an HTML template).

The same option works on every host shape: `DocSiteOptions` and `BlogSiteOptions` forward a `SocialCards` property; a bare host sets it on `PenningtonOptions` directly.

## Before you begin

- A working Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- A production origin for `CanonicalBaseUrl` — OpenGraph crawlers require an absolute `og:image` URL, so without it the tags emit root-relative paths that work in dev but do not unfurl when shared

---

## Turn on generated cards

Set `SocialCards` with a `Render` hook. This is the complete `BlogSiteSocialCardsExample` host:

```csharp:symbol
examples/BlogSiteSocialCardsExample/Program.cs
```

`Render` is invoked once per page with the page's resolved metadata, the request's `IServiceProvider` (resolve anything registered — a font cache, theme options), and a cancellation token. Return the image bytes, or `null` to skip the page: its card route serves 404 and is omitted from the build.

Everything the hook receives arrives on the request:

```csharp:symbol
src/Pennington/SocialCards/SocialCardOptions.cs > SocialCardRequest
```

`Metadata` is the page's full typed front matter, so a renderer can pattern-match capability interfaces — tags, author, series — beyond the common fields. The remaining `SocialCardOptions` members have working defaults: cards publish under `/social-cards/`, at the 1200x630 OpenGraph standard, as `image/png`.

## Which pages get cards

A card exists for every page that projects a content record — the discovery seam that carries a page's typed front matter.

### Markdown pages

Automatic on every host shape: DocSite pages, DocSite and BlogSite blog posts, and markdown registered with `AddMarkdownContent<T>` on a bare host all project records, so each gets a card and the matching meta tags.

### The home page

BlogSite projects a site-identity record for `/` — title and description from `BlogSiteOptions` — so the root URL gets a card at `/social-cards/index.png` with no extra configuration. To give it a distinct design, branch on `request.CanonicalPath`, as the example above does.

### Razor pages

A routed `@page` component gets a card when it has sidecar metadata — a `{Component}.razor.metadata.yml` file next to the component:

```yaml
title: About us
description: Who we are and why we build this.
```

A Razor page without a sidecar projects no record, so it gets no card and no card meta tags.

### Pages to skip

Return `null` from `Render`. The card route 404s, the build omits the file, and the page keeps any site-wide default image instead.

## Use your own image for some pages

A page that authors its own `og:image` wins — the generated card's tags only fill gaps, through the same head reconciliation every contributor goes through (see [the head subsystem](xref:explanation.core.head-subsystem)). On BlogSite, `SocialMediaImageUrlFactory` is the per-post hook: return an image URL to use it for that post, or `null` to fall back to the generated card.

```csharp
new BlogSiteOptions
{
    SocialMediaImageUrlFactory = post =>
        post.FrontMatter.Tags.Contains("announcement") ? "/img/announcement-card.png" : null,
    SocialCards = new SocialCardOptions { Render = ... },
}
```

## Result

Every recorded page carries meta tags pointing at its card, absolute when `CanonicalBaseUrl` is set:

```html
<meta property="og:image" content="https://example.com/social-cards/blog/hello-card.png" data-head="meta:prop:og:image">
<meta name="twitter:image" content="https://example.com/social-cards/blog/hello-card.png" data-head="meta:name:twitter:image">
<meta name="twitter:card" content="summary_large_image" data-head="meta:name:twitter:card">
```

The static build bakes one PNG per page under `output/social-cards/`, mirroring the page tree, with the home page reserved as `index.png`.

## Verify

- Run `dotnet run` and open `/social-cards/<page-path>.png` — expect your rendered card; a 404 means the page projects no record (or `Render` returned `null`)
- View-source any post and confirm the three meta tags above point at the page's own card
- Run `dotnet run -- build output` and confirm `output/social-cards/` contains one `.png` per page, including `index.png` on BlogSite

## Related

- Explanation: [The head subsystem](xref:explanation.core.head-subsystem) — how card meta tags compose with page-authored tags
- How-to: [Add tags to the document head](xref:how-to.response-pipeline.head-contributor) — write your own head contributor
- Reference: <xref:reference.api.blog-site-options>
- How-to: [Publish an RSS feed](xref:how-to.feeds.rss)

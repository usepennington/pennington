---
title: Structured data and accessibility, on by default
description: Pennington sites now emit JSON-LD for rich search results, ship skip links and ARIA landmarks, and preload fonts to cut the flash of unstyled text.
author: Phil Scott
date: 2026-04-07
isDraft: false
tags:
  - seo
  - accessibility
  - structured-data
---

Three things a content site should do for its readers are now handled by the
DocSite and BlogSite templates: structured data for
search engines, accessibility landmarks for keyboard and screen-reader users,
and font preloading to reduce the flash of unstyled text.

## Structured data, so search engines understand your pages

Search engines read HTML, but they reward pages that tell them what they're
looking at. Pennington emits schema.org JSON-LD as a `<script>` tag in every
page's head. The DocSite template wires `Article`, `BreadcrumbList`, and
`WebSite` automatically; the BlogSite template wires `Article` and `WebSite`:

```html
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Article",
  "headline": "Introducing Pennington",
  "datePublished": "2026-04-04",
  "author": { "@type": "Person", "name": "Phil Scott" }
}
</script>
```

That's what powers rich results — the breadcrumb trails and article cards in
search listings. It needs one thing: an absolute base URL. Set `CanonicalBaseUrl`
and the JSON-LD appears; leave it unset and Pennington skips it rather than emit
broken relative links. See [hosting under a base
URL](xref:how-to.deployment.base-url).

The framework ships a small set of concrete records in `Pennington.StructuredData` —
`JsonLdArticle`, `JsonLdPerson`, `JsonLdWebSite`, and `JsonLdBreadcrumbList`, all
subclassing the abstract `JsonLdEntity` — and the templates emit those. You can add
your own for any schema.org type the framework doesn't ship. Subclass `JsonLdEntity`, attribute your fields with
`[JsonPropertyName]`, and pass the entity to `<StructuredData Entities="...">`.
Pennington serializes it through the same path as the built-in types. See
[Add a custom schema.org JSON-LD type](xref:how-to.rich-content.structured-data-custom-types)
for a worked `JsonLdRecipe` example.

## Accessibility landmarks

Pennington pages ship visually-hidden skip links so keyboard users can jump past
navigation straight to the content. The sidebar is a real `<nav>` element with
an `aria-label`, the header and footer navs are labeled too, and the main
content carries `id="main-content"` as the skip-link target.

None of this is configurable — these landmarks are wired up by default, so the
skip link and labels are there from the first page render rather than something
you have to remember to add.

## Font preloading

Custom fonts load after the browser parses your stylesheet, which means a beat
of fallback text before the real typeface swaps in. A preload hint tells the
browser to fetch the font earlier. Pass a `FontPreload[]` to
`DocSiteOptions.FontPreloads` and Pennington emits the `<link rel="preload">`
tags ahead of the stylesheet:

```csharp
new DocSiteOptions
{
    FontPreloads =
    [
        new FontPreload("/fonts/body.woff2"),
        new FontPreload("/fonts/display.woff2"),
    ],
};
```

The [font how-to](xref:how-to.theming.fonts) covers the full setup, including
the `@font-face` rules.

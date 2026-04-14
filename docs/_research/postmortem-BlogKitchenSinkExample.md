# Post-mortem — BlogKitchenSinkExample

## What was built

`examples/BlogKitchenSinkExample/` — the second how-to demo app. One
`AddBlogSite` call wires a fully-populated `BlogSiteOptions`: hero, five
`Project` entries, all four built-in social-icon fragments, four
`HeaderLink` entries, `CanonicalBaseUrl = "https://blog.example.com"`,
`AuthorName`/`AuthorBio`, and explicit `EnableRss = true` /
`EnableSitemap = true` (both default-true — set explicitly so reference
prose has a real property assignment to point at). Three dated posts
(`2024-01-15-…`, `2024-02-20-…`, `2024-03-10-…`) populate the recent-posts
card, archive, tag pages, and RSS channel. Two posts share a `series`
(`Pennington Field Notes`); one carries `repository:`; tags overlap across
all three to exercise the tag-index surface.

Configuration lives in `ServiceConfiguration.cs` with one small static
method per option surface — `BuildHero`, `BuildMyWork`, `BuildSocials`,
`BuildMainSiteLinks`, `BuildBlogSiteOptions`. How-to and reference fences
target these via `M:...,bodyonly`. `Program.cs` stays at five lines.

## Coverage

**How-tos (2/2):** 2.2.70 rss → `BuildBlogSiteOptions` (`EnableRss`,
`CanonicalBaseUrl`) + the three post markdown files (each with a
`date:`). 2.2.90 blogsite-homepage → the four builder helpers. RSS
channel and sitemap verified live.

**Reference pages (primary fixture for 2):** §3.1
`/reference/options/blogsite-options` — inventory now carries every
property symbol on `BlogSiteOptions` plus the four helper record types.
§3.8 `/reference/blogsite/social-icons` — `BuildSocials` wires all four
`SocialIcons.*Icon` fields; verified path counts (Github 1, Bluesky 1,
LinkedIn 4, Mastodon 2) match source.

## RSS channel shape (verified)

Channel: `<title>` (from `SiteTitle`), `<link>` (from `CanonicalBaseUrl`
+ `/`), `<description>` (from `Description`), `<atom:link rel="self">`.
Per item: `<title>`, `<link>` (absolute), `<guid isPermaLink="true">`
matching link, `<description>`, `<pubDate>` (RFC 1123 UTC), `<author>`.
No body HTML in RSS — description-only, as documented in app #8's
post-mortem.

## Sitemap shape (verified)

14 URLs total: 3 post URLs (with `<lastmod>` from `date:`), `/archive/`,
`/`, `/tags/`, `/topics/`, and 7 `/tags/<tag>/` URLs. Posts are the only
entries carrying `<lastmod>`. No hreflang (single-locale site). Each
`/tags/<tag>/` URL corresponds to one tag across the three posts.

## API reach notes (for reference authors)

- `BlogSiteOptions.ColorScheme`, `ExtraStyles`, `DisplayFontFamily`,
  `BodyFontFamily`, `AdditionalHtmlHeadContent`, `FontPreloads`,
  `AdditionalRoutingAssemblies`, and `SocialMediaImageUrlFactory` **exist
  on the type** but are not populated by this app. Reference prose can
  cite the property symbols; no runnable example demonstrates the
  surface end-to-end. This is deliberate — kitchen-sink scope is
  homepage data + RSS/sitemap.
- `BlogSiteFrontMatter.Uid`, `IsDraft`, `RedirectUrl`, `Search`,
  `Llms`, and `Search`/`Llms` opt-out behaviour are also not exercised
  in this fixture (covered in the DocSite kitchen sink via the
  equivalent `DocSiteFrontMatter` flags).

## Conventions for app #16

- The **helper-methods-over-regions** pattern (from #13) works equally
  well on BlogSite hosts. Keep one helper per option surface; the
  `Build…` prefix keeps the helper list sortable and the
  `BuildBlogSiteOptions` composition site obvious.
- Post filenames with a `YYYY-MM-DD-` prefix produce clean
  `/blog/<full-filename>/` URLs — BlogContentResolver does not strip
  the prefix. If a cleaner URL is needed, drop the date from the
  filename and keep it in `date:` front matter. For the kitchen sink,
  the dated filenames doubled as ordering hints.
- `EnableSitemap = true` produces `/sitemap.xml` automatically under
  `UseBlogSite` with no additional middleware — #16 can rely on this
  when demonstrating base-URL sub-path deployment.

No blockers. Entry #14 flipped to `complete`.

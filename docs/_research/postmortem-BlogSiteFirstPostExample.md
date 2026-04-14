# Post-mortem — BlogSiteFirstPostExample

## What was built

`examples/BlogSiteFirstPostExample/` — the second BlogSite tutorial app.
`Program.cs` is identical in shape to app #7 but drops the placeholder
`hello-world.md`; `EnableRss = true` and `EnableSitemap = true` are set
explicitly. The teaching surface is entirely in
`Content/Blog/my-first-post.md`, which populates every
`BlogSiteFrontMatter` field a post author touches. Two stage files —
`Stage1_BareFrontMatter.cs` (just `title`/`description`/`date`) and
`Stage2_FullFrontMatter.cs` (all fields) — expose `static string Source()`
helpers the tutorial extracts via `csharp:xmldocid,bodyonly`.

## BlogSiteFrontMatter reality — locked for apps #9 and #14

From `src/Pennington.BlogSite/BlogSiteFrontMatter.cs`. Record implements
`IFrontMatter`, `ITaggable`, `ISectionable`, `IRedirectable`. All fields
are `init` properties:

- `Title` (string, default `"Empty title"`)
- `Author` (string, default `""`)
- `Description` (string?, nullable)
- `Repository` (string, default `""`)
- `Date` (DateTime?, nullable)
- `IsDraft` (bool, default `false`)
- `Tags` (string[], default `[]`)
- `Series` (string, default `""`)
- `RedirectUrl` (string?, nullable)
- `Section` (string?, nullable)
- `Uid` (string?, nullable)
- `Search` (bool, default `true`)
- `Llms` (bool, default `true`)

Every spec-listed field (title, description, date, author, tags, series,
repository, section, redirectUrl) exists — none had to be skipped.

## Behavior observations

- **`EnableRss = true` is redundant** — default is `true` per app #7.
  Setting it explicitly is pure teaching value; the tutorial prose can
  point at a real property assignment.
- **RSS item shape**: `<title>`, `<link>`, `<guid isPermaLink="true">`,
  `<description>`, `<pubDate>` (RFC 1123 UTC), `<author>`. Body HTML is
  NOT emitted — the RSS channel is description-only. The channel itself
  carries `<title>`, `<link>` (from `CanonicalBaseUrl`), `<description>`
  (from `BlogSiteOptions.Description`), and an `atom:link` self-ref.
- **Tag pages** live at `/tags/<tag>/` (and `/topics/<tag>/` aliases).
  `/tags` is the index; each tag in `Tags[]` gets its own page. Tag
  links on the post-chrome footer render as `/tags/<tag>` (no trailing
  slash, but the route handles both).
- **Series banner** — populating `series:` makes the post chrome render
  a "This post is part of a series" block naming the post. With only
  one post in the series it lists the current post itself.
- **Repository surface** — populating `repository:` renders a "Source
  Code" link card at the bottom of the post chrome pointing at the URL.
- **Author not rendered on post page** — the BlogSite's default post
  template doesn't show `author:` inline on the post body (only through
  RSS `<author>` and the footer "© YYYY AuthorName"). App #9 should
  not assume a byline surface.
- **Section** flows into `ContentTocItem.Section` but does not visibly
  render in the default listing chrome.

## Verification

`dotnet build Pennington.slnx` clean, 0 errors. Dev server on
`http://localhost:5520/` — Playwright confirmed home card, post page
(with series banner, all tags, Source Code link), archive listing,
`/rss.xml` XML item with author + pubDate + description, and
`/tags/pennington/` tag page. Static build produced 12 pages including
`blog/my-first-post/index.html`, `rss.xml`, `sitemap.xml`, three tag
pages, `/archive`, and `/topics`. Generated post HTML carries every
front-matter surface (title, date, author, tags, section, repository).
`output/` cleaned.

No blockers.

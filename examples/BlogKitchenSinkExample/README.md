# BlogKitchenSinkExample

Kitchen-sink BlogSite — the BlogSite analogue of `DocSiteKitchenSinkExample`. Wide configuration surface (hero, projects, all four built-in social icons, multiple header links, RSS, sitemap, structured data) split across small `ServiceConfiguration` helpers so feed/SEO how-tos can xmldocid-fence into the exact surface they teach.

## Concepts

- Full `BlogSiteOptions` surface in one host
- `StructuredDataBuilder` — user-defined `JsonLdRecipe : JsonLdEntity` showing how to extend Pennington's JSON-LD surface without framework changes
- RSS channel at `/rss.xml`, sitemap at `/sitemap.xml` — discoverable on every page; the index also ships a `<link rel="alternate" type="application/rss+xml" href="/rss.xml">` so feed readers pick it up automatically
- Three dated posts so archive / tags / RSS are populated

## URL convention for posts

`Content/Blog/2024-01-15-getting-started-with-pennington.md` is served at `/blog/2024-01-15-getting-started-with-pennington/`. The framework treats the filename verbatim as the slug — the leading `YYYY-MM-DD-` is preserved in the URL because that's how the kitchen-sink template arranges posts chronologically in the source tree, and a stable date-prefixed slug is friendlier to archive listings and RSS GUID stability than a stripped form. To omit the date prefix from URLs, rename the file (e.g. `getting-started-with-pennington.md`) and rely on front-matter `date:` for ordering instead. Guessing the un-dated URL of a dated file returns a 404 — see `BlogSite/Blog.razor` (it sets `Response.StatusCode = 404` via the `Pennington.NotFound` marker when `BlogContentResolver` returns null).

## Referenced from

- `docs/.../how-to/feeds/rss.md` (`ServiceConfiguration.BuildBlogSiteOptions`) — xref verified
- `docs/.../how-to/feeds/sitemap.md` (same) — xref verified

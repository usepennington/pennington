# Post-mortem — BlogSiteScaffoldExample

## What was built

`examples/BlogSiteScaffoldExample/` — the first BlogSite-template app.
`Program.cs` is a single `AddBlogSite(() => new BlogSiteOptions { ... })`
call followed by `app.UseBlogSite()` + `await app.RunBlogSiteAsync(args)`.
One placeholder post (`Content/Blog/hello-world.md`) keeps the home listing
and RSS feed non-empty until tutorial #8 teaches `BlogSiteFrontMatter`.
Two stage files — `Stage1_BeforeAddBlogSite.cs` (the app-#1 bare-host
shape) and `Stage2_AfterAddBlogSite.cs` (final) — each expose a static
`Run(string[] args)` the tutorial extracts via `csharp:xmldocid,bodyonly`.

## BlogSite API reality — locked conventions for apps #8, #9, #14

- **`BlogSiteOptions` record** (from `src/Pennington.BlogSite/BlogSiteOptions.cs`):
  **Required `init`** — `SiteTitle` (string), `Description` (string).
  **Optional** — `CanonicalBaseUrl` (string?), `ColorScheme` (IColorScheme?),
  `ContentRootPath` (string, default `"Content"`), `BlogContentPath`
  (string, default `"Blog"`), `BlogBaseUrl` (string, default `"/blog"`),
  `TagsPageUrl` (string, default `"/tags"`), `AuthorName`/`AuthorBio`
  (string?), `EnableRss` (bool, default **true**), `EnableSitemap`
  (bool, default **true**), `HeroContent`, `MyWork` (`Project[]`),
  `Socials` (`SocialLink[]`), `MainSiteLinks` (`HeaderLink[]`),
  `ExtraStyles`, `DisplayFontFamily`, `BodyFontFamily`,
  `AdditionalHtmlHeadContent`, `FontPreloads`, `AdditionalRoutingAssemblies`,
  `SocialMediaImageUrlFactory`. Note: spec called out "author name,
  canonical base URL, content paths" — confirmed verbatim. There is no
  `SiteUrl` or `BaseUrl` field; it is `CanonicalBaseUrl`.
- **Default content path** is `{ContentRootPath}/{BlogContentPath}` =
  `Content/Blog` (capital B). Posts map to `/blog/<slug-without-date>`.
- **`AddBlogSite` transitively wires** `AddPennington` (with `SiteTitle`,
  `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, and an
  `AddMarkdownContent<BlogSiteFrontMatter>` bound to `Content/Blog` → `/blog`),
  `AddMonorailCss`, `AddRazorComponents`, all eight Pennington.UI Mdazor
  components (`Badge`, `BigTable`, `Card`, `CardGrid`, `CodeBlock`,
  `LinkCard`, `Step`, `Steps`), and BlogContentResolver/BlogSiteContentService.
  **Apps #8, #9, #14 must NOT re-register any of these.**
- **URL shape:** `/` (Home), `/archive`, `/blog/<slug>/`, `/tags`,
  `/tags/<encoded-tag>/`, `/topics` + `/topics/<encoded-tag>/` (aliases),
  `/rss.xml`. No date prefix — the slug is the bare markdown filename.
- **RSS auto-emits** at this scaffold level (`EnableRss = true` by default);
  `/rss.xml` is wired inside `UseBlogSite`. Sitemap likewise defaults on.
  No explicit flag needed until you want to suppress them.

## Verification

`dotnet build Pennington.slnx` clean, 0 new warnings. Dev server on
`http://localhost:5510/` — Playwright confirmed title `Scaffold Blog -
Recent Posts`, hello-world article card linking to `/blog/hello-world/`,
footer `© 2026 Author Name`, search + dark-mode buttons in header;
`/blog/hello-world/` returned title `Scaffold Blog - Hello world`;
`/archive`, `/tags`, `/rss.xml` all 200, RSS channel carried
`<link>https://example.com/</link>`. Static build produced 9 pages
(`index.html`, `archive/`, `blog/hello-world/index.html`, `rss.xml`,
`sitemap.xml`, `styles.css`, `tags/`, `topics/`, `search-index-en.json`,
`_content/`). `output/` cleaned.

No blockers.

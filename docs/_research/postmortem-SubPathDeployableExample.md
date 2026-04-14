# Post-mortem — SubPathDeployableExample

## What was built

`examples/SubPathDeployableExample/` — fourth and final how-to demo,
backing all five §2.4 Publishing & Deployment recipes. Tiny `AddDocSite`
host (single `Guides` area, home page, one nested page) so
`BaseUrlHtmlRewriter` is observable without noise. Teaching artefacts
are five **sibling fixture files** the how-tos embed via `path:` fences.

## Fixtures shipped

- **`deploy.yml`** — GitHub Pages: `setup-dotnet@v4`, `BASE_URL=/${{ github.event.repository.name }}` (portable across forks), `.nojekyll`, `upload-pages-artifact@v3` → `deploy-pages@v4`.
- **`staticwebapp.config.json`** — Azure SWA: routes for sitemap/llms/`_content/*`, `navigationFallback` excluding static extensions, MIME map, security headers.
- **`netlify.toml`** — build command consuming `${BASE_URL:-/}`, `publish = "output"`, immutable cache headers, `status = 404` redirect.
- **`nginx.conf`** — `try_files $uri $uri/ /404.html` covers DocSite's `<slug>/index.html`, immutable cache for `_content/`, `error_page 404`.
- **`web.config`** — IIS staticContent MIME, `httpErrors` → `/404.html`, directory-trailing-slash redirect.

## Base-URL rewriting verification

Built `dotnet run -- build /my-sub-path` and inspected output.

**Rewritten by `BaseUrlHtmlRewriter` (HTML only):** `<a href>` (authored
+ framework), `<link rel=stylesheet href>`, `<script src>`,
`<form action>`, plus `<body data-base-url>` so the SPA engine can
reproduce the prefix on dynamic links.

**Not rewritten — gaps worth knowing:**

- **`sitemap.xml`** — FIXED (plan P0-3). Previously `<loc>` stayed unprefixed
  unless `CanonicalBaseUrl` was set. The feed-builder DI factory in
  `PenningtonExtensions.cs` now falls back to `OutputOptions.BaseUrl` when
  `CanonicalBaseUrl` is missing, so `dotnet run -- build /sub/` produces
  `<loc>/sub/page/</loc>` — a crawler-resolvable URL. Users who want
  fully-qualified `<loc>` values continue to set `CanonicalBaseUrl =
  "https://host/my-sub-path"` (the correct form per the sitemap protocol);
  that path still wins over the BaseUrl fallback. `RssFeedBuilder` gets
  the same fallback. Regression coverage:
  `Build_SubPathBaseUrl_WithoutCanonicalUrl_ProducesPrefixedRelativeUrls`,
  `PenningtonDiFactory_FallsBackToBaseUrl_WhenCanonicalBaseUrlIsMissing`,
  and `PenningtonDiFactory_ExplicitCanonicalBaseUrl_WinsOverBaseUrl` in
  `tests/Pennington.Tests/Feeds/SitemapBuilderTests.cs`.
- **`search-index-en.json`** — **intentionally not rewritten**. The page
  `url` fields stay root-relative because the client-side search UI in
  `src/Pennington.UI/wwwroot/scripts.js` stitches in `data-base-url` at
  click time (`baseUrl ? ${baseUrl}${doc.url} : doc.url`). Prefixing
  server-side would double-prefix at runtime. Static analyzers reading
  the JSON as a standalone artifact will see unprefixed paths — by
  design, not a bug.
- **`llms.txt`** — link targets written relative (`_llms/...`, no leading
  `/`), so they resolve under any deploy mount. Intentional, no fix needed.
- **Note on `HtmlResponseRewritingProcessor`**: still gates on `text/html`
  (line 32). That is correct — sitemap is fixed at the source above, and
  search-index is intentionally not rewritten. Non-HTML outputs no longer
  need the HTML rewriter chain.

## GitHub Actions base-URL approach

Derived, not hardcoded: `BASE_URL: /${{ github.event.repository.name }}`.
Custom-domain users override with `BASE_URL=""`. Workflow comment calls
this out explicitly.

## Deployment caveats

- **Trailing slashes.** DocSite emits `<slug>/index.html`, so canonical
  URLs end in `/`. Nginx `try_files`, IIS default document, Azure
  `trailingSlash: "auto"`, and Netlify Pretty URLs all handle it.
- **`_content/` prefix is required for RCL assets.** Hosts that strip
  leading underscores (Jekyll, some CDN defaults) break `Pennington.UI`'s
  shipped JS. `.nojekyll` is the GitHub Pages workaround; equivalent
  settings exist on every other host.

## No blockers

Build clean, Playwright validates `/` and `/guides/first-page/`, static
build at root and at `/my-sub-path` both emit 8 pages. #16 → `complete`.

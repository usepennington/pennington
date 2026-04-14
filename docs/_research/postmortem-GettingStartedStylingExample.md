# Post-mortem — GettingStartedStylingExample

## What was built

`examples/GettingStartedStylingExample/` — the three-page shape from app #2
plus MonorailCSS. `Program.cs` wires `AddMonorailCss` (with a
`NamedColorScheme` of indigo / pink / cyan / amber / slate) alongside the
existing `AddPennington`, then adds `app.UseMonorailCss()` between
`UsePennington()` and the markdown `MapGet` fallback. The previously inline
HTML-template layout is extracted into a shared `Layout.Render(title, navTree,
bodyHtml)` helper so the body, header, nav, article, and footer all carry
utility classes (`bg-base-50`, `text-primary-700`, `max-w-3xl`, etc.) and
`contact.md` additionally contains an inline `<p class="text-primary-700
font-semibold">` to prove class collection from markdown-authored elements.

Stage files: `Stage1_WithoutStyling.cs` (no MonorailCSS), `Stage2_AddMonorailCss.cs`
(DI + layout, no endpoint), `Stage3_UseMonorailCss.cs` (final). Each has a
static `Run(string[] args)` body the tutorial can pull via
`csharp:xmldocid,bodyonly`.

## Verification

- `dotnet build Pennington.slnx` — clean, 0 errors.
- Dev server at `http://localhost:5477/` — Playwright confirmed titles
  (Welcome / About / Contact), `<link rel="stylesheet" href="/styles.css">`
  on every page, computed `background-color: oklch(0.984 0.003 247.858)` on
  `<body>` (slate-50), 30px h1 from `text-3xl`, and on `/contact` the inline
  `<p>` computed `font-weight: 600` and an indigo-700 color from
  `text-primary-700`. `/styles.css` returned 200, `text/css`, 61,830 bytes,
  containing the expected class selectors.
- Static build — `dotnet run -- build output` reports
  `Build Complete — 6 pages in 0.5s` (three HTML pages, `styles.css` at
  61,832 bytes, `sitemap.xml`, `search-index-en.json`). `output/` cleaned.

## API reality vs. spec / conventions

- **Bare-host "layout" = an HTML-string helper, not a Razor component.**
  `Pennington.MonorailCss` is agnostic about how HTML is produced —
  `CssClassCollectorProcessor` is an `IResponseProcessor` that observes any
  `text/html` response body as it flows through `ResponseProcessingMiddleware`.
  So extracting the page shell into `Layout.Render` is the cleanest shape
  without pulling in `Pennington.UI` or `Pennington.DocSite`. Teaching moment
  preserved, dependency graph stays minimal.
- **`ColorNames` lives in `MonorailCss.Theme`**, not `MonorailCss`. First
  attempt to `using MonorailCss;` failed CS0103 — the enum-like string
  constants ship in the `MonorailCss.Theme` namespace. Worth noting for
  later apps that configure palettes.
- **`NamedColorScheme` requires all five `*ColorName` properties** — they're
  `required init`, so picking just `PrimaryColorName` is not an option at
  this tutorial's level.

No blockers.

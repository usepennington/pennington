# Post-mortem — DocSiteScaffoldExample

> **Resolution (2026-04-14):** All flagged items addressed. See plan at
> `~/.claude/plans/abstract-noodling-taco.md`.
>
> - **S1 — `build output` BaseUrl quirk.** Engine arg parsing was already
>   correct (`OutputOptions.FromArgs` at `src/Pennington/Generation/OutputOptions.cs:25-26`
>   — `args[1]` is BaseUrl, `args[2]` is output dir). Fixed the misleading
>   comments in `examples/DocSiteScaffoldExample/Program.cs`, `Stage3_UseDocSite.cs`,
>   `examples/GettingStartedMinimalSiteExample/`, and `examples/BlogSiteScaffoldExample/Program.cs`
>   to the canonical `build <baseUrl> <outputDir>` form (both optional;
>   defaults `/` and `output`).

## What was built

`examples/DocSiteScaffoldExample/` — the first DocSite-template app. `Program.cs`
is a single `AddDocSite(() => new DocSiteOptions { ... })` call followed by
`app.UseDocSite()` + `await app.RunDocSiteAsync(args)`. `DocSiteOptions` is
populated with `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`,
`FooterContent`, and a two-entry `Areas` list (`Guides`, `Reference`). Each
area's `Slug` matches a top-level folder under `Content/` that carries one
`index.md` page.

Stage files: `Stage1_PenningtonOnly.cs` (app #1 shape — `AddPennington` + a
manual `MapGet` fallback), `Stage2_AddDocSite.cs` (the `AddDocSite` DI call
alone, no middleware yet), `Stage3_UseDocSite.cs` (final — identical to
`Program.cs`). Each is a static `Run(string[] args)` body the tutorial
extracts via `csharp:xmldocid,bodyonly`.

## Verification

- `dotnet build Pennington.slnx` — clean, 0 new errors/warnings.
- Dev server on `http://localhost:5488/` — Playwright confirmed the full
  DocSite chrome: left-rail sidebar with the "Content areas" navigation
  listing **Guides** and **Reference**; header with search, dark-mode toggle,
  and GitHub icon linking to the configured URL; configured footer HTML
  rendered below the article region; page titles in the format
  "`{Title}` — Scaffold Docs" (from `DocSiteOptions.SiteTitle`). `/guides/`
  and `/reference/` both resolved to their area index pages with the active
  area highlighted.
- Static build — `dotnet run -- build output` reported `Build Complete —
  8 pages in 0.6s` (per-area `index.html`, `404.html`, `sitemap.xml`,
  `search-index-en.json`, `styles.css`, plus `_content`/`_llms`/`_spa-data`
  support dirs). `output/` cleaned.

## API reality / conventions for later DocSite apps (#5, #6, #13)

- **Areas are a pure `DocSiteOptions.Areas` declaration.** `ContentArea(Title,
  Slug, Icon?)` is a record; the `Slug` is the **only** thing binding an area
  to content — it matches both the URL prefix and the top-level directory
  name under `ContentRootPath`. No per-area `ContentPath` option, no second
  `AddMarkdownContent` call. `ContentResolver.GetTocItemsForAreaAsync` does
  the filtering by comparing `ContentTocItem.HierarchyParts[0]` to each
  area's slug.
- **No explicit MonorailCSS registration needed.** `AddDocSite` already calls
  `AddMonorailCss` and `UseDocSite` calls `UseMonorailCss`. Same for Pennington
  core (`AddPennington`/`UsePennington`), Mdazor components, and SPA nav.
  Later DocSite apps should *not* re-register these.
- **`DocSiteOptions.HeaderContent` and `FooterContent` are raw HTML strings**
  emitted via `MarkupString`, not Razor fragments. Keep them minimal at this
  tutorial level.
- **Build-report "broken links" on base-URL args.** Same quirk as app #1:
  `build output` sends `output` into `BaseUrl`, producing `output/`-prefixed
  anchors on internal links. Harmless for verification; later deployment
  tutorials (app #16) will teach the real base-URL story.

No blockers.

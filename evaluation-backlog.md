# Pennington Evaluation Backlog

Prioritized backlog from 10 parallel agent evaluations against varied static-site scenarios. Each agent had only `https://usepennington.github.io/pennington/llms.txt` and a blank context.

**Scenario verdicts:** 9 YELLOW, 1 RED (magazine site). Scenarios referenced below as [S1]–[S10]:

| | Scenario | Verdict |
|---|---|---|
| S1 | Personal portfolio | YELLOW |
| S2 | Technical blog | YELLOW |
| S3 | API documentation site | YELLOW |
| S4 | Multi-language docs (5 locales) | YELLOW |
| S5 | SaaS marketing + product docs | YELLOW |
| S6 | Recipe / cookbook | YELLOW |
| S7 | Multi-year conference site | YELLOW |
| S8 | OSS library project site | YELLOW |
| S9 | 2k-page internal wiki | YELLOW |
| S10 | News / magazine (5k articles) | RED |

---

## P0 — Hard blockers

Each of these turned at least one scenario red or yellow on a feature that is table stakes for the category.

- ~~**Pagination** for archive / taxonomy / author pages. No documented support; untenable at 5k pages. [S10, S9]~~ — **Done.** `BlogSiteOptions.PostsPerPage` (default 10) paginates `/archive` and per-tag listings at `/page/N/`. Manual pattern for custom `MarkdownContentService` documented at `how-to/discovery/pagination.md`.
- ~~**Versioning** for docs (e.g., `/v1/`, `/v2/`). No primitive, no how-to. Blocks API docs and OSS sites that promise multiple supported majors. [S3, S8]~~ — **Done.** `DocSiteOptions.Areas` slugs become URL prefixes for per-version content folders; keyed `AddApiMetadataFromCompiledAssembly` + `AddApiReference` with nested `RoutePrefix` covers per-version API reference. Two versions of the *same* assembly require `<PackageDownload>` for the off-version (NuGet's one-version-per-assembly rule). Documented in `how-to/versioning/docsite.md` with paired `VersionedDocSiteExample` (Humanizer.Core 2.8.26 + 2.14.1).
- ~~**Scheduled / future-dated publish.** Only binary `isDraft` exists. Editorial sites depend on embargoed releases. [S10]~~ — **Done.** A page whose `date:` front matter is later than the build clock is treated the same as `isDraft: true` (renders in dev, excluded from build output, feeds, search). `TimeProvider` is injectable so CI overrides and tests use `FakeTimeProvider`. Documented in `how-to/pages/drafts-tags-ordering.md`.
- **Per-taxonomy RSS / Atom feeds** (e.g., `/feeds/news.xml`). Only one site-wide `/rss.xml`. [S10, S8]
- **Incremental / cached builds.** Build-by-HTTP-crawl with no incremental story at 5k pages × daily CI is a hard sell. [S10, S9]
- **Extending shipped template front-matter types** without dropping to bare `AddPennington` (which loses BlogSite/DocSite routes). Affects almost every non-trivial scenario. [S1, S2, S5, S6, S7, S10]

## P1 — High-value gaps (multi-scenario)

Features flagged as missing or under-documented by 2+ evaluators.

- **Dark-mode toggle** — reference page, persistence model, `prefers-color-scheme` integration. [S1, S5, S8]
- **Edit-on-GitHub button** — front-matter key + rewriter or DocSite slot. [S8]
- **Copy-to-clipboard on code blocks** — ship in `Pennington.UI`. [S8]
- **Per-page OG / Twitter cards with image rendering.** `SocialMetadata` record exists but wiring is undocumented; no PNG-rendering primitive. [S2, S5, S7, S8, S10]
- **Reading time** primitive (front-matter key or shortcode). [S2, S10]
- **Backlinks** — graph of "pages that link to this page." [S9]
- **Math notation** (KaTeX or MathJax). [S9]
- **Git `last-updated` dates.** No `IDateable` capability, no git integration. [S9]
- **"Render typed front matter through a Razor view"** — close the loop from `AddMarkdownContent<T>` to a per-page Razor template that has typed access to `T`. The biggest doc gap; recipe and conference agents both got stuck here. [S6, S7]
- **`Pennington.StructuredData` extensibility** — `JsonLdSerializer` supports only Article / Breadcrumb / WebSite, with no documented extension seam. Recipe sites (and many others) need their own schema.org types. [S6]
- **Mdazor non-primitive parameter binding** — currently primitive-only, so passing structured front-matter data into a component-in-markdown is impossible. [S6]
- **Auto-API in sidebar** — currently search/xref only. Hard problem for API-reference-first sites. [S3]
- **Sitemap with `priority` and per-locale `hreflang`.** Sitemap how-to silent on both. [S4, S10]

## P2 — Important, workaround feasible

Features missing or thin but with clear (if non-trivial) workarounds.

- **`Project` record richness** for BlogSite — add `Image`, `Tags`, `TechStack`, `RepoUrl`, `LiveUrl`. Today: `Title`, `Description`, `Url`. [S1]
- **Project / case-study detail page pattern** that doesn't repurpose blog posts. [S1, S5]
- **Year / month archive routes** (e.g., `/2025/06/`). [S2]
- **Author profile pages** — built-in `Author` is a string; magazine needs `string[]` + per-author pages. [S10]
- **Featured-content selection** pattern (weekly editor pick, hero rotation). [S5, S10]
- **`Image` / `Gallery` / `Hero` UI components** in `Pennington.UI`. [S6, S1]
- **Print stylesheet / print-friendly view** affordance. [S6]
- **Wiki-style `[[PageName]]` short-slug links** (vs. requiring `uid:` discipline on every page). [S9]
- **Cross-content-service references** — linking from a custom service's items (talks) to another custom service's items (speakers). [S7]
- **Pagination of taxonomy term pages.** [S9, S10]
- **Series index page** (`/series/{name}/`) — `series:` key creates a banner but no index route. [S2]

## P3 — Polish, niche, or architectural

- **CJK tokenization** for search (Japanese, Chinese-Simplified, Korean). [S4]
- **RTL** end-to-end how-to — `LocaleInfo.Direction` exists, downstream consumption is undocumented. [S4]
- **Atom feed** (only RSS today). [S2]
- **Custom `SearchIndexDocument` fields** (or document "fold into rendered HTML" as the canonical workaround). [S6]
- **Bulk-load `TranslationOptions`** from .resx / JSON for many locales. [S4]
- **Pseudo-localization / missing-key reporting** for translation workflows. [S4]
- **Per-locale redirects.** [S4]
- **Confirm default TextMateSharp language list** (TS, SQL, HCL specifically). [S2]
- **`AddTaxonomy` over data-file-backed content** (currently markdown-only). [S7]
- **Build crawler discovery of `MapGet` endpoints** from custom `IContentService` — clarify whether registration is single-source or dual. [S7]

## Documentation backlog

These are documentation problems, not framework problems — many would convert YELLOW verdicts to GREEN.

### Bugs / inconsistencies

- **HTML site 404s** for pages whose `_llms/*.md` mirrors resolve. Flagged by [S1] and [S6]. Likely a publishing pipeline gap.
- **`how-to/pages/images-and-assets.md` contradicts `reference/front-matter/keys.md`** on cover-image support. One says front matter has it; the other says it doesn't. [S2]
- **llms.txt lists topics without URLs** in places, forcing agents (and humans) to guess slugs. [S1]

### Missing how-tos / tutorials

- **"Render typed front matter through a Razor view"** end-to-end. The #1 doc gap. [S6, S7]
- **"Extend BlogSiteFrontMatter / DocSiteFrontMatter"** while keeping built-in routes — or definitively document the migration path to bare `AddPennington`. [S2, S5, S6, S10]
- **"Marketing + docs + blog in one host"** — assemble three existing recipes into a combined tutorial. [S5]
- **"Magazine / multi-author publication"** worked example. [S10]
- **"Conference / data-driven site"** worked example tying `AddDataFile`, custom `IContentService`, bare-host Razor, and OG artifacts. [S7]
- **"API reference for a versioned SDK"** how-to. [S3]
- **"Multi-locale SEO"** combining sitemap + hreflang + robots with sample XML output. [S4]
- **"Right-to-left locales"** end-to-end (CSS flip, DocSite chrome). [S4]
- **"Wiki patterns"** explanation covering short-link / backlinks / git-date gaps and recommended workarounds. [S9]
- **"Personal site / portfolio"** recipe — wire BlogSite for non-blog use (rename `/blog`, disable RSS, custom front matter, detail pages). [S1]
- **OG / Twitter cards** how-to beyond `SocialMediaImageUrlFactory`. [S10, S8]
- **Generating PNG OG images** at build time — recommend SkiaSharp/ImageSharp and show a worked layout. [S7]
- **Reading time** recipe via shortcode / response rewriter. [S2]
- **Roslyn in CI** — does `MSBuildWorkspace` need a full restore? CI-specific guidance. [S8]
- **Math (KaTeX/MathJax)** how-to — even if it's "BYO preprocessor, here's 20 lines." [S9]
- ~~**Versioning** how-to (or an explicit "not yet supported" note). [S8]~~ — **Done.** See `how-to/versioning/docsite.md`.
- **Edit-on-GitHub** how-to. [S8]
- **`,xmldocid-diff`** modifier — mentioned but unexplained. [S3]

### Reference gaps to fill

- **Search engine identification** (Lunr? FlexSearch? custom?) and minimal client-UI integration in `how-to/discovery/search.md`. [S9, S2, S10]
- **Search coverage by content type** — confirm whether auto-API pages and blog enter the index. [S3, S8]
- **Dark-mode reference page** in `reference/ui/`. [S1, S8]
- **`SocialMetadata` end-to-end** — who populates it, what HTML it emits, how to override per-page. [S2]
- **Default TextMateSharp language list.** [S2]
- **Drafts in dev vs. build** — does `isDraft: true` show locally and strip at build? [S2]
- **xref collision warnings** — does the system flag duplicate `uid:` declarations? [S9]
- **Navigation tree depth limits** — UI behavior at 8–10 levels. [S9]
- **`csharp:xmldocid` requires sibling `.slnx`** — call out the implication for split docs/library repos. [S8]
- **CDN dependencies** (Mermaid loads from jsdelivr) — explicit air-gapped/VPN guidance. [S9]

### Scale & operations

- **Publish build-time benchmarks** at 100 / 500 / 2k / 5k / 10k pages. Single biggest credibility gap for considering Pennington at scale. [S9, S10, S7]
- **Document the absence (or presence) of incremental builds** explicitly. [S10]
- **Hot-reload responsiveness at scale** (2k watched files). [S9]

---

## Suggested sequencing

**Sprint 1 (close the magazine RED):** Pagination, scheduled publish, per-taxonomy RSS, multi-author front-matter pattern. Convert S10 to YELLOW.

**Sprint 2 (close the OSS YELLOW):** Versioning primitive, edit-on-GitHub, copy-to-clipboard, dark-mode toggle. Converts S3, S5, S8 toward GREEN.

**Sprint 3 (close the doc-loop gap):** Worked example for "typed front matter → Razor view," extend-template recipes, fix HTML 404s, reconcile contradictions. Converts S1, S2, S5, S6, S7 toward GREEN.

**Sprint 4 (scale story):** Benchmarks, optional incremental builds, search engine documentation, custom search index fields. Converts S9, S10 toward GREEN.

**Sprint 5 (i18n polish):** Sitemap hreflang, RTL, CJK tokenization, bulk translation loading. Converts S4 toward GREEN.

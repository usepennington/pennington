# Post-mortem — DocSiteKitchenSinkExample

## What was built

`examples/DocSiteKitchenSinkExample/` — the first how-to demo app. One
`AddDocSite` call wires two areas (`main`, `api`), two locales (en
default + fr), an `AlgorithmicColorScheme`, two font preloads, a
`@font-face` block as `ExtraStyles`, a custom footer, and a `GitHubUrl`.
A single `AddMdazorComponent<FeatureCallout>` registers the app's own
Razor component. `Content/main/` holds 14 markdown pages — one per how-to
— plus colocated assets; `Content/api/` holds a two-page API area;
`Content/fr/main/` holds a French index and a translated alerts page.
`wwwroot/shared.png` is the shared-asset fixture.

Configuration lives in a small `ServiceConfiguration` static class with
one helper method per surface (`BuildAreas`, `ConfigureLocalization`,
`BuildColorScheme`, `BuildFontPreloads`, `BuildExtraStyles`,
`BuildFooter`, `BuildDocSiteOptions`). How-to fences target these
methods via `M:...,bodyonly` so each recipe lifts exactly the one
option surface it teaches — no line-range math, no `#region` markers
needed. `Program.cs` stays at five lines and is itself a
`:path`-addressable fence candidate.

## How-to coverage (18/18 wired, with caveats)

**All 18** how-tos referenced by entry #13 have a concrete fence target
(a markdown page or a helper method body). Content-authoring §2.1
(all 12): all wired as dedicated pages and verified via Playwright
(alerts: 5 distinct `markdown-alert-*` classes; tabbed code: 3-tab
ARIA tablist with `bash` active by default; FeatureCallout: 3 instances
with `Fast`, `Theme aware`, `Heads up` h4 titles; cross-references:
`uid` links resolve to `/main/cross-references-b/`; images: shared.png
and colocated.png both 200 OK).

Configuration §2.2 (7 wired): 2.2.10 multiple-sources →
`BuildAreas`; 2.2.20 search → `Content/main/hidden.md` with
`search: false` verified absent from `/search-index-en.json`;
2.2.30 monorail-css → `BuildColorScheme` + `BuildExtraStyles`;
2.2.40 fonts → `BuildFontPreloads` + `BuildExtraStyles`
(`DisplayFontFamily`/`BodyFontFamily`); 2.2.50 localization →
`ConfigureLocalization` verified at `/fr/main/alerts` (page title
"Alertes et encadrés"); 2.2.60 llms-txt → `Content/main/llms-hidden.md`
verified absent from `/llms.txt`; 2.2.80 sitemap → `/sitemap.xml`
emits 34 URLs with `xhtml:link rel="alternate"` hreflang entries;
redirect-source and draft pages excluded.

## API dead-ends discovered

Three how-to surfaces are **not directly configurable through
`DocSiteOptions`** — `AddDocSite` hard-codes them internally and the
options record exposes no override:

1. **`PenningtonOptions.AddMarkdownContent<T>` is fire-once.**
   `AddDocSite` calls it with `DocSiteFrontMatter` inside its own
   `AddPennington(...)` callback. Registering a second markdown source
   with a custom front-matter type (`ApiFrontMatter`) requires dropping
   to the bare `AddPennington` host. The kitchen sink ships
   `ApiFrontMatter` as a compile-only capability-interface example and
   demonstrates "multiple content roots" via the second `ContentArea`
   entry — the DocSite-idiomatic answer. `MarkdownContentOptions.ExcludePaths`
   is not surfaced either, same reason.
2. **`SearchIndexOptions.ContentSelector` / `LlmsTxtOptions` are pinned
   to `#main-content`.** `DocSiteServiceExtensions` sets them inside its
   `AddPennington` callback via `??=` — so a post-configure override
   would need to arrive before `AddPennington` runs, which the
   user's DI chain cannot express. Search / llms.txt how-tos are
   instead backed by front-matter exclusions (`search: false`,
   `llms: false`), which **are** user-controllable and which the
   fixture pages verify end-to-end.
3. **`MonorailCssOptions.CustomCssFrameworkSettings` not exposed on
   `DocSiteOptions`.** Only `ColorScheme` + `ExtraStyles` flow
   through. The monorail-css how-to covers the two reachable knobs.

None of these block the kitchen sink — they flag pieces the future
ExtensibilityLab app (#15) or a future `DocSiteOptions` extension can
pick up.

## Runtime redirect, xref-FR fallback

`DocSiteFrontMatter.RedirectUrl` suppresses the page from nav/search/
llms.txt (via `IRedirectable` in `MarkdownContentService`) but
**DocSite's `Pages.razor` does not emit a 301/302 at runtime** —
visiting `/main/redirect-source` in dev renders the page body. Only
`OutputGenerationService` translates a 301/302 response into a
meta-refresh HTML file during static build, and the DocSite never
produces that response. Fixture prose ("This page has moved — if you
see this body the redirect did not fire, in static output…") makes
the behaviour self-documenting.

Xref resolution in multi-locale sites surfaced a subtle quirk:
`DiscoverRoutesWithFallbacks` emits a fallback route per non-default
locale for every default-locale file, and `XrefResolver`'s dictionary
overwrites on the LAST insertion. Result: `<xref:...-b>` in EN body
content can resolve to `/fr/main/cross-references-b/` (a fallback URL
that still renders EN content via locale fallback). Documented for
future resolver tweaks; does not break the tutorial.

## Conventions the next agent should inherit

- **Helper-methods-over-regions** for Program.cs how-tos. One static
  method per option surface is a clean `xmldocid,bodyonly` target and
  scales past six or seven surfaces where `#region` markers noise up
  the file.
- **Fixture pages for boolean front-matter flags.** `hidden.md` +
  `llms-hidden.md` are small, named for the exclusion they prove, and
  their absence from the auxiliary outputs is verifiable in one curl.
- **Compile-only types for "define-your-own" patterns.** `ApiFrontMatter`
  isn't wired at runtime but is a symbol table target that the how-to
  can read. Legal pattern when the host can't actually consume the
  type; the how-to prose names the bare-host registration shape.

No blockers. Entry #13 flipped to `complete`.

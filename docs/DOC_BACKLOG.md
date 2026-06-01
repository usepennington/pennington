# Documentation backlog — coverage gaps

Deferred work from the pre-launch documentation audit (2026-05-28). These are **shipped, user-facing features with no or thin Diátaxis coverage** — not launch blockers. The audit's accuracy/voice/sample fixes were applied directly; this file tracks the new pages still to write.

Each item names the public API, where it lives in `src/`, and the recommended doc home. Pick by reader value, not list order; the multi-locale and `dotnet new` items are the highest-leverage.

## Undocumented shipped features (no Diátaxis page)

| Feature | Public surface | Lives in | Recommended home |
|---|---|---|---|
| **Translation audit** | `AddTranslationAudit`, `TranslationAuditOptions` (`RepositoryPath`, `IncludedLocales`, …) | `src/Pennington.TranslationAudit/` | How-to under `how-to/discovery/`, next to `localization.md`. Top gap for a multi-locale launch — currently undiscoverable that missing/outdated translations can gate the build and surface in the overlay. |
| **Metadata enrichers** | `IMetadataEnricher`, built-in `ReadingTimeEnricher` | `Pennington.Pipeline` (`ParsedItem.Derived` merge seam) | How-to under `how-to/markdown-pipeline/` — register an enricher, read `ParsedItem.Derived`; note reading-time ships built in and how to render it. |
| **Word-break rewriter** | `AddWordBreak`, `WordBreakOptions` (`CssSelector`, `MinimumCharacters`, `WordBreakCharacters`) | `Pennington.Infrastructure` (opt-in `IHtmlResponseRewriter`) | Section in `how-to/response-pipeline/html-rewriter.md` — currently named only as an example, never configured. |
| **Scrollable-tables extension** | Always-on; wraps `<table>` in `<div class="overflow-x-auto">` | `Pennington.Markdown` | Row in the `reference/markdown/extensions.md` catalog — it emits wrapper markup authors may style, and is the one always-on extension missing from the catalog. |
| **Remaining `Api*` Mdazor components** | `ApiReturns`, `ApiRemarks`, `ApiSeeAlso`, `ApiDefinitionList`, `FieldList`, `Field`, `ApiMemberList` (7 of 11) | `Pennington.DocSite.Api` (`ApiReferenceServiceExtensions.RegisterSharedOnce`) | Reference page listing all 11 inline API components (XmlDocId/Kind/Receiver/Source attributes + one-line purpose). Only 4 are documented today. |
| **Content-renderer replacement** | `ContentRendererServiceExtensions.ReplaceContentRenderer<TOld,TNew>` | `Pennington.Pipeline` | Note/example in `how-to/content-services/custom-content-service.md` — the safe swap seam; without it readers reach for fragile last-wins registration. |
| **AOT/trim YAML context** | `AddYamlContext` (source-generated `YamlSerializerContext`) | `Pennington.Infrastructure` | Brief how-to for AOT/trim authors — reflection-free front-matter parsing, relevant to the library's stated AOT goal. |
| **Content-service fan-out helpers** | `ContentServiceExtensions.DiscoverAllAsync`, `CollectTocEntriesAsync`, … | `Pennington.Content` | Mention in `how-to/content-services/custom-content-service.md` where advanced authors iterate `IEnumerable<IContentService>`, so they don't re-implement the fan-out. |

## Thinly documented topics (page exists but too thin to succeed from)

| Topic | Current state | Recommended addition |
|---|---|---|
| **TUI dev dashboard** | `AddTui`, `PenningtonTuiOptions` — blog post only; wiring is commented out in `docs/Pennington.Docs/Program.cs`. | How-to (dev experience): registration, the no-op conditions (build mode, under `dotnet watch`, redirected stdout), and the four panels. |
| **`dotnet new` templates** | `pennington`, `pennington-docs`, `pennington-blog` — blog-only coverage. | How-to (or getting-started variant) walking `dotnet new install Pennington.Templates` + each shortName; cross-link from the scaffold tutorials. |
| **Full `DocSiteOptions` surface** | Only ~5 chrome-slot seams walked in prose; ~25 properties exist. | Reference page enumerating `DocSiteOptions` grouped by concern (identity, chrome, theming, routing, localization, advanced), one line each. |
| **`ConfigureMarkdownPipeline`** | Referenced only indirectly (migration briefing). | How-to under `how-to/markdown-pipeline/` showing a concrete custom Markdig extension added after the built-ins. |
| **Dark-mode / theme toggle** | Mentioned as chrome; no dedicated page. | Reference/explanation note: toggle behaviour, inline head script + storage key, the `reinitializeForTheme` hook, the `dark:` variant convention. |
| **`IProgrammaticContentGenerator`** | `ProgrammaticContent.Text`/`.Binary` mentioned around `content-source.md`; no focused page. | Section in `custom-content-service.md` implementing the generator, distinct from a full `IContentService`. |
| **TreeSitter language config** | `:symbol` fence syntax is documented; `TreeSitterOptions.LanguageConfigs` / `WatchFilePatterns` are not. | Section in `focused-code-samples.md` (or a TreeSitter setup how-to) on per-language declaration config and watch patterns. |
| **`WithLlmsTxtEntry`** | Endpoint convention mentioned only in `blog/llms-txt.md`. | Section in `how-to/feeds/llms-txt.md` showing `WithLlmsTxtEntry<TBuilder>(title, description)` on a `MapGet` endpoint alongside the `*.llms.md` convention. |
| **`ISiteProjection` concept** | `ContentSelector` documented in `llms-txt.md`/`search.md`; the shared one-walk projection is not. | Short explanation page (`explanation/core/`) describing `ISiteProjection` as the single corpus walk feeding search, llms.txt, and the link audit, and how `ContentSelector` scopes it. |

## Known external limitation (not a docs gap)

- **Mdazor attribute parsing truncates at an apostrophe.** `<Card Title="What's new">` renders only `What`. Mdazor is an external NuGet package, so this is not fixable in this repo. The `ui-components-in-markdown.md` example was reworded to avoid the apostrophe; track an upstream fix and, until then, avoid apostrophes in Mdazor component attribute values authored from markdown.

# P1: Formalize Multi-Locale Composition Layer

## Problem
Localization infrastructure exists as skeleton types (`LocaleInfo`, `AlternateLanguagePage`, `LocalizationOptions`) but there's no orchestration layer that coordinates content services across locales. Setting up a multi-locale site requires manual wiring of per-locale content services in the app layer.

## Current State
- `LocaleInfo` record: `DisplayName`, `Direction` (ltr/rtl), `HtmlLang`
- `AlternateLanguagePage` record: links a page to its translation in another locale
- `LocalizationOptions`: registers locales with codes and display names, has `DefaultLocale`
- `ContentRoute` has a `Locale` property and `IsDefaultLocale` computed property
- `ContentRouteFactory` methods accept an optional `locale` parameter that prefixes URLs with `/{locale}/`
- `MarkdownContentServiceOptions` has no `Locale` field — each content source is locale-unaware
- `DocSiteOptions.ConfigureLocalization` exists as an `Action<LocalizationOptions>?` but the wiring is unclear
- `LanguageSwitcher.razor` component exists in Pennington.UI but has limited backing infrastructure
- An `examples/LocalizationExample/` exists

## Requirements
- Add a `Locale` property to `MarkdownContentOptions` so each markdown source can declare its locale
- Create a locale-aware content discovery mechanism: when multiple locales are configured, the system should automatically discover content per locale and establish alternate-page relationships
- Generate `<link rel="alternate" hreflang="...">` tags in the HTML head for pages that have translations
- The `LanguageSwitcher` component should receive alternate pages for the current route and render locale links
- The default locale content should be served at the base URL (no locale prefix); non-default locales at `/{locale}/` prefix
- Sitemap should include `xhtml:link rel="alternate"` entries for translated pages per the sitemap protocol
- Consider how navigation (sidebar TOC) works per locale — each locale should have its own navigation tree

## Key Files
- `src/Pennington/Infrastructure/PenningtonOptions.cs` — `MarkdownContentOptions` needs `Locale`
- `src/Pennington/Localization/LocaleInfo.cs`, `AlternateLanguagePage.cs` — existing types
- `src/Pennington/Content/MarkdownContentService.cs` — locale-aware discovery
- `src/Pennington/Feeds/SitemapService.cs` — add hreflang alternate links
- `src/Pennington.DocSite/Components/Layout/MainLayout.razor` — render hreflang links in head
- `src/Pennington.UI/Components/LanguageSwitcher.razor` — wire to alternate pages
- `examples/LocalizationExample/` — reference for expected behavior

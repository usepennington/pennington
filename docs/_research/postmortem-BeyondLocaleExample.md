# Post-mortem — BeyondLocaleExample

## Host choice — DocSite (not bare)

Chose `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`. Three reasons:

1. `DocSiteOptions.ConfigureLocalization` (an `Action<LocalizationOptions>`)
   is the one-liner API the tutorial wants to teach — no separate DI step.
2. `UseDocSite` already calls `UsePenningtonLocaleRouting` first-thing, so
   the reader never has to think about middleware ordering vs
   `MapRazorComponents`. (`UsePennington` also calls it defensively, but
   after endpoint routing — too late for Blazor `@page`.)
3. DocSite's `MainLayout.razor` embeds `LanguageSwitcher` inside the
   header bar with auto-built alternate URLs via
   `ContentResolver.GetAlternateLanguagesAsync`. The tutorial doesn't have
   to compose a layout — a bare-host version would have required building
   a Razor host and re-registering Mdazor/MonorailCSS components.

## Exact method/type names (verified in source)

- `Pennington.Infrastructure.LocalizationOptions` (despite the folder,
  this type is in the `Pennington.Infrastructure` namespace — the class
  lives inside `PenningtonOptions.cs`).
- `LocalizationOptions.DefaultLocale` (string), `Locales`
  (`IReadOnlyDictionary<string, LocaleInfo>`), `IsMultiLocale` (bool).
- `LocalizationOptions.AddLocale(string code, LocaleInfo info)` and a
  convenience overload `AddLocale(string code, string displayName)`.
- `Pennington.Localization.LocaleInfo(string DisplayName, string
  Direction = "ltr", string? HtmlLang = null)` — positional record.
- `Pennington.Infrastructure.PenningtonExtensions.UsePenningtonLocaleRouting(
  WebApplication)` — idempotent; no-ops when only one locale.
- `Pennington.UI.Components.LanguageSwitcher` — Razor component (default
  root namespace is `Pennington.UI`, file in `Components/`).
- `Pennington.DocSite.DocSiteOptions.ConfigureLocalization` — init-only
  `Action<LocalizationOptions>?`.

## URL scheme — locked for app #13

- **Default locale owns the root**: `/`, `/about/`, `/getting-started/`.
  No `/en/` prefix, no redirect from `/` → `/en/`.
- **Non-default locales use their code as a URL prefix**: `/es/`,
  `/es/about/`, `/es/getting-started/`.
- `LocaleDetectionMiddleware` strips the prefix into `PathBase` before
  endpoint matching so Blazor's route pattern is locale-agnostic.
- Cookie + `Accept-Language` providers are registered, but URL-first
  (`PenningtonUrlRequestCultureProvider`) — the URL always wins.

## Content location convention

- Default-locale content lives **directly under** `ContentRootPath`
  (`Content/`). Default-locale discovery skips locale subfolders.
- Non-default locales live under `Content/<code>/` — one subfolder per
  registered locale. Per-file `locale:` front matter is not required; the
  subfolder drives the locale assignment on `DiscoveredItem.Route.Locale`.
- Missing translation → `ContentResolver` falls back to the default-locale
  copy and surfaces `FallbackRequestedDisplayName` /
  `FallbackDefaultDisplayName` so `DocSiteArticle` can render a banner.

## Verification

`dotnet build Pennington.slnx` clean. Dev server on `localhost:5530` —
Playwright confirmed: `/` shows "Welcome" with EN sidebar; switcher shows
"English" and drops a menu with `English` → `/` and `Español` → `/es/`;
clicking Español navigated to `/es/` with title "Bienvenido" and sidebar
retitled to "Acerca de" / "Primeros Pasos"; deep-link `/es/about`
resolved with prev/next banners in Spanish; switching back to English
from `/es/about` landed on `/about/` with "About" heading. Static build
produced 17 pages — both `output/en`-equivalents (root) and `output/es/`
carry the correct translated H1 and body. Output cleaned.

No blockers.

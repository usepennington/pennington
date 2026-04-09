---
title: "Adding Localization"
description: "Take a single-locale Penn site and add multiple languages with locale subdirectories, translated UI strings, content fallback, and a language switcher"
uid: "penn.tutorials.adding-localization"
order: 10
---

## Beat 1: Starting Point — A Single-Locale Site

Establish the baseline: a Penn site with 3 content pages, all in English, with no locale configuration. The reader will add Spanish support by the end of this tutorial.

### What to show
- Describe the existing content structure: `Content/index.md`, `Content/getting-started.md`, `Content/configuration.md`
- Note that the site serves at `/`, `/getting-started/`, `/configuration/` with no locale prefixes
- State the goal: after this tutorial, English content serves at the same URLs (no prefix for the default locale), Spanish content serves at `/es/`, and a language switcher lets users toggle between them

### Key points
- Penn's localization is opt-in — a site with no locale configuration works exactly as before
- The default locale (English in this tutorial) retains clean URLs without a prefix
- Localization affects three layers: URL routing, content discovery, and UI string translations

## Beat 2: Configure Locales in PennOptions

Show the reader how to declare the available locales and the default locale.

### What to show
- Code reference: `T:Penn.Infrastructure.LocalizationOptions` — show the class definition with key members:
  - `P:Penn.Infrastructure.LocalizationOptions.DefaultLocale` — defaults to `"en"`
  - `M:Penn.Infrastructure.LocalizationOptions.AddLocale(System.String,Penn.Localization.LocaleInfo)` — registers a locale with full metadata
  - `M:Penn.Infrastructure.LocalizationOptions.AddLocale(System.String,System.String)` — convenience overload that takes just a display name
  - `P:Penn.Infrastructure.LocalizationOptions.IsMultiLocale` — returns `true` when more than one locale is configured
  - `P:Penn.Infrastructure.LocalizationOptions.Locales` — the dictionary of registered locales
- Code reference: `T:Penn.Localization.LocaleInfo` — show the record: `DisplayName`, `Direction` (defaults to `"ltr"`), `HtmlLang` (nullable, defaults to the locale code)
- Show the configuration code in `Program.cs`:
  ```csharp
  penn.Localization.DefaultLocale = "en";
  penn.Localization.AddLocale("en", "English");
  penn.Localization.AddLocale("es", "Español");
  ```
- For DocSite users, show the alternative via `DocSiteOptions.ConfigureLocalization`: reference `P:Penn.DocSite.DocSiteOptions.ConfigureLocalization` — an `Action<LocalizationOptions>?` that is invoked during service registration

### Key points
- The locale code (first argument to `AddLocale`) is used in URL prefixes, content directory names, and translation lookups — keep it short and lowercase (e.g., `"en"`, `"es"`, `"fr"`)
- The `DisplayName` appears in the language switcher UI
- `HtmlLang` on `LocaleInfo` overrides the `<html lang="...">` attribute if the locale code does not match a valid BCP 47 tag (e.g., locale code `"gen-z"` might map to `HtmlLang = "en"`)
- `Direction` supports `"rtl"` for right-to-left languages like Arabic and Hebrew

## Beat 3: Add Locale Routing Middleware

Show the reader the critical middleware ordering requirement.

### What to show
- Code reference: `M:Penn.Infrastructure.PennExtensions.UsePennLocaleRouting(Microsoft.AspNetCore.Builder.WebApplication)`
- Show the ordering in `Program.cs`:
  ```csharp
  app.UsePennLocaleRouting(); // Must be BEFORE MapRazorComponents
  app.MapRazorComponents<App>();
  ```
- Reference how DocSite handles this: `:path src/Penn.DocSite/DocSiteServiceExtensions.cs` (line 80) — `app.UsePennLocaleRouting()` called before `MapRazorComponents`

### Key points
- `UsePennLocaleRouting()` MUST be called before `MapRazorComponents` — if called after, Blazor routing will not see the rewritten path and locale URLs will return 404
- The middleware only activates when `IsMultiLocale` is true — single-locale sites skip it entirely
- Path rewriting is transparent to Razor components — `@page "/getting-started"` handles both `/getting-started/` and `/es/getting-started/`

## Beat 4: Restructure Content into Locale Subdirectories

Walk the reader through reorganizing their content files into locale-specific directories.

### What to show
- The directory structure before and after:
  ```
  Before:                    After:
  Content/                   Content/
    index.md                   index.md              (English - default locale)
    getting-started.md         getting-started.md
    configuration.md           configuration.md
                               es/
                                 index.md            (Spanish)
                                 getting-started.md
                                 (configuration.md intentionally missing)
  ```
- Explain that the default locale's files stay at the content root — they do not go into an `en/` subdirectory
- Non-default locale files go into `Content/{locale}/` subdirectories that mirror the same file structure
- Show example front matter for the Spanish index:
  ```yaml
  ---
  title: "Bienvenido"
  description: "Documentacion del proyecto"
  ---
  ```
- Explain that one page (`configuration.md`) is deliberately left untranslated to demonstrate fallback behavior in a later beat

### Key points
- The default locale's content lives at the content root, not in a subdirectory — this keeps clean URLs without a prefix
- Non-default locale subdirectories must exactly match the locale code (e.g., `es/` for the `"es"` locale)
- File names must match between locales for Penn to recognize them as translations of the same page
- Penn discovers locale content automatically based on the configured locales and the subdirectory structure
- The default locale has no URL prefix — `/getting-started/` serves English while `/es/getting-started/` serves Spanish. Non-default locale URLs map to `Content/{locale}/` subdirectories

## Beat 5: Add UI String Translations

Wire up translated strings for navigation labels, placeholders, and other UI elements.

### What to show
- Code reference: `T:Penn.Localization.TranslationOptions` — show the class:
  - `M:Penn.Localization.TranslationOptions.Add(System.String,System.String,System.String)` — add a single entry (locale, key, value)
  - `M:Penn.Localization.TranslationOptions.Add(System.String,System.Collections.Generic.Dictionary{System.String,System.String})` — add a batch of entries for a locale
- Code reference: `P:Penn.Infrastructure.PennOptions.Translations` — show that it is exposed directly on `PennOptions`
- Show the configuration code:
  ```csharp
  penn.Translations.Add("en", new Dictionary<string, string>
  {
      ["nav.search"] = "Search...",
      ["nav.onThisPage"] = "On This Page",
      ["nav.previous"] = "Previous",
      ["nav.next"] = "Next",
  });
  penn.Translations.Add("es", new Dictionary<string, string>
  {
      ["nav.search"] = "Buscar...",
      ["nav.onThisPage"] = "En Esta Pagina",
      ["nav.previous"] = "Anterior",
      ["nav.next"] = "Siguiente",
  });
  ```
- Code reference: `T:Penn.Localization.PennStringLocalizer` — briefly explain that Penn implements ASP.NET's `IStringLocalizer` interface, backed by `TranslationOptions`. It resolves the current locale from `CultureInfo.CurrentUICulture` (set by the locale detection middleware), looks up the translation, and falls back to the default locale, then to the raw key itself

### Key points
- Translation keys are arbitrary strings — use a dotted namespace convention (e.g., `"nav.search"`) for organization
- The fallback chain is: current locale -> default locale -> raw key. This means English strings can serve as both keys and fallback values
- In Razor components, inject `IStringLocalizer` and use `@Localizer["nav.search"]` to render translated strings

## Beat 6: Add the LanguageSwitcher Component

Drop the language switcher into the layout and explain how it works.

### What to show
- Code reference: `:path src/Penn.UI/Components/LanguageSwitcher.razor` — show the full component:
  - It injects `LocaleContext` and `LocalizationOptions`
  - It renders a `<details>` dropdown with a globe icon and the current locale's display name
  - Each alternate language is an `<a>` tag with `data-spa-reload` (forces full page reload on click, since locale changes need a full re-render) and `data-locale` (prevents the link rewriter from adding a locale prefix)
  - The `AlternateLanguages` parameter allows explicit URLs; when null, it auto-computes from `LocaleContext` and `LocalizationOptions.GetAlternateLanguages()`
- Show the layout integration — reference the DocSite MainLayout: `:path src/Penn.DocSite/Components/Layout/MainLayout.razor` (lines 140-143):
  ```razor
  @if (Localization.IsMultiLocale)
  {
      <LanguageSwitcher AlternateLanguages="_langSwitcherItems" />
  }
  ```
- For a custom layout, the minimal integration is:
  ```razor
  <LanguageSwitcher />
  ```

### Key points
- The `data-spa-reload` attribute on language links ensures a full page reload instead of an SPA navigation — this is necessary because locale changes affect the entire page (layout, navigation, all content)
- When `AlternateLanguages` is not provided, the component auto-computes URLs using `LocalizationOptions.GetAlternateLanguages()` and the current `LocaleContext.ContentPath`
- The component only renders when there are multiple items (the `@if (_items.Count > 1)` guard)
- Internal links are automatically rewritten to include the locale prefix for non-default locales — Razor components can use plain root-relative paths without worrying about locale prefixes

## Beat 7: Understand Content Fallback Behavior

Explain what happens when a page does not exist in the requested locale.

### What to show
- Code reference: `P:Penn.Routing.ContentRoute.IsFallback` — show the property on `ContentRoute`: `bool IsFallback { get; init; }` — set to `true` when serving default-locale content as a fallback
- Code reference: `:path src/Penn.UI/Components/FallbackNotice.razor` — show the full component:
  - Renders an amber warning banner: "This page is not yet available in **{RequestedLocale}**. Showing the **{DefaultLocale}** version."
  - Parameters: `RequestedLocale` and `DefaultLocale` (both nullable strings)
  - Only renders when `RequestedLocale` is not null/empty
- Reference how DocSiteArticle uses it: `:path src/Penn.DocSite/Slots/Components/DocSiteArticle.razor` (lines 3-6):
  ```razor
  @if (!string.IsNullOrEmpty(FallbackRequestedLocale))
  {
      <FallbackNotice RequestedLocale="@FallbackRequestedLocale"
                      DefaultLocale="@FallbackDefaultLocale" />
  }
  ```
- Walk through the test case: navigate to `/es/configuration/` — since `Content/es/configuration.md` does not exist, Penn serves the English version with the FallbackNotice banner

### Key points
- Fallback is automatic — when a content file is missing in the requested locale, Penn serves the default locale's version
- The `FallbackNotice` component gives the user a clear visual indication that they are seeing untranslated content
- The `IsFallback` flag on `ContentRoute` allows custom rendering logic beyond just the banner (e.g., hiding the language switcher on fallback pages)
- Fallback only applies to content pages — UI translations always fall back silently via the `PennStringLocalizer` chain

## Beat 8: Use LocaleContext and Verify

Show how Razor components can access the current locale, then verify the complete localized site.

### What to show
- Code reference: `T:Penn.Localization.LocaleContext` — show the full class:
  - `P:Penn.Localization.LocaleContext.Locale` — the current locale code (e.g., `"en"`, `"es"`)
  - `P:Penn.Localization.LocaleContext.Info` — the `LocaleInfo` record for the current locale
  - `P:Penn.Localization.LocaleContext.ContentPath` — the URL with locale prefix stripped (e.g., `/getting-started/` regardless of locale)
  - `P:Penn.Localization.LocaleContext.IsDefaultLocale` — true when the current locale is the default
  - `P:Penn.Localization.LocaleContext.HtmlLang` — the BCP 47 lang tag for the `<html>` element
  - `P:Penn.Localization.LocaleContext.Direction` — `"ltr"` or `"rtl"`
  - `M:Penn.Localization.LocaleContext.Url(System.String)` — builds a locale-aware URL from a content path
- Show practical Razor usage:
  ```razor
  @inject LocaleContext Locale

  <html lang="@Locale.HtmlLang" dir="@Locale.Direction">

  <!-- Build a locale-aware link -->
  <a href="@Locale.Url("/getting-started")">@Localizer["nav.gettingStarted"]</a>

  <!-- Conditionally render locale-specific content -->
  @if (Locale.Locale == "es")
  {
      <p>Contacto: soporte@ejemplo.com</p>
  }
  ```
- Reference how the DocSite App.razor uses LocaleContext: `:path src/Penn.DocSite/Components/App.razor` — `@inject Penn.Localization.LocaleContext Locale`
- Run command: `dotnet run`
- Verification checklist:
  1. Navigate to `/` — see the English index page ("Welcome")
  2. Click the language switcher in the header — see "English" and "Espanol" options
  3. Select "Espanol" — the page reloads at `/es/` with the Spanish index ("Bienvenido")
  4. Navigate to `/es/getting-started/` — see the translated Spanish content ("Primeros Pasos")
  5. Navigate to `/es/configuration/` — see the English content with the amber FallbackNotice banner: "This page is not yet available in Espanol. Showing the English version."
  6. Check the navigation labels — "Previous"/"Next" buttons should show "Anterior"/"Siguiente" in Spanish mode
  7. View page source — confirm `<html lang="es">` and hreflang `<link>` tags
  8. Navigate back to English by clicking the language switcher — confirm clean URLs without prefix

### Key points
- `LocaleContext` is scoped per request — it is populated by `LocaleDetectionMiddleware` before your component renders
- `Locale.Url("/path")` is the recommended way to build internal links in multi-locale sites — it automatically prefixes non-default locale paths
- For the default locale, `Locale.Url("/path")` returns `/path/` unchanged — no unnecessary prefix
- `LocaleContext` is analogous to Astro's `Astro.currentLocale` — a single injection point for all locale information
- The language switcher triggers a full page reload (via `data-spa-reload`) because locale changes affect the entire page layout and navigation
- Fallback pages serve at the requested locale URL (`/es/configuration/`) even though they display default-locale content — this ensures bookmarks and shared links remain valid
- Adding a new locale later requires three steps: (1) `AddLocale` in configuration, (2) create a content subdirectory with translated files, (3) add UI string translations
- For SEO with hreflang alternate language links, see the RSS, Sitemap, and Structured Data how-to

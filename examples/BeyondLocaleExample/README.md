# BeyondLocaleExample

Adds a second locale to a DocSite. The host shape is the tutorial DocSite plus a `ConfigureLocalization` action populating `LocalizationOptions` with the default locale and one URL-prefixed locale (`es`).

## Concepts

- `LocalizationOptions.AddLocale(code, LocaleInfo(...))`
- `DefaultLocale` lives at `/`; additional locales URL-prefixed (`/es/...`)
- `Content/<locale>/` subfolders holding translated markdown
- `LanguageSwitcher` lighting up once `Locales.Count > 1` (renders as `<details data-lang-switcher>` with a localized dropdown — inspect by attribute, not class)
- `TranslationOptions` registered through the `ConfigurePennington` escape hatch

## Fallback behavior

Visit `/es/missing-page/` to see how missing translations land: the response is a real 404 (the locale request didn't match a translated markdown file and Pennington does **not** silently fall back to the English page at that URL). The 404 page itself still ships with English chrome regardless of the request locale — see the cross-cutting "localized 404 chrome" item in `examples/AUDIT_LOG.md` for the framework follow-up that will pick the request locale's translated strings for the 404 body.

Translated pages that *do* exist (such as `/es/about/`) render normally with `<html lang="es">`. Pages that have a default-locale source but no `es/` translation use the framework's runtime fallback (see `Pennington.DocSite/Services/DocSiteContentResolver.cs`) — the body renders English content but `<html lang>` is rewritten to `en` by `FallbackLangHtmlRewriter` so screen readers identify the language correctly.

## Referenced from

- `docs/.../tutorials/beyond-basics/add-a-locale.md`
- `docs/.../how-to/discovery/localization.md`

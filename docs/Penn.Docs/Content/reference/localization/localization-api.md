---
title: "Localization API"
description: "Reference for LocalizationOptions (DefaultLocale, AddLocale, URL manipulation), LocaleInfo, LocaleContext (request-scoped locale state), TranslationOptions (key-value store), AlternateLanguage record, and LocaleDetectionMiddleware behavior"
uid: "penn.reference.localization-api"
order: 10
---

Document all localization types. `LocalizationOptions`: DefaultLocale (string), `AddLocale(code, displayName)` and `AddLocale(code, LocaleInfo)` methods, `Locales` (IReadOnlyDictionary), `IsMultiLocale` (bool), URL manipulation methods — `GetLocaleFromUrl(url)`, `StripLocalePrefix(url, locale)`, `BuildLocaleUrl(contentPath, locale)`, `GetAlternateLanguages(url)`. `LocaleInfo` record: DisplayName, HtmlLang, Direction. `LocaleContext` (request-scoped): Locale, Info, ContentPath, IsDefaultLocale, HtmlLang, Direction, `Url(contentPath)` method. `TranslationOptions`: `Add(locale, key, value)`, `Add(locale, dictionary)`, `Get(locale, key)`, `GetAll(locale)`. `AlternateLanguage` record: Locale, DisplayName, HtmlLang, Url, IsCurrentLocale. Document `LocaleDetectionMiddleware` behavior: URL prefix detection, cookie fallback, Accept-Language header fallback, path rewriting.

---
title: "Enable multiple locales"
description: "Populate LocalizationOptions, lay out translated content in locale subdirectories, register UI translations, and wire the locale-routing middleware."
uid: how-to.configuration.localization
order: 202050
sectionLabel: Configuration
tags: [localization, locales, translations, routing]
---

Use these steps when you have a working single-locale Pennington site and need to add one or more additional languages. The page covers wiring options, content layout on disk, routing middleware, and UI translations — everything needed to go live with multiple locales.

> [!TIP]
> If this is your first locale, start with the tutorial [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale) — it walks through the same moving parts at a teaching pace.

## Assumptions

- You have an existing Pennington site with at least one markdown page (see [Create your first Pennington site](xref:tutorials.getting-started.first-site) if not).
- You are on either an `AddDocSite` host (which accepts a `ConfigureLocalization` callback) or a bare `AddPennington` host (which exposes `LocalizationOptions` directly on `PenningtonOptions.Localization`).
- Your default-locale content already lives directly under `ContentRootPath` (not in a locale subfolder) — Pennington treats the default locale as URL-root and every other locale as URL-prefixed.

For a complete reference setup, the `BeyondLocaleExample` project has English under `Content/` and Spanish under `Content/es/`, wired with a single `ConfigureLocalization` action.

---

## Steps

### 1. Populate `LocalizationOptions` with the default locale and every additional locale

On a DocSite host, set `DefaultLocale` and call `AddLocale` once per additional language inside `ConfigureLocalization`. On a bare `AddPennington` host, configure `PenningtonOptions.Localization` the same way. The default locale owns the URL root; each additional locale gets a URL prefix matching its code, so choose codes you are comfortable seeing in URLs.

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.Stage3.Run(System.String[])
```

```csharp:xmldocid
T:Pennington.Infrastructure.LocalizationOptions
P:Pennington.Infrastructure.LocalizationOptions.DefaultLocale
P:Pennington.Infrastructure.LocalizationOptions.Locales
M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,Pennington.Localization.LocaleInfo)
T:Pennington.Localization.LocaleInfo
```

### 2. Mirror your content tree under `Content/<locale>/` for every non-default locale

Default-locale files stay directly under `ContentRootPath` with no prefix. For each additional locale, create a sibling folder named after the locale code and place translated files there, mirroring the default-locale filenames so `ContentResolver` can pair them. Pages without a translation fall back to the default locale automatically — you do not need to translate every page before shipping.

```markdown:path
examples/BeyondLocaleExample/Content/es/about.md
```

### 3. Confirm `UsePenningtonLocaleRouting` is in the pipeline

`UseDocSite` and `UseBlogSite` already register `UsePenningtonLocaleRouting` as the first middleware — no extra call needed on template hosts. On a bare `AddPennington` host, insert it before `UseRouting` so `LocaleDetectionMiddleware` can strip the locale prefix into `PathBase` ahead of endpoint matching.

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.UsePenningtonLocaleRouting(Microsoft.AspNetCore.Builder.WebApplication)
```

### 4. Add UI string translations through `TranslationOptions`

UI strings rendered by Razor components flow through `IStringLocalizer`, which Pennington backs with the in-memory `TranslationOptions` on `PenningtonOptions.Translations`. Register one entry per locale/key pair inside your `AddPennington` or `AddDocSite` configuration — keys are free-form, and missing keys fall back to the default locale automatically.

<!-- TODO: xmldocid needed -->
```csharp
builder.Services.AddPennington(options =>
{
    options.Translations.Add("en", "nav.home", "Home");
    options.Translations.Add("es", "nav.home", "Inicio");
});
```

```csharp:xmldocid
T:Pennington.Localization.TranslationOptions
M:Pennington.Localization.TranslationOptions.Add(System.String,System.String,System.String)
T:Pennington.Localization.PenningtonStringLocalizer
```

### 5. Surface the language switcher

On DocSite, the `LanguageSwitcher` component is already wired into `MainLayout.razor` and activates automatically when `LocalizationOptions.IsMultiLocale` is true — no extra markup required. On a bare host, drop `<LanguageSwitcher />` into your layout wherever you want the locale picker to appear.

```razor:path
src/Pennington.UI/Components/LanguageSwitcher.razor
```

```csharp:xmldocid
P:Pennington.Infrastructure.LocalizationOptions.IsMultiLocale
```

---

## Verify

- Run `dotnet run` and visit `/` — the default-locale page renders at the URL root with no prefix.
- Visit `/{locale}/` (for example `/es/`) and confirm the translated home renders; remove one translated file and verify the same URL falls back with a fallback banner.
- The site header (DocSite) or your layout (bare host) shows `LanguageSwitcher` with one entry per registered locale.
- UI strings registered through `TranslationOptions` resolve to the locale-appropriate value; missing keys fall back to the default locale.

## Related

- Tutorial: [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale)
- Reference: [`LocalizationOptions`](xref:reference.options.localization-options)
- Reference: [`TranslationOptions`](xref:reference.options.translations)
- Background: TODO — Explanation page on locale routing and content fallback (not yet written in TOC)

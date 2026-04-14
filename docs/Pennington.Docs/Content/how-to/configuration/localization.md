---
title: "Enable multiple locales"
description: "Populate LocalizationOptions, lay out translated content in locale subdirectories, register UI translations, and wire the locale-routing middleware."
uid: how-to.configuration.localization
order: 50
sectionLabel: Configuration
tags: [localization, locales, translations, routing]
---

> **In this page.** _One sentence paraphrasing the TOC "Covers" line: populate `LocalizationOptions` with `DefaultLocale` and `Locales`, organize markdown under `Content/<locale>/` subfolders, register UI string translations, and ensure `UsePenningtonLocaleRouting` is in the pipeline._
>
> **Not in this page.** _One sentence paraphrasing the TOC "Does not cover" line: writing a custom `IRequestCultureProvider` is out of scope — the built-in `PenningtonUrlRequestCultureProvider` is documented in Reference, and this page uses it as-is._

## When to use this

_Two sentences. Frame the realistic arrival: the reader already has a working single-locale site (DocSite or bare `AddPennington`) and needs to ship a second language. Point back to the tutorial for first-time readers so they do not land here before they have ever touched a locale._

> [!TIP]
> If this is your first locale, start with the tutorial [Add a second locale to your site](/tutorials/beyond-basics/add-a-locale) — it walks through the same moving parts at a teaching pace.

## Assumptions

_Keep to 3 bullets. The reader must already have a working site; translating content and enabling routing belongs on top of that, not alongside a first-time setup._

- You have an existing Pennington site with at least one markdown page (see [Create your first Pennington site](/tutorials/getting-started/first-site) if not)
- You are on either an `AddDocSite` host (which accepts a `ConfigureLocalization` callback) or a bare `AddPennington` host (which exposes `LocalizationOptions` directly on `PenningtonOptions.Localization`)
- Your default-locale content already lives directly under `ContentRootPath` (not in a locale subfolder) — Pennington treats the default locale as URL-root and every other locale as URL-prefixed

To copy a working setup, see [`examples/BeyondLocaleExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondLocaleExample) — English lives under `Content/`, Spanish under `Content/es/`, and one `ConfigureLocalization` action wires the whole thing. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Five steps, imperative-first. Step 1 registers locales; step 2 lays out translated content on disk; step 3 (optional, skippable on `AddDocSite`) confirms the routing middleware; step 4 adds UI translations; step 5 adds the language switcher. Keep prose ≤ 2 sentences per step._

### 1. Populate `LocalizationOptions` with the default locale and every additional locale

_Two sentences. On a DocSite host, assign `DocSiteOptions.ConfigureLocalization` and set `DefaultLocale` + call `AddLocale` once per additional language; on a bare host, configure `PenningtonOptions.Localization` the same way. The default locale owns the URL root and every other locale gets a URL prefix equal to its code, so pick short codes you are willing to see in URLs._

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.Stage3.Run(System.String[])
```

_Backing symbols for lookup:_

```csharp:xmldocid
T:Pennington.Infrastructure.LocalizationOptions
P:Pennington.Infrastructure.LocalizationOptions.DefaultLocale
P:Pennington.Infrastructure.LocalizationOptions.Locales
M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,Pennington.Localization.LocaleInfo)
T:Pennington.Localization.LocaleInfo
```

### 2. Mirror your content tree under `Content/<locale>/` for every non-default locale

_Two sentences. Default-locale files stay directly under `ContentRootPath` (no `/en/` prefix, no redirect); every other locale is a sibling folder named with its locale code, and each translated file must mirror the default-locale filename so `ContentResolver` can pair them. Missing translations fall back to the default locale with a banner — you do not need to translate every page on day one._

```markdown:path
examples/BeyondLocaleExample/Content/es/about.md
```

### 3. Confirm `UsePenningtonLocaleRouting` is in the pipeline

_Two sentences. `UseDocSite` and `UseBlogSite` already call `UsePenningtonLocaleRouting` as the first middleware — you do not add it yourself on template hosts. On a bare `AddPennington` host, insert it before `UseRouting` so `LocaleDetectionMiddleware` can strip the locale prefix into `PathBase` ahead of endpoint matching._

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.UsePenningtonLocaleRouting(Microsoft.AspNetCore.Builder.WebApplication)
```

### 4. Add UI string translations through `TranslationOptions`

_Two sentences. UI strings rendered by Razor components or inline markup flow through `IStringLocalizer`, which Pennington backs with the in-memory `TranslationOptions` on `PenningtonOptions.Translations`. Register one entry per locale/key pair inside your `AddPennington` / `AddDocSite` configuration — keys are free-form and falling back to the default locale is automatic._

```csharp
builder.Services.AddPennington(options =>
{
    options.Translations.Add("en", "nav.home", "Home");
    options.Translations.Add("es", "nav.home", "Inicio");
});
```

_Backing symbols:_

```csharp:xmldocid
T:Pennington.Localization.TranslationOptions
M:Pennington.Localization.TranslationOptions.Add(System.String,System.String,System.String)
T:Pennington.Localization.PenningtonStringLocalizer
```

### 5. Surface the language switcher (DocSite) or render `LanguageSwitcher` yourself (bare host)

_Two sentences. On DocSite the `LanguageSwitcher` component is already wired into `MainLayout.razor` and lights up automatically as soon as `LocalizationOptions.IsMultiLocale` is true — no extra markup required. On a bare host, drop `<LanguageSwitcher />` into your layout wherever you want the picker._

```csharp:xmldocid
T:Pennington.UI.Components.LanguageSwitcher
P:Pennington.Infrastructure.LocalizationOptions.IsMultiLocale
```

---

## Verify

_Terse. Four bullets, one per moving part so each can be checked independently._

- Run `dotnet run` and visit `/` — the default-locale page renders at the URL root with no prefix
- Visit `/{non-default-locale}/` (e.g. `/es/`) and confirm the translated home renders; delete one translated file and the same URL falls back with a fallback banner
- The site header (DocSite) or your layout (bare host) shows the `LanguageSwitcher` with one entry per registered locale
- Any UI string you translated through `TranslationOptions` resolves to the locale-appropriate value; missing keys fall back to the default locale's value

## Related

_Three cross-quadrant links. Reference for the exhaustive options surface, Reference for translation plumbing, Explanation for the routing model, plus the tutorial cross-link for first-time readers._

- Tutorial: [Add a second locale to your site](/tutorials/beyond-basics/add-a-locale)
- Reference: [`LocalizationOptions`](/reference/options/localization-options)
- Reference: [`TranslationOptions`](/reference/options/translations)
- Background: TODO — Explanation page on locale routing and content fallback (not yet written in TOC)

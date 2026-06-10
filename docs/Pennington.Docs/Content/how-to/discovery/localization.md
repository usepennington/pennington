---
title: "Serve the site in multiple languages"
description: "Populate LocalizationOptions, lay out translated content in locale subdirectories, register UI translations, and wire the locale-routing middleware."
uid: how-to.discovery.localization
order: 4
sectionLabel: "Content Discovery"
tags: [localization, locales, translations, routing]
---

When the site needs to ship in more than one language, the options below cover the wiring, content layout on disk, routing middleware, and UI translations — everything needed to take a single-locale site multilingual. For a first locale, the tutorial [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale) walks through the same moving parts at a teaching pace.

## Before you begin
- An existing Pennington site with at least one markdown page (see [Create your first Pennington site](xref:tutorials.getting-started.first-site) if not).
- Either an `AddDocSite` host (which accepts a `ConfigureLocalization` callback) or a bare `AddPennington` host (which exposes `LocalizationOptions` directly on `PenningtonOptions.Localization`).
- Default-locale content already directly under `ContentRootPath` (not in a locale subfolder). Pennington treats the default locale as URL-root and every other locale as URL-prefixed.

For a complete reference setup, [`examples/BeyondLocaleExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondLocaleExample) has English under `Content/` and Spanish under `Content/es/`, wired with a single `ConfigureLocalization` action.

---

## Wire localization end to end

Each subsection below is one part of the same setup, not an alternative — work through all five to take a single-locale site multilingual.

### Populate `LocalizationOptions` with the default locale and every additional locale

On a DocSite host, set `DefaultLocale` and call `AddLocale` once per additional language inside `ConfigureLocalization`. On a bare `AddPennington` host, configure `PenningtonOptions.Localization` the same way. The default locale owns the URL root; each additional locale gets a URL prefix matching its code, so choose codes that read well in URLs.

```csharp
ConfigureLocalization = loc =>
{
    loc.DefaultLocale = "en";
    loc.AddLocale("en", new LocaleInfo("English"));
    loc.AddLocale("es", new LocaleInfo("Español", HtmlLang: "es"));
},
```

See <xref:reference.api.localization-options> for the `LocalizationOptions` members (`DefaultLocale`, `Locales`, `AddLocale`, `LocaleInfo`).

### Mirror your content tree under `Content/<locale>/` for every non-default locale

Default-locale files stay directly under `ContentRootPath` with no prefix. For each additional locale, create a sibling folder named after the locale code and place translated files there, mirroring the default-locale filenames so the two are paired. Pages without a translation fall back to the default locale automatically, so shipping does not require a full translation pass.

```markdown:symbol
examples/BeyondLocaleExample/Content/es/about.md
```

### Confirm `UseLocaleRouting` is in the pipeline

`UseDocSite` and `UseBlogSite` already register `UseLocaleRouting` as the first middleware — template hosts need no extra call. On a bare `AddPennington` host, call it before mapping endpoints; it handles locale detection and `UseRouting` together, so `LocaleDetectionMiddleware` can strip the locale prefix into `PathBase` ahead of endpoint matching without a separate `UseRouting` call.

```csharp
app.UseLocaleRouting();
```

### Add UI string translations through `TranslationOptions`

UI strings rendered by Razor components flow through `IStringLocalizer`, which Pennington backs with the in-memory `TranslationOptions` on `PenningtonOptions.Translations`. Register one entry per locale/key pair inside the `AddPennington` or `AddDocSite` configuration. Keys are free-form, and missing keys fall back to the default locale automatically.

```csharp
builder.Services.AddPennington(options =>
{
    options.Translations.Add("en", "nav.home", "Home");
    options.Translations.Add("es", "nav.home", "Inicio");
});
```

See <xref:reference.api.translation-options> for the full `TranslationOptions` surface.

### Surface the language switcher

On DocSite, the `LanguageSwitcher` component is already wired into `MainLayout.razor` and activates automatically when `LocalizationOptions.IsMultiLocale` is true; no extra markup required. On a bare host, drop `<LanguageSwitcher />` into the layout wherever the locale picker should appear:

```razor
<LanguageSwitcher />
```

See <xref:reference.ui.utility> for the `LanguageSwitcher` parameter surface.

---

## Result

The default locale owns the URL root; each additional locale gets a prefix that matches its code. For a site with English (default) and Spanish:

```text
/                       English home
/about/                 English about page
/es/                    Spanish home
/es/about/              Spanish about page
/es/missing-page/       falls back to the English page with a fallback banner
```

The language switcher in the layout lists one entry per registered locale, and each `IStringLocalizer["nav.home"]` resolves to the locale-specific value from `TranslationOptions`.

## Verify

- Run `dotnet run` and visit `/`. The default-locale page renders at the URL root with no prefix.
- Visit `/{locale}/` (for example `/es/`) and confirm the translated home renders. Remove one translated file and verify the same URL falls back with a fallback banner.
- The site header (DocSite) or the layout (bare host) shows `LanguageSwitcher` with one entry per registered locale.
- UI strings registered through `TranslationOptions` resolve to the locale-appropriate value; missing keys fall back to the default locale.

## Related

- Tutorial: [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale)
- How-to: [Flag missing and outdated translations in the build report and dev overlay](xref:how-to.discovery.audit-translations)
- Reference: [`LocalizationOptions`](xref:reference.api.localization-options)
- Reference: [`TranslationOptions`](xref:reference.api.translation-options)
- Background: [Locale-aware URLs and content fallback](xref:explanation.localization.urls-and-fallback)

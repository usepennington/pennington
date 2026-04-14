---
title: "Enable multiple locales"
description: "Populate LocalizationOptions with a default locale and additional locales, organize content in locale subdirectories, add UI translations, and wire UsePenningtonLocaleRouting."
section: "configuration"
order: 50
tags: []
uid: how-to.configuration.localization
isDraft: true
search: false
llms: false
---

> **In this page.** Populating `LocalizationOptions` with `DefaultLocale` and `Locales`, organizing content in locale subdirectories, adding UI translations, and wiring `UsePenningtonLocaleRouting`.
>
> **Not in this page.** Implementing a custom culture provider — the built-in `PenningtonUrlRequestCultureProvider` is explained in Reference.

## When to use this

- You have a working Pennington site (doc or blog) in one language and need to publish the same content under additional URL-prefixed locales (`/pl/...`, `/sv/...`).
- You want locale-aware link rewriting, per-locale search indexes, and hreflang alternates to "just work" without hand-wiring a custom `IRequestCultureProvider`.

## Assumptions

- Existing Pennington site using `AddDocSite`, `AddBlogSite`, or `AddPennington`.
- At least one additional language to add — everything below assumes the default locale is already rendering.
- You are comfortable editing `Program.cs` and moving markdown files into subfolders.
- Not required: understanding `LocaleDetectionMiddleware` internals or `PenningtonUrlRequestCultureProvider` — link out to Reference if you want that.

To copy a working setup, see [`examples/LocalizationExample`](https://github.com/usepennington/pennington/tree/main/examples/LocalizationExample). It ships a `DocSite` with five locales and parallel `Content/<code>/` subfolders. Do not walk the whole example — this page is a recipe.

---

## Steps

### 1. Configure locales on `LocalizationOptions`

Set `DefaultLocale` and call `AddLocale(code, LocaleInfo)` for every additional language. On `DocSite` do this through `DocSiteOptions.ConfigureLocalization`; on a bare Pennington site, configure `penn.Localization` directly inside `AddPennington`.

```csharp
// In Program.cs — DocSite form, mirrors examples/LocalizationExample/Program.cs
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Multilingual Site",
    ConfigureLocalization = loc =>
    {
        loc.DefaultLocale = "en";
        loc.AddLocale("en", new LocaleInfo("English"));
        loc.AddLocale("pl", new LocaleInfo("Polski"));
        loc.AddLocale("sv", new LocaleInfo("Svenska", HtmlLang: "sv-SE"));
    },
});
```

- `DefaultLocale` content renders at the root (`/about`), not under a prefix.
- Non-default locales render under `/{code}/...` — `LocalizationOptions.IsMultiLocale` flips true as soon as two locales are registered.
- `LocaleInfo` takes `DisplayName`, optional `Direction` (default `"ltr"`), and optional `HtmlLang` (defaults to the locale code) used for `<html lang>`.

### 2. Mirror content under locale subdirectories

Parallel the default-locale layout under `Content/<locale-code>/`. Every non-default locale discovers its markdown from the matching subfolder; missing pages fall back to the default locale.

```yaml
# Layout copied from examples/LocalizationExample/Content:
# Content/
#   index.md           <- default locale (en)
#   about.md
#   menu.md
#   pl/
#     index.md         <- pl locale
#     about.md
#     menu.md
#   sv/
#     index.md
```

- Front matter (`title`, `order`, `uid`) is duplicated per file — translate the values, not the keys.
- Partial coverage is fine. Pages missing from a locale resolve to the default-locale page, with the locale prefix preserved in the URL.
- Razor `@page` routes with locale prefixes require the explicit routing call in Step 4.

### 3. Add UI string translations (optional)

If your Razor layouts call `IStringLocalizer`, populate `penn.Translations` inside `AddPennington` / the `PenningtonOptions` callback. `PenningtonStringLocalizer` is wired automatically when `AddPennington` registers `IStringLocalizerFactory`.

```csharp
// Inside AddPennington(penn => { ... }) — or on a DocSite, reach penn through the ConfigureLocalization closure's sibling hook.
penn.Translations.Add("en", "greeting", "Welcome");
penn.Translations.Add("pl", "greeting", "Witamy");
penn.Translations.Add("sv", "greeting", "Valkommen");
```

- Keys are case-insensitive; missing translations fall through to the key itself (standard `IStringLocalizer` semantics).
- Skip this step if every layout string comes from markdown content — `TranslationOptions` is only for Razor-side UI chrome.

### 4. Call `UsePenningtonLocaleRouting` before Razor endpoint mapping

`UsePennington` registers locale detection internally, but Razor `@page` routes need the middleware active before `MapRazorComponents`. Add the explicit call in `Program.cs`:

```csharp
// Mirrors examples/YogaStudioExample/Program.cs
var app = builder.Build();

app.UsePenningtonLocaleRouting(); // Must precede MapRazorComponents so @page routes see stripped paths
app.UseAntiforgery();              // UsePenningtonLocaleRouting calls UseRouting internally — keep Antiforgery after it
app.MapStaticAssets();
app.MapRazorComponents<App>();

app.UseDocSite();
await app.RunDocSiteAsync(args);
```

- The call is idempotent (`Pennington.LocaleRoutingAdded` key). `UsePennington` will invoke it again later with no effect.
- `UseDocSite` / `UseBlogSite` already chain the right order if you are *not* declaring custom Razor `@page` components, so the explicit call is only required for custom-routed sites.

---

## Verify

- Run `dotnet run` and visit `/`, `/pl/`, and `/sv/` — each should render its locale's `index.md`.
- Inspect the `<html lang="...">` attribute on the rendered page; it must match the active locale (or its `HtmlLang` override).
- Confirm `/search-index-pl.json` and `/search-index-sv.json` are served alongside `/search-index-en.json` — `SearchIndexService` emits one file per locale.

## Related

- Reference: [`LocalizationOptions`](/reference/options/localization-options)
- Reference: [`TranslationOptions`](/reference/options/translations)
- Background: [Locale-aware URLs and content fallback](/explanation/localization/urls-and-fallback)

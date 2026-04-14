---
title: "Add a second locale to your site"
description: "Turn a single-language DocSite into a bilingual one by registering a second locale, translating three pages, and letting the built-in LanguageSwitcher appear in the header."
sectionLabel: "Beyond the Basics"
order: 10
tags:
  - localization
  - i18n
  - routing
  - docsite
uid: tutorials.beyond-basics.add-a-locale
---

> **In this page.** _One sentence paraphrasing the Covers line: the reader enables `LocalizationOptions` via `DocSiteOptions.ConfigureLocalization`, creates a `Content/es/` subdirectory with translated markdown mirroring the English pages, relies on `UseDocSite`'s built-in `UsePenningtonLocaleRouting` call to route locale-prefixed URLs, and watches the built-in `LanguageSwitcher` component light up in the DocSite header once `IsMultiLocale` flips to true._
>
> **Not in this page.** _One sentence paraphrasing the Does-not-cover line: this tutorial does not dig into per-locale search index internals (see [the `LocalizationOptions` reference](/reference/options/localization-options) and [`Enable multiple locales`](/how-to/configuration/localization) for UI string translation plumbing) and it does not explain the content-fallback mechanics in depth (see the [Locale-aware URLs and content fallback explanation](/explanation/localization/urls-and-fallback))._

## What you'll do

_**Artifact** (one sentence): a running DocSite at `http://localhost:5000` that serves three English pages at `/`, `/about`, `/getting-started` and three Spanish translations at `/es/`, `/es/about`, `/es/getting-started`, with a `LanguageSwitcher` pill in the header that toggles between them without any manual layout edits._

_**Skill** (one sentence): the reader walks away knowing that a single `ConfigureLocalization` action on `DocSiteOptions` is the toggle that enables multi-locale behavior, that the default locale lives at the URL root while every other locale gets a folder-and-URL prefix equal to its code, and that the `LanguageSwitcher` is already wired into DocSite chrome — it simply stays hidden until a second locale is registered._

## Prerequisites

_Keep this list to tools and prior tutorials. The starting host shape is identical to the DocSite scaffolding tutorial — link back to it rather than re-explaining `AddDocSite` / `UseDocSite` / `RunDocSiteAsync`. No Razor, no custom services, no Roslyn._

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](/tutorials/docsite/scaffold) (provides the single-locale DocSite host this tutorial extends)
- Completed [Author a documentation page with DocFrontMatter](/tutorials/docsite/first-doc-page) (so the front-matter shape of each page is already familiar)

The finished code for this tutorial lives in [`examples/BeyondLocaleExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondLocaleExample).

---

## 1. Start from a single-locale DocSite

_One sentence: the reader begins with a plain DocSite host serving three English pages from `Content/` — no localization, no switcher — so the baseline is clear before anything is added._

### Step 1.1 — Confirm the English-only host

_Show the Stage 1 host body. Call out that there is no `ConfigureLocalization` action on `DocSiteOptions` yet: `LocalizationOptions.IsMultiLocale` is false, so `UseDocSite`'s built-in `UsePenningtonLocaleRouting` is effectively a no-op and the built-in `LanguageSwitcher` in `MainLayout.razor` renders nothing. Make clear this is the only time the reader will modify the host file — everything else in the tutorial is filesystem work under `Content/`._

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.Stage1.Run(System.String[])
```

_Explain one non-obvious line: `UseDocSite()` internally calls [`UsePenningtonLocaleRouting`](/reference/host/extensions) first thing; the reader does not need to invoke it explicitly. Delete this paragraph if the code speaks for itself._

### Step 1.2 — Drop the three English pages into `Content/`

_Have the reader place `index.md`, `about.md`, and `getting-started.md` directly under `Content/`. These are the default-locale pages — they own the URL root because `DefaultLocale` will be `"en"` in the next unit. No locale subfolder appears here._

```markdown:path
examples/BeyondLocaleExample/Content/index.md
```

```markdown:path
examples/BeyondLocaleExample/Content/about.md
```

```markdown:path
examples/BeyondLocaleExample/Content/getting-started.md
```

### Checkpoint — Three English pages, no switcher

_Concrete verification. The site runs, pages render, the header has no language switcher above the nav._

- Run `dotnet run` from `examples/BeyondLocaleExample`
- Visit `http://localhost:5000/`, `http://localhost:5000/about`, `http://localhost:5000/getting-started` — each English page renders
- The DocSite header shows the site title and GitHub link but **no language switcher pill** — because only one locale is registered

---

## 2. Register a second locale with `ConfigureLocalization`

_One sentence: the reader adds a `ConfigureLocalization` action to `DocSiteOptions` that names `"en"` as the default and registers `"es"` as a second locale — the single code change that activates every piece of locale routing, link rewriting, and UI chrome downstream._

### Step 2.1 — Add the `ConfigureLocalization` action to `DocSiteOptions`

_Show the Stage 2 host body. Walk the reader through each line of the new action: set `DefaultLocale = "en"`, register `en` with `new LocaleInfo("English")`, register `es` with `new LocaleInfo("Español", HtmlLang: "es")`. Explain that `AddLocale` is overloaded — the string-only form is shorthand; the `LocaleInfo` form lets you set the display name (used by the switcher) and the HTML `lang` attribute emitted on the `<html>` element._

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.Stage2.Run(System.String[])
```

_One sentence on the practical rule: once `Locales` contains more than one entry, [`LocalizationOptions.IsMultiLocale`](/reference/options/localization-options) flips to `true` — that single boolean gates the switcher, the locale detection middleware, and the per-locale search index._

### Step 2.2 — Do not touch `UseDocSite()`

_This is a one-line reassurance step. The reader should not add `app.UsePenningtonLocaleRouting()` to `Program.cs`; `UseDocSite` already calls it internally. The middleware now starts rewriting `/es/<path>` requests into `<path>` + `PathBase = "/es"` so Blazor routing sees the stripped path._

### Checkpoint — The switcher appears, but Spanish URLs still 404

_Concrete verification. The header changes, but the content side of the story is missing._

- Rebuild and run the site (or let hot reload pick up the change)
- Refresh `http://localhost:5000/` — the DocSite header now shows a **language switcher pill** offering *English* and *Español*
- Click *Español* — the URL becomes `http://localhost:5000/es/` and you see a DocSite fallback notice explaining that Spanish content is missing, because no `Content/es/` files exist yet

---

## 3. Add translated markdown under `Content/es/`

_One sentence: the reader mirrors the three English pages under a `Content/es/` subfolder with Spanish translations — same file names, same front-matter keys, translated body content — so the content resolver can serve each Spanish URL from the matching Spanish file._

### Step 3.1 — Create `Content/es/` and translate `index.md`

_Have the reader create the `Content/es/` subfolder and add `index.md` with Spanish front-matter (`title: Bienvenido`) and Spanish body copy. Stress the load-bearing rule: **the subfolder name must match the locale code passed to `AddLocale`** — `es` here, because that is the code used in Stage 2. Files under `Content/es/` serve from `/es/*`; files directly under `Content/` serve from `/*`._

```markdown:path
examples/BeyondLocaleExample/Content/es/index.md
```

### Step 3.2 — Translate `about.md` and `getting-started.md`

_The reader repeats the move for the two remaining pages. Each Spanish file keeps the same filename as its English sibling; URL routing derives the path from the filename, not from any front-matter key. Skipping a translation is allowed — the content resolver falls back to the default-locale copy and renders a `FallbackNotice` banner naming the requested and default locales._

```markdown:path
examples/BeyondLocaleExample/Content/es/about.md
```

```markdown:path
examples/BeyondLocaleExample/Content/es/getting-started.md
```

### Checkpoint — Spanish URLs serve Spanish content

_Concrete verification. Every Spanish URL should now return its translation; the fallback banner from Step 2 is gone._

- With the host still running, visit `http://localhost:5000/es/` — the page renders in Spanish with no fallback banner
- Visit `http://localhost:5000/es/about` and `http://localhost:5000/es/getting-started` — both serve Spanish translations
- Inspect the `<html>` element in dev tools on a Spanish page — `lang="es"` (from the `LocaleInfo.HtmlLang` set in Step 2.1)

---

## 4. Use the built-in `LanguageSwitcher` to move between locales

_One sentence: the reader verifies that the [`LanguageSwitcher`](/reference/ui/utility) component — already baked into DocSite's `MainLayout.razor` — swaps locales in place by rewriting the current URL, so the reader lands on the same page in the other language rather than getting bounced to the home page._

### Step 4.1 — Confirm the final host shape matches `Program.cs`

_Show the Stage 3 host body — identical to what the reader built in unit 2, and identical in shape to the committed `Program.cs`. This is a sanity-check step, not a new code change. Nothing in `UseDocSite()` or `RunDocSiteAsync(args)` changes when a second locale is added._

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.Stage3.Run(System.String[])
```

### Step 4.2 — Click the switcher from `/es/about`

_Tell the reader to navigate to `http://localhost:5000/es/about`, open the language switcher in the header, and click *English*. The URL becomes `http://localhost:5000/about` — the switcher strips the `/es` prefix because English is the default locale, and it preserves the rest of the path so the reader stays on the About page. One sentence: this URL rewriting is the whole job of the switcher — no client-side state, no cookies._

### Checkpoint — Locale switching preserves the current page

_Concrete verification. The switcher moves the reader between languages on the same logical page, for every page in the site._

- From `http://localhost:5000/es/about`, click *English* in the switcher — you land on `http://localhost:5000/about`
- From `http://localhost:5000/getting-started`, click *Español* — you land on `http://localhost:5000/es/getting-started`
- From `http://localhost:5000/`, click *Español* — you land on `http://localhost:5000/es/` (the default locale's root maps to the secondary locale's prefix root)

---

## Summary

_Three to five bullets. Each one names a capability the reader now has, not a topic the page covered._

- You can turn a single-locale DocSite into a multi-locale one by adding a single `ConfigureLocalization` action to `DocSiteOptions` — no explicit middleware call, no layout edits.
- You know that the default locale owns the URL root and every other locale gets a code prefix equal to the string passed to `AddLocale`, with the matching `Content/<code>/` subfolder providing the translations.
- You can rely on the `LanguageSwitcher` appearing automatically once `LocalizationOptions.IsMultiLocale` flips to true, and you know it rewrites the current URL in place rather than redirecting to the home page.
- You can predict what happens when a translation is missing: the content resolver falls back to the default-locale copy and renders a `FallbackNotice` banner naming the requested and default locales.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

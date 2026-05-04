---
title: "Add a second locale to your site"
description: "Turn a single-language DocSite into a bilingual one by registering a second locale, translating three pages, and letting the built-in LanguageSwitcher appear in the header."
sectionLabel: "Beyond the Basics"
order: 104010
tags:
  - localization
  - i18n
  - routing
  - docsite
uid: tutorials.beyond-basics.add-a-locale
---

By the end of this tutorial you'll have a running DocSite at `http://localhost:5000` that serves three English pages at `/`, `/about`, and `/getting-started`, plus three Spanish translations at `/es/`, `/es/about`, and `/es/getting-started`. A `LanguageSwitcher` pill appears in the header and toggles between the two languages without any manual layout edits.

A single `ConfigureLocalization` action on `DocSiteOptions` is the toggle that enables multi-locale behavior. The default locale lives at the URL root; every other locale gets a folder prefix equal to its code. The `LanguageSwitcher` is already wired into DocSite chrome and stays hidden until a second locale is registered.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the single-locale DocSite host this tutorial extends)
- Completed [Author a documentation page with DocFrontMatter](xref:tutorials.docsite.first-doc-page) (so the front-matter shape of each page is already familiar)

The finished code for this tutorial lives in [`examples/BeyondLocaleExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondLocaleExample).

---

## 1. Start from a single-locale DocSite

Start with a plain DocSite host serving three English pages from `Content/` — no localization, no switcher. A clear baseline makes the contrast obvious when localization arrives in section 2.

<Steps>
<Step StepNumber="1">

**Confirm the English-only host**

Here's the starting host. There is no `ConfigureLocalization` action on `DocSiteOptions`. That means `LocalizationOptions.IsMultiLocale` is false, `UseDocSite`'s built-in `UsePenningtonLocaleRouting` is a no-op, and the built-in `LanguageSwitcher` in `MainLayout.razor` renders nothing.

```csharp:xmldocid,bodyonly,usings
M:BeyondLocaleExample.Stage1.Run(System.String[])
```

`UseDocSite()` already calls `UsePenningtonLocaleRouting` internally, so no direct call is needed at any point in this tutorial.

</Step>
<Step StepNumber="2">

**Drop the three English pages into `Content/`**

Place `index.md`, `about.md`, and `getting-started.md` directly under `Content/`. These are the default-locale pages — they own the URL root. No locale subfolder belongs here yet.

```markdown:path
examples/BeyondLocaleExample/Content/index.md
```

```markdown:path
examples/BeyondLocaleExample/Content/about.md
```

```markdown:path
examples/BeyondLocaleExample/Content/getting-started.md
```

</Step>
</Steps>

### Checkpoint — Three English pages, no switcher

- Run `dotnet run` from `examples/BeyondLocaleExample`
- Visit `http://localhost:5000/`, `http://localhost:5000/about`, and `http://localhost:5000/getting-started` — each English page renders
- The DocSite header shows the site title and GitHub link but **no language switcher pill** — because only one locale is registered

---

## 2. Register a second locale with `ConfigureLocalization`

Now let's make the site aware of Spanish. A single `ConfigureLocalization` action on `DocSiteOptions` names `"en"` as the default and registers `"es"` as a second locale. That one change activates every piece of locale routing, link rewriting, and UI chrome downstream.

<Steps>
<Step StepNumber="1">

**Add the `ConfigureLocalization` action to `DocSiteOptions`**

Here's the updated host:

```csharp:xmldocid,bodyonly,usings
M:BeyondLocaleExample.Stage2.Run(System.String[])
```

The new action has three pieces:

- `DefaultLocale = "en"` — English owns the URL root with no prefix.
- `AddLocale("en", new LocaleInfo("English"))` — registers English with the display name the switcher shows.
- `AddLocale("es", new LocaleInfo("Español", HtmlLang: "es"))` — registers Spanish. The `HtmlLang` value is what Pennington emits on the `<html>` element for that locale's pages.

`AddLocale` is overloaded: the string-only form is shorthand when a custom display name or `HtmlLang` is not needed.

Once `Locales` contains more than one entry, [`LocalizationOptions.IsMultiLocale`](xref:reference.api.localization-options) flips to `true`. That single boolean gates the switcher, the locale detection middleware, and the per-locale search index.

</Step>
<Step StepNumber="2">

**Leave `UseDocSite()` alone**

There's no need to add `app.UsePenningtonLocaleRouting()` to `Program.cs`. `UseDocSite` already calls it internally. Now that `IsMultiLocale` is true, the middleware rewrites `/es/<path>` requests into `<path>` with `PathBase = "/es"` so Blazor routing sees the stripped path.

</Step>
</Steps>

### Checkpoint — The switcher appears, but Spanish URLs still 404

- Rebuild and run the site (or let hot reload pick up the change)
- Refresh `http://localhost:5000/` — the DocSite header now shows a **language switcher pill** offering *English* and *Español*
- Click *Español* — the URL becomes `http://localhost:5000/es/` and you see a DocSite fallback notice explaining that Spanish content is missing, because no `Content/es/` files exist yet

---

## 3. Add translated markdown under `Content/es/`

Now let's give Spanish its content. Mirror the three English pages under a `Content/es/` subfolder — same file names, same front-matter keys, translated body copy. The content resolver matches each Spanish URL to the corresponding Spanish file.

<Steps>
<Step StepNumber="1">

**Create `Content/es/` and translate `index.md`**

Create the `Content/es/` subfolder and add `index.md` with Spanish front-matter and Spanish body copy. The load-bearing rule: **the subfolder name matches the locale code passed to `AddLocale`** — `es` here, because that is the code registered in step 2.1. Files under `Content/es/` serve from `/es/*`; files directly under `Content/` serve from `/*`.

```markdown:path
examples/BeyondLocaleExample/Content/es/index.md
```

</Step>
<Step StepNumber="2">

**Translate `about.md` and `getting-started.md`**

Repeat the move for the two remaining pages. Each Spanish file keeps the same filename as its English sibling; URL routing derives the path from the filename, not from any front-matter key.

Skipping a translation is fine. The content resolver falls back to the default-locale copy and renders a `FallbackNotice` banner naming the requested and default locales.

```markdown:path
examples/BeyondLocaleExample/Content/es/about.md
```

```markdown:path
examples/BeyondLocaleExample/Content/es/getting-started.md
```

</Step>
</Steps>

### Checkpoint — Spanish URLs serve Spanish content

- With the host still running, visit `http://localhost:5000/es/` — the page renders in Spanish with no fallback banner
- Visit `http://localhost:5000/es/about` and `http://localhost:5000/es/getting-started` — both serve Spanish translations
- Inspect the `<html>` element in dev tools on a Spanish page — `lang="es"` (from the `LocaleInfo.HtmlLang` set in step 2.1)

---

## 4. Use the built-in `LanguageSwitcher` to move between locales

The [`LanguageSwitcher`](xref:reference.ui.utility) component is already baked into DocSite's `MainLayout.razor`. Now let's verify that it swaps locales in place by rewriting the current URL, landing on the same page in the other language rather than bouncing back to the home page.

<Steps>
<Step StepNumber="1">

**Confirm the final host shape matches `Program.cs`**

Here's the final host — identical to what section 2 produced. This is a sanity-check step, not a new code change. Nothing in `UseDocSite()` or `RunDocSiteAsync(args)` changes when a second locale is added.

```csharp:xmldocid,bodyonly,usings
M:BeyondLocaleExample.Stage3.Run(System.String[])
```

</Step>
<Step StepNumber="2">

**Click the switcher from `/es/about`**

Navigate to `http://localhost:5000/es/about`, open the language switcher in the header, and click *English*. The URL becomes `http://localhost:5000/about`. The switcher strips the `/es` prefix because English is the default locale and preserves the rest of the path, so the About page stays in view. That URL rewriting is the switcher's entire job — no client-side state, no cookies involved.

</Step>
</Steps>

### Checkpoint — Locale switching preserves the current page

- From `http://localhost:5000/es/about`, click *English* — the URL becomes `http://localhost:5000/about`
- From `http://localhost:5000/getting-started`, click *Español* — the URL becomes `http://localhost:5000/es/getting-started`
- From `http://localhost:5000/`, click *Español* — the URL becomes `http://localhost:5000/es/` (the default locale's root maps to the secondary locale's prefix root)

---

## Summary

- A single-locale DocSite becomes multi-locale by adding one `ConfigureLocalization` action to `DocSiteOptions` — no explicit middleware call, no layout edits.
- The default locale owns the URL root and every other locale gets a code prefix equal to the string passed to `AddLocale`, with the matching `Content/<code>/` subfolder providing the translations.
- The `LanguageSwitcher` appears automatically once `LocalizationOptions.IsMultiLocale` flips to true, and it rewrites the current URL in place rather than redirecting to the home page.
- When a translation is missing, the content resolver falls back to the default-locale copy and renders a `FallbackNotice` banner naming the requested and default locales.

---
title: "Add a second locale to your site"
description: "Enable LocalizationOptions, add a translated locale subdirectory, wire locale routing, and drop in the LanguageSwitcher component."
section: "beyond-basics"
order: 10
tags: []
uid: tutorials.beyond-basics.add-a-locale
isDraft: true
search: false
llms: false
---

> **In this page.** Enabling `LocalizationOptions`, creating a locale subdirectory with translated markdown, wiring `UsePenningtonLocaleRouting`, and adding the `LanguageSwitcher` component.
>
> **Not in this page.** Per-locale search index internals or UI string translation plumbing — those are reference pages.

## What you'll do

- **Artifact.** A running DocSite that serves English at `/` and Spanish at `/es/`, with a working language switcher in the header.
- **Skill.** You'll know how to add a second locale, mirror the content into a locale folder, and move between the two versions of the site.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](/tutorials/docsite/scaffold)
- A running DocSite with at least one page under `Content/`

The finished code lives in [`examples/LocalizationTutorialExample`](https://github.com/PhilJollans/Pennington/tree/main/examples/LocalizationTutorialExample).

---

## 1. Declare a second locale in `DocSiteOptions`

_Tell the site it now has more than one language._

### Step 1.1 — Add `ConfigureLocalization` to your `DocSiteOptions`

- Open `Program.cs` where you call `AddDocSite(() => new DocSiteOptions { ... })`.
- Add `ConfigureLocalization`.
- Inside it, set English as the default locale and add Spanish as a second locale.

```text rawfile="examples/LocalizationTutorialExample/Program.cs"
```

_This example shows the minimal two-locale setup._

### Step 1.2 — Confirm locale routing is already wired

- Keep `UseDocSite()` in place.
- You do not need extra routing code for this tutorial.
- Save the file and restart the site if needed.

### Checkpoint — one locale, still working

- Run `dotnet run`
- Confirm `http://localhost:5000/` still loads in English
- Confirm the site is ready for a second locale

---

## 2. Mirror the content into a locale subdirectory

_Now add Spanish content so the second locale has something to show._

### Step 2.1 — Create `Content/es/` and translate your landing page

- Create `Content/es/` next to the existing content.
- Copy `Content/index.md` to `Content/es/index.md`.
- Translate the title, description, and body text into Spanish.

```text rawfile="examples/LocalizationTutorialExample/Content/index.md"
```

_This is the English page you start from._

```text rawfile="examples/LocalizationTutorialExample/Content/es/index.md"
```

_This is the Spanish version of the same page._

### Step 2.2 — Add a second translated page

- Copy one more English page into `Content/es/`.
- Translate that second page too.
- Keep the overall page structure the same so both locales feel parallel.

```text rawfile="examples/LocalizationTutorialExample/Content/es/getting-started.md"
```

_This example shows the second translated page in place._

### Checkpoint — two locales, two pages each

- Visit `http://localhost:5000/` and confirm the English pages still work
- Visit `http://localhost:5000/es/` and confirm the Spanish landing page loads
- Confirm the Spanish sidebar shows the translated pages you added

---

## 3. Verify the `LanguageSwitcher` component

_Finish by moving between the two versions of the site from the header._

### Step 3.1 — Click through the switcher

- Open the English home page.
- Use the language switcher to move to the Spanish home page.
- Open a translated Spanish page and switch back to English.

### Step 3.2 — Observe the result

- Try the switcher on a page that exists in both languages.
- Confirm the URL and page text both change.
- Leave deeper fallback behavior for the explanation and reference pages.

### Checkpoint — the switcher round-trips

- Switching from English to Spanish lands on `/es/`
- Switching back to English returns to the English page
- The language switcher reflects the current locale

---

## Summary

- You can add a second locale to a DocSite
- You can mirror content into a locale folder and translate the page values
- You can serve English at `/` and Spanish at `/es/`
- You can move between the two versions of the site with the language switcher

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

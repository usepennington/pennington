---
title: Documentation in every language
description: Localization is now a first-class part of the content engine — ambient locale context, automatic link rewriting, locale fallback, and ASP.NET integration.
author: Phil Scott
date: 2026-04-08
isDraft: false
tags:
  - localization
  - locale-fallback
---

Adding a second language to a .NET docs site used to be a slog: an `@page`
directive per locale on every page, a hand-rolled language switcher that knew
every route, and links between pages that didn't know which language they were
in. Pennington now handles most of that.

## Content lives in locale folders

Translated content goes into `Content/{locale}/` folders, and Pennington
discovers all of it through a single content service:

```text
Content/
  en/
    index.md
    guide/setup.md
  de/
    index.md
```

Every route is tagged with its locale, and the rest follows: navigation, search,
the sitemap with its `hreflang` alternates, and content resolution all work
per-locale without extra wiring. The [localization
how-to](xref:how-to.discovery.localization) walks through the folder setup.

## Fallback instead of 404

Translation is never finished all at once. When a non-default locale is missing
a page, Pennington serves the default locale's content instead of a 404, with a
`FallbackNotice` banner so the reader knows they're looking at the original. You
can publish `/de/` with a single German page and the rest of the site still
works — the reasoning is in [locale-aware URLs and content
fallback](xref:explanation.localization.urls-and-fallback).

## Ambient locale, automatic links

`LocaleContext` is a scoped, per-request value you can inject anywhere to get the
current locale. Middleware strips the locale prefix from the URL so a single
`@page` route matches every language, and rendered `<a href>` links are rewritten
to carry the current locale, so a link clicked on a German page lands on the
German target.

It also bridges to ASP.NET's `UseRequestLocalization`, so cookie persistence and
`Accept-Language` detection behave the way you'd expect. In the example sites,
the YogaStudioExample dropped 11 duplicate `@page` directives and removed its
custom `LanguageSwitcher`.

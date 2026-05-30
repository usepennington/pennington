---
title: Welcome
description: A DocSite homepage teaching Pennington localization.
order: 10
---

This site is written in two languages. The English version you're reading
lives under `Content/` — the default locale owns the URL root so its pages
serve from `/`, `/about`, and `/getting-started`.

Use the language switcher in the site header to jump to the Spanish version.
Every URL on this site has an equivalent in each configured locale, and the
`LanguageSwitcher` component in `MainLayout.razor` builds those links
automatically from the current request path.

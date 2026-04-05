---
title: About the Tavern
order: 4
---

The Multilingual Tavern was founded in the year 2026 by a group of traveling developers who were tired of documentation that only spoke one language.

## Our Mission

To prove that documentation can be both **useful** and **entertaining**, regardless of what tongue you speak.

## The Staff

- **The Barkeep** -- Manages the default locale. Speaks plain English and keeps things running.
- **The Pig Latin Scholar** -- A retired linguist who insists that Pig Latin is a legitimate language.
- **The Swedish Chef** -- Nobody is entirely sure what he's saying, but the docs taste great.
- **The Pirate Captain** -- Translates everything with appropriate nautical flair.

## How Translation Works

Each page exists as a separate markdown file per language. The system links them by matching file paths:

```
Content/about.md          -> /about         (English)
Content/pl/about.md       -> /pl/about      (Pig Latin)
Content/sv/about.md       -> /sv/about      (Swedish Chef)
Content/pi/about.md       -> /pi/about      (Pirate)
```

When a page hasn't been translated yet, the system automatically falls back to the English version and shows a friendly notice.

## Technical Details

This example demonstrates:

- Folder-based locale routing
- Automatic language detection from URL prefix
- Fallback to default language with notice banner
- Language switcher in the header
- Locale-filtered navigation (sidebar only shows current language)
- hreflang tags for SEO

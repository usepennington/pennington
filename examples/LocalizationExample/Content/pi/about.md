---
title: About This Here Tavern
order: 4
---

The Multilingual Tavern were founded in the year 2026 by a crew o' travelin' developers who were sick to their gills o' documentation that only spoke one tongue. So they raised the Jolly Roger an' set sail fer better docs!

## Our Mission

To prove that documentation can be both **useful** an' **entertainin'**, no matter what tongue ye speak. Even if that tongue be Pirate, arrr!

## The Crew

- **The Barkeep** -- Manages the default locale. Speaks plain English an' keeps the grog flowin'.
- **The Pig Latin Scholar** -- A retired linguist who insists Pig Latin be a legitimate language. We humor the old salt.
- **The Swedish Chef** -- Nobody be entirely sure what he's sayin', but the docs taste great. Bork bork bork, he says.
- **The Pirate Captain** -- That be me, ye scallywag! I translate everythin' with appropriate nautical flair!

## How Translation Works

Each page exists as a separate markdown file per language. The system links 'em by matchin' file paths, like matchin' a treasure map to the actual island:

```
Content/about.md          -> /about         (English)
Content/pl/about.md       -> /pl/about      (Pig Latin)
Content/sv/about.md       -> /sv/about      (Swedish Chef)
Content/pi/about.md       -> /pi/about      (Pirate - the best one!)
```

When a page hasn't been translated yet, the system drops anchor at the English version an' shows a friendly notice. No walkin' the plank required!

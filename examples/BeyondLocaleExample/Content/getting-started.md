---
title: Getting Started
description: Get started with the localized DocSite example.
order: 30
---

# Getting Started

To add a new locale to your own Pennington site:

1. Open `Program.cs` and call `loc.AddLocale(code, new LocaleInfo(displayName))`
   inside the `ConfigureLocalization` action on `DocSiteOptions`.
2. Create `Content/<code>/` and copy each page you want translated from the
   default-locale tree, translating the front matter `title:` and the body.
3. Run `dotnet run` — `LanguageSwitcher` appears in the site header as soon
   as `LocalizationOptions.Locales.Count > 1`.

There is no other wiring. The default locale keeps its URLs unchanged; every
additional locale gets a URL prefix equal to its code.

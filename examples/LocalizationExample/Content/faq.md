---
title: Frequently Asked Questions
order: 5
---

## Can I add more languages?

Absolutely! Just add another entry to the `Locales` dictionary and create a matching subfolder in your Content directory.

## What happens if a page isn't translated?

The system falls back to the default language (English in this case) and displays a notice at the top of the page letting readers know the translation isn't available yet.

## Does the navigation change per language?

Yes! The sidebar table of contents only shows pages for the current locale. If you're browsing in Pig Latin, you'll only see Pig Latin pages in the nav.

## How are pages linked across languages?

By matching file paths. `Content/faq.md` and `Content/fr/faq.md` are considered translations of the same page. No configuration needed.

## Can I have a page that only exists in one language?

Yes. It will simply not appear in the navigation for other languages. If someone follows a direct link, they'll get the fallback behavior.

## Is the URL prefix required for non-default languages?

Yes. Non-default languages always get a prefix (`/pl/`, `/sv/`, `/pi/`). The default language has no prefix -- this means existing single-language sites can add i18n without breaking URLs.

## What about RTL languages?

The system supports a `Direction` property on `LocaleInfo`. Set it to `"rtl"` and the `<html dir="rtl">` attribute will be set automatically.

```csharp
.Add("ar", new LocaleInfo("العربية", Direction: "rtl", HtmlLang: "ar"))
```

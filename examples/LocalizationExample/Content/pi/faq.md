---
title: Frequently Asked Questions, Arrr
order: 5
---

## Can I add more languages to me ship?

Aye, matey! Just add another entry to the `Locales` dictionary an' create a matchin' subfolder in yer Content hold. Easy as plunderin' a merchant vessel!

## What happens if a page ain't been translated?

The system drops anchor at the default language (English in this case) an' flies a notice flag lettin' readers know the translation ain't available yet. No one walks the plank!

## Does the navigation change per language?

Aye! The sidebar table o' contents only shows pages fer the current locale. If ye be browsing in Pirate, ye only see Pirate pages in the nav. As it should be!

## How be pages linked across languages?

By matchin' file paths, like matchin' stars to a navigation chart. `Content/faq.md` an' `Content/pi/faq.md` be considered translations o' the same page. No configuration needed, ye lazy swab!

## Can I have a page that only exists in one language?

Aye! It simply won't appear in the navigation fer other languages. If someone follows a direct link, they'll get the fallback behavior -- like findin' a message in a bottle written in the wrong language!

## Is the URL prefix required fer non-default languages?

Aye, it be required! Non-default languages always get a prefix (`/pl/`, `/sv/`, `/pi/`). The default language sails without a prefix -- keepin' the seas clear fer existing single-language ships to add i18n without breakin' their existing routes. Yo ho ho!

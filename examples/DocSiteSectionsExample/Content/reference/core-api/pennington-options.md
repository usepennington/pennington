---
title: PenningtonOptions
description: Core configuration surface handed to AddPennington.
sectionLabel: Core API
order: 10
---

# PenningtonOptions

`PenningtonOptions` is the record the configuration callback on
`AddPennington` mutates. It holds a handful of knobs that apply to every
page in the site regardless of content source.

## Keys worth knowing

- `ContentRootPath` — folder (or embedded-resource root) content is
  discovered from.
- `LowercaseUrls` — whether to force every generated URL to lowercase.
- `AppendTrailingSlash` — pick slash vs no-slash for clean URLs.
- `UrlStyle` — `Clean` (folder/index.html) or `Extension` (filename.html).

Every option has a sensible default; populate only the ones you need to
deviate from.

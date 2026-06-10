---
title: Metadata enricher demo
description: Show derived metadata an IMetadataEnricher contributes, read back through an Mdazor component.
---

`GitTimestampEnricher` runs during the parse phase and merges a
`git_last_modified` date into `ParsedItem.Derived`, alongside the built-in
`reading_time_minutes`. Derived metadata never touches authored front matter —
it is computed from the page.

## Reading it back

The renderer exposes the `Derived` dictionary to every Mdazor component under
the `Derived` context key. `<LastModified />` reads it and prints the date:

<LastModified />

This page's enriched keys also surface in its `/llms.txt` sidecar at
`/_llms/metadata-demo.md` — the front-matter block there carries both
`git_last_modified` and `reading_time_minutes`.

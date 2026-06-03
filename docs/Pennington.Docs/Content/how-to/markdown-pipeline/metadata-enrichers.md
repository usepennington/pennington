---
title: "Attach derived metadata to every page"
description: "Implement IMetadataEnricher to merge derived values like reading time or git timestamps into ParsedItem.Derived, kept separate from authored front matter."
uid: how-to.markdown-pipeline.metadata-enrichers
order: 5
sectionLabel: "Markdown Pipeline"
tags: [extensibility, pipeline, metadata, reading-time]
---

To compute values from a page rather than have an author type them — reading time, a git last-modified date, a word count — implement `IMetadataEnricher`. Each enricher contributes a dictionary that `MetadataEnrichmentService` merges into `ParsedItem.Derived`, a bag kept separate from the strongly-typed `Metadata` so authored front matter stays the single source of truth.

## Before you begin

- An existing Pennington site with markdown rendering wired (see <xref:tutorials.getting-started.first-site> if not).

## Reading time ships built in

`AddPennington` registers `ReadingTimeEnricher` by default, so every page already carries an estimate under the `reading_time_minutes` key. The estimate divides the word count by 200 words per minute and rounds up, with a floor of one minute. Read it downstream from `ParsedItem.Derived`:

```csharp
if (item.Derived.TryGetValue(ReadingTimeEnricher.Key, out var minutes))
{
    // minutes is an int — "5 min read"
}
```

## Write an enricher

Implement `IMetadataEnricher` and return the keys you contribute. `EnrichAsync` receives the parsed item and returns an `IReadOnlyDictionary<string, object?>`; return an empty dictionary to contribute nothing for a given page. The shipped `ReadingTimeEnricher` is the reference implementation — a pure function of `ParsedItem.RawMarkdown` with no file access:

```csharp:symbol
src/Pennington/Pipeline/ReadingTimeEnricher.cs
```

Expose the key as a `const` (as `ReadingTimeEnricher.Key` does) so consumers reference it without retyping the string.

## Register your enricher

Register the implementation after `AddPennington` — there is no `PenningtonOptions` knob. `MetadataEnrichmentService` runs every registered enricher in registration order and merges each contribution into `Derived`, so a later enricher overrides an earlier one on a key collision.

```csharp
builder.Services.AddTransient<IMetadataEnricher, GitTimestampEnricher>();
```

## Result

Every page's `ParsedItem.Derived` now carries the contributed keys alongside the built-in `reading_time_minutes`. The values feed any consumer that reads `Derived` — most visibly `llms.txt`, which emits derived metadata into its per-page front-matter blocks.

## Verify

- Render a page and confirm `item.Derived[ReadingTimeEnricher.Key]` returns the expected `int`.
- Add a custom enricher, build the site, and confirm its key appears in `/llms.txt` for a page it enriched.

## Related

- Reference: [`IMetadataEnricher`](xref:reference.api.i-metadata-enricher) and [`ReadingTimeEnricher`](xref:reference.api.reading-time-enricher)
- How-to: [Generate an llms.txt index](xref:how-to.feeds.llms-txt) — a built-in consumer of `Derived`

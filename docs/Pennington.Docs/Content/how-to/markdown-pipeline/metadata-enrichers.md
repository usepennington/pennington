---
title: "Attach derived metadata to every page"
description: "Implement IMetadataEnricher to merge derived values like reading time or git timestamps into ParsedItem.Derived, kept separate from authored front matter."
uid: how-to.markdown-pipeline.metadata-enrichers
order: 5
sectionLabel: "Markdown Pipeline"
tags: [extensibility, pipeline, metadata, reading-time]
---

To compute values from a page rather than have an author type them — reading time, a git last-modified date, a word count — implement `IMetadataEnricher`. Each enricher contributes a dictionary that `MetadataEnrichmentService` merges into `ParsedItem.Derived`. Derived values land in their own bag, not in the strongly-typed `Metadata`, so authored front matter stays the single source of truth and computed values can change between builds without rewriting any page.

## Before you begin

- An existing Pennington site with markdown rendering wired (see <xref:tutorials.getting-started.first-site> if not).

## Reading time ships built in

`AddPennington` registers `ReadingTimeEnricher` by default, so every page with body text carries an estimate under the `reading_time_minutes` key. The estimate divides the word count by 200 words per minute and rounds up, with a floor of one minute. A page with no words contributes no key, so consumers guard the read with `TryGetValue`. The shipped enricher is a pure function of `ParsedItem.RawMarkdown` — no file access:

```csharp:symbol
src/Pennington/Pipeline/ReadingTimeEnricher.cs
```

Expose the key as a `const` (as `ReadingTimeEnricher.Key` does) so consumers reference it without retyping the string.

## Write an enricher

Implement `IMetadataEnricher` and return the keys you contribute. `EnrichAsync` receives the parsed item and returns an `IReadOnlyDictionary<string, object?>`; return an empty dictionary to contribute nothing for a given page. `GitTimestampEnricher` reads the source file's timestamp from `ParsedItem.Route.SourceFile` and contributes a `git_last_modified` date — the value a real enricher would instead pull from `git log -1`. Pages with no file on disk (generated content) contribute nothing:

```csharp:symbol
examples/ExtensibilityLabExample/GitTimestampEnricher.cs > GitTimestampEnricher
```

## Register your enricher

Register the implementation after `AddPennington` — there is no `PenningtonOptions` knob. `MetadataEnrichmentService` runs every registered enricher in registration order and merges each contribution into `Derived`, so a later enricher overrides an earlier one on a key collision.

```csharp
builder.Services.AddTransient<IMetadataEnricher, GitTimestampEnricher>();
```

## Read derived metadata in a component

The renderer exposes the merged `Derived` dictionary to every Mdazor component under the `Derived` context key. A component reads it through `[CascadingParameter] public MdazorContext? Context` — no tag attributes, the dictionary cascades in from the page being rendered. `LastModified.razor` reads the `git_last_modified` key and renders the date:

```razor:symbol
examples/ExtensibilityLabExample/LastModified.razor
```

Register the component with `AddMdazorComponent<LastModified>()`, then drop `<LastModified />` into any page body.

## Verify

- Build the lab (`dotnet run --project examples/ExtensibilityLabExample -- build`) and open `/_llms/metadata-demo.md` in the output — its front-matter block carries both `git_last_modified` and `reading_time_minutes`, the two keys `Derived` accumulated for that page.
- Render `/metadata-demo/` and confirm the `<LastModified />` component prints the date, proving a component read `Context["Derived"]`.

## Related

- Reference: [`IMetadataEnricher`](xref:reference.api.i-metadata-enricher) and [`ReadingTimeEnricher`](xref:reference.api.reading-time-enricher)
- How-to: [Generate an llms.txt index](xref:how-to.feeds.llms-txt) — a built-in consumer of `Derived`

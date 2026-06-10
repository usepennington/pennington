---
title: "Publish a custom feed from a content service"
description: "Build the same RSS pattern BlogSite uses for /rss.xml — a content service that caches records, an XML builder method, and a MapGet endpoint — for podcast episodes, conference sessions, changelogs, or any non-blog content type."
uid: how-to.feeds.custom-feed
order: 3
sectionLabel: "Feeds & Indexes"
tags: [feeds, rss, atom, podcast, content-service]
---

`BlogSiteOptions.EnableRss` only applies to `BlogSiteFrontMatter` records. For any other content type — podcast episodes, conference sessions, a changelog — reuse the pattern `BlogSite` builds on: a content service caches the records, a `Task<string>` builder turns them into feed XML, and a `MapGet` endpoint serves that XML. Every `MapGet` endpoint is fetched and baked during the static build, so the feed file lands in `output/` next to every other page with no extra registration.

The reference implementation lives in `BlogSiteContentService.GetRssXmlAsync` and the `MapGet` call in `UseBlogSite`. This guide walks the three points you adapt in that pair, so the same shape can carry a podcast feed (with the iTunes namespace), an events feed (with iCalendar enclosures), or any custom format.

## Before you begin

- A bare `AddPennington` host (see <xref:tutorials.getting-started.first-site>) or any host where the records come from a custom `IContentService` — see <xref:how-to.content-services.custom-content-service> for the discovery shape.
- `CanonicalBaseUrl` set on `PenningtonOptions`, `DocSiteOptions`, or `BlogSiteOptions`. Without it, `<link>` and `<guid>` emit relative URLs that aggregators cannot follow.
- For the BlogSite-shipped `/rss.xml`, use <xref:how-to.feeds.rss> instead — this guide is for the other shapes.

## Build the feed XML on the content service

Add a `Task<string>` method to your content service that orders the cached records and emits XML with `System.Xml.Linq`. `BlogSiteContentService.GetRssXmlAsync` is the reference body:

```csharp:symbol
src/Pennington.BlogSite/Services/BlogSiteContentService.cs > BlogSiteContentService.GetRssXmlAsync
```

The pieces to adapt for your records:

- **The cache.** `DiscoverAsync` and the feed builder read from one cached list loaded once per generation, so the source files are parsed once and both paths see the same records. The `Lazy<T>` cache that already backs `DiscoverAsync` works here without changes.
- **The filter.** BlogSite drops posts without a `Date`. Replace this with whatever predicate keeps an entry in the feed (`IsPublished`, `Status == Released`, future-date skip via `TimeProvider`).
- **The ordering.** Newest-first is conventional for RSS; podcast aggregators expect it.
- **Absolute URLs.** Prefix every `<link>` and `<guid>` with `canonicalBase`. Relative paths break in feed readers.
- **The atom self-link.** `<atom:link rel="self" .../>` tells readers where the feed canonically lives. Match the URL you map below.
- **Per-item elements.** Keep `<title>`, `<link>`, `<guid>`. Add what your content type needs: `<category>` per tag, `<enclosure>` for media attachments, namespaced elements for iTunes/Atom/Dublin Core.

## Wire DI so the endpoint and the discovery list share one instance

Register the concrete service, then forward `IContentService` to the same instance with a transient indirection. Two separate registrations would let the container hand the endpoint a fresh copy with a cold cache:

```csharp
// File-watched when the service reads from disk; AddSingleton<T>() when the
// data source is in-process. The transient IContentService wrapper resolves
// against the current factory-managed instance so file-change recreates flow
// through to both the endpoint and the pipeline.
services.AddFileWatched<PodcastContentService>();
services.AddTransient<IContentService>(sp =>
    sp.GetRequiredService<PodcastContentService>());
```

`AddSingleton<IContentService>` here would cache the first file-watched copy and never refresh — the transient wrapper avoids that trap. The full lifetime contract for `AddFileWatched<T>` and the stale-data failure mode is in [Register the service](xref:how-to.content-services.custom-content-service#register-the-service).

## Map the endpoint

Inject the concrete service into a `MapGet` handler that returns the XML with the right MIME type:

```csharp
app.MapGet("/feed.xml", async (PodcastContentService service) =>
    Results.Content(await service.GetRssXmlAsync(), "application/rss+xml"));
```

Two reasons this single line carries both dev and build:

- **Dev mode** serves `/feed.xml` straight from the handler.
- **Static build** fetches every `MapGet` endpoint over HTTP through the live pipeline and writes each body to `output/` — so `output/feed.xml` is baked from the same handler. No `ContentToCreate` registration is needed.

Reach for `IContentService.GetContentToCreateAsync` instead only when there is no dev-time URL — for example, a `robots.txt` that should not respond live. See <xref:how-to.content-services.emit-generated-artifacts> for that shape.

## Adapt for podcast feeds (iTunes namespace)

A podcast RSS feed extends the same XML with the iTunes namespace plus per-item duration, episode number, and enclosure elements. Declare the namespace on the root and add the children inside the per-item loop:

```csharp
XNamespace atom = "http://www.w3.org/2005/Atom";
XNamespace itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";

var rss = new XElement("rss",
    new XAttribute("version", "2.0"),
    new XAttribute(XNamespace.Xmlns + "atom", atom.NamespaceName),
    new XAttribute(XNamespace.Xmlns + "itunes", itunes.NamespaceName),
    channel);

// Per-item additions inside the foreach in GetRssXmlAsync:
entry.Add(
    new XElement(itunes + "duration", episode.Duration.ToString(@"hh\:mm\:ss")),
    new XElement(itunes + "episode", episode.EpisodeNumber),
    new XElement(itunes + "season", episode.SeasonNumber),
    new XElement("enclosure",
        new XAttribute("url", absoluteAudioUrl),
        new XAttribute("length", episode.AudioBytes),
        new XAttribute("type", "audio/mpeg")));
```

Channel-level iTunes elements (`<itunes:image>`, `<itunes:category>`, `<itunes:owner>`) sit alongside the existing `<title>` / `<link>` / `<description>` block. Apple's [Podcasters Connect](https://podcasters.apple.com/support/823-podcast-requirements) page is the authoritative list.

## Adapt for Atom feeds

Atom 1.0 uses a different root and element vocabulary. The shape is identical — same cache, same builder method, same `MapGet` — only the XML changes. The sketch below shows the element structure; `canonicalBase`, `ordered`, and `absoluteUrl` are the same locals the RSS builder above sets up, dropped here for focus:

```csharp
XNamespace atom = "http://www.w3.org/2005/Atom";

var feed = new XElement(atom + "feed",
    new XElement(atom + "title", _options.SiteTitle),
    new XElement(atom + "id", canonicalBase + "/"),
    new XElement(atom + "updated", DateTime.UtcNow.ToString("o")));

foreach (var entry in ordered)
{
    feed.Add(new XElement(atom + "entry",
        new XElement(atom + "title", entry.Title),
        new XElement(atom + "id", absoluteUrl),
        new XElement(atom + "updated", entry.Date.ToString("o")),
        new XElement(atom + "link", new XAttribute("href", absoluteUrl))));
}
```

Serve at a separate path (`/atom.xml`) with `application/atom+xml`. Nothing stops a site from publishing both RSS and Atom — register two endpoints against the same service.

## Verify

- Run `dotnet run` and fetch `/feed.xml`. The response is the right MIME type with one item per record.
- Run `dotnet run -- build output` and confirm `output/feed.xml` exists with the same body. The build crawler reuses the live endpoint.
- Validate the XML externally — `xmllint --noout feed.xml` catches well-formedness errors. For podcasts, run the file through Apple's podcast validator before submitting to directories.
- Edit a source record and refetch `/feed.xml` in dev. The file-watched cache rebuilds and the change appears without a restart.

## Related

- How-to: [Publish an RSS feed (BlogSite)](xref:how-to.feeds.rss)
- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- How-to: [Source content from a remote API](xref:how-to.content-services.remote-api)
- How-to: [Emit generated output artifacts](xref:how-to.content-services.emit-generated-artifacts)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)

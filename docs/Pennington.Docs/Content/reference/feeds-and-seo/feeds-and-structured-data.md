---
title: "Feeds and Structured Data"
description: "Reference for RssFeedBuilder (IDateable requirement), RssFeedItem, SitemapBuilder, SitemapEntry, JSON-LD types (JsonLdArticle, JsonLdBreadcrumbList, JsonLdBreadcrumbItem, JsonLdWebSite), JsonLdSerializer, and LlmsTxtOptions"
uid: "penn.reference.feeds-and-structured-data"
order: 10
---

Document feed and SEO types. `RssFeedBuilder`: `Build(RenderedItem[])` returns `RssFeedItem[]` — requires `IDateable` with a valid date, skips drafts, sorts descending. `RssFeedItem` record fields. `SitemapBuilder`: `Build()` generates `SitemapEntry[]` from all routes. `SitemapEntry` record fields. JSON-LD types: `JsonLdArticle` (Headline, Description, Url, DatePublished, AuthorName), `JsonLdBreadcrumbList` (Items), `JsonLdBreadcrumbItem` (Position, Name, Url), `JsonLdWebSite` (Name, Url, Description). `JsonLdSerializer` methods and their schema.org output format. `LlmsTxtOptions`: OutputDirectory (string, default "_llms"), GenerateFullFile (bool). `LlmsTxtService` and `LlmsTxtContentService` behavior.

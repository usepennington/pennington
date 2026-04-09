---
title: "Content Services"
description: "Reference for IContentService (5 methods, 2 properties) and all implementations: MarkdownContentService<T>, RazorPageContentService, LlmsTxtContentService, SpaNavigationContentService — plus supporting types ContentToCopy, ContentToCreate, ContentTocItem, CrossReference"
uid: "penn.reference.content-services"
order: 20
---

Document the `IContentService` interface comprehensively: each of the 5 methods (`DiscoverAsync`, `GetContentToCopyAsync`, `GetContentToCreateAsync`, `GetContentTocEntriesAsync`, `GetCrossReferencesAsync`) with return types, expected behavior, and when each is called by the pipeline. Document the 2 properties (`DefaultSection`, `SearchPriority`). Then catalog each built-in implementation: `MarkdownContentService<T>` (generic, file-based, locale-aware), `RazorPageContentService` (assembly-scanned, @page discovery), `SpaNavigationContentService` (generates _spa-data JSON files), `LlmsTxtContentService` (generates llms.txt from markdown). Document supporting types: `ContentToCopy` (SourcePath, DestinationPath), `ContentToCreate` (Route, Content, ContentType), `ContentTocItem` (Title, Route, Order, HierarchyParts, Section, Locale), `CrossReference` (Uid, Title, Route).

---
title: "Front Matter System"
description: "Reference for IFrontMatter and the 8 capability interfaces (IDraftable, ITaggable, ISectionable, ICrossReferenceable, IOrderable, IDescribable, IDateable, IRedirectable) — property types, YAML conventions, and built-in type comparison table"
uid: "penn.reference.front-matter-system"
order: 10
---

The authoritative reference for Penn's front matter system. Start with the `IFrontMatter` base interface (requires `Title` only). Then document each of the 8 capability interfaces in a consistent format: interface name, the property it adds, its type, and how the pipeline uses it. The 8 are: `IDraftable` (IsDraft bool), `ITaggable` (Tags string[]), `ISectionable` (Section string), `ICrossReferenceable` (Uid string), `IOrderable` (Order int), `IDescribable` (Description string), `IDateable` (Date DateTime?), `IRedirectable` (RedirectUrl string). Include a comparison table of the built-in types (`DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`) showing which capabilities each implements. Document YAML conventions: camelCase property names (not snake_case), how lists map to arrays, how dates parse. Use `:xmldocid` code blocks to show the actual interface definitions from source.

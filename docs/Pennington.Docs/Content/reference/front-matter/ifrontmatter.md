---
title: "IFrontMatter and capability defaults"
description: "The IFrontMatter contract — its required Title, six default members, and the four remaining capability interfaces that pattern-match separately."
sectionLabel: "Front Matter"
order: 402020
tags: [front-matter, capabilities, interfaces]
uid: reference.front-matter.ifrontmatter
---

`IFrontMatter` is the universal front-matter contract that every Pennington content page implements, declaring one required `Title` property and six default members covering drafts, indexing opt-outs, uid, description, and date. It is declared in `Pennington.FrontMatter`; the four remaining capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) are in the same namespace.

## Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

## Members

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.IFrontMatter" />

## Capability interfaces

The four remaining capability interfaces stay separate from `IFrontMatter` because not every content type implements them; consumers pattern-match these interfaces independently, and each interface declares exactly one property.

### `ITaggable`

```csharp:xmldocid
T:Pennington.FrontMatter.ITaggable
```

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.ITaggable" />

Implemented by `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter`.

### `IOrderable`

```csharp:xmldocid
T:Pennington.FrontMatter.IOrderable
```

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.IOrderable" />

Implemented by `DocFrontMatter` and `DocSiteFrontMatter`, both defaulting to `int.MaxValue` so unset pages sort last.

### `ISectionable`

```csharp:xmldocid
T:Pennington.FrontMatter.ISectionable
```

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.ISectionable" />

Implemented by `DocFrontMatter` and `DocSiteFrontMatter`.

### `IRedirectable`

```csharp:xmldocid
T:Pennington.FrontMatter.IRedirectable
```

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.IRedirectable" />

Implemented by `DocSiteFrontMatter` and `BlogSiteFrontMatter`; see <xref:how-to.content-authoring.redirects> for authoring practice.

## Example

```csharp:xmldocid,bodyonly
T:DocSiteKitchenSinkExample.ApiFrontMatter
```

This record demonstrates the reference shape for a custom front-matter type with full capability coverage: `IFrontMatter` plus all four capability interfaces in a single declaration.

## See also

- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Related reference: [Front matter key reference](xref:reference.front-matter.keys)
- Related reference: [Built-in front-matter types](xref:reference.front-matter.built-in-types)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)

---
title: "IFrontMatter and capability defaults"
description: "The IFrontMatter contract, which keys have default implementations, and how the consolidated capabilities (ITaggable, IOrderable, ISectionable, IRedirectable) surface as default members."
section: "front-matter"
order: 20
tags: []
uid: reference.front-matter.ifrontmatter
isDraft: true
search: false
llms: false
---

> **In this page.** The `IFrontMatter` contract, which keys have default implementations, and how the consolidated capabilities (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) surface as default members.
>
> **Not in this page.** The rationale for consolidation — see Explanation.

## Summary

The interface every front-matter record implements; declares one required property and six default members.
Namespace `Pennington.FrontMatter`, declared in `src/Pennington/FrontMatter/IFrontMatter.cs` and `src/Pennington/FrontMatter/Capabilities.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

## Members

| Name | Type | Default | Required |
|---|---|---|---|
| `Title` | `string` | — | yes |
| `Date` | `DateTime?` | `null` | no |
| `Description` | `string?` | `null` | no |
| `IsDraft` | `bool` | `false` | no |
| `Llms` | `bool` | `true` | no |
| `Search` | `bool` | `true` | no |
| `Uid` | `string?` | `null` | no |

## Capability interfaces

Separate interfaces in `Pennington.FrontMatter`; records opt in by listing them alongside `IFrontMatter`. None provide default members — each declared property must be supplied by the implementer.

| Interface | Member | Type |
|---|---|---|
| `IOrderable` | `Order` | `int` |
| `IRedirectable` | `RedirectUrl` | `string?` |
| `ISectionable` | `Section` | `string?` |
| `ITaggable` | `Tags` | `string[]` |

### `IOrderable`

```csharp:xmldocid
T:Pennington.FrontMatter.IOrderable
```

### `IRedirectable`

```csharp:xmldocid
T:Pennington.FrontMatter.IRedirectable
```

### `ISectionable`

```csharp:xmldocid
T:Pennington.FrontMatter.ISectionable
```

### `ITaggable`

```csharp:xmldocid
T:Pennington.FrontMatter.ITaggable
```

## Example

```csharp:xmldocid,bodyonly
T:MultipleContentSourceExample.ContentFrontMatter
```

A record implementing `IFrontMatter` plus all four capability interfaces.

## See also

- Related reference: [Front matter key reference](/reference/front-matter/keys)
- Related reference: [Built-in front-matter types](/reference/front-matter/built-in-types)
- Background: [The front-matter capability system](/explanation/core/front-matter-capabilities)

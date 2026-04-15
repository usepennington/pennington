---
title: "BlogSiteOptions"
description: "Configuration surface passed to AddBlogSite — blog metadata, content paths, author chrome, homepage data, and feed toggles."
sectionLabel: "Configuration Options"
order: 401030
tags: [options, blog, configuration]
uid: reference.options.blogsite-options
---

`BlogSiteOptions` is the record supplied to `services.AddBlogSite(...)` that configures the `Pennington.BlogSite` template — site identity, content layout, homepage composition, and feed toggles. It is declared in namespace `Pennington.BlogSite`, alongside the helper records `HeroContent`, `Project`, `SocialLink`, and `HeaderLink`.

## Declaration

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteOptions
```

## Properties

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.BlogSiteOptions" />

## Helper records

### `HeroContent`

```csharp:xmldocid
T:Pennington.BlogSite.HeroContent
```

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.HeroContent" />

### `Project`

```csharp:xmldocid
T:Pennington.BlogSite.Project
```

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.Project" />

### `SocialLink`

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.SocialLink" />

### `HeaderLink`

```csharp:xmldocid
T:Pennington.BlogSite.HeaderLink
```

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.HeaderLink" />

## Example

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

## See also

- How-to: [Configure the BlogSite homepage](xref:how-to.configuration.blogsite-homepage)
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Related reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) <!-- TODO verify explanation URL once the page lands -->

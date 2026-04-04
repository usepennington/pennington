---
title: "Front Matter Properties"
description: "Reference for the capability-based front matter system — IFrontMatter, capability interfaces, and built-in types"
uid: "penn.reference.front-matter-properties"
order: 4002
---

Penn v2 threw out the kitchen sink. Where v1 stuffed every conceivable property into `IFrontMatter` and dared you to ignore the ones you didn't need, v2 starts with almost nothing and lets you opt in. It's the difference between a buffet and a tasting menu — fewer regrets either way.

## The Base Interface

```csharp:path
src/Penn/FrontMatter/IFrontMatter.cs
```

That's it. One property. Every content page has a title. Everything else is a capability you compose by implementing additional interfaces.

## Capability Interfaces

Capabilities are small interfaces in the `Penn.FrontMatter` namespace. Your front matter record implements only the ones it needs, and the pipeline checks for them at runtime via pattern matching.

```csharp:xmldocid
T:Penn.FrontMatter.IDraftable
```

```csharp:xmldocid
T:Penn.FrontMatter.ITaggable
```

```csharp:xmldocid
T:Penn.FrontMatter.IDescribable
```

```csharp:xmldocid
T:Penn.FrontMatter.IOrderable
```

```csharp:xmldocid
T:Penn.FrontMatter.IDateable
```

```csharp:xmldocid
T:Penn.FrontMatter.ICrossReferenceable
```

```csharp:xmldocid
T:Penn.FrontMatter.ISectionable
```

```csharp:xmldocid
T:Penn.FrontMatter.IRedirectable
```

### Quick Reference

| Interface | Property | Type | Purpose |
|-----------|----------|------|---------|
| `IDraftable` | `IsDraft` | `bool` | Exclude page from generation when `true` |
| `ITaggable` | `Tags` | `string[]` | Tag-based categorization and filtering |
| `IDescribable` | `Description` | `string?` | Meta description, RSS summaries |
| `IOrderable` | `Order` | `int` | Sort position in navigation and TOC |
| `IDateable` | `Date` | `DateTime?` | Publication date for feeds and sorting |
| `ICrossReferenceable` | `Uid` | `string?` | Unique ID for `xref:` cross-references |
| `ISectionable` | `Section` | `string?` | Logical section grouping |
| `IRedirectable` | `RedirectUrl` | `string?` | HTTP redirect to another URL |

## How the Pipeline Uses Capabilities

The pipeline doesn't care what your front matter type looks like — it checks capabilities with pattern matching. No interface, no behaviour:

```csharp
// Skip drafts — only if the type opted into IDraftable
if (frontMatter is IDraftable { IsDraft: true })
    continue;

// Build navigation order — only if the type opted into IOrderable
var order = frontMatter is IOrderable orderable
    ? orderable.Order
    : int.MaxValue;

// Filter by tag — only if the type opted into ITaggable
if (frontMatter is ITaggable taggable && taggable.Tags.Contains("archived"))
    continue;
```

This means a minimal front matter type with just `IFrontMatter` will never be skipped as a draft, will sort to the end of navigation, and will have no tags. That's not a bug — it's a feature. You get exactly the behaviour you asked for.

## Built-in Front Matter Types

Penn ships three front matter records that cover common use cases. You can use them directly or treat them as examples for your own types.

### DocFrontMatter

The general-purpose documentation type. Implements everything except `IDateable` and `IRedirectable` — documentation pages don't typically have publication dates or redirects.

```csharp:path
src/Penn/FrontMatter/DocFrontMatter.cs
```

**YAML example:**

```yaml
---
title: "Getting Started"
description: "Your first Penn site in under five minutes (optimistic estimate)"
uid: "penn.getting-started"
order: 1000
section: "guides"
tags:
  - Tutorial
  - Quick Start
---
```

### BlogFrontMatter

Adds `IDateable` for publication dates. Includes two extra properties — `Author` and `Series` — that aren't capability interfaces, because not everything needs to be an abstraction.

```csharp:path
src/Penn/FrontMatter/BlogFrontMatter.cs
```

**YAML example:**

```yaml
---
title: "On the Merits of Capability Interfaces"
description: "A meditation on doing less, better"
date: 2026-03-15
author: "Penn Contributor"
series: "Architecture Decisions"
tags:
  - Architecture
  - Design
is_draft: false
uid: "blog.capability-interfaces"
---
```

### DocSiteFrontMatter

The most fully-featured built-in type — implements all eight capability interfaces. Used by `Penn.DocSite` for documentation sites that need redirects (moved pages) alongside the standard documentation features.

```csharp:path
src/Penn.DocSite/DocSiteFrontMatter.cs
```

**YAML example:**

```yaml
---
title: "Configuration Reference"
description: "All configuration options for Penn.DocSite"
uid: "docsite.configuration"
order: 2000
section: "reference"
redirect_url:
tags:
  - Reference
  - Configuration
---
```

## Creating Your Own Front Matter Type

Pick the capabilities you need. Ignore the rest. Penn won't judge.

```csharp
public record RecipeFrontMatter : IFrontMatter, ITaggable, IDateable, IDescribable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }

    // Custom properties — not everything has to be an interface
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public string[] Ingredients { get; init; } = [];
}
```

Register it with a markdown content source:

```csharp
services.AddPenn(options =>
{
    options.SiteTitle = "My Recipe Site";
    options.AddMarkdownContent<RecipeFrontMatter>(source =>
    {
        source.ContentPath = "Content/recipes";
        source.BasePageUrl = "/recipes";
    });
});
```

This type will support tags, dates, and descriptions. It won't support drafts, ordering, sections, cross-references, or redirects — because it doesn't implement those interfaces.

## YAML Naming Convention

Penn uses YamlDotNet's `UnderscoredNamingConvention` for deserializing front matter. C# property names map to underscored YAML keys:

| C# Property | YAML Key |
|-------------|----------|
| `Title` | `title` |
| `IsDraft` | `is_draft` |
| `RedirectUrl` | `redirect_url` |
| `PrepTimeMinutes` | `prep_time_minutes` |

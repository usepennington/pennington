---
title: "Front Matter Properties"
description: "Reference for the capability-based front matter system — IFrontMatter, capability interfaces, and built-in types"
uid: "penn.reference.front-matter-properties"
order: 4002
---

Penn's front matter system starts with a single required property and lets you opt into additional behavior through capability interfaces. Your front matter type implements only the interfaces it needs. The pipeline checks for each capability at runtime via pattern matching and ignores capabilities your type doesn't implement.

## The Base Interface

Every front matter type implements `IFrontMatter`:

```csharp:path
src/Penn/FrontMatter/IFrontMatter.cs
```

`Title` is the only required property. Every content page has a title. Everything else — drafts, tags, ordering, dates — is a capability you compose by implementing additional interfaces.

## Capability Interfaces

Capabilities are single-property interfaces in the `Penn.FrontMatter` namespace. Each one declares a specific behavior that the pipeline can check for.

```csharp:path
src/Penn/FrontMatter/Capabilities.cs
```

### Quick Reference

| Interface | Property | Type | Purpose |
|-----------|----------|------|---------|
| `IDraftable` | `IsDraft` | `bool` | Exclude page from output when `true` |
| `ITaggable` | `Tags` | `string[]` | Tag-based categorization and filtering |
| `IDescribable` | `Description` | `string?` | Meta description, RSS summaries |
| `IOrderable` | `Order` | `int` | Sort position in navigation and table of contents |
| `IDateable` | `Date` | `DateTime?` | Publication date for feeds and chronological sorting |
| `ICrossReferenceable` | `Uid` | `string?` | Unique identifier for `xref:` cross-references |
| `ISectionable` | `Section` | `string?` | Logical navigation section grouping |
| `IRedirectable` | `RedirectUrl` | `string?` | HTTP redirect to another URL |

## How the Pipeline Uses Capabilities

The pipeline holds a reference to `IFrontMatter` and checks for capabilities with C# pattern matching. If a front matter type does not implement a capability interface, the corresponding behavior does not apply.

### Draft Filtering

The content pipeline, search index builder, RSS feed builder, and sitemap builder all check for `IDraftable`. Pages marked as drafts are excluded from output and recorded in the build report:

```csharp
if (rendered.Metadata is IDraftable { IsDraft: true })
{
    reportBuilder.AddSkippedPage(rendered.Route);
}
```

A front matter type that does not implement `IDraftable` is never skipped as a draft.

### Navigation Ordering

`MarkdownContentService` reads `IOrderable` to determine sort position in the table of contents. Types that do not implement `IOrderable` default to `int.MaxValue`, placing them at the end:

```csharp
var order = fm is IOrderable orderable ? orderable.Order : int.MaxValue;
```

### Section Assignment

`ISectionable` overrides the content source's default section. If the front matter does not implement `ISectionable`, the section configured on the content source options is used instead:

```csharp
var section = fm is ISectionable sectionable ? sectionable.Section : _options.Section;
```

### Cross-Reference Registration

`MarkdownContentService` registers cross-references for any front matter that implements `ICrossReferenceable` with a non-empty `Uid`:

```csharp
if (fm is ICrossReferenceable { Uid: { } uid } && !string.IsNullOrEmpty(uid))
{
    builder.Add(new CrossReference(uid, fm.Title, route));
}
```

Pages without `ICrossReferenceable` are not addressable via `xref:` links.

### RSS Feed Eligibility

The RSS feed builder requires both `IDraftable` (to skip drafts) and `IDateable` (to determine publication date). A page must implement `IDateable` with a non-null `Date` to appear in the feed:

```csharp
if (item.Metadata is IDraftable { IsDraft: true })
    continue;

if (item.Metadata is IDateable { Date: { } date })
{
    eligible.Add((item, date));
}
```

Front matter that is `IDateable` but not `IDraftable` is included in the feed without a draft check.

## Built-in Front Matter Types

Penn ships three front matter records that cover common use cases. Use them directly, or use them as a reference when creating your own types.

### DocFrontMatter

The general-purpose documentation type. Implements six capability interfaces. Does not implement `IDateable` (documentation pages do not typically have publication dates) or `IRedirectable` (no redirect support).

```csharp:path
src/Penn/FrontMatter/DocFrontMatter.cs
```

**YAML example:**

```yaml
---
title: "Getting Started"
description: "Your first Penn site in under five minutes"
uid: "penn.getting-started"
order: 1000
section: "Guides"
tags:
  - Tutorial
  - Quick Start
---
```

### BlogFrontMatter

Adds `IDateable` for publication dates. Does not implement `IOrderable`, `ISectionable`, or `IRedirectable`. Includes two additional properties — `Author` and `Series` — that are not capability interfaces. Custom properties work the same as capability properties in YAML; they just don't trigger pipeline behavior.

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
isDraft: false
uid: "blog.capability-interfaces"
tags:
  - Architecture
  - Design
---
```

### DocSiteFrontMatter

The most fully-featured built-in type. Implements all eight capability interfaces. Used by `Penn.DocSite` for documentation sites that need redirect support for moved pages alongside the standard documentation features.

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
section: "Reference"
redirectUrl:
tags:
  - Reference
  - Configuration
---
```

### Comparison

| Capability | `DocFrontMatter` | `BlogFrontMatter` | `DocSiteFrontMatter` |
|------------|:---:|:---:|:---:|
| `IDraftable` | Yes | Yes | Yes |
| `ITaggable` | Yes | Yes | Yes |
| `IDescribable` | Yes | Yes | Yes |
| `ICrossReferenceable` | Yes | Yes | Yes |
| `IOrderable` | Yes | -- | Yes |
| `ISectionable` | Yes | -- | Yes |
| `IDateable` | -- | Yes | -- |
| `IRedirectable` | -- | -- | Yes |

## Creating Your Own Front Matter Type

Define a record that implements `IFrontMatter` and whichever capability interfaces apply to your content. You can add custom properties beyond the capability interfaces.

```csharp
public record RecipeFrontMatter : IFrontMatter, ITaggable, IDateable, IDescribable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }

    // Custom properties — not capability interfaces, but valid YAML properties
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public string[] Ingredients { get; init; } = [];
}
```

Register the type with a markdown content source:

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

This type supports tags, dates, and descriptions. It does not support drafts, ordering, sections, cross-references, or redirects, because it does not implement those interfaces.

You can register multiple front matter types by calling `AddMarkdownContent` more than once, each with a different type parameter and content path. See [Multiple Content Sources](xref:penn.guides.multiple-content-sources) for details.

```csharp
services.AddPenn(options =>
{
    options.AddMarkdownContent<DocFrontMatter>(source =>
    {
        source.ContentPath = "Content/docs";
        source.BasePageUrl = "/docs";
        source.Section = "Documentation";
    });
    options.AddMarkdownContent<BlogFrontMatter>(source =>
    {
        source.ContentPath = "Content/blog";
        source.BasePageUrl = "/blog";
    });
});
```

For full control over content discovery and rendering beyond markdown files, see [Custom Content Services](xref:penn.guides.custom-content-service).

## YAML Naming Convention

Penn uses YamlDotNet's `CamelCaseNamingConvention` for deserializing front matter. C# property names map to camelCase YAML keys:

| C# Property | YAML Key |
|-------------|----------|
| `Title` | `title` |
| `IsDraft` | `isDraft` |
| `RedirectUrl` | `redirectUrl` |
| `PrepTimeMinutes` | `prepTimeMinutes` |

This is **camelCase**, not underscore_case. Write `isDraft`, not `is_draft`. Write `redirectUrl`, not `redirect_url`.

The parser is configured in `FrontMatterParser`:

```csharp:path
src/Penn/FrontMatter/FrontMatterParser.cs
```

The `IgnoreUnmatchedProperties()` call means that unrecognized YAML keys are silently ignored rather than causing parse errors. This allows forward compatibility when front matter blocks contain keys that are not yet mapped to properties on the target type.

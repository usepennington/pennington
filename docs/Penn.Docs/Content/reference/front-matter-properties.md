---
title: "Front Matter Properties"
description: "Reference guide for all available front matter properties and their usage"
uid: "docs.reference.front-matter-properties"
order: 4002
---

All front matter implementations must implement the `IFrontMatter` interface, which defines the base contract for
content metadata.

## Quick Reference

### YAML Properties

These are the properties you write in the `---` front matter block of your Markdown files. The YAML key uses
underscored naming (e.g. `is_draft`, not `isDraft`).

| YAML Key | C# Property | Type | Default | Purpose |
|----------|-------------|------|---------|---------|
| `title` | `Title` | `string` | required | Page title — used in navigation, browser tab, RSS |
| `uid` | `Uid` | `string?` | `null` | Unique ID for cross-referencing with `xref:` |
| `tags` | `Tags` | `string[]` | `[]` | Tag-based categorization and filtering |
| `is_draft` | `IsDraft` | `bool` | `false` | When `true`, page is excluded from generation |
| `redirect_url` | `RedirectUrl` | `string?` | `null` | Redirect this URL to another page |

### Metadata Returned by `AsMetadata()`

Your `IFrontMatter` implementation maps to these standard fields via the `AsMetadata()` method:

| Field | Type | Default | Purpose |
|-------|------|---------|---------|
| `Title` | `string` | required | Page headers and HTML `<title>` |
| `Description` | `string` | `""` | Meta description, RSS summaries |
| `LastMod` | `DateTime?` | `null` | Sitemaps and RSS `<lastmod>` |
| `RssItem` | `bool` | `true` | Include this page in the RSS feed |
| `Order` | `int` | `int.MaxValue` | Sort order in navigation and TOC |

---

```csharp:xmldocid
T:MyLittleContentEngine.Models.IFrontMatter
```

## Required Properties

### Title

- **Type**: `string`
- **Purpose**: The title of the content page
- **Usage**: Used in page headers, metadata, RSS feeds, and navigation
- **Example**: `title: "Getting Started with Blazor"`

## Optional Properties

### Tags

- **Type**: `string[]` (array of strings)
- **Purpose**: Content categorization and tagging
- **Usage**: Used for tag-based navigation, filtering, and content organization
- **Example**:
  ```yaml
  tags:
    - Blazor
    - .NET
    - Web Development
  ```

### Uid

- **Type**: `string?` (nullable string)
- **Purpose**: Unique identifier for the content page
- **Usage**: Used for cross-referencing and unique identification
- **Default**: `null`
- **Example**: `uid: "getting-started-blazor"`


### IsDraft

- **Type**: `bool`
- **Purpose**: Controls whether the content page will be generated
- **Usage**: When `true`, the page's excluded from static generation
- **Default**: `false`
- **Example**: `is_draft: true`

### RedirectUrl

- **Type**: `string?` (nullable string)
- **Purpose**: Creates an HTML redirect page that automatically redirects to another URL
- **Usage**: When specified, generates an HTML page with `<meta http-equiv="refresh">` instead of rendering markdown content
- **Default**: `null`
- **Example**: `redirect_url: "config-runsettings"`
- **Note**: Pages with redirect URLs are excluded from the table of contents but are still included in static generation

## Required Methods

All front matter must implement `AsMetadata`, which returns a `Metadata` instance. This class contains fields that all content generations rely on being standard. The `AsMetadata` method acts as a conversion from custom metadata to this standard format.

The `Metadata` class also provides additional computed information for RSS feeds and sitemaps.

### The `AsMetadata` Method

#### Title

- **Type**: `string`
- **Purpose**: The title of the content page
- **Usage**: Used in page headers, metadata, RSS feeds, and navigation
- **Example**: `title: "Getting Started with Blazor"`

#### Description

- **Type**: `string`
- **Purpose**: The description of the content page
- **Usage**: Used in page headers, metadata, RSS feeds, and navigation to describe the page
- **Example**: `description: "A how-to guide on taking your first steps with Blazor"`



#### LastMod

- **Type**: `DateTime?`
- **Purpose**: Date when the page was last modified
- **Usage**: Used in XML sitemaps and RSS feeds

#### RssItem

- **Type**: `bool`
- **Purpose**: Controls whether the page should be included in RSS feeds
- **Usage**: RSS feed filtering
- **Default**: `true`

#### Order

- **Type**: `int`
- **Purpose**: Controls page order in navigation or table of contents
- **Usage**: Navigation ordering and TOC generation
- **Default**: `int.MaxValue`


## YAML Front Matter Examples

### Blog Post Example

```csharp
public class BlogFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "Empty title";
    public string? Uid { get; init; } = null;
    public bool IsDraft { get; init; } = false;
    public string[] Tags { get; init; } = [];

    // custom properties for blog posts
    public string Description { get; init; } = string.Empty;
    public DateTime Date { get; init; } = DateTime.Now;
    
    public Metadata AsMetadata()
    {
        return new Metadata()
        {
            Title = Title,
            Description = Description,
            LastMod = Date,
            RssItem = true
        };
    }
}
```

Each post would have front matter like this:

```yaml
---
title: "Getting Started with Blazor"
description: "A comprehensive guide to building your first Blazor application"
date: 2025-01-15
tags:
  - Blazor
  - .NET
  - Tutorial
is_draft: false
uid: "blazor-getting-started"
---
```

### Documentation Page Example

```csharp
internal class DocsFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "Empty title";
    public bool IsDraft { get; init; } = false;
    public string[] Tags { get; init; } = [];
    public string? Uid { get; init; } = null;
    
    // custom properties for documentation pages
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; } = int.MaxValue;
    
    public Metadata AsMetadata()
    {
        return new Metadata()
        {
            Title = Title,
            Description = Description,
            LastMod = DateTime.MinValue,
            RssItem = false,
            Order = Order
        };
    }

}
```

```yaml
---
title: "API Reference"
description: "Complete API documentation for MyLittleContentEngine"
order: 4001
tags:
  - Reference
  - API
is_draft: false
---
```

## YAML Naming Convention

By default, MyLittleContentEngine uses YamlDotNet's `UnderscoredNamingConvention` for deserializing front matter properties. This means:

- C# properties like `RedirectUrl` are mapped to YAML properties like `redirect_url`
- C# properties like `IsDraft` are mapped to YAML properties like `is_draft`
- C# properties like `PublishDate` are mapped to YAML properties like `publish_date`

### Example Property Mapping

```csharp
public class BlogFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "";           // maps to: title
    public bool IsDraft { get; init; } = false;        // maps to: is_draft
    public string? RedirectUrl { get; init; } = null;  // maps to: redirect_url
    public DateTime PublishDate { get; init; };        // maps to: publish_date
}
```

```yaml
---
title: "My Blog Post"
is_draft: false
redirect_url: "new-location"
publish_date: 2024-01-15
---
```

### Customizing YAML Naming Convention

You can customize the naming convention by configuring the `FrontMatterDeserializer` in the `ContentEngineOptions` class found in `ContentOptions.cs`. You can replace the default `UnderscoredNamingConvention` with other YamlDotNet conventions like:

- `CamelCaseNamingConvention` - maps `RedirectUrl` to `redirectUrl`
- `PascalCaseNamingConvention` - maps `RedirectUrl` to `RedirectUrl`
- `KebabCaseNamingConvention` - maps `RedirectUrl` to `redirect-url`

## Best Practices

1. **Consistent Naming**: Use consistent property names across your content
2. **Meaningful Defaults**: Provide sensible defaults for optional properties
3. **Type Safety**: Leverage the C# type system for validation
4. **Required Properties**: Mark essential properties as required
5. **Naming Convention**: Be aware of the YAML naming convention when writing front matter
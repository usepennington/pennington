---
title: "Table of Contents Generation"
description: "How MyLittleContentEngine builds hierarchical navigation from your content structure"
uid: "docs.under-the-hood.table-of-contents-generation"
order: 3020
---

The Table of Contents (TOC) system in MyLittleContentEngine automatically creates hierarchical navigation menus from your content pages. It analyzes your page URLs and front matter to build organized, nested navigation structures that reflect your site's content organization.

To retrieve the TOC entries, you can use the `GetNavigationTocAsync()` method from the `ITableOfContentService` interface.



## How TOC Generation Works

The TOC generation process transforms your flat collection of content files into a structured navigation tree. It handles complex scenarios like folder structures, index pages, and custom ordering to create intuitive navigation.

<Steps>
<Step stepNumber="1">
## Collecting Pages and Metadata

The system starts by gathering all pages from your content sources and extracting the information needed for navigation:

### Page Discovery

Pages come from multiple sources:

- **Markdown files** with front matter in your content directories
- **API documentation** automatically generated from your code
- **Custom content sources** you've configured

Each `IContentService` must implement `GetContentTocEntriesAsync()` to provide the necessary metadata for TOC generation.

### Required Information

For each page, the system needs four pieces of information:

- **Title**: The display name for navigation (from the `title` property in front matter)
- **URL**: The page's web address
- **Order**: A number for sorting (from the `order` property, defaults to a high number if not specified)
- **Hierarchy Parts**: An array of strings that defines the navigation structure

Pages without titles are automatically excluded from navigation menus.

</Step>
<Step stepNumber="2">

## Building the Hierarchy

The system organizes pages into a tree structure based on hierarchy parts provided by each content service.

### Hierarchy Parts Organization

Each content service provides hierarchy parts that determine the page's position in the navigation. For example:

- `["getting-started", "installation"]` becomes a child of the "Getting Started" section
- `["api", "classes", "ContentService"]` creates nested folders: API → Classes → ContentService

Content services have full control over their hierarchy structure and can customize it. For example, the `MarkdownContentService` uses the folder structure to generate the hierarchy parts. A service providing a product list might use product categories as hierarchy parts.

### Folder Structure Creation

The system automatically creates folder-like navigation entries for hierarchy parts that don't have corresponding pages. These folders help organize related content even when there's no explicit index page.

### Index Page Handling

Pages named `index` get special treatment—they represent both the folder and a navigable page. An index page like `/getting-started/index` becomes:

- A clickable navigation item with the folder's name
- A container that can hold child pages

</Step>
<Step stepNumber="3">

## Navigation Entry Types

The system creates different types of navigation entries based on your content structure:

Regular Pages
   : Standard content pages that appear as individual navigation items. They can have child pages if other content exists beneath them in the URL hierarchy.

Index Pages
   : Special pages that serve dual purposes—they're both clickable navigation items and containers for child pages. When you have an index page, it becomes the representative for its entire folder.

Folder Containers
   : When you have pages in a subfolder but no index page, the system creates a non-clickable folder entry that organizes the child pages.

Folder Absorption
   : If a folder contains an index page, the folder "absorbs" the index page's properties. The navigation shows the index page's title and link, but includes all the folder's other children as sub-items.

</Step>
<Step stepNumber="4">

## Automatic Naming

The system uses a priority-based approach to determine folder titles in the navigation:

### Title Priority System

When generating navigation entries, the system determines folder titles using this priority order:

Priority 1: Folder Metadata
   : If a `_index.metadata.yml` file exists in the folder with a `title` property, that title is used. This gives you complete control over folder display names.

Priority 2: Index Page Title
   : If the folder contains an `index.md` file with a title in its front matter, that title is used for both the page and the folder.

Priority 3: Auto-Generated Title
   : If neither of the above exists, the system automatically generates a readable title from the folder name.

### Auto-Generated Titles

When no explicit title is provided through metadata or index pages, hierarchy parts like `getting-started` are automatically converted to proper titles like "Getting Started". The system:

- Converts dashes to spaces
- Handles double dashes specially (preserves them as single dashes)
- Applies proper title case formatting

### Title Case Rules

The system uses APA title case:

- Always capitalizes the first word and important words
- Keeps articles, conjunctions, and short prepositions lowercase (unless they start a title)
- Capitalizes both parts of hyphenated words

Examples:

- `api-reference` → "API Reference"
- `under-the-hood` → "Under the Hood"
- `getting-started` → "Getting Started"
- `how--to` → "How-To" (double dash preserved as single dash)

</Step>
<Step stepNumber="5">

## Selection and Active States

The navigation system tracks which page you're currently viewing and highlights the appropriate navigation items:

The navigation entry matching your current URL is marked as selected and visually highlighted.


All parent folders and sections containing the current page are also marked as selected, creating a visual breadcrumb effect in the navigation.

</Step>
<Step stepNumber="6">

## Ordering and Sorting

Navigation items are sorted based on order values from your content:

### Page Order Control

Set explicit `order` values in your front matter to control navigation sequence:

```yaml
---
title: "Installation Guide"
order: 100
---
```

Pages without explicit order values appear after those with order values, sorted alphabetically.

### Folder Order Control

Folders can have their order controlled in two ways:

Explicit Folder Order
   : Create a `_index.metadata.yml` file in the folder with an `order` property to set the folder's position explicitly:

   ```yaml
   title: "Getting Started"
   order: 100
   ```

   This gives you precise control over where folders appear in the navigation, independent of their contents.

Inherited Order (default)
   : Without explicit folder metadata, folders inherit the order of their lowest-ordered child page.

This inheritance ensures logical grouping—a folder containing a page with `order: 100` will appear before a folder whose lowest child has `order: 200`.

</Step>
</Steps>

## Folder Metadata Configuration

You can customize folder behavior and appearance using `_index.metadata.yml` files. These files provide metadata for folders without requiring an index page.

### Creating Folder Metadata Files

Create a file named `_index.metadata.yml` in any content folder to customize that folder's properties:

```yaml
title: "Custom Folder Title"
order: 100
```

The file will be discovered automatically at startup and cached for performance.

### Available Properties

The following properties can be configured in folder metadata files:

title
   : Override the auto-generated folder name with a custom title. This takes priority over both auto-generated names and index page titles.

order
   : Explicitly set the folder's position in navigation. This overrides the default behavior of inheriting the lowest child order.

description
   : Provide a description for the folder (used in RSS feeds and sitemaps if applicable).

lastMod
   : Specify when the folder was last modified (used in sitemaps). Format: `2024-01-15` or full ISO 8601 datetime.

rssItem
   : Control whether the folder appears in RSS feeds (boolean, defaults to `true`).

section
   : Specify which table of contents section this folder belongs to.

### When to Use Folder Metadata

Use folder metadata files when you need to:

Customize folder titles without creating index pages
   : When you want a readable folder name but don't need a landing page.

   ```yaml
   title: "How-To Guides"
   order: 1000
   ```

Control folder ordering explicitly
   : When you need a folder to appear in a specific position regardless of its children's order values.

   ```yaml
   title: "Getting Started"
   order: 100
   ```

Handle special characters in folder names
   : When your folder uses URL-safe naming (like `how--to` for "How-To") but you want a clean display title.

   ```yaml
   title: "How-To"
   order: 2000
   ```

### Multi-Section Content

Folder metadata works seamlessly with multi-section content. The metadata files are discovered across all registered content sources and differentiated by their base URLs:

- `/console/how-to/_index.metadata.yml` → cache key: `console/how-to`
- `/cli/how-to/_index.metadata.yml` → cache key: `cli/how-to`

This allows different sections to have folders with the same name but different metadata.

## Practical Example

Consider this content structure using both front matter and folder metadata:

```
Content/
├── index.md (order: 1)
├── getting-started/
│   ├── _index.metadata.yml
│   ├── installation.md (order: 110)
│   └── first-steps.md (order: 120)
├── guides/
│   ├── index.md (order: 200)
│   ├── basic-usage.md (order: 210)
│   └── advanced-features.md (order: 220)
└── api/
    ├── _index.metadata.yml
    ├── classes/
    │   ├── _index.metadata.yml
    │   └── ContentService.md (order: 311)
    └── interfaces/
        ├── _index.metadata.yml
        └── IContentService.md (order: 321)
```

With folder metadata files:

**getting-started/_index.metadata.yml:**
```yaml
title: "Getting Started"
order: 100
```

**api/_index.metadata.yml:**
```yaml
title: "API Reference"
order: 300
description: "Complete API documentation for MyLittleContentEngine"
```

**api/classes/_index.metadata.yml:**
```yaml
title: "Classes"
order: 310
```

**api/interfaces/_index.metadata.yml:**
```yaml
title: "Interfaces"
order: 320
```

This generates navigation like:

1. **Home** (clickable, order: 1)
2. **Getting Started** (non-clickable folder, order: 100 from metadata)
    - **Installation** (clickable, order: 110)
    - **First Steps** (clickable, order: 120)
3. **User Guides** (clickable, order: 200 from guides/index.md)
    - **Basic Usage** (clickable, order: 210)
    - **Advanced Features** (clickable, order: 220)
4. **API Reference** (non-clickable folder, order: 300 from metadata)
    - **Classes** (non-clickable folder, order: 310 from metadata)
        - **ContentService** (clickable, order: 311)
    - **Interfaces** (non-clickable folder, order: 320 from metadata)
        - **IContentService** (clickable, order: 321)

Key observations:

- The **Getting Started** folder has a custom title and explicit order from `_index.metadata.yml`, but no index page (non-clickable)
- The **User Guides** folder has both an index page and children (clickable, with title from index.md front matter)
- The **API Reference** section uses folder metadata throughout to provide clean titles and explicit ordering
- Without folder metadata, "API Reference" would be titled "Api" and inherit order 311 from its lowest child

The system automatically handles the hierarchy, creates readable folder names, respects your custom ordering, and provides both tree-based and sequential navigation.
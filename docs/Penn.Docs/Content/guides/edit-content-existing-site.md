---
title: "Editing Content"
description: "Create and edit markdown content in your MyLittleContentEngine site with instant live reload"
uid: "docs.guides.edit-content-existing-site"
order: 2020
---

This guide covers the day-to-day workflow for creating and editing content in your site.

## Creating a New Page

Add a new Markdown file to your `Content` directory (or any subdirectory). The filename becomes part of the URL:

| File path | URL |
|-----------|-----|
| `Content/index.md` | `/` |
| `Content/about.md` | `/about` |
| `Content/guides/setup.md` | `/guides/setup` |

Every page needs front matter at the top with at least a `title`:

```markdown
---
title: "My New Page"
description: "A brief description for SEO and link previews"
---

# My New Page

Content goes here, written in standard Markdown.
```

The `title` field is required — it controls what appears in the browser tab, navigation sidebar, and any
RSS feeds. The `description` is used in social previews and search results.

## Organizing Content

Subdirectories become sections in your sidebar navigation automatically. A common structure:

```
Content/
├── index.md              ← home page
├── getting-started/
│   ├── index.md          ← section landing page
│   ├── installation.md
│   └── first-steps.md
└── guides/
    ├── index.md
    └── advanced.md
```

To control the display order within a section, add an `order` property to your front matter:

```yaml
---
title: "Installation"
order: 1
---
```

Pages without an explicit `order` sort alphabetically after any ordered pages.

## Drafts

Set `is_draft: true` to exclude a page from generation without deleting it:

```yaml
---
title: "Work in Progress"
is_draft: true
---
```

Draft pages are skipped during static generation but are accessible during development with `dotnet watch`.

## The Live Reload Workflow

Start the development server with file watching:

```bash
dotnet watch
```

Expected output:

```
dotnet watch 🔥 Hot reload enabled. For a list of supported edits, see https://aka.ms/dotnet/hot-reload.
dotnet watch 💡 Press Ctrl+R to restart.
Using launch settings from /projects/MyWebsite/Properties/launchSettings.json...
info: Microsoft.Hosting.Lifetime[14] Now listening on: http://localhost:5131
info: Microsoft.Hosting.Lifetime[0] Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0] Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0] Content root path: /projects/MyWebsite/
```

Open your site in a browser and position it side-by-side with your code editor. Make changes to any markdown file in
your `Content` directory, then save. The console will show:

```
dotnet watch ⌚ File updated: .\Content\guides\edit-content-existing-site.md
dotnet watch ⌚ No C# changes to apply.
```

The "No C# changes to apply" message indicates we don't have a code change, but our content will still be refreshed in
the browser.

## Hot Reload vs. Full Restart

Most content changes trigger **hot reload** (fast, browser auto-refreshes):

- Editing markdown content
- Modifying existing frontmatter values
- Updating images and static assets in the `Content` directory
- Changes to frontmatter that affect navigation structure
- Adding or removing content files
- Adjusting Razor pages

Some changes require a **full restart** (stop with Ctrl+C, then rerun `dotnet watch`):
 
- Modifying `Program.cs` 


## Related Topics

- [Markdown Extensions](xref:docs.guides.markdown-extensions) - Code tabs, alerts, Mermaid diagrams, and more
- [Linking Documents and Media](xref:docs.guides.linking-documents-and-media) - Cross-references, images, and static
  assets
- [Configure Custom Styling](xref:docs.guides.configure-custom-styling) - Customize appearance with MonorailCSS

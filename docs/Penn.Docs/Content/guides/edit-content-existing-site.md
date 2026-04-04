---
title: "Editing Content"
description: "Create and edit markdown content in your Penn site with hot reload that mostly works"
uid: "penn.guides.edit-content-existing-site"
order: 2020
---

This guide covers the day-to-day workflow for creating and editing content. It is, arguably, the only workflow that matters. Everything else is ceremony.

## Creating a New Page

Add a new Markdown file to your content directory. The filename becomes part of the URL, because Penn is not going to invent a routing scheme when the filesystem already has one:

| File path | URL |
|-----------|-----|
| `Content/index.md` | `/` |
| `Content/about.md` | `/about` |
| `Content/guides/setup.md` | `/guides/setup` |

Every page needs YAML front matter with at least a `title`. Penn's <xref:T:Penn.FrontMatter.IFrontMatter> interface requires exactly one property, and that's it:

```markdown
---
title: "My New Page"
---

# My New Page

Content goes here, written in standard Markdown.
```

If you're using <xref:T:Penn.FrontMatter.DocFrontMatter> (Penn's built-in front matter type), you also get `description`, `order`, `tags`, `uid`, `section`, and `is_draft` for free. It implements the capability interfaces so you don't have to think about it. Penn is opinionated about this. You are welcome to disagree, quietly.

## Organizing Content

Subdirectories become sections in your sidebar navigation automatically. Penn uses <xref:T:Penn.Routing.ContentRoute> to map files to URLs via `ContentRouteFactory.FromMarkdownFile`, which strips extensions and lowercases everything, because mixed-case URLs are a cry for help:

```
Content/
├── index.md              <- home page
├── getting-started/
│   ├── index.md          <- section landing page
│   ├── installation.md
│   └── first-steps.md
└── guides/
    ├── index.md
    └── advanced.md
```

To control display order within a section, implement <xref:T:Penn.FrontMatter.IOrderable> on your front matter type (or just use `DocFrontMatter`, which already does):

```yaml
---
title: "Installation"
order: 1
---
```

Pages without an explicit `order` sort alphabetically after any ordered pages. This is fine until you have a page named "aardvark" that insists on going first.

## Drafts

If your front matter type implements <xref:T:Penn.FrontMatter.IDraftable>, set `is_draft: true` to exclude a page from generation without deleting it:

```yaml
---
title: "Work in Progress"
is_draft: true
---
```

Draft pages are skipped during static generation but remain accessible during development with `dotnet watch`. Penn's `MarkdownContentService` checks for `IDraftable` when building the table of contents and quietly omits drafts. No judgment.

## The Live Reload Workflow

Start the development server with file watching:

```bash
dotnet watch
```

Expected output:

```
dotnet watch Hot reload enabled.
Using launch settings from /projects/MyWebsite/Properties/launchSettings.json...
info: Microsoft.Hosting.Lifetime[14] Now listening on: http://localhost:5131
info: Microsoft.Hosting.Lifetime[0] Application started. Press Ctrl+C to shut down.
```

Open your site in a browser alongside your editor. Edit any markdown file in your content directory, then save. The console will show:

```
dotnet watch File updated: .\Content\guides\edit-content-existing-site.md
dotnet watch No C# changes to apply.
```

The "No C# changes to apply" message means no code changed, but your content will still be refreshed in the browser.

## How File Watching Works

Under the hood, Penn registers `FileWatcher` (implementing `IFileWatcher`) during `AddPenn()`. The `FileWatcher` wraps `FileSystemWatcher` instances and monitors your content directories for `.md` file changes -- creates, updates, deletes, and renames.

When a file changes, `FileWatcher` notifies its subscribers. The `FileWatchDependencyFactory<T>` listens for these notifications and invalidates cached service instances, so the next request gets fresh content. This is the same pattern the `MarkdownContentService` uses to clear its cached metadata.

The flow:

1. You save a `.md` file
2. `FileWatcher` detects the change via `FileSystemWatcher`
3. `FileWatchDependencyFactory<T>.InvalidateInstance()` disposes the cached service and nulls it
4. Next HTTP request triggers `GetInstance()`, which creates a fresh instance with `ActivatorUtilities`
5. `dotnet watch` triggers a browser refresh

It is not sophisticated. It is sufficient.

## Hot Reload vs. Full Restart

Most content changes trigger **hot reload** (fast, browser auto-refreshes):

- Editing markdown content
- Modifying existing front matter values
- Updating images and static assets in the content directory
- Changes to front matter that affect navigation structure
- Adding or removing content files
- Adjusting Razor pages

Some changes require a **full restart** (stop with Ctrl+C, then rerun `dotnet watch`):

- Modifying `Program.cs` (including your `AddPenn()` configuration)

## Related Topics

- [Markdown Extensions](xref:penn.guides.markdown-extensions) -- Code tabs, alerts, and more
- [Linking Documents and Media](xref:penn.guides.linking-documents-and-media) -- Cross-references, images, and static assets
- [Configure Custom Styling](xref:penn.guides.configure-custom-styling) -- Customize appearance with MonorailCSS

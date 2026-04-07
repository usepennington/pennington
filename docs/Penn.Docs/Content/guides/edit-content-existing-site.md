---
title: "Editing Content"
description: "Day-to-day workflow for creating pages, organizing content, using drafts, and live reload"
uid: "penn.guides.edit-content-existing-site"
order: 2020
---

This guide covers the day-to-day workflow for creating pages, organizing them into sections, using drafts, and getting live feedback as you write.

## Creating a New Page

Add a markdown file to your content directory. The only required front matter property is `title`, defined by the `IFrontMatter` interface:

```markdown
---
title: "My New Page"
---

Your content goes here.
```

That is a complete, valid Penn page. It will appear in navigation, get a URL derived from its filename, and render through the markdown pipeline. Penn discovers all `.md` files recursively under your content directory, so you can place the file anywhere in the tree.

If your site uses `DocFrontMatter` (the default for doc sites), you also have access to `description`, `order`, `tags`, `uid`, `section`, and `isDraft`. These come from the capability interfaces that `DocFrontMatter` implements (`IOrderable`, `IDraftable`, `ICrossReferenceable`, `ISectionable`, `IDescribable`, `ITaggable`). See <xref:penn.reference.front-matter-properties> for the full list of available properties.

```yaml
---
title: "Installation"
description: "How to install Penn"
uid: "penn.guides.installation"
order: 10
tags: ["setup", "getting-started"]
---
```

## File-to-URL Mapping

Penn derives URLs from file paths using `ContentRouteFactory.FromMarkdownFile`. The rules are:

1. Strip the file extension.
2. Lowercase the entire path.
3. Replace backslashes with forward slashes.
4. Append a trailing slash.
5. Treat `index.md` as the directory root.

Given a content root of `Content/` and a base URL of `/docs`:

| File path | URL |
|-----------|-----|
| `Content/index.md` | `/docs/` |
| `Content/getting-started.md` | `/docs/getting-started/` |
| `Content/guides/setup.md` | `/docs/guides/setup/` |
| `Content/guides/index.md` | `/docs/guides/` |
| `Content/API-Reference.md` | `/docs/api-reference/` |

The output file for each page follows the same structure. `/docs/getting-started/` becomes `docs/getting-started/index.html` on disk. This is how clean URLs work with static file hosting -- every page is an `index.html` inside a directory named after the page.

Note that the lowercasing is unconditional. A file named `API-Reference.md` always becomes `/docs/api-reference/`, regardless of platform. Penn normalizes all URL segments through `ToLowerInvariant()` to ensure consistent URLs across case-sensitive and case-insensitive file systems.

## Organizing Content in Directories

Subdirectories become sections in your site's table of contents. Create an `index.md` in each directory to give the section a landing page:

```
Content/
  index.md
  getting-started/
    index.md
    installation.md
    first-steps.md
  guides/
    index.md
    configuration.md
    advanced.md
```

Use the `order` front matter property to control how pages sort within a section:

```yaml
---
title: "Installation"
order: 10
---
```

```yaml
---
title: "First Steps"
order: 20
---
```

Pages without an `order` value default to `int.MaxValue` and sort after all explicitly ordered pages. Among unordered pages, the sort order depends on file system discovery order. Use a consistent numbering scheme (10, 20, 30 or 100, 200, 300) to leave room for inserting pages later without renumbering everything.

The `section` property lets you override which navigation group a page belongs to. This is useful when you need a page to appear in a different section of the sidebar than its directory would imply. If you omit `section`, the page inherits the section configured on its `MarkdownContentServiceOptions`.

## Working with Drafts

Mark a page as a draft by setting `isDraft: true` in the front matter:

```yaml
---
title: "Work in Progress"
isDraft: true
---
```

> [!IMPORTANT]
> Penn's YAML parser uses **camelCase** naming. The property is `isDraft`, not `is_draft`. Similarly, other properties use camelCase: `redirectUrl`, not `redirect_url`. This applies to all front matter properties.

Draft pages are excluded from:

- The table of contents and sidebar navigation
- Search indexes
- Static site generation output

During development, draft pages are still accessible by navigating directly to their URL. `MarkdownContentService` only filters drafts from the TOC entries (via the `IDraftable` check in `GetContentTocEntriesAsync`), not from route discovery. This means you can preview a draft page while you write it.

Your front matter type must implement `IDraftable` for draft filtering to work. Both `DocFrontMatter` and `BlogFrontMatter` include this capability by default. If you define a custom front matter record, add `IDraftable` to the interface list and include a `bool IsDraft` property to opt in to draft support.

## The Live Reload Workflow

Start the development server with file watching:

```bash
dotnet watch --project path/to/YourSite
```

Expected output:

```
dotnet watch  Hot reload enabled.
info: Penn.Infrastructure.FileWatcher[0]
      Adding file watch: /path/to/Content with pattern *.*
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5131
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Open the site in your browser. Edit a markdown file and save. The page reloads automatically.

Penn injects a WebSocket client that connects to `/__penn/reload`. When `FileWatcher` detects a change, `LiveReloadServer` sends a reload message to every connected browser tab. The reload path is only active when the `DOTNET_WATCH` environment variable is set, so it has zero impact on production.

This workflow works well for content authoring: edit markdown, save, see the result. There is no manual refresh step and no build command to run between edits.

For information about the markdown syntax extensions available while writing content (code tabs, alerts, and more), see <xref:penn.guides.markdown-extensions>. For linking between pages, see <xref:penn.guides.linking-documents-and-media>.

## How File Watching Works

Penn's content refresh pipeline has three components:

**FileWatcher** wraps `FileSystemWatcher` instances. When `MarkdownContentService` is created, it calls `fileWatcher.AddPathWatch` to register its content directory for monitoring. The watcher listens for all four change types: creates, modifications, deletes, and renames. It watches all subdirectories by default, matching all files (`*.*`).

**FileWatchDependencyFactory&lt;T&gt;** is a singleton that manages a cached service instance. It subscribes to `FileWatcher` via `SubscribeToChanges`. When any watched file changes, `InvalidateInstance` acquires a lock, disposes the current cached instance if it implements `IDisposable`, and sets the reference to null.

**On the next HTTP request**, the DI container calls `GetInstance` on the factory. Finding no cached instance, the factory creates a fresh one using `ActivatorUtilities.CreateInstance`. The new `MarkdownContentService` instance rediscovers all content files from disk and rebuilds its metadata, picking up your changes.

The sequence:

1. You save a markdown file.
2. `FileWatcher` detects the change and calls `NotifySubscribers`.
3. `FileWatchDependencyFactory<T>.InvalidateInstance()` disposes and nulls the cached service.
4. `LiveReloadServer.NotifyClients()` sends `"reload"` over WebSocket to all connected browsers.
5. The browser reloads. The incoming request calls `GetInstance()`, which creates a new service instance.
6. The new instance reads the updated content from disk.

This means Penn never polls for changes and never holds stale content after a file modification. The same `FileWatcher` instance also drives invalidation for other cached services like `SearchIndexService` and `SitemapService`, so search results and sitemaps stay current during development too.

## Hot Reload vs. Full Restart

Changes that **auto-refresh** (no restart needed):

- Editing markdown content or front matter values
- Adding or deleting content files
- Renaming content files
- Updating images and static files in content directories
- Modifying Razor component markup (via `dotnet watch` hot reload)

Changes that **require a full restart** (stop with Ctrl+C, then rerun `dotnet watch`):

- Modifying `Program.cs` or service registration code (the `AddDocSite` / `AddPenn` calls)
- Adding new content source registrations
- Changing configuration that is read once at startup (site title, base URL, content root paths)
- Adding or updating NuGet package references

The distinction is straightforward: if the change affects files that `FileWatcher` monitors (content, static assets, Razor markup), it auto-refreshes. If the change affects code that runs at startup or alters the DI container, you need a restart.

> [!TIP]
> When in doubt, save and check the browser. If the change did not appear, stop the server with Ctrl+C and rerun `dotnet watch`. The restart takes a few seconds.

For a deeper look at how the invalidation pipeline is wired together, see <xref:penn.under-the-hood.hot-reload-architecture>.

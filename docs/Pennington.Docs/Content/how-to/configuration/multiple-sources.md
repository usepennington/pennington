---
title: "Use multiple content sources"
description: "Chain PenningtonOptions.AddMarkdownContent<TFrontMatter> calls with different ContentPath/BasePageUrl/Section/ExcludePaths, and how overlap detection warns on misconfiguration."
section: "configuration"
order: 30
tags: []
uid: how-to.configuration.multiple-sources
isDraft: true
search: false
llms: false
---

> **In this page.** Chaining `PenningtonOptions.AddMarkdownContent<TFrontMatter>` calls with different `ContentPath`/`BasePageUrl`/`Section`/`ExcludePaths`, and how overlap detection warns on misconfiguration.
>
> **Not in this page.** Implementing a non-markdown `IContentService` — see the extensibility section.

## When to use this

- You have a working Pennington site with one `AddMarkdownContent<T>` registration and need to split content into more than one folder (e.g. root pages, `/blog`, `/docs`) that each carry their own metadata shape.
- You want each folder to mount at its own `BasePageUrl`, group under its own `Section` in navigation, and parse with its own front-matter record.

## Assumptions

- An existing Pennington site with one `AddMarkdownContent<T>(...)` registration already wired inside `AddPennington(...)`.
- You already know how to define an `IFrontMatter` record (see [_Work with front matter_](/how-to/content-authoring/front-matter) if not).
- You understand that each registered source runs through the same pipeline — discovery, parse, render, generate — with only its `MarkdownContentOptions` differing.

To copy a working setup, see [`examples/MultipleContentSourceExample`](https://github.com/phil-scott-78/Pennington/tree/main/examples/MultipleContentSourceExample). It wires three parallel sources (root pages, `/blog`, `/docs`) with three front-matter records. Do not walk the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Define one front-matter record per source

- Give each source a record implementing `IFrontMatter` plus whichever capability interfaces that folder actually needs (`ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable`).
- Records do not need to agree on their capability set — each source parses with its own `TFrontMatter`, so a docs record can be `IOrderable` while a blog record is not.
- Records in the example:

```csharp:xmldocid
T:MultipleContentSourceExample.ContentFrontMatter
```

```csharp:xmldocid
T:MultipleContentSourceExample.BlogFrontMatter
```

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

### 2. Register one `AddMarkdownContent<T>` call per folder

- Inside `AddPennington(penn => { ... })`, call `penn.AddMarkdownContent<TFrontMatter>(md => { ... })` once per folder. Each call appends a new `MarkdownContentOptions` to `PenningtonOptions.MarkdownSources`.
- Set `ContentPath` to the folder on disk (relative to the project), `BasePageUrl` to the URL prefix the folder should mount at, and `Section` to the navigation grouping key.
- `BasePageUrl` defaults to `"/"`; an empty string `""` also mounts at the site root. `Section` is optional and is read by `NavigationBuilder` as the grouping key.

```csharp
// Program.cs — three parallel markdown sources, verbatim from
// examples/MultipleContentSourceExample/Program.cs
builder.Services.AddPennington(penn =>
{
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<ContentFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
        md.ExcludePaths = ["blog", "docs"];
    });

    penn.AddMarkdownContent<BlogFrontMatter>(md =>
    {
        md.ContentPath = "Content/blog";
        md.BasePageUrl = "/blog";
        md.Section = "blog";
    });

    penn.AddMarkdownContent<DocsFrontMatter>(md =>
    {
        md.ContentPath = "Content/docs";
        md.BasePageUrl = "/docs";
        md.Section = "docs";
    });
});
```

### 3. Carve overlapping subtrees out of the catch-all with `ExcludePaths`

- If one source's `ContentPath` is a parent of another's (e.g. root source at `Content` plus a docs source at `Content/docs`), the outer source must opt out of the inner's subtree via `ExcludePaths`.
- `ExcludePaths` is an `ImmutableArray<string>` of subpaths relative to `ContentPath`; collection-expression literals (`["blog", "docs"]`) assign directly.
- Matching is segment-based (see `MarkdownSourceOverlapDetector`), so `"docs"` excludes both `docs/` and `docs/sub/`, but not `docs-draft/`.
- In the example above, the root source sets `ExcludePaths = ["blog", "docs"]` so the blog and docs sources own those subtrees outright.

### 4. Consume all registered sources with `IEnumerable<IContentService>`

- Every registered markdown source becomes an `IContentService` in the container. Inject `IEnumerable<IContentService>` to iterate them, not a single service.
- The example's generic helper parameterizes each page by its `IFrontMatter` type so the caller picks which record to deserialize into.

```csharp:xmldocid
T:MultipleContentSourceExample.ContentHelper
```

### 5. Scope navigation to a single source by its `Section`

- `NavigationBuilder.BuildTree` takes a flat list of `ContentTocItem`; filter `IContentService` instances by `DefaultSection` to build a per-section tree instead of one mega-tree.
- The example helper does this when callers pass a section name (`"blog"`, `"docs"`):

```csharp:xmldocid
M:MultipleContentSourceExample.ContentHelper.GetNavigationAsync(System.String,System.String)
```

---

## Verify

- Run `dotnet run` and confirm each base URL renders: the root page, `/blog/...`, `/docs/...`.
- Run `dotnet run -- build` and confirm no `Markdown content source rooted at ... overlaps a more specific source rooted at ...` warning appears in the build report. If it does, add the reported subpath to the outer source's `ExcludePaths`.
- Confirm a markdown file lives in exactly one source's output — search the generated `output/` tree for its slug and expect a single file.

## Related

- Reference: [`MarkdownContentOptions<T>`](/reference/options/markdown-content-options)
- Background: [How the content pipeline processes sources](/explanation/content-pipeline)

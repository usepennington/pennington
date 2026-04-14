---
title: "Work with front matter"
description: "Declare YAML front matter on a markdown page, pick a built-in front-matter record, and define your own."
section: "content-authoring"
order: 10
tags: []
uid: how-to.content-authoring.front-matter
isDraft: true
search: false
llms: false
---

> **In this page.** Declaring front matter in YAML, picking a built-in front-matter record that fits, and defining your own.
>
> **Not in this page.** The full key catalog — see [front matter key reference](/reference/front-matter/keys). The capability-interface architecture belongs in [Explanation](/explanation/core/front-matter-capabilities).

## When to use this

When you have a markdown page and need to attach metadata (title, description, tags, etc.) to it, and you want that metadata typed on the C# side so Razor layouts, navigation, and the search index can read it.

## Assumptions

- A working Pennington site with at least one `AddMarkdownContent<T>(...)` registration.
- The content folder wired to that registration already exists under `Content/` (or the `ContentPath` you set).
- You are comfortable editing a C# record and restarting the host.

To copy a working setup, see [`examples/MultipleContentSourceExample`](https://github.com/usepennington/pennington/tree/main/examples/MultipleContentSourceExample). It ships three front-matter records that implement different capability combinations.

---

## Steps

### 1. Add a YAML block to the top of the markdown file

Put a `---` fenced YAML block at the very top of the `.md` file. Everything after the closing `---` is the page body.

```yaml
---
title: "The Great Pizza Topping Debate"
description: "Exploring the most controversial food debate of our time"
date: 2025-01-15
tags:
  - food
  - opinion
isDraft: false
---
```

Real example the block was copied from: `examples/MultipleContentSourceExample/Content/blog/best-pizza-toppings.md`.

### 2. Fill in the keys your page needs

YAML keys are `camelCase`. Every page can set `title`, `description`, `tags`, `section`, `order`, `isDraft`, `uid`, `date`, `search`, and `llms` — but not every front-matter record exposes every key, because `ITaggable`, `ISectionable`, `IOrderable`, and `IRedirectable` are opt-in capabilities.

For the full key list, types, and defaults, see [front matter key reference](/reference/front-matter/keys).

### 3. Use the framework records if the baseline fits

If you only need the standard keys, register one of the framework records instead of writing your own. `DocFrontMatter` covers `IFrontMatter + ITaggable + ISectionable + IOrderable`; `BlogSiteFrontMatter` covers `IFrontMatter + ITaggable` and adds `Date`, `Author`, `Series`.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.AddMarkdownContent<DocFrontMatter>(o => { o.ContentPath = "Content"; });
});
```

Skip to step 5 if a framework record is enough.

### 4. Define your own record when you need extra fields

Declare a record that implements `IFrontMatter` and any capability interfaces you want. Start minimal — add only the properties your pages actually set.

```csharp:xmldocid
T:ForgePortalExample.PageFrontMatter
```

To opt into tags, ordering, redirects, or sections, implement the capability interface and add its property. Keep property names matching the YAML keys.

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

### 5. Register the record on a content source

Wire the record up with `AddMarkdownContent<TFrontMatter>(...)` so the matching content service uses it when parsing your folder. Each markdown folder gets one record type; multiple folders may register different records (see `examples/MultipleContentSourceExample/Program.cs` for three parallel registrations).

```csharp
builder.Services.AddPennington(penn =>
{
    penn.AddMarkdownContent<DocsFrontMatter>(o =>
    {
        o.ContentPath = "Content/docs";
        o.BasePageUrl = "/docs";
        o.Section = "Documentation";
    });
});
```

---

## Verify

- Run `dotnet run` and open a page that uses your record.
- Set `isDraft: true` on a page and rebuild; it should disappear from the generated output.
- Set a `uid:` and link to it elsewhere as `[Link](xref:your-uid)`; the link should resolve to the canonical URL.

## Related

- Reference: [Front matter keys](/reference/front-matter/keys)
- Background: [The front-matter capability model](/explanation/core/front-matter-capabilities)

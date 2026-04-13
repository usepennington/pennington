---
title: "Work with front matter"
description: "Declare YAML front matter on a markdown page, use the baseline IFrontMatter keys, and define your own front-matter record."
section: "content-authoring"
order: 10
tags: []
uid: how-to.content-authoring.front-matter
isDraft: true
search: false
llms: false
---

> **In this page.** Declaring front matter in YAML, the baseline `IFrontMatter` keys (`title`, `description`, `tags`, `section`, `order`, `isDraft`, `uid`, `date`, `search`, `llms`), and defining your own front-matter record.
>
> **Not in this page.** The full key reference (see Reference) or the capability-interface architecture (see Explanation).

## When to use this

- You have a markdown page and need to attach metadata (title, description, tags, etc.) to it.
- You want the metadata typed on the C# side so your Razor layouts, navigation, and search index can read it.
- You are extending an existing Pennington site — not starting from zero.

## Assumptions

- A working Pennington site with at least one `AddMarkdownContent<T>(...)` registration.
- The content folder wired to that registration already exists under `Content/` (or the `ContentPath` you set).
- You are comfortable editing a C# record and restarting the host.
- Not required: any knowledge of the capability-interface architecture — link out to Explanation if you want that.

To copy a working setup, see [`examples/MultipleContentSourceExample`](https://github.com/phil-scott-78/Pennington/tree/main/examples/MultipleContentSourceExample). It ships three front-matter records that implement different capability combinations. Do not walk the whole example — this page is a recipe.

---

## Steps

### 1. Add a YAML block to the top of the markdown file

Put a `---` fenced YAML block at the very top of the `.md` file. `FrontMatterParser` (wrapping `YamlDotNet` with `CamelCaseNamingConvention`) reads this block and returns the remaining text as the page body.

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

- Real example the block was copied from: `examples/MultipleContentSourceExample/Content/blog/best-pizza-toppings.md` — a working markdown page with tags, description, and date.

### 2. Fill in the baseline `IFrontMatter` keys

Every page can set these ten keys; they map to properties on `IFrontMatter` (and three capability interfaces) without writing any C# yet if you use the framework-provided `DocFrontMatter` or `BlogFrontMatter`.

| Key | Type | Drives |
|---|---|---|
| `title` | string (required) | `IFrontMatter.Title` — page `<title>`, nav label |
| `description` | string | `IFrontMatter.Description` — `<meta>`, search index, llms.txt |
| `tags` | string list | `ITaggable.Tags` — tag pages, blog archive |
| `section` | string | `ISectionable.Section` — navigation grouping |
| `order` | int | `IOrderable.Order` — navigation sort (lower = earlier) |
| `isDraft` | bool | `IFrontMatter.IsDraft` — skipped by `ContentPipeline.GenerateAsync` |
| `uid` | string | `IFrontMatter.Uid` — target for `<xref:uid>` links (`XrefResolver`) |
| `date` | date | `IFrontMatter.Date` — blog post date, RSS pubDate |
| `search` | bool (default `true`) | `IFrontMatter.Search` — include in per-locale `search-index-{code}.json` |
| `llms` | bool (default `true`) | `IFrontMatter.Llms` — include in `llms.txt` |

- YAML keys are `camelCase` because `FrontMatterParser` uses `CamelCaseNamingConvention`.
- `search` and `llms` default to `true` via `IFrontMatter` default interface members; only set them to opt out.
- Not every record exposes every key — `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable` are opt-in capabilities.

### 3. Use the framework records if the baseline fits

If you only need the keys above, register one of the framework records instead of writing your own. `DocFrontMatter` covers `IFrontMatter + ITaggable + ISectionable + IOrderable`; `BlogFrontMatter` covers `IFrontMatter + ITaggable` and adds `Date`, `Author`, `Series`.

```yaml
# in Program.cs (conceptual — pass to PenningtonOptions.AddMarkdownContent<T>):
# penn.AddMarkdownContent<DocFrontMatter>(o => { o.ContentPath = "Content"; });
```

- Skip ahead to step 5 if `DocFrontMatter` or `BlogFrontMatter` is enough.

### 4. Define your own record when you need extra fields

Declare a record that implements `IFrontMatter` and any capability interfaces you want. Start minimal — add only the properties your pages actually set.

```csharp:xmldocid
T:ForgePortalExample.PageFrontMatter
```

- From `examples/ForgePortalExample/PageFrontMatter.cs`: the smallest possible record — a single `Title` property. Every other `IFrontMatter` member comes from the default interface implementation.

To opt into tags, ordering, redirects, or sections, implement the capability interface and add its property. Keep property names matching the YAML keys.

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

- From `examples/MultipleContentSourceExample/DocsFrontMatter.cs`: a richer record implementing `ITaggable`, `IOrderable`, `IRedirectable`, and explicitly declaring `Description`, `IsDraft`, and `Uid` on top of the defaults.

### 5. Register the record on a content source

Wire the record up with `PenningtonOptions.AddMarkdownContent<TFrontMatter>(...)` so `MarkdownContentService<TFrontMatter>` uses it when parsing your folder.

```yaml
# conceptual Program.cs snippet:
# penn.AddMarkdownContent<DocsFrontMatter>(o => {
#     o.ContentPath = "Content/docs";
#     o.BasePageUrl = "/docs";
#     o.Section = "Documentation";
# });
```

- Each markdown folder gets one record type — the generic argument on `AddMarkdownContent<T>` is what `FrontMatterParser.Parse<T>(...)` deserializes into.
- Multiple folders may register different records; see `examples/MultipleContentSourceExample/Program.cs` for three parallel registrations.

---

## Verify

- Run `dotnet run` and open a page that uses your record.
- Set `isDraft: true` on a page and rebuild; it should disappear from the generated output (`ContentPipeline.GenerateAsync` skips drafts).
- Set a `uid:` and link to it elsewhere as `[Link](xref:your-uid)`; the link should resolve via `XrefResolver`.

## Related

- Reference: [Front matter keys](/reference/front-matter/keys)
- Background: [The front-matter capability model](/explanation/content/front-matter-capabilities)

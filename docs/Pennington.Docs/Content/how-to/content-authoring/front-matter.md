---
title: "Work with front matter"
description: "Declare YAML front matter, pick a built-in record, or define your own for custom keys."
uid: how-to.content-authoring.front-matter
order: 201010
sectionLabel: Content Authoring
tags: [front-matter, authoring, yaml]
---

> **In this page.** _Paraphrase the TOC "Covers" line: declaring front matter in YAML, picking a built-in front-matter record that fits, and defining your own._
>
> **Not in this page.** _Paraphrase "Does not cover": the full catalog of built-in keys lives at [Front matter key reference](xref:reference.front-matter.keys); the capability-interface design is in [The front-matter capability system](xref:explanation.core.front-matter-capabilities)._

## When to use this

_Two sentences. Frame the goal: the reader has a markdown page and wants the right YAML block at the top, or they want to add a key the built-in records don't have. Do not teach what front matter is — link to reference/explanation for that._

## Assumptions

_Keep the list to three bullets. Each should be realistic prior state, not a tutorial step._

- You have an existing Pennington site with markdown content under a `Content/` folder (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- You know which host template you are on — `AddDocSite`, `AddBlogSite`, or bare `AddPennington` with `AddMarkdownContent<T>`.
- You have a markdown file open and want to fill in (or extend) its YAML block.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/front-matter.md` is the page this how-to is fenced from, and `ApiFrontMatter.cs` is the custom-record demo.

---

## Steps

_Five steps. Each is one imperative action. Keep prose to one sentence before each fence._

### 1. Declare the YAML block at the top of the file

_One sentence: open the markdown file, and put the YAML between two `---` fences as the very first thing in the file — before the `# Heading`. Fence the fixture page so the reader sees the exact shape._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

### 2. Pick the built-in record that matches your host

_One sentence: the record is chosen by the host — `AddDocSite` binds `DocSiteFrontMatter`, `AddBlogSite` binds `BlogSiteFrontMatter`, and bare `AddPennington` lets you pass either `DocFrontMatter` or `BlogFrontMatter` (or your own) to `AddMarkdownContent<T>`. Show the `DocFrontMatter` surface as the baseline doc-shaped record._

```csharp:xmldocid
T:Pennington.FrontMatter.DocFrontMatter
```

_Then show the blog-shaped counterpart for posts._

```csharp:xmldocid
T:Pennington.FrontMatter.BlogFrontMatter
```

### 3. Fill in only the keys you need

_One sentence: every key on the built-in records has a sensible default, so the YAML block can be as small as `title:` plus whatever the page actually needs — tags, order, description, uid. Point at the `DocSiteFrontMatter` symbol for the DocSite template superset._

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

### 4. Define your own record when you need extra keys

_Two sentences: declare a `public record` implementing `IFrontMatter` (and any capability interfaces you want — `ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`). Link out to the capability-defaults reference for the full list of optional interfaces._

```csharp:xmldocid
T:DocSiteKitchenSinkExample.ApiFrontMatter
```

### 5. Register the custom record with a markdown source

_One sentence: pass the record type to `AddMarkdownContent<T>` so the pipeline knows how to deserialize the YAML. Note inline that `AddDocSite` / `AddBlogSite` already register one source each — chaining a second record requires bare `AddPennington` (link to [Use multiple content sources](xref:how-to.configuration.multiple-sources))._

```yaml
---
title: Symbol reference
namespace: Pennington.Search
stability: preview
order: 30
---
```

---

## Verify

_Three bullets. Each is one observable check._

- Run `dotnet run` and visit the page — the rendered `<h1>` matches the `title:` value.
- The sidebar entry appears with the label from `title:` at the position set by `order:`.
- If you added a custom record, pages under its content source still build without `FrontMatterParseError` diagnostics in the build report.

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys) — every built-in key, type, and default
- Reference: [Built-in front-matter types](xref:reference.front-matter.built-in-types) — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`
- Reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter) — the capability interfaces you can add to a custom record
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities) — why the design collapsed ten interfaces into default members

# Shared Bramble corpus

A fixed, fictional documentation corpus that any example can mount instead of
shipping its own content. Use it when the thing under test is **volume** — nav at
scale, heading-level search, TOC/outline density, sitemap/RSS size, llms.txt
breadth — rather than a specific authored page.

Everything here describes **Bramble**, an invented scripting language and
toolchain. It reads like a real developer-docs site but is entirely made up. Do
not cite any of it as fact, and do not reference real companies/products as if
they were involved.

## Layout

```
Content/
  index.md            site landing
  tutorials/          learning path        (area)
  how-to/             task guides          (area; language/ thicket/ trellis/ sprig/ tooling/)
  reference/          lookup               (area; language/ stdlib/ cli/ config/ api/)
  explanation/        background essays    (area)
  blog/               dated posts          (auto-activates the DocSite blog)
```

`how-to/` and `reference/` use subfolders so nested-section navigation gets
exercised. The four Diátaxis folders are top-level areas; `blog/` is not an area —
dropping it under a DocSite content root lights up the blog automatically.

## Front matter

Docs (`tutorials/`, `how-to/`, `reference/`, `explanation/`) use the key set
common to both `DocFrontMatter` and `DocSiteFrontMatter`:

```yaml
---
title: "Pattern match on records"
description: "One-sentence summary used in nav, search, and feeds."
uid: bramble.how-to.language.pattern-match-on-records
order: 130
sectionLabel: "Language"
tags: [how-to, language, pattern-matching]
---
```

Blog (`blog/`) uses the subset common to both the DocSite `BlogPostFrontMatter`
and the BlogSite `BlogSiteFrontMatter`, so the same files work under either host:

```yaml
---
title: "Why Bramble has no null"
description: "..."
date: 2023-09-01
author: Maple Okafor
tags: [design, language]
uid: bramble.blog.why-bramble-has-no-null
---
```

Series, repository, and section labels are intentionally omitted from blog posts:
the DocSite blog front matter does not parse them, and unknown keys throw in build
mode. `redirectUrl` is omitted from docs for the same reason (it is not on
`DocFrontMatter`).

Cross-links use `uid` references — `[text](xref:bramble.reference.stdlib.json)`
or `<xref:bramble.reference.stdlib.json>` — never hardcoded URLs. The `uid`
scheme is `bramble.<folders>.<slug>` (for example
`bramble.tutorials.your-first-program`).

## Mounting it

Paths are relative to the consuming project at runtime, so they resolve against
`examples/<YourExample>/`.

### As a DocSite (docs + blog)

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Bramble",
    ContentRootPath = "../_shared/Bramble/Content",
    CanonicalBaseUrl = "https://bramble.example.com",
    Areas =
    [
        new ContentArea("Tutorials", "tutorials"),
        new ContentArea("How-to", "how-to"),
        new ContentArea("Reference", "reference"),
        new ContentArea("Explanation", "explanation"),
    ],
});
```

### As a bare host or BlogSite (one source per folder)

Point a markdown source at whichever subtree you need:

```csharp
md.ContentPath = "../_shared/Bramble/Content/reference"; // docs
md.ContentPath = "../_shared/Bramble/Content/blog";      // posts
```

### Live reload

The dev file-watcher only watches the consuming project by default. To pick up
edits to the shared files, add this to the example's `.csproj`:

```xml
<ItemGroup>
  <Watch Include="..\_shared\Bramble\Content\**\*.*" />
</ItemGroup>
```

## Used by

- `DocSiteSharedCorpusExample` — mounts the whole corpus as a four-area DocSite with the blog.

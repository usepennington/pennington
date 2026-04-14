---
title: Authoring a doc page
description: Populate DocSiteFrontMatter, add an alert, and group code samples into tabs.
tags:
  - authoring
  - front-matter
  - markdown
sectionLabel: Guides
order: 20
---

# Authoring a doc page

This page demonstrates every authoring surface a typical DocSite page uses:
a fully-populated `DocSiteFrontMatter` block, a GitHub-style alert, and a
tabbed code group. The outline on the right is populated automatically from
the `##` and `###` headings on this page.

## Front matter

Every doc page starts with a YAML front-matter block. `DocSiteFrontMatter`
maps each key to a strongly-typed property: `title` appears in the page
`<h1>` and in the browser tab, `description` fills the meta description,
`tags` feed tag indexes, `section` groups the page in the sidebar, and
`order` decides where the page sits among its siblings.

### Required keys

Only `title` is strictly required. Everything else is optional, but a
populated `description` improves search results and social previews, and a
`section` + `order` pair keeps the sidebar predictable.

### Optional keys

`tags`, `section`, `order`, and the capability-interface extras
(`uid`, `search`, `llms`, `redirectUrl`, `isDraft`) are all optional. Leave
them off when they would add noise; add them when you need the behaviour.

## Callouts

Use a GitHub-style alert to pull a note, tip, or warning out of the prose.
The syntax is a plain block quote whose first line is `[!KIND]`:

> [!NOTE]
> Alerts render with a coloured left border and an icon matching the kind.
> Supported kinds include `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and
> `CAUTION`.

## Tabbed code groups

Mark two or more adjacent fenced code blocks with `tabs=true` and give each a
`title` so readers can flip between variants (languages, platforms, or
package managers) in place:

```bash tabs=true title="dotnet CLI"
dotnet add package Pennington
```

```powershell tabs=true title="PowerShell"
Install-Package Pennington
```

```xml tabs=true title="csproj"
<PackageReference Include="Pennington" Version="*" />
```

### How it renders

Pennington turns the grouped blocks into an ARIA tablist. Only the first
panel is visible at a time; clicking a tab swaps panels without a page
navigation.

### When to reach for tabs

Tabs are best when the three variants convey the same *information* in
different forms. If the content is meaningfully different, prefer separate
sections.

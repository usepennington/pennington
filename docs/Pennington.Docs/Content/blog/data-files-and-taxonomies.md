---
title: Data files and taxonomies
description: Two new building blocks — hot-reloading YAML/JSON datasets, and browse-by-field pages generated from any front-matter key.
author: Phil Scott
date: 2026-05-14
isDraft: false
tags:
  - data-files
  - taxonomies
---

Most of a content site is pages — a markdown file becomes a URL. But every site
has parts that aren't a routed page: a sponsor strip, a conference schedule, a
"recipes by cuisine" index. This release adds two building blocks for those
cases.

## Data files: structured content without a page

Some content isn't a page. A list of sponsors, a navigation config, a table of
release dates — that's data, and putting it in a markdown file or hard-coding it
into a component is the wrong fit.

`AddDataFile<T>` registers a YAML or JSON file as a typed dataset:

```csharp
builder.Services.AddDataFile<List<Sponsor>>("sponsors", "data/sponsors.yml");
```

A Razor page reads it back through `IDataFiles`:

```razor
@inject IDataFiles Data

@foreach (var sponsor in Data.Get<List<Sponsor>>("sponsors"))
{
    <SponsorCard Sponsor="sponsor" />
}
```

The file deserializes on first access and invalidates its cache when it changes
on disk, so editing `sponsors.yml` shows up on the next request with no restart.
The [data file how-to](xref:how-to.content-services.data-files) has the full
setup.

## Taxonomies: browse-by-anything

A taxonomy slices your content by a field: recipes by cuisine, docs by audience,
posts by series. Each of those browse axes used to mean a hand-written
`IContentService`.

`AddTaxonomy<TFrontMatter, TKey>` replaces that. Point it at a front-matter field
and it walks every registered content service, collects the values, and emits an
index page plus one page per term:

```csharp
builder.Services.AddTaxonomy<RecipeFrontMatter, string>(opts =>
{
    opts.SelectKey = fm => fm.Cuisine;
});
```

The snippet is abbreviated: a real registration also sets `BaseUrl` and the `IndexPage` / `TermPage` Razor components, which the how-to below walks in full.

Multiple axes against the same content coexist — a single-valued `/topic` and a
multi-valued `/tag` can both run off the same front matter. The [taxonomy
how-to](xref:how-to.content-services.taxonomy) covers single- versus
multi-valued keys and wiring the term templates.

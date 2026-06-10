---
title: "Build browse-by-{field} pages with AddTaxonomy"
description: "Group your content by any front-matter field (cuisine, tag, audience, series) and render the resulting term pages from a Razor component. Hot-reloads when source files change."
uid: how-to.content-services.taxonomy
order: 6
sectionLabel: "Content Services"
tags: [taxonomy, navigation, content-service, hot-reload]
---

To make the same content reachable through more than one browse axis — recipes by cuisine *and* by dietary tag, docs by audience, posts by series — wire each axis with `AddTaxonomy<TFrontMatter, TKey>`. Each call emits a `/{base}/` index plus one `/{base}/{slug}/` term page per distinct key, each rendered from a Razor component you supply.

`AddTaxonomy` groups the records every other registered `IContentService` already projects — it does not re-parse files. Markdown is one such source, but so is any custom content service whose records carry `TFrontMatter` (see <xref:how-to.content-services.custom-content-service>).

## Define your front matter

Add a property for the field you want to group on. Implement <xref:reference.api.i-taggable> when one of your axes is multi-valued.

```csharp
public record RecipeFrontMatter : IFrontMatter, ITaggable
{
    public string Title { get; init; } = "";
    public string Cuisine { get; init; } = "";
    public string[] Tags { get; init; } = [];
}
```

A recipe page then carries:

```yaml
---
title: Carbonara
cuisine: italian
tags: [pasta, eggs, weeknight]
---
```

## Register the axis

Each `AddTaxonomy<TFrontMatter, TKey>` call is one axis. Use `SelectKey` for single-valued projections, `SelectKeys` for multi-valued — exactly one of the two is required.

```csharp
builder.Services.AddTaxonomy<RecipeFrontMatter, string>(opts =>
{
    opts.BaseUrl    = "/cuisine";
    opts.SelectKey  = fm => fm.Cuisine;
    opts.IndexPage  = typeof(Pages.CuisineIndex);
    opts.TermPage   = typeof(Pages.CuisineTerm);
});

builder.Services.AddTaxonomy<RecipeFrontMatter, string>(opts =>
{
    opts.BaseUrl    = "/tag";
    opts.SelectKeys = fm => fm.Tags;
    opts.IndexPage  = typeof(Pages.TagIndex);
    opts.TermPage   = typeof(Pages.TagTerm);
});
```

A `Pasta` recipe tagged `[pasta, eggs, weeknight]` ends up under `/tag/pasta/`, `/tag/eggs/`, and `/tag/weeknight/`. A `Sushi` recipe with `cuisine: japanese` ends up under `/cuisine/japanese/`. The two registrations coexist on the same `RecipeFrontMatter` because they target different `BaseUrl`s.

## Mount the endpoints

`AddTaxonomy` registers an `IContentService` so the build crawler discovers the routes; the live HTTP handlers are mounted by `MapTaxonomy`:

```csharp
app.MapTaxonomy<RecipeFrontMatter, string>();
```

Call `MapTaxonomy` once per `<TFrontMatter, TKey>` pair — it walks every `AddTaxonomy` registration of that pair and mounts both index and term endpoints for each.

`HtmlRenderer` is required to render the components — wire it the same way the bare-host Razor recipe does:

```csharp
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
```

See <xref:how-to.response-pipeline.razor-page-on-bare-host> for the full bare-host setup.

## Author the term page

The Razor component receives the matching `TaxonomyTerm<TFrontMatter, TKey>` as a `Term` parameter:

```razor
@using Pennington.Taxonomy

<h1>@Term.Label</h1>
<p>@Term.Items.Count recipes</p>

<ul>
    @foreach (var item in Term.Items)
    {
        <li><a href="@item.Url">@item.FrontMatter.Title</a></li>
    }
</ul>

@code {
    [Parameter] public TaxonomyTerm<RecipeFrontMatter, string> Term { get; set; } = null!;
}
```

The index page receives the full term list as `Terms`:

```razor
@using Pennington.Taxonomy
@using System.Collections.Immutable

<h1>Browse by cuisine</h1>
<ul>
    @foreach (var term in Terms)
    {
        <li><a href="@term.Url">@term.Label (@term.Items.Count)</a></li>
    }
</ul>

@code {
    [Parameter] public ImmutableList<TaxonomyTerm<RecipeFrontMatter, string>> Terms { get; set; } = [];
}
```

The snippets above are deliberately minimal — bare fragments that get the term data onto the page. Each component backs a route, so wrap its markup in your site layout the same way you would any bare-host Razor page.

## Customize slugs and labels

Default slug encoding lowercases the key, replaces whitespace with hyphens, and URL-encodes the rest. Override either:

```csharp
opts.SlugFor  = key => key.ToLowerInvariant();                                       // skip the URL-encode for plain ASCII
opts.LabelFor = key => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key);         // pretty-print on the term page
```

## Hot reload

When a markdown file the taxonomy reads changes, the cached term list is invalidated and the next request rebuilds it.

Edits during `dotnet run` propagate immediately.

## Verify

- Run `dotnet run` and visit `/cuisine/` — the index lists every cuisine, and `/cuisine/japanese/` renders the term page with the sushi recipe in it.
- Visit `/tag/pasta/` — the same carbonara recipe appears under its tag axis, confirming both registrations coexist.
- Run `dotnet run -- build` and confirm the static build writes `output/cuisine/japanese/index.html` (and one folder per term under `output/tag/`).

## Related

- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- How-to: [Paginate a long listing](xref:how-to.discovery.pagination)
- How-to: [Render a Razor page on a bare host](xref:how-to.response-pipeline.razor-page-on-bare-host)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)

## Caveats

- **Listed in the sitemap.** Taxonomy routes use `EndpointSource` (the canonical HTML lives behind `MapTaxonomy`'s endpoints), but they serve real HTML, so they appear in navigation, search, cross-references, *and* `/sitemap.xml` — same as a <xref:how-to.content-services.custom-content-service> page.
- **Records of `TFrontMatter`, from any source.** An axis collects only records whose metadata is a `TFrontMatter`; everything else is ignored. To feed it from something other than markdown, project that type from a custom service (see <xref:how-to.content-services.custom-content-service>).
- **Drafts and future-dated posts are skipped.** Items whose `IsHiddenFromBuild` is `true` — `IsDraft` set, or a `Date` in the future — are excluded from every term, same convention as the rest of the pipeline.
- **One Razor component per axis.** Different cuisines can't render with different templates; switch on `Term.Key` inside `TermPage` if some terms need a custom layout.

---
title: "Add a custom schema.org JSON-LD type"
description: "Define a record that subclasses JsonLdEntity, attribute its properties for System.Text.Json, and either let the front matter own it via IHasStructuredData or render it inline from a Razor page."
uid: how-to.rich-content.structured-data-custom-types
order: 4
sectionLabel: "Rich Content"
tags: [seo, structured-data, json-ld, schema-org]
---

Pennington's `<StructuredData>` component takes any `JsonLdEntity` and emits it as a `<script type="application/ld+json">` in the page head. To support a schema.org type the framework doesn't ship — `Recipe`, `Product`, `ScholarlyArticle`, `Event`, or anything else — write a record in your own assembly.

There are two ways to wire it in: implement `IHasStructuredData` on your front matter so the template emits it automatically, or build the entity inline from a Razor page. The capability-interface path is the default; the inline path is the fallback when the page doesn't have a front matter (a hand-routed Razor page) or when the entity depends on something other than front-matter values.

## Before you begin
- A working Pennington site with `CanonicalBaseUrl` set on `PenningtonOptions` or `DocSiteOptions`. The shipped templates skip JSON-LD when this is empty so URLs don't end up relative.

## 1. Define the record

Subclass `JsonLdEntity`, override `Type` with the schema.org type literal, and attribute every field with `[JsonPropertyName]`. Repeat the `[JsonPropertyName("@type")]` attribute on the override — `System.Text.Json` doesn't inherit attributes through `override`.

```csharp:symbol
examples/BlogKitchenSinkExample/StructuredDataBuilder.cs
```

This example defines `JsonLdRecipe`, a `Recipe` entity record. It is not a framework type — you own it in your own assembly — and it is the record the wiring snippets in steps 3a and 3b instantiate.

The base `JsonLdEntity` already supplies `@context` (defaulted to `https://schema.org`). Override the `Context` initializer if you need a different vocabulary.

Optional fields stay nullable; `JsonLdSerializer` is configured with `JsonIgnoreCondition.WhenWritingNull`, so unset fields drop out of the JSON.

## 2. Apply the date converter when you have dates

For schema.org dates, attribute the property with `[JsonConverter(typeof(JsonLdDateConverter))]`. The converter emits `yyyy-MM-ddTHH:mm:ssZ` regardless of `DateTimeKind`, matching the wire format Google's rich-results validator expects.

```csharp
[JsonPropertyName("datePublished")]
[JsonConverter(typeof(JsonLdDateConverter))]
public DateTime? DatePublished { get; init; }
```

## 3a. Wire it through the front matter (capability path)

When the entity's data lives in front matter, implement `IHasStructuredData` on your front-matter record. The DocSite and BlogSite templates check for the capability and emit whatever entities the front matter yields — no Razor code required.

```csharp
public record RecipeFrontMatter : IFrontMatter, IHasStructuredData
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public IReadOnlyList<string> Ingredients { get; init; } = [];
    public IReadOnlyList<string> Steps { get; init; } = [];

    public IEnumerable<JsonLdEntity> GetStructuredData(StructuredDataContext context)
    {
        yield return new JsonLdRecipe
        {
            Name = Title,
            Description = Description,
            Url = context.CanonicalUrl,
            Ingredients = Ingredients,
            Instructions = Steps,
        };
    }
}
```

`StructuredDataContext.CanonicalUrl` is the absolute URL the template has already resolved (canonical base plus the page's path). `StructuredDataContext.FallbackAuthorName` is honored by BlogSite when the front matter's `Author` is empty.

A page can yield multiple entities — pair a `Recipe` with a `BreadcrumbList`, or emit a `HowTo` alongside a `Recipe` for instruction-heavy pages.

## 3b. Render inline from a Razor page (escape hatch)

When the entity isn't a function of front matter — a hand-routed landing page, a page that pulls from a data file, a page that wraps a third-party feed — pass the entities directly into `<StructuredData>`:

```razor
@using Pennington.StructuredData
@inject PenningtonOptions Options

@if (!string.IsNullOrEmpty(Options.CanonicalBaseUrl))
{
    <StructuredData Entities="BuildEntities()" />
}

@code {
    private IEnumerable<JsonLdEntity> BuildEntities()
    {
        yield return new JsonLdRecipe
        {
            Name = "Weeknight pasta with garlic and oil",
            Ingredients = ["1 lb spaghetti", "6 cloves garlic, thinly sliced"],
            Instructions = ["Boil the pasta", "Toast the garlic", "Toss and serve"],
        };
    }
}
```

## Verify

1. Visit the page in dev mode and view source. Look for `<script type="application/ld+json">` with your `@type`.
2. Copy the rendered HTML into [Google's Rich Results test](https://search.google.com/test/rich-results) and confirm the type validates.
3. If a field is missing from the JSON, check that the property is non-null and that it carries a `[JsonPropertyName]` attribute — properties without one use the C# member name verbatim.

## See also

- Reference: [Utility components](xref:reference.ui.utility) — `<StructuredData>` parameters.
- How-to: [Render a Razor component as a page on a bare host](xref:how-to.response-pipeline.razor-page-on-bare-host) — wires the page that emits the JSON-LD.
- The schema.org vocabulary at [schema.org](https://schema.org) for available types and field names.

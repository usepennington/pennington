---
title: "Adding JSON-LD Structured Data"
description: "Embed Schema.org structured data in page headers using Penn's StructuredData Razor component with JsonLdArticle, JsonLdBreadcrumbList, and JsonLdWebSite"
uid: "penn.how-to.adding-structured-data"
order: 20
---

You want search engines to show rich results for your pages -- article metadata, breadcrumb trails, and site identity. Penn provides a StructuredData Razor component that generates JSON-LD Schema.org markup.

## Beat 1: The StructuredData component and JSON-LD types

How to embed Schema.org structured data in page `<head>` using Penn's Razor component and JSON-LD type records.

### What to show
- Show the three JSON-LD record types in `T:Penn.StructuredData.JsonLdArticle` (with properties `Headline`, `Description`, `Url`, `DatePublished`, `AuthorName`), `T:Penn.StructuredData.JsonLdBreadcrumbList` (with `Items` of type `IReadOnlyList<JsonLdBreadcrumbItem>`), and `T:Penn.StructuredData.JsonLdWebSite` (with `Name`, `Url`, `Description`). Also show `T:Penn.StructuredData.JsonLdBreadcrumbItem` (with `Position`, `Name`, `Url`).
- Show the `StructuredData` Razor component at `:path src/Penn.UI/Components/StructuredData.razor`: it accepts three parameters (`[Parameter] public JsonLdArticle? Article`, `[Parameter] public JsonLdBreadcrumbList? Breadcrumbs`, `[Parameter] public JsonLdWebSite? WebSite`). In `OnParametersSet`, it serializes each non-null parameter using `T:Penn.StructuredData.JsonLdSerializer` and renders `<script type="application/ld+json">` blocks inside `<HeadContent>`.
- Show `T:Penn.StructuredData.JsonLdSerializer` and its three static methods: `M:Penn.StructuredData.JsonLdSerializer.SerializeArticle(Penn.StructuredData.JsonLdArticle)`, `M:Penn.StructuredData.JsonLdSerializer.SerializeBreadcrumbList(Penn.StructuredData.JsonLdBreadcrumbList)` (returns null when the list is empty), and `M:Penn.StructuredData.JsonLdSerializer.SerializeWebSite(Penn.StructuredData.JsonLdWebSite)`. Each produces a JSON string with `@context: "https://schema.org"` and the appropriate `@type`.
- Show the serializer's `EscapeForScriptTag` method which replaces `</` with `<\/` to prevent premature `<script>` tag closing when JSON contains HTML-like content.

### Key points
- The `StructuredData` component uses `<HeadContent>` so it works with Blazor's head management -- no need to manually inject into `_Host.cshtml`
- `JsonLdSerializer` uses `System.Text.Json` with `UnsafeRelaxedJsonEscaping` for minimal escaping, then applies script-tag-safe escaping separately
- Null properties are omitted from the JSON output (`JsonIgnoreCondition.WhenWritingNull`)
- `SerializeBreadcrumbList` returns null when passed an empty list, preventing empty structured data from being rendered
- The `Author` in `JsonLdArticle` is serialized as a nested `Person` schema object with `@type: "Person"`

## Beat 2: Populating structured data

How BlogSite populates the StructuredData component, and how to do it manually in a custom layout.

### What to show
- Show how BlogSite populates the component: in `Blog.razor`, `BuildArticleLd()` creates a `JsonLdArticle` from `BlogFrontMatter` properties. In `Home.razor`, `BuildWebSiteLd()` creates a `JsonLdWebSite` from `BlogSiteOptions`. Both check `CanonicalBaseUrl` before rendering.
- For custom Penn core sites, explain that `P:Penn.Infrastructure.PennOptions.CanonicalBaseUrl` must be set for absolute URLs in structured data. Show how to construct `JsonLdArticle`, `JsonLdBreadcrumbList`, and `JsonLdWebSite` manually and pass them to the `<StructuredData>` component in a layout page.

### Key points
- BlogSite populates `JsonLdArticle` from `T:Penn.FrontMatter.IDateable`, `T:Penn.FrontMatter.IDescribable`, and `P:Penn.FrontMatter.IFrontMatter.Title` on the blog post front matter
- `CanonicalBaseUrl` is required for absolute URLs in structured data -- without it, the component skips rendering
- Custom sites must manually build the JSON-LD records and pass them as parameters to the `<StructuredData>` component

## Beat 3: Verify JSON-LD

How to confirm that JSON-LD structured data is embedded correctly during development and in the static build output.

### What to show
- For JSON-LD: view source on a blog post page and locate the `<script type="application/ld+json">` blocks in the `<head>`. There should be one for `Article` (with headline, url, datePublished, author) and one for `BreadcrumbList` (with navigation breadcrumbs). On the homepage, there should be a `WebSite` block.
- Run `dotnet run -- build` to generate the static site. Open a generated HTML file and verify JSON-LD is embedded in the `<head>`.

### Key points
- JSON-LD is embedded per-page at render time, so it appears in both dev mode and static build output
- Each page may have multiple `<script type="application/ld+json">` blocks -- one per structured data type (Article, BreadcrumbList, WebSite)
- Verify that the `@type`, `headline`, `url`, and `datePublished` fields match the page's front matter values

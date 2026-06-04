---
title: "Add a custom content format"
description: "Register a non-markdown file format — here Cooklang .cook recipes — so its files are discovered, parsed, and rendered as pages through the same pipeline as markdown: supply a front-matter type, an IContentParser, an IContentRenderer, and one AddContentFormat call."
uid: how-to.content-services.custom-content-format
order: 7
sectionLabel: "Content Services"
tags: [extensibility, content-format, parser, renderer, cooklang]
---

To serve a file format Pennington doesn't parse natively — a recipe format, an org-mode file, a bespoke DSL — register it as a *content format*. Pennington discovers the files and tracks them for navigation, search, and resolution; a parser and a renderer you supply turn each file into a page. The pipeline dispatches by the format's key, so your format and markdown coexist on one site.

Reach for this when your content is files with a consistent front-matter-plus-body shape. To source content that isn't file-backed — a remote API, a database — implement `IContentService` directly instead (see <xref:how-to.content-services.custom-content-service>).

This guide follows `examples/BeyondCookFormatExample`, which registers [Cooklang](https://cooklang.org/) `.cook` recipes at `/recipes/{slug}/` next to a markdown landing page.

## Before you begin

- A bare `AddPennington` host with a catch-all that resolves through `IPageResolver` (see <xref:tutorials.getting-started.first-site>).
- A file format with a `---` YAML front-matter block and a body your code can turn into HTML. The example parses recipe bodies with the `CooklangSharp` NuGet package.

## Define the front-matter type

Every page carries typed front matter implementing `IFrontMatter` (the contract guarantees a `Title`). Add capability interfaces — `ITaggable`, `IOrderable`, `ISectionable` — for the fields navigation and search should pick up.

```csharp:symbol
examples/BeyondCookFormatExample/CookFrontMatter.cs
```

> [!IMPORTANT]
> Front-matter keys bind to camelCased property names, and a build (`-- build`) throws on any key no property matches. Name your YAML keys to match — `prepTime`, not `prep time` — or author the property to a single word. A multi-word key with a space never binds and fails the build.

## Write the parser

An `IContentParser` reads a discovered file and returns a `ParsedItem` — the typed front matter plus the raw body. Inject the framework's `FrontMatterParser` to split the YAML exactly as the markdown parser does, and hand the body on untouched for the renderer.

```csharp:symbol
examples/BeyondCookFormatExample/CookContentParser.cs
```

The discovered item's source is a `FileSource` carrying the file path and the format key. You don't stamp the format onto the `ParsedItem` yourself — the dispatcher does that from the source, so the matching renderer is selected downstream.

## Write the renderer

Markdown renders through a text pipeline; a structured format like a recipe renders through a **Razor component**. Subclass `RazorContentRenderer<TComponent>` (in `Pennington.Pipeline`): the base owns the Blazor `HtmlRenderer` dispatch, heading anchors, and outline extraction, so you write only a component and a `BuildParameters` that projects the parsed body into the component's parameters.

The component binds the parsed model and emits the markup. The page structure is Razor; the tight inline token run within a step is built as a string (inline HTML is whitespace-sensitive — a stray space would land before a `.` or `(`):

```razor:symbol
examples/BeyondCookFormatExample/Components/RecipeView.razor
```

The renderer parses the Cooklang body and hands the model to the component:

```csharp:symbol
examples/BeyondCookFormatExample/CookContentRenderer.cs
```

Throwing from `BuildParameters` (here, when the body won't parse) is captured as a `FailedItem` — it lands in the build report and the dev overlay like any markdown failure. The base produces the page-body HTML and its outline; the host or layout supplies the surrounding chrome, the same way it wraps a rendered markdown body. The host registers Razor's component services with `AddRazorComponents()`.

## Register the format

`AddContentFormat` ties the pieces together — a content directory, a file glob, the format key, and the parser and renderer types (resolved from DI). Register it alongside `AddMarkdownContent` so prose and recipes share the host:

```csharp
penn.AddContentFormat<CookFrontMatter>("cook", cook =>
{
    cook.ContentPath = "recipes";
    cook.FilePattern = "*.cook";
    cook.BasePageUrl = "/recipes";
    cook.SectionLabel = "Recipes";
})
.UseParser<CookContentParser>()
.UseRenderer<CookContentRenderer>();
```

That's the whole wiring. The pipeline routes each URL to the parser and renderer registered for its format, so `IPageResolver`, the build crawler, navigation, search, and the sitemap treat cook pages exactly like markdown ones — the catch-all `MapGet("/{*path}", IPageResolver resolver)` resolves both without changes.

## Verify

- Run `dotnet run --project examples/BeyondCookFormatExample` and open `/` (the markdown landing page) and `/recipes/chicken-piccata/` (a recipe rendered to HTML — title, ingredient list, and method steps).
- Run `dotnet run --project examples/BeyondCookFormatExample -- diag routes`. Each `/recipes/{slug}/` is listed with the `cook` kind next to the markdown `/`.
- Run `dotnet run --project examples/BeyondCookFormatExample -- build output`. Confirm `output/recipes/{slug}/index.html` exists for every recipe and that `output/sitemap.xml` lists each `/recipes/{slug}/` URL.

## Related

- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service) — implement `IContentService` directly when content isn't file-backed.
- Background: [Why ContentSource is a union](xref:explanation.core.content-source) — what `FileSource` is and how the dispatcher routes a format to its parser and renderer.
- Background: [The content pipeline](xref:explanation.core.content-pipeline) — the discover → parse → render path your format plugs into.

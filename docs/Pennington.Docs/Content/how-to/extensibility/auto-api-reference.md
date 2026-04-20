---
title: "Auto-generate an API reference tree for a class library"
description: "Add Pennington.DocSite.Api, call AddApiReference, and get one /reference/api/{type}/ page per public type plus inline Mdazor components for member tables, summaries, and extension-method catalogs."
uid: how-to.extensibility.auto-api-reference
order: 203080
sectionLabel: Extensibility
tags: [extensibility, roslyn, xmldoc, api-reference, content-service]
---

To ship a DocSite whose reference section stays in sync with a class library's public surface, add the `Pennington.DocSite.Api` package and call `AddApiReference()`. One Razor template renders every public type, and a handful of Mdazor components (`<ApiMemberTable>`, `<ApiSummary>`, `<ExtensionMethods>`, `<ApiParameterTable>`) are available inline in markdown for hand-authored reference pages. The Roslyn workspace is walked once on first request; every downstream page, search entry, and xref keys off that single pass.

## Before you begin

- Completed <xref:tutorials.beyond-basics.connect-roslyn>, so `AddPenningtonRoslyn` is wired and `SolutionPath` points at the solution containing the library you want to document.
- The target library has `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — without that, `ISymbol.GetDocumentationCommentXml()` returns empty strings and the generated pages have no prose.
- `AddDocSite` is already wired: `AddApiReference` appends its own assembly to `DocSiteOptions.AdditionalRoutingAssemblies` at registration time, so it must run after `AddDocSite`.

## Wire the package

Add a project reference to `Pennington.DocSite.Api` and call `AddApiReference` after `AddDocSite` and `AddPenningtonRoslyn`. With no arguments, the default `ProjectFilter` excludes `*.Tests` / `*.IntegrationTests` projects and the entry assembly itself (so the docs host does not publish reference pages for its own types).

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions { /* ... */ });
builder.Services.AddPenningtonRoslyn(r => r.SolutionPath = "../MyLibrary.slnx");
builder.Services.AddApiReference();
```

The call registers the `/reference/api/` index page, the per-type `/reference/api/{slug}/` template, the `IContentService` that publishes them, and every Mdazor component listed below.

## Narrow what gets published

### Limit the projects walked with `ProjectFilter`

Pass a predicate when a single solution mixes libraries you want to document with ones you do not — integration fixtures, sample apps, unrelated utility projects. The predicate receives the Roslyn `Project` directly, so filters on `Name`, `AssemblyName`, or language work equally well.

```csharp
builder.Services.AddApiReference(opts =>
{
    opts.ProjectFilter = project =>
        project.Name.StartsWith("MyLibrary", StringComparison.Ordinal)
        && !project.Name.EndsWith(".Tests", StringComparison.Ordinal);
});
```

### Hide individual types with `TypeFilter`

`TypeFilter` runs on top of the built-in rules (public, non-delegate, non-attribute, non-`ComponentBase`, has xmldoc). Use it to drop a namespace that is public only by build necessity, or to skip types tagged with a marker attribute.

```csharp
builder.Services.AddApiReference(opts =>
{
    opts.TypeFilter = type =>
        type.ContainingNamespace.ToDisplayString() != "MyLibrary.Internal"
        && !type.GetAttributes().Any(a => a.AttributeClass?.Name == "InternalApiAttribute");
});
```

## Render reference fragments inline

### Summarize one symbol with `<ApiSummary>`

Pulls the `<summary>` tag off a type or member and renders it as prose. Pass an xmldocid as `XmlDocId`.

````markdown
<ApiSummary XmlDocId="T:Pennington.DocSite.Api.ApiReferenceOptions" />
````

<ApiSummary XmlDocId="T:Pennington.DocSite.Api.ApiReferenceOptions" />

### Enumerate type members with `<ApiMemberTable>`

`Kind="All"` groups members by category (Properties, Constructors, Fields, Methods, Events) with headings between; narrow it with `Kind="Properties"` or `Kind="Methods"` for a single bucket.

````markdown
<ApiMemberTable XmlDocId="T:Pennington.DocSite.Api.ApiReferenceOptions" Kind="Properties" />
````

<ApiMemberTable XmlDocId="T:Pennington.DocSite.Api.ApiReferenceOptions" Kind="Properties" />

### List a method's parameters with `<ApiParameterTable>`

Pass a method xmldocid (`M:...`). The table pulls parameter names and types from the symbol and descriptions from each `<param>` tag.

````markdown
<ApiParameterTable XmlDocId="M:Pennington.DocSite.Api.ApiReferenceServiceExtensions.AddApiReference(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.DocSite.Api.ApiReferenceOptions})" />
````

<ApiParameterTable XmlDocId="M:Pennington.DocSite.Api.ApiReferenceServiceExtensions.AddApiReference(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.DocSite.Api.ApiReferenceOptions})" />

### Catalog extension methods by receiver with `<ExtensionMethods>`

Groups every public extension method in the workspace by the unqualified short name of its first (receiver) parameter. `Receiver="IServiceCollection"` gathers every `services.AddX()` helper the library ships.

````markdown
<ExtensionMethods Receiver="IServiceCollection" />
````

## Result

Every public type with an xmldoc comment gets a route under `/reference/api/{slug}/`:

```text
/reference/api/                        -> uid: reference.api
/reference/api/api-reference-options/  -> uid: reference.api.api-reference-options
/reference/api/extension-method-index/ -> uid: reference.api.extension-method-index
```

Xref links like `<xref:reference.api.api-reference-options>` resolve, the pages flow through search and llms.txt, and the index page at `/reference/api/` lists every type grouped by namespace. Nothing appears in the sidebar TOC — the generated pages are reached via type-name search, xref links, and the index page.

## Verify

- Run `dotnet run` and visit `/reference/api/` — expect one `<li>` per public documented type, grouped by namespace.
- Visit `/reference/api/{some-type-slug}/` for a type you know has an xmldoc `<summary>` — expect the summary prose and a member table grouped by kind.
- Add `<xref:reference.api.{slug}>` to any markdown page and confirm it resolves to the generated page after a rebuild.

## Related

- Tutorial: <xref:tutorials.beyond-basics.connect-roslyn> — the `AddPenningtonRoslyn` / `SolutionPath` wire-up this how-to builds on.
- How-to: <xref:how-to.extensibility.custom-content-service> — hand-write an `IContentService` when `AddApiReference`'s discovery rules are not the right shape.
- Reference: <xref:reference.api.api-reference-options>, <xref:reference.api.api-reference-service-extensions>.

---
title: "Auto-generate an API reference tree for a class library"
description: "Wire a metadata backend (Roslyn workspace or a compiled .dll + .xml pair), call AddApiReference, and get one /reference/api/{type}/ page per public type plus inline Mdazor components for member tables, summaries, and extension-method catalogs."
uid: how-to.content-services.auto-api-reference
order: 208020
sectionLabel: "Content Services"
tags: [extensibility, roslyn, reflection, xmldoc, api-reference, content-service]
---

To ship a DocSite whose reference section stays in sync with a class library's public surface, register a metadata backend and call `AddApiReference()`. One Razor template renders every public type, and a handful of Mdazor components (`<ApiMemberTable>`, `<ApiSummary>`, `<ExtensionMethods>`, `<ApiParameterTable>`) are available inline in markdown for hand-authored reference pages. Every downstream page, search entry, and xref keys off a single pass over the configured backend.

Two backends are available:

- `Pennington.Roslyn.ApiMetadata.AddApiMetadataFromRoslyn()` — walks a live Roslyn workspace. Use when you build the documented library from source alongside the docs host.
- `Pennington.ApiMetadata.Reflection.AddApiMetadataFromCompiledAssembly()` — reflects over a compiled `.dll` and parses the companion xmldoc `.xml` file. Use when you document a third-party assembly (for example, a NuGet package) without vendoring its source.

## Before you begin

- `AddDocSite` is already wired: `AddApiReference` appends its own assembly to `DocSiteOptions.AdditionalRoutingAssemblies` at registration time, so it must run after `AddDocSite`.
- One metadata backend is registered before `AddApiReference`. Without one, the content service has nothing to publish.

## Wire the Roslyn backend

Add project references to `Pennington.Roslyn` and `Pennington.DocSite.Api`, then call `AddApiMetadataFromRoslyn` followed by `AddApiReference` after `AddDocSite`. With no options, the default `ProjectFilter` excludes `*.Tests` / `*.IntegrationTests` projects and the entry assembly itself.

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions { /* ... */ });
builder.Services.AddPenningtonRoslyn(r => r.SolutionPath = "../MyLibrary.slnx");
builder.Services.AddApiMetadataFromRoslyn();
builder.Services.AddApiReference();
```

The target library needs `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — without that, `ISymbol.GetDocumentationCommentXml()` returns empty strings and the generated pages have no prose.

## Wire the reflection backend

Add a project reference to `Pennington.ApiMetadata.Reflection`, add a `<PackageReference>` to the library you want to document, and have Pennington resolve the assembly by simple name. A complete single-package DocSite host:

```csharp:path
examples/FusionCacheDocSiteExample/Program.cs
```

`FromPackageReference` calls `Assembly.Load` against the project's deps.json and grabs the resolved `.dll` path out of the NuGet cache. The companion `.xml` file ships alongside the dll for any package built with `<GenerateDocumentationFile>true</GenerateDocumentationFile>`; the provider picks it up automatically. No staging, no committed binary, and bumping the documented version is a `<PackageReference Version=…>` change.

When the target isn't a normal NuGet reference — a locally-built assembly, a single-file bundle, or something else without a file location — fall back to the explicit form:

```csharp
builder.Services.AddApiMetadataFromCompiledAssembly(opts =>
    opts.AssemblyFiles.Add(Path.Combine(builder.Environment.ContentRootPath, "lib", "net9.0", "Foo.dll")));
```

The reflection backend uses `MetadataLoadContext` to inspect metadata without running the assembly's code — no MSBuild workspace, no source needed. `<ExtensionMethods>` and the `:xmldocid` source fence require a live symbol graph and are unavailable under this backend.

## Customize the route prefix

The default prefix is `/reference/api/`. Override it per registration via `AddApiReference`'s `RoutePrefix` option when the shorter `/api/` (or any other shape) is a better fit:

```csharp
builder.Services.AddApiMetadataFromCompiledAssembly(opts => { /* ... */ });
builder.Services.AddApiReference(configure: opts => opts.RoutePrefix = "/api/");
```

Type pages land at `/api/{slug}/`, and xref uids stay as `reference.api.{slug}` for back-compat with the default registration.

## Document multiple libraries on one site

Pair each library with its own named `AddApiMetadataFrom*` call and a matching `AddApiReference` call. Names are the key that wires a reference tree to its provider, and every tree gets its own URL prefix:

```csharp
builder.Services.AddApiMetadataFromCompiledAssembly("spectre-console", opts =>
    opts.FromPackageReference("Spectre.Console"));
builder.Services.AddApiMetadataFromCompiledAssembly("spectre-console-cli", opts =>
    opts.FromPackageReference("Spectre.Console.Cli"));

builder.Services.AddApiReference("spectre-console", opts =>
    opts.RoutePrefix = "/api/spectre/");
builder.Services.AddApiReference("spectre-console-cli", opts =>
    opts.RoutePrefix = "/api/spectre-cli/");
```

Each `FromPackageReference` call resolves one DLL from its matching `<PackageReference>`. Cross-package type references resolve automatically across the NuGet cache root.

**Cross-references between named trees:** uids pick up a qualifier. Default-named registrations emit `reference.api.{slug}` (unchanged). Named registrations emit `reference.api.{name}.{slug}` — for example, `<xref:reference.api.spectre-console.ansi-console>` and `<xref:reference.api.spectre-console-cli.command-app>`.

**Hand-authored markdown:** components like `<ApiSummary>` auto-pick up the source from the enclosing generated page. For markdown pages outside the generated tree that reach into a specific named registration, add an explicit `Source` attribute:

```markdown
<ApiSummary XmlDocId="T:Spectre.Console.Cli.CommandApp" Source="spectre-console-cli" />
```

## Narrow what gets published

### Limit the projects walked with `ProjectFilter`

Pass a predicate when a single solution mixes libraries you want to document with ones you do not — integration fixtures, sample apps, unrelated utility projects. The predicate receives the Roslyn `Project` directly, so filters on `Name`, `AssemblyName`, or language work equally well.

```csharp
builder.Services.AddApiMetadataFromRoslyn(opts =>
{
    opts.ProjectFilter = project =>
        project.Name.StartsWith("MyLibrary", StringComparison.Ordinal)
        && !project.Name.EndsWith(".Tests", StringComparison.Ordinal);
});
```

### Hide individual types with `TypeFilter`

`TypeFilter` runs on top of the built-in rules (public, non-delegate, non-attribute, non-`ComponentBase`, has xmldoc). Use it to drop a namespace that is public only by build necessity, or to skip types tagged with a marker attribute.

```csharp
builder.Services.AddApiMetadataFromRoslyn(opts =>
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
<ApiSummary XmlDocId="T:Pennington.ApiMetadata.ApiTypeSummary" />
````

<ApiSummary XmlDocId="T:Pennington.ApiMetadata.ApiTypeSummary" />

### Enumerate type members with `<ApiMemberTable>`

`Kind="All"` groups members by category (Properties, Constructors, Fields, Methods, Events) with headings between; narrow it with `Kind="Properties"` or `Kind="Methods"` for a single bucket.

````markdown
<ApiMemberTable XmlDocId="T:Pennington.ApiMetadata.ApiTypeSummary" Kind="Properties" />
````

<ApiMemberTable XmlDocId="T:Pennington.ApiMetadata.ApiTypeSummary" Kind="Properties" />

### List a method's parameters with `<ApiParameterTable>`

Pass a method xmldocid (`M:...`). The table pulls parameter names and types from the provider's pre-formatted `ApiMember` and descriptions from each `<param>` tag.

````markdown
<ApiParameterTable XmlDocId="M:Pennington.ApiMetadata.IApiMetadataProvider.GetMembersAsync(System.String,Pennington.ApiMetadata.MemberKind,Pennington.ApiMetadata.AccessFilter,Pennington.ApiMetadata.MemberOrder)" />
````

<ApiParameterTable XmlDocId="M:Pennington.ApiMetadata.IApiMetadataProvider.GetMembersAsync(System.String,Pennington.ApiMetadata.MemberKind,Pennington.ApiMetadata.AccessFilter,Pennington.ApiMetadata.MemberOrder)" />

### Catalog extension methods by receiver with `<ExtensionMethods>`

Groups every public extension method in the workspace by the unqualified short name of its first (receiver) parameter. `Receiver="IServiceCollection"` gathers every `services.AddX()` helper the library ships. Roslyn-only; the DocFx backend returns an empty list.

````markdown
<ExtensionMethods Receiver="IServiceCollection" />
````

## Result

Every public type with an xmldoc comment gets a route under `/reference/api/{slug}/`:

```text
/reference/api/                        -> uid: reference.api
/reference/api/api-type-summary/       -> uid: reference.api.api-type-summary
/reference/api/api-member/             -> uid: reference.api.api-member
```

Xref links like `<xref:reference.api.api-type-summary>` resolve, the pages flow through search and llms.txt, and the index page at `/reference/api/` lists every type grouped by namespace. Nothing appears in the sidebar TOC — the generated pages are reached via type-name search, xref links, and the index page.

## Verify

- Run `dotnet run` and visit `/reference/api/` — expect one `<li>` per public documented type, grouped by namespace.
- Visit `/reference/api/{some-type-slug}/` for a type you know has an xmldoc `<summary>` — expect the summary prose and a member table grouped by kind.
- Add `<xref:reference.api.{slug}>` to any markdown page and confirm it resolves to the generated page after a rebuild.

## Related

- Tutorial: <xref:tutorials.beyond-basics.connect-roslyn> — the `AddPenningtonRoslyn` / `SolutionPath` wire-up the Roslyn backend builds on.
- How-to: <xref:how-to.content-services.custom-content-service> — hand-write an `IContentService` when `AddApiReference`'s discovery rules are not the right shape.
- Reference: <xref:reference.api.api-type-summary>, <xref:reference.api.api-member>.

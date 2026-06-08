---
title: "Auto-generate an API reference tree for a class library"
description: "Wire the reflection metadata backend (a compiled .dll + .xml pair), call AddApiReference, and get one /reference/api/{type}/ page per public type plus inline Mdazor components for member tables, summaries, and extension-method catalogs."
uid: how-to.content-services.auto-api-reference
order: 3
sectionLabel: "Content Services"
tags: [extensibility, reflection, xmldoc, api-reference, content-service]
---

To ship a DocSite whose reference section stays in sync with a class library's public API, register a metadata backend and call `AddApiReference()`. One Razor template renders every public type, and a handful of Mdazor components (`<ApiMemberTable>`, `<ApiSummary>`, `<ExtensionMethods>`, `<ApiParameterTable>`) are available inline in markdown for hand-authored reference pages. Every generated page, search entry, and xref keys off a single pass over the configured backend.

`Pennington.ApiMetadata.Reflection.AddApiMetadataFromCompiledAssembly()` is the metadata backend: it reflects over a compiled `.dll` and parses the companion xmldoc `.xml` file. It documents any assembly — one you build alongside the docs host, or a third-party NuGet package — without needing its source.

## Before you begin

- `AddDocSite` is already wired: `AddApiReference` appends its own assembly to `DocSiteOptions.AdditionalRoutingAssemblies` at registration time, so it must run after `AddDocSite`.
- One metadata backend is registered before `AddApiReference`. Without one, the content service has nothing to publish.

## Wire the reflection backend

Add a project reference to `Pennington.ApiMetadata.Reflection`, add a `<PackageReference>` to the library you want to document, and have Pennington resolve the assembly by simple name. A complete single-package DocSite host:

```csharp:symbol
examples/FusionCacheDocSiteExample/Program.cs
```

`FromPackageReference` calls `Assembly.Load` against the project's deps.json and reads the resolved `.dll` path from `Assembly.Location` (typically the project's bin folder). The companion `.xml` file ships alongside the dll for any package built with `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — MSBuild does not always stage that xml into `bin/`, so when the xml is missing next to the resolved bin-path DLL the provider falls back to the matching NuGet cache copy (where the package originally ships both files together). No staging, no committed binary, and bumping the documented version is a `<PackageReference Version=…>` change.

When the target isn't a normal NuGet reference — a locally-built assembly, a single-file bundle, or something else without a file location — fall back to the explicit form:

```csharp
builder.Services.AddApiMetadataFromCompiledAssembly(opts =>
    opts.AssemblyFiles.Add(Path.Combine(builder.Environment.ContentRootPath, "lib", "net9.0", "Foo.dll")));
```

The reflection backend inspects metadata without running the assembly's code — no MSBuild workspace, no source needed. It resolves `<inheritdoc/>` and union-case xmldoc from metadata. Only the `:xmldocid` source fence is unavailable under this backend, since it extracts source text rather than metadata.

## Customize the route prefix

The default prefix is `/reference/api/`. Override it per registration via `AddApiReference`'s `RoutePrefix` option when the shorter `/api/` (or any other prefix) is a better fit:

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

The reflection backend documents the assemblies you point it at, so narrowing is a matter of which assemblies you add. The built-in rules already exclude types that are not public, are delegates or attributes, derive from `ComponentBase`, or carry no xmldoc `<summary>`.

Use `AssemblyFiles` to document an explicit list of `.dll` paths when a folder holds more assemblies than you want documented — for example, dependencies copied alongside the target only so `MetadataLoadContext` can resolve them:

```csharp
builder.Services.AddApiMetadataFromCompiledAssembly(opts =>
{
    opts.AssemblyFiles.Add(Path.Combine(libDir, "MyLibrary.dll"));
    opts.AssemblyFiles.Add(Path.Combine(libDir, "MyLibrary.Extensions.dll"));
});
```

Use `AssemblyDirectories` instead to document every `.dll`/`.xml` pair in a folder — the typical NuGet `lib/<tfm>/` layout.

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

Groups every public extension method in the assembly by the unqualified short name of its first (receiver) parameter. `Receiver="IServiceCollection"` gathers every `services.AddX()` helper the library ships.

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

Xref links like `<xref:reference.api.api-type-summary>` resolve, the pages flow through search and llms.txt, and the index page at `/reference/api/` lists every type grouped by namespace. One TOC entry — the index page, titled "API reference" by default — appears in the sidebar; per-type pages stay out of the sidebar and are reached via type-name search, xref links, and the index. Override `TocTitle` and `TocSectionLabel` on `ApiReferenceRegistrationOptions` to customize, or set `TocTitle = null` to suppress the index entry entirely.

## Verify

- Run `dotnet run` and visit `/reference/api/` — expect one `<li>` per public documented type, grouped by namespace.
- Visit `/reference/api/{some-type-slug}/` for a type you know has an xmldoc `<summary>` — expect the summary prose and a member table grouped by kind.
- Add `<xref:reference.api.{slug}>` to any markdown page and confirm it resolves to the generated page after a rebuild.

## Related

- How-to: <xref:how-to.content-services.custom-content-service> — hand-write an `IContentService` when `AddApiReference`'s discovery rules are not the right fit.
- Reference: <xref:reference.api.api-type-summary>, <xref:reference.api.api-member>.

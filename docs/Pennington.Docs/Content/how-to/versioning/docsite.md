---
title: "Version a DocSite"
description: "Ship /v1/ and /v2/ URL trees from one DocSite host, each with its own content area and its own reflection-based API reference."
uid: how-to.versioning.docsite
order: 205010
sectionLabel: "Versioning"
tags: [versioning, api-reference, docsite]
---

To document multiple versions of a library side by side — `/v1/` and `/v2/`, each with its own content tree and its own API reference — pair `DocSiteOptions.Areas` (one area per version) with one `AddApiMetadataFromCompiledAssembly` plus matching `AddApiReference` registration per version. The framework already supports this pattern; the only friction is NuGet's one-version-per-assembly rule, which forces a `<PackageDownload>` workaround for the off-version DLL.

The recipe references `examples/VersionedDocSiteExample/`, which documents `Humanizer.Core` 2.8.26 alongside 2.14.1. For the backend basics — how `AddApiMetadataFromCompiledAssembly` and `AddApiReference` work in isolation — see <xref:how-to.content-services.auto-api-reference>.

## Before you begin

- A DocSite host already wired with `AddDocSite` (see <xref:tutorials.docsite.scaffold>).
- A decision about which version is the *active* `PackageReference`. That version resolves via `FromPackageReference("AssemblyName")`. Every other version is staged via `<PackageDownload>` and an explicit `AssemblyFiles` path.

## Lay out content by version

Use one `ContentArea` per version. The `Slug` is both the URL prefix and the folder name under `Content/`, so files at `Content/v1/foo.md` route to `/v1/foo` and the sidebar renders an area selector that doubles as a version switcher.

```csharp:path
examples/VersionedDocSiteExample/Program.cs
```

The `Areas` declaration is the only place the version names appear in the host wiring. Adding a `v3` later is two lines plus a `Content/v3/` folder.

## Reference two versions of the same NuGet package

NuGet allows only one `<PackageReference>` per assembly per project. To document a second version, add a `<PackageDownload>` element pinned with square-bracket exact-version syntax. `<PackageDownload>` fetches the package into the NuGet cache without adding it to the compile graph, leaving the `<PackageReference>` version as the one resolved through the default load context.

```xml:path
examples/VersionedDocSiteExample/VersionedDocSiteExample.csproj
```

In `Program.cs`, register one named provider per version:

- The active reference uses `FromPackageReference("Humanizer")` — `Assembly.Load` finds the v2 DLL via the project's `deps.json`.
- The off-version uses `AssemblyFiles.Add(path)` with an explicit path under `%NUGET_PACKAGES%` (or `%USERPROFILE%\.nuget\packages\`). The simple-name folder is lowercased; the version is the literal `<PackageDownload>` value; the TFM is whichever `lib/<tfm>/` the package ships.

Pair each provider with an `AddApiReference` registration whose `RoutePrefix` nests under the matching area slug:

| Provider name | `RoutePrefix` | Resolves |
|---|---|---|
| `humanizer-v1` | `/v1/api/` | `Humanizer.dll` 2.8.26 from the NuGet cache |
| `humanizer-v2` | `/v2/api/` | `Humanizer.dll` 2.14.1 via the active `PackageReference` |

The Mdazor components (`<ApiMemberTable>`, `<ApiSummary>`, …) are registered once and resolve metadata per page via the keyed provider, so two trees coexist with no further wiring.

## Cross-link between versions

Each named registration emits xref uids under `reference.api.{name}.{slug}` — for example, `<xref:reference.api.humanizer-v1.string-humanize-extensions>` and `<xref:reference.api.humanizer-v2.string-humanize-extensions>`. Use the qualified form when a v2 content page links to a v1 type to show what changed, or vice versa.

## Verify

- Run `dotnet run --project examples/VersionedDocSiteExample`.
- Visit `/v1/`, `/v2/`, `/v1/api/`, and `/v2/api/` — each renders independently.
- The startup log prints one `ApiReferenceIndex({name}): published N auto-discovered type pages` line per registration. `N` differs between versions when the two assemblies differ.
- Confirm the sidebar area selector switches between `v1` and `v2` while staying on the same page kind.

## Related

- How-to: <xref:how-to.content-services.auto-api-reference> — the single-source backend setup this recipe builds on.
- Tutorial: <xref:tutorials.docsite.sections-and-areas> — the area-driven URL prefix mechanism reused for version slugs.
- Reference: <xref:reference.host.extensions> — `AddDocSite` and `DocSiteOptions` surface.

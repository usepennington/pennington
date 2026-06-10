---
title: "Version a DocSite"
description: "Ship /v1/ and /v2/ URL trees from one DocSite host, each with its own content area and its own reflection-based API reference."
uid: how-to.versioning.docsite
order: 1
sectionLabel: "Versioning"
tags: [versioning, api-reference, docsite]
---

To serve `/v1/` and `/v2/` URL trees from one DocSite host, give each version its own `ContentArea`. One area per version is the whole mechanism for prose-only docs — the [Lay out content by version](#lay-out-content-by-version) section below is all you need.

The rest of this page layers a per-version API reference on top, which is where the only real friction lives: NuGet allows one version of an assembly per project, so the off-version DLL needs a `<PackageDownload>` workaround. If you don't need a reflected API tree, stop after the areas section.

The recipe references `examples/VersionedDocSiteExample/`, which documents `Humanizer.Core` 2.8.26 alongside 2.14.1. For how `AddApiMetadataFromCompiledAssembly` and `AddApiReference` work on a single version, see <xref:how-to.content-services.auto-api-reference>.

## Before you begin

- A DocSite host already wired with `AddDocSite` (see <xref:tutorials.docsite.scaffold>).
- A decision about which version is the *active* `PackageReference`. That version resolves via `FromPackageReference("AssemblyName")`. Every other version is staged via `<PackageDownload>` and an explicit `AssemblyFiles` path.

## Lay out content by version

Use one `ContentArea` per version. The `Slug` is both the URL prefix and the folder name under `Content/`, so files at `Content/v1/foo.md` route to `/v1/foo` and the sidebar renders an area selector that doubles as a version switcher.

```csharp:symbol
examples/VersionedDocSiteExample/Program.cs > Wiring.AddVersionedAreas
```

The `Areas` declaration is the only place the version names appear in the host wiring. Adding a `v3` later is two lines plus a `Content/v3/` folder.

A bare `/` request — anyone landing on the site root with no version prefix — falls through to the DocSite not-found page unless you give the root a page; add a `Content/index.md` (or a routed landing component) that redirects to or links the version you treat as current. Marking one version "latest" and showing a deprecation banner on older trees are content-level conventions, not host wiring: drop a shared `[!INCLUDE]` partial into each old version's pages for the banner, and point the root and header link at the current slug. See <xref:how-to.pages.redirects> for the root-redirect mechanics.

## Reference two versions of the same NuGet package

NuGet allows only one `<PackageReference>` per assembly per project. To document a second version, add a `<PackageDownload>` element pinned with square-bracket exact-version syntax. `<PackageDownload>` fetches the package into the NuGet cache without adding it to the compile graph, leaving the `<PackageReference>` version as the one resolved through the default load context.

```xml:symbol
examples/VersionedDocSiteExample/VersionedDocSiteExample.csproj
```

In `Program.cs`, register one named provider per version, then pair each with an `AddApiReference` registration whose `RoutePrefix` nests under the matching area slug:

```csharp:symbol
examples/VersionedDocSiteExample/Program.cs > Wiring.AddVersionedApiReferences
```

- The active reference uses `FromPackageReference("Humanizer")` — `Assembly.Load` finds the v2 DLL via the project's `deps.json`.
- The off-version uses `AssemblyFiles.Add(path)` with an explicit path under the NuGet global-packages folder. Read that folder from the `NUGET_PACKAGES` environment variable and fall back to the per-user default (`~/.nuget/packages` on Linux and macOS, `%USERPROFILE%\.nuget\packages` on Windows) — `Environment.GetFolderPath(SpecialFolder.UserProfile)` resolves the home directory on every platform. Inside it, the simple-name folder is lowercased; the version is the literal `<PackageDownload>` value; the TFM is whichever `lib/<tfm>/` the package ships.

The two registrations resolve as follows:

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

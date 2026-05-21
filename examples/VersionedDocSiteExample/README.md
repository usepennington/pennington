# VersionedDocSiteExample

Two-area DocSite documenting two versions of `Humanizer.Core` as side-by-side reference trees (`/v1/` and `/v2/`), each with its own content area and its own API reference. Demonstrates the *versioned* shape of `Pennington.ApiMetadata.Reflection` — two named providers pointing at two assembly versions of the same package.

## Concepts

- `DocSiteOptions.Areas` as a URL prefix mechanism — `v1` and `v2` map to `Content/v1/` and `Content/v2/`
- Two named `AddApiMetadataFromCompiledAssembly` providers pointing at two assembly versions
- `<PackageReference>` + `<PackageDownload>` working around NuGet's one-version-per-assembly constraint
- Two `AddApiReference` registrations with nested `RoutePrefix` values (`/v1/api/`, `/v2/api/`)

## Referenced from

- `how-to/versioning/docsite.md`

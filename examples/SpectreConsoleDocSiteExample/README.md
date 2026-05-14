# SpectreConsoleDocSiteExample

Two-area DocSite documenting `Spectre.Console` and `Spectre.Console.Cli` as separate reference trees, both resolved from the NuGet `PackageReference` assemblies. Demonstrates the *multi-source* shape of `Pennington.ApiMetadata.Reflection` — named providers + named `AddApiReference` registrations.

## Concepts

- Multiple named `AddApiMetadataFromCompiledAssembly` providers in one host
- Multiple `AddApiReference` registrations with distinct `RoutePrefix` values
- One sidebar containing two independent API trees (`/console/api/`, `/cli/api/`)

## Referenced from

Not currently referenced by any docs page — this example exists as a real-target multi-source demo of `Pennington.ApiMetadata.Reflection`.

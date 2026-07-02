---
name: new-example
description: Scaffold and wire a new example project under examples/, or rename/restructure an existing one. Use when adding an example site, changing what an example teaches, or moving/renaming files inside an example — covers naming, project shape, README, the examples/CLAUDE.md index row, solution registration, and docs :symbol coupling.
---

# New example project

Conventions live in `examples/CLAUDE.md` (read it first). This skill is the wiring checklist — an example isn't done until every step below holds.

## Checklist

1. **Name it.** Category prefix + `Example` suffix: `GettingStarted*` (tutorial baselines), `Beyond*` (advanced features), `DocSite*`/`BlogSite*` (template-specific), or feature-focused (`Focused*`, `Multiple*`, etc.).
2. **Shape it.** `Program.cs` + `.csproj` + `Content/`; add `Components/` only for Mdazor-referenced Razor components. Copy the `.csproj` of the closest existing example — project refs are relative (`..\..\src\Pennington\...`).
3. **Volume over bespoke pages?** Mount the shared corpus via a relative `ContentRootPath` to `../_shared/Bramble/Content[/subfolder]` instead of bundling markdown (see `DocSiteSharedCorpusExample`). Such an example legitimately has no local `Content/`.
4. **Write `README.md`.** Required for every example: purpose, concepts taught, which docs pages reference it. Keep it current when the teaching surface changes.
5. **Add a row to the index table** in `examples/CLAUDE.md` (alphabetical order). If no docs page references it yet, use an italic note in the Docs pages column rather than listing aspirational pages as fact.
6. **Register it in `Pennington.slnx`.**
7. **Wire docs coupling** if docs will embed its source: tree-sitter `:symbol` fences (syntax in `docs/Pennington.Docs/CLAUDE.md`). Staged tutorials: C# stages as root-level `StageN_<Label>.cs`; non-C# stages under `snippets/stageN.md` — never collapse stages into one file or inline non-C# stages as raw strings.
8. **Verify:** `dotnet build Pennington.slnx`, then `dotnet run --project examples/<Name> -- diag warnings` (and `diag toc` / `diag routes` when navigation or routing is the point of the example).

## Renames break docs

Before renaming a file, moving a symbol, or restructuring an existing example, grep `docs/Pennington.Docs/Content/` for the old name/path. Symbol fences fail the docs build on dangling references — find them before the build does.

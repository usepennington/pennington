# examples/ — Example site conventions

## Naming
All example projects suffix `Example`. Categories:
- `GettingStarted*` — baseline Pennington setup, used by tutorials
- `Beyond*` — advanced features (Roslyn, locales, custom Razor components)
- `DocSite*` / `BlogSite*` — template-specific (scaffold, kitchen sink, sections, authors)
- `Focused*` / `Multiple*` — feature-focused (code samples, multi-source)

## Shape
- Every example has `Program.cs` + `.csproj` + `Content/`.
- Some examples add `Components/` for Mdazor-referenced Razor components.
- Template/library refs are relative: `..\..\src\Pennington\...`.

## Staged tutorial files
Examples used by step-by-step tutorials split the teaching into `StageN_<Label>.cs` files (e.g., `Stage1_BareHost.cs`, `Stage2_AddPennington.cs`, `Stage3_UsePennington.cs`). Each stage is the teaching artifact — the docs pull them via `:xmldocid`/`:path` fences. Don't collapse stages into a single file.

## Roslyn examples
`BeyondRoslynExample` (and any future Roslyn-using example) sets `<DefaultItemExcludes>` in the csproj so the sibling `Sample/` library isn't swept into its own compile. Preserve that when editing the csproj.

## Docs coupling — renames break docs
Examples are referenced from `docs/Pennington.Docs/Content/` via:
- `csharp:path` fences (file paths — `Program.cs`, `StageN_*.cs`, Razor, config)
- `csharp:xmldocid` fences (XmlDocIds — `T:Ns.Type`, `M:Ns.Type.Method`, `P:Ns.Type.Prop`)

Before renaming a file, moving a symbol, or restructuring an example, grep `docs/Pennington.Docs/Content/` for the old name/ID. The docs build will fail on a broken reference — but you want to know before the build stage.

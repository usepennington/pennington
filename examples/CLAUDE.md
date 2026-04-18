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
Examples used by step-by-step tutorials split the teaching into per-stage artifacts:
- **C# stages** live at the project root as `StageN_<Label>.cs` (e.g., `Stage1_BareHost.cs`, `Stage2_AddPennington.cs`). Docs pull them via `csharp:xmldocid[,bodyonly]` fences.
- **Markdown (or other non-C#) stages** live under `snippets/` as `stageN.md` (e.g., `examples/DocSiteAuthorExample/snippets/stage1.md`). Docs pull them via `markdown:path` fences — `:xmldocid` is C#/VB-only.

Don't store markdown-as-a-C#-raw-string just to reach it by xmldocid; the `"""` delimiters leak into the rendered code block. Don't collapse stages into a single file.

## Roslyn examples
`BeyondRoslynExample` (and any future Roslyn-using example) sets `<DefaultItemExcludes>` in the csproj so the sibling `Sample/` library isn't swept into its own compile. Preserve that when editing the csproj.

## Docs coupling — renames break docs
Examples are referenced from `docs/Pennington.Docs/Content/` via:
- `<lang>:path` fences (file paths) — `csharp:path` for `Program.cs` / `StageN_*.cs`, `markdown:path` for `snippets/stageN.md`, `razor:path` for `.razor`, etc.
- `csharp:xmldocid` / `csharp:xmldocid,bodyonly` (XmlDocIds — `T:Ns.Type`, `M:Ns.Type.Method`, `P:Ns.Type.Prop`). **C#/VB only.**

Before renaming a file, moving a symbol, or restructuring an example, grep `docs/Pennington.Docs/Content/` for the old name/ID. The docs build will fail on a broken reference — but you want to know before the build stage.

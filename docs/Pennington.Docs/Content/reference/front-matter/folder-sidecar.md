---
title: "Folder sidecar (`_meta.yml`)"
description: "Per-folder YAML sidecar that overrides a folder's display title, sort order in its parent, and llms.txt subtree opt-in."
sectionLabel: "Front Matter"
order: 2
tags: [front-matter, yaml, folders, navigation, llms]
uid: reference.front-matter.folder-sidecar
---

A `_meta.yml` file dropped in any content folder declares folder-level metadata: an alternative display title, an explicit position in the parent navigation level, and (optionally) opt-in to a dedicated `llms.txt` subtree split. All fields are optional.

Without a sidecar, a folder's sort position is the **min-of-children** value: the smallest `order:` found on any page inside it. The `order` key below overrides that emergent value.

## Schema

```yaml
title: "Reference"
order: 5
llms:
  description: "API surface, host extensions, front-matter keys, ..."
```

| Key | Type | Default | Effect |
|---|---|---|---|
| `title` | string | `null` | Overrides both `FormatSectionTitle` (auto-generated from the folder slug) and the title from a sibling `index.md`. |
| `order` | int | `null` | Sets the folder's position among its parent's children. Overrides the emergent min-of-children rule and any `order:` set on a sibling `index.md`. |
| `llms.description` | string | `null` | When present, opts the folder into `llms.txt` subtree generation. Requires `title` to also be set; without it the description is silently ignored (no subtree, no warning, no error). |

## Resolution rules

- A field that's set wins over every other source for that folder.
- A field that's omitted falls through to the original behavior: `FormatSectionTitle(folderSlug)` for the title, min-of-children for the order, and "not an llms subtree" for the llms split.
- Folders without `_meta.yml` are unaffected — adoption is folder-by-folder.

## Discovery

`MarkdownContentService` discovers `_meta.yml` files under its content path, but it is not the only source. `FolderMetadataRegistry` aggregates rows from every registered `IContentService` that implements `IFolderMetadataProvider`, keyed by the folder's canonical URL prefix. A custom content service that surfaces its own folder metadata through that interface contributes sidecars on equal footing. Hot-reload refreshes the registry on file change.

## Example

```yaml
# docs/Pennington.Docs/Content/reference/_meta.yml
title: Reference
llms:
  description: "API surface, host extensions, front-matter keys, Markdig extensions, UI components, diagnostics codes."
```

The `Reference` folder appears in the sidebar with that title, and `/reference/llms.txt` is generated as a subtree split of the main `llms.txt`.

## See also

- Background: [Navigation-tree construction](xref:explanation.routing.navigation-tree) — how `_meta.yml` interacts with `HierarchyParts` and per-page `order:`.
- Related reference: [Front-matter keys](xref:reference.front-matter.keys) — per-page YAML keys including `order:`.

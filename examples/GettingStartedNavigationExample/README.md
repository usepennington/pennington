# GettingStartedNavigationExample

Adds a navigation menu to the styled Blazor-pages site from the previous tutorial. A `NavMenu.razor` component builds the menu from the content pipeline — `NavigationBuilder` turns the flat table-of-contents entries every `IContentService` exposes into an ordered, folder-nested tree.

## Concepts

- `IContentService.GetContentTocEntriesAsync()` exposes each source's pages as flat `ContentTocItem` entries.
- `NavigationBuilder.BuildTreeAsync(...)` sorts those entries by `order:` front matter and nests them by folder into `NavigationTreeItem` nodes; a folder with no `index.md` becomes a section node.
- Passing the current `ContentRoute` to `BuildTreeAsync` stamps `IsSelected` on the matching node, which `NavMenu.razor` renders as the active link.
- `NavigationBuilder` is registered by `AddPennington` — the menu needs no extra service wiring in `Program.cs`.

## Referenced from

- `docs/.../tutorials/getting-started/navigation.md`

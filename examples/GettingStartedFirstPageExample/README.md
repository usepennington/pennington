# GettingStartedFirstPageExample

Builds on the minimal site by adding more `.md` pages under `Content/` and a `NavigationBuilder`-driven nav strip. Teaches that adding a page is just adding a file — discovery, routing, and nav all flow from the filesystem.

## Concepts

- File-on-disk → URL convention (`Content/about.md` → `/about/`)
- `NavigationBuilder.BuildTree` over `GetIndexableEntriesAsync`
- Rendering a nav alongside the article in a hand-rolled layout — see `Program.cs` (the `MapGet("/{*path}", …)` handler builds the `<nav>` and `<article>` in the same template literal) and the staged `Stage3_AddContactPage.cs` that the tutorial walks readers through.

## Tutorial stages

`Stage1_OneFile.cs` → `Stage2_AddAboutPage.cs` → `Stage3_AddContactPage.cs`.

## Referenced from

- `docs/.../tutorials/getting-started/first-page.md`

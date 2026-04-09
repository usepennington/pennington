---
title: "Creating a Custom Front Matter Type"
description: "Define a custom front matter record with capability interfaces, register it as a content source, and consume it in Razor templates via pattern matching"
uid: "penn.tutorials.creating-a-custom-front-matter-type"
order: 10
---

## Beat 1: The Problem — Why You Need a Custom Front Matter Type

Introduce the scenario: the reader wants to add a changelog section to their Pennington site. Each release page needs a version number, a release date, a "breaking changes" flag, and component tags. Explain that the built-in front matter types (`DocFrontMatter`, `BlogFrontMatter`) do not include `Version` or `IsBreaking` properties, so a custom type is required.

### What to show
- Briefly reference the two built-in types so the reader understands what ships out of the box
- Code reference: `T:Pennington.FrontMatter.DocFrontMatter` — show the record definition to illustrate what a "stock" front matter type looks like (Title, Description, IsDraft, Tags, Section, Uid, Order)
- Code reference: `T:Pennington.FrontMatter.BlogFrontMatter` — mention as the blog-oriented alternative (adds Date, Author, Series)
- State the goal: a `ChangelogFrontMatter` record with version, breaking-change flag, and selected capabilities

### Key points
- Pennington's front matter system is composable — you pick the capabilities you need and add your own fields on top
- The built-in types are just records implementing the same interfaces you will use; there is nothing special about them

## Beat 2: Understand the IFrontMatter Interface

Explain that every front matter type must implement `IFrontMatter`, which requires a single `Title` property. This is the minimum contract Pennington needs to display content in navigation and page headers.

### What to show
- Code reference: `T:Pennington.FrontMatter.IFrontMatter` — show the full interface (just `string Title { get; }`)
- Explain the `new()` constraint used by `FrontMatterParser.Parse<T>()` — the type must have a parameterless constructor, which C# records provide automatically

### Key points
- `IFrontMatter` is intentionally minimal; everything else is opt-in via capability interfaces
- The `new()` constraint means your record must be constructible without arguments (init-only properties with defaults satisfy this)

## Beat 3: Explore the Capability Interfaces

Walk through each capability interface. Explain that these are small, single-property interfaces that Pennington's pipeline checks via pattern matching. A front matter type can implement any combination of them.

### What to show
- Code reference: `T:Pennington.FrontMatter.IDraftable` — `bool IsDraft` — Pennington excludes draft pages from production builds
- Code reference: `T:Pennington.FrontMatter.ITaggable` — `string[] Tags` — enables tag-based filtering and Badge rendering
- Code reference: `T:Pennington.FrontMatter.IDateable` — `DateTime? Date` — enables date display and chronological sorting
- Code reference: `T:Pennington.FrontMatter.IOrderable` — `int Order` — controls explicit ordering in navigation (lower = higher)
- Code reference: `T:Pennington.FrontMatter.IDescribable` — `string? Description` — used for meta descriptions and search index snippets
- Code reference: `T:Pennington.FrontMatter.ISectionable` — `string? Section` — groups pages under a navigation heading
- Code reference: `T:Pennington.FrontMatter.ICrossReferenceable` — `string? Uid` — enables xref cross-references between pages
- Code reference: `T:Pennington.FrontMatter.IRedirectable` — `string? RedirectUrl` — marks a page as a redirect
- Show all of these from a single file: `:path src/Pennington/FrontMatter/Capabilities.cs`

### Key points
- Capabilities are composable mix-ins, not a deep inheritance hierarchy
- Pennington's navigation builder, content pipeline, and rendering components all check for capabilities via `is` pattern matching — they never assume a concrete front matter type
- You only implement the interfaces you need; unimplemented capabilities simply have no effect

## Beat 4: Define the ChangelogFrontMatter Record

Now the reader creates the custom front matter type. Walk through the record definition step by step.

### What to show
- The reader creates a new file `ChangelogFrontMatter.cs` in their project
- The record implements `IFrontMatter`, `IDateable`, `IOrderable`, `ITaggable`, `IDescribable`
- It adds two custom properties: `string Version { get; init; } = ""` and `bool IsBreaking { get; init; }`
- Show the complete record definition (approximately 12 lines)
- Reference `T:Pennington.FrontMatter.DocFrontMatter` again as the pattern to follow — emphasize that the reader's type has the same shape but different capabilities and custom fields

### Key points
- All properties use `{ get; init; }` with sensible defaults so the record satisfies the `new()` constraint
- `Tags` defaults to `[]` (empty array), `Order` defaults to `int.MaxValue` (sorts last), `Date` defaults to `null`
- YAML property names map to record properties via camelCase convention (e.g., `isBreaking` in YAML becomes `IsBreaking` in C#)
- The `Version` and `IsBreaking` properties are not part of any Pennington interface — they are entirely custom to this use case

## Beat 5: Understand How FrontMatterParser Deserializes YAML

Explain the parser that converts YAML front matter blocks into the record type. The reader does not need to configure it, but understanding how it works helps debug issues.

### What to show
- Code reference: `T:Pennington.FrontMatter.FrontMatterParser` — show the class definition and the `Parse<T>()` method signature
- Code reference: `M:Pennington.FrontMatter.FrontMatterParser.Parse``1(System.String)` — explain the flow: extract YAML between `---` delimiters, deserialize with YamlDotNet using camelCase naming convention and `IgnoreUnmatchedProperties`
- Code reference: `T:Pennington.FrontMatter.FrontMatterResult``1` — the return type: `Metadata` (the deserialized record or null) and `Body` (the remaining markdown)
- Show an example YAML block and its mapping to the record properties:
  ```yaml
  ---
  title: "v2.1.0"
  version: "2.1.0"
  date: 2026-03-01
  order: 30
  isBreaking: true
  tags: ["api", "auth"]
  description: "Authentication token format change"
  ---
  ```

### Key points
- `IgnoreUnmatchedProperties` means extra YAML keys that do not exist on the record are silently ignored — you will not get errors from leftover fields
- Date parsing supports standard YAML date formats (ISO 8601)
- Arrays in YAML (both flow `["a", "b"]` and block `- a\n- b`) map to `string[]`

## Beat 6: Register the Custom Content Source

Show the reader how to wire the custom front matter type into their `Program.cs` via `PenningtonOptions.AddMarkdownContent<T>()`.

### What to show
- Code reference: `M:Pennington.Infrastructure.PenningtonOptions.AddMarkdownContent``1(System.Action{Pennington.Infrastructure.MarkdownContentOptions})` — show the method signature and explain the generic parameter
- Code reference: `T:Pennington.Infrastructure.MarkdownContentOptions` — show the three key properties: `ContentPath`, `BasePageUrl`, `Section`
- Show the registration code the reader adds to their `Program.cs`:
  ```csharp
  penn.AddMarkdownContent<ChangelogFrontMatter>(md =>
  {
      md.ContentPath = "Content/changelog";
      md.BasePageUrl = "/changelog";
      md.Section = "Changelog";
  });
  ```
- Reference how the DocSite template registers its own content: `:path src/Pennington.DocSite/DocSiteServiceExtensions.cs` (lines 27-31) — show the `AddMarkdownContent<DocSiteFrontMatter>` call as a real-world example

### Key points
- `ContentPath` is relative to the project's content root — Pennington resolves it against `IWebHostEnvironment.ContentRootPath` at runtime
- `BasePageUrl` determines the URL prefix for this content source (e.g., pages under `Content/changelog/` become `/changelog/v2-1-0/`)
- `Section` sets the navigation section header text
- You can register multiple content sources with different front matter types in the same site

## Beat 7: Write the Release Notes Markdown Files

The reader creates three markdown files with the custom YAML front matter. This beat focuses on content authoring, not code.

### What to show
- The directory structure: `Content/changelog/v2-1-0.md`, `Content/changelog/v2-0-1.md`, `Content/changelog/v2-0-0.md`
- Full YAML front matter for each file, demonstrating:
  - `v2-1-0.md`: `version: "2.1.0"`, `isBreaking: true`, `date: 2026-03-01`, `order: 30`, `tags: ["api", "auth"]`
  - `v2-0-1.md`: `version: "2.0.1"`, `isBreaking: false`, `date: 2026-02-15`, `order: 20`, `tags: ["cli", "bugfix"]`
  - `v2-0-0.md`: `version: "2.0.0"`, `isBreaking: true`, `date: 2026-01-10`, `order: 10`, `tags: ["api", "cli", "config"]`
- Example markdown body content for one file (release notes with a code block showing a before/after diff)

### Key points
- File names become URL slugs: `v2-1-0.md` serves at `/changelog/v2-1-0/`
- The `order` property controls the display sequence in navigation (lower numbers appear first)
- The `section` property from `MarkdownContentOptions` applies to all pages in this content source; individual pages do not need to set it in their front matter

## Beat 8: Build the Changelog Header Component

Create a Razor component that receives front matter metadata and uses pattern matching to render capability-driven and custom UI.

### What to show
- The reader creates `Components/ChangelogHeader.razor`
- The component receives an `IFrontMatter` parameter (not the concrete type)
- Pattern matching for capabilities:
  - `if (Metadata is IDateable { Date: { } date })` — render the formatted release date
  - `if (Metadata is ITaggable { Tags.Length: > 0 } taggable)` — render a `Badge` component for each tag
  - `if (Metadata is ChangelogFrontMatter changelog)` — render the version in a large Badge, and conditionally render a "Breaking Changes" warning banner when `changelog.IsBreaking` is true
- Code reference for the Badge component: `:path src/Pennington.UI/Components/Badge.razor` — show the component's parameters (`Text`, `Variant`, `Size`) and the `Variant` switch mapping ("success", "tip", "caution", "danger")
- Show the full Razor component markup (approximately 25 lines)

### Key points
- The `IFrontMatter` parameter type means this component could work with any front matter type — capabilities degrade gracefully
- Pattern matching with property patterns (`{ Date: { } date }`) safely extracts non-null values
- The concrete cast to `ChangelogFrontMatter` is only needed for the custom `Version` and `IsBreaking` properties — everything else is accessed via capability interfaces
- Badge `Variant` values map to colors: "success" = emerald, "tip" = sky, "caution" = amber, "danger" = rose

## Beat 9: Wire the Component into the Content Layout

Explain how the changelog component renders within the site's content area. The navigation sidebar picks up the "Changelog" section automatically from the registered content source.

### What to show
- Explain that when Pennington renders a page under `/changelog/`, the front matter metadata is available to the page's layout or content component
- Show how a custom page component or the existing DocSite article slot can be extended to include the `ChangelogHeader` above the rendered markdown body
- Reference the DocSiteArticle slot as an example of how the rendering pipeline passes content to a component: `:path src/Pennington.DocSite/Slots/Components/DocSiteArticle.razor` — show the `Title`, `HtmlContent`, and other parameters

### Key points
- Navigation sections are driven by the `Section` property on `MarkdownContentOptions` — Pennington groups pages under section headers automatically
- Pages are ordered within the section by the `Order` property from `IOrderable`
- The reader does not need to manually build navigation entries; Pennington's `NavigationBuilder` handles it

## Beat 10: Run and Verify

Walk the reader through running the site and verifying everything works end to end.

### What to show
- Run command: `dotnet run`
- Navigate to `/changelog/` — the sidebar shows a "Changelog" section with three entries ordered by version
- Click into `/changelog/v2-1-0/` — the page shows:
  - A large version badge ("2.1.0")
  - A red "Breaking Changes" alert banner (because `isBreaking: true`)
  - The formatted release date (March 1, 2026)
  - Tag badges for "api" and "auth"
  - The rendered markdown body with release notes
- Navigate to `/changelog/v2-0-1/` — same layout but no breaking-change banner (because `isBreaking: false`)
- Describe the visual result at each step so the reader can verify correctness

### Key points
- The pipeline does not care about the concrete front matter type — it only checks capabilities
- Adding new release notes is as simple as creating a new markdown file with the correct YAML front matter
- If the reader later adds a new capability interface to `ChangelogFrontMatter` (e.g., `ICrossReferenceable`), existing pages continue to work — they just gain the new feature

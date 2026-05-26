---
title: "Migrating to Pennington — guidance for LLMs"
description: "Pattern-oriented guide for LLMs porting a docs site from Docusaurus, VitePress, or Astro Starlight to Pennington. Names the primitives to use and the dead ends to avoid."
uid: llms.migrating-via-ai
tags: [migration, llm, internal]
---

You're porting a docs site to Pennington. This page lists what Pennington already does (so you don't rebuild it), the conventions that differ from the source system, and the decisions to escalate to the human. The named source systems are Docusaurus, VitePress, and Astro Starlight; the patterns generalise to MkDocs, Nextra, GitBook.

This page is hidden from humans — they will not see it. Treat it as an internal briefing.

## Start with `AddPennington`, not the templates

`AddDocSite` and `AddBlogSite` impose layout, color scheme, chrome, and component set. They are good defaults for greenfield sites. On a migration where the source site has its own design you want to preserve, the template will fight you.

Build the chrome yourself instead. The minimal shape:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPennington(options =>
{
    options.AddMarkdownContent<DocFrontMatter>(md => md.ContentPath = "Content");
    options.ConfigureSearch(s => s.Enabled = true);
});

var app = builder.Build();
app.UsePennington();
app.MapFallbackToPage("/_Host");
await app.RunOrBuildAsync(args);
```

Add four things alongside it:

1. `Components/Layout/MainLayout.razor` — your shell (header, sidebar slot, footer).
2. `Components/Layout/NavMenu.razor` — a `NavigationBuilder`-driven sidebar. See `examples/GettingStartedNavigationExample`.
3. `Components/Pages/_Host.razor` — a `@page "/{*slug}"` catch-all that resolves the URL through `IContentService` and renders the result.
4. `wwwroot/` — your CSS, fonts, images.

Reach for `AddDocSite` later if its chrome happens to match. It rarely does on a migration.

## The pipeline you don't rebuild

These ship in `AddPennington`. Use them; don't replace them.

| Capability | Use this | Don't write |
|---|---|---|
| Markdown → HTML | `IContentRenderer` (Markdig + Mdazor + highlighting + shortcodes) | A second markdown parser |
| Code highlighting | TextMate via `ICodeHighlighter` (TextMateSharp). ``` ```ts ``` works. | Shiki, Prism, highlight.js |
| GitHub alerts | `> [!NOTE]`, `> [!TIP]`, `> [!WARNING]`, `> [!IMPORTANT]`, `> [!CAUTION]` parse natively | A custom callout block |
| Tabbed code | `:tabs` code-fence args — see <xref:reference.markdown.code-block-args> | A `<Tabs>` component runtime |
| Inline Razor components | Mdazor parses `<Component>` directly in markdown — see <xref:how-to.rich-content.ui-components-in-markdown> | An MDX-style pre-pass |
| Cross-references | `uid:` in front matter, `<xref:uid>` or `[text](xref:uid)` in body — survives renames | Path-based internal links |
| Search | `AddSearch(...)` ships DeweySearch + `_content/DeweySearch.Web/dewey-search.js` — see <xref:how-to.discovery.search> | Algolia, lunr.js wiring |
| Sitemap, RSS, llms.txt | One line each in `AddPennington`. See <xref:how-to.feeds.sitemap>, <xref:how-to.feeds.rss>, <xref:how-to.feeds.llms-txt> | Plugin code |
| i18n | Locale subfolders under `Content/{locale}/` + `ConfigureLocalization` — see <xref:how-to.discovery.localization> | A custom translation provider |
| Front matter | Compose `IFrontMatter` + `ITaggable`/`IOrderable`/`ISectionable`/`IRedirectable` — see <xref:explanation.core.front-matter-capabilities> | A schema-validation library |
| Build-time output | `dotnet run -- build` writes static HTML to `wwwroot-build/` | A build CLI |

## Components: MDX → Mdazor

MDX lets you embed JSX in markdown. Mdazor lets you embed Razor in markdown. The mental shift is mostly mechanical.

| Source pattern | Pennington equivalent |
|---|---|
| `import { Foo } from '@/components'` at the top of `.mdx` | Drop the import. Register once in `Program.cs`: `services.AddMdazorComponent<Foo>()`. Then `<Foo>...</Foo>` works in any `.md`. |
| `<Tabs><TabItem value="js" label="JS">...</TabItem></Tabs>` (Docusaurus, Starlight) | Built-in `:tabs` code-fence for code, or port `<Tabs>` as a Razor component. See <xref:how-to.rich-content.content-tabs>. |
| `:::info`, `:::tip`, `:::warning`, `:::danger`, `:::caution` (Docusaurus, VitePress) | `> [!NOTE]`, `> [!TIP]`, `> [!WARNING]`, `> [!CAUTION]`, `> [!IMPORTANT]`. See <xref:how-to.rich-content.alerts>. |
| `<Aside type="tip">...</Aside>` (Starlight) | Same: `> [!TIP]`. |
| `::: code-group` (VitePress code groups) | `:tabs` fence. |
| `{frontmatter.title}` / `{props.x}` JSX expressions in body | Razor: `@Model.Title` from the rendered page context; `@ChildContent` inside a Mdazor component. |
| `<Image src="..." />` (Astro/Next image optimizer) | Plain `<img>`. No optimizer ships. Flag to the human if the source relied on it heavily. |
| `<LinkCard>`, `<CardGrid>` (Starlight) | Use `Pennington.UI` `Card`, `CardGrid` — see <xref:reference.ui.content> — or port the source components. |
| `<Steps>` (Nextra, Starlight) | Native ordered Markdown list. The `<Steps>` mdazor component exists if you need numbered step blocks — see <xref:reference.ui.content>. |
| `<script setup>` Vue blocks (VitePress) | Move the logic into a `.razor` component, register with Mdazor, embed by tag. |
| `defineClientComponent(() => import(...))` (VitePress hydration) | Pennington does SSR by default; for client hydration see <xref:explanation.spa.islands>. |

If the source has 30+ unique components, port structural ones first (callouts, code groups, image grids). Decorative widgets can wait — leave them as plain HTML or stub `<div>` until the human prioritises them.

## Styling: MonorailCSS (Tailwind-compatible, no npm)

MonorailCSS is a runtime CSS layer that speaks the Tailwind utility dialect — `bg-primary-500`, `text-base-900 dark:text-base-50`, `border-accent-200`, `flex items-center gap-4`. Most Tailwind utility markup in the source survives the move untouched. See <xref:explanation.rendering.monorail-css>.

No build step required. No `npm`, no `tailwind.config.js`, no `postcss`. MonorailCSS scans compiled IL for class literals at runtime and serves `/styles.css` on demand; the first time a utility appears anywhere in the loaded assemblies it shows up in the stylesheet.

Two conventions to learn:

- **Semantic palette slots, not raw colors.** Use `bg-primary-500`, `text-accent-200`, `border-base-300` — not `bg-blue-500`. Map the brand once at the scheme level; every utility resolves through the slots.
- **Dark mode is `dark:` variants on a `scheme-dark` root wrapper.** Same authoring as Tailwind.

### Plug in (bare host)

```csharp
builder.Services.AddPennington(opts => { /* ... */ });
builder.Services.AddMonorailCss(opts =>
{
    opts.ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorName.Indigo,
        AccentColorName = ColorName.Pink,
        BaseColorName = ColorName.Slate,
    };
});

app.UseMonorailCss();   // maps GET /styles.css
```

Link the stylesheet from `MainLayout.razor`:

```html
<link rel="stylesheet" href="/styles.css" />
```

For a single-hue brand, swap `NamedColorScheme` for `AlgorithmicColorScheme { PrimaryHue = 214, Chroma = 0.15, CoordinatingScheme = CoordinatingScheme.SplitComplementary }` — the whole palette derives from one number.

### What to bring from the source

| Source artifact | Pennington equivalent |
|---|---|
| `tailwind.config.js` custom colors | `NamedColorScheme` or `AlgorithmicColorScheme` — translate, don't port the file |
| Tailwind `@apply` directives in CSS | `MonorailCssOptions.ExtraStyles` (verbatim CSS), or `CustomCssFrameworkSettings.Applies` for utility-style apply rules |
| Custom fonts via `@font-face` | Drop the block in `ExtraStyles`; copy the font files to `wwwroot/fonts/`. See <xref:how-to.theming.fonts>. |
| Tailwind typography plugin (`prose`) | Already baked in — prose styles ship by default and can be tuned via `CustomCssFrameworkSettings` |
| Other Tailwind plugins (forms, aspect-ratio, etc.) | Re-author the relevant rules in `ExtraStyles` |
| Syntax-highlight theme (Shiki / Prism CSS) | `MonorailCssOptions.SyntaxTheme` — five token roles (keyword, string, variable, function, comment) mapped to named palettes |
| Plain CSS files referenced from HTML | Drop in `wwwroot/`, link from `MainLayout.razor` — no transform needed |

Full customization surface: <xref:how-to.theming.monorail-css>.

## Front matter: bulk-rewrite with a script

Source systems use different front-matter keys. Don't hand-edit 100+ files. Walk the tree with Python.

Common renames:

| Source key | Pennington |
|---|---|
| `sidebar_position`, `position`, `weight`, `nav_order` | `order` |
| `sidebar_label` | `title` (and the original `title:` becomes the H1 / page header) |
| `draft`, `published: false` | `isDraft: true` |
| `slug:` (Docusaurus, Starlight) | drop — file path drives the URL |
| `id:` (Docusaurus) | drop, or promote to `uid:` if you want xref support |
| `hide_table_of_contents`, `toc: false` | drop — no per-page toggle today (flag to human) |
| `pagination_next`, `pagination_prev` | drop — derived from sidebar order |
| `keywords`, `categories` | merge into `tags` |
| `image:`, `cover:`, `head.image` (VitePress) | `imageUrl:` |
| `layout: home` (VitePress hero), `template: splash` (Starlight) | route through a custom Razor `@page` instead of trying to express it in front matter |

A sketch — adapt the rename map to your source:

```python
import re, sys
from pathlib import Path

FM = re.compile(r"^---\r?\n(.*?)\r?\n---\r?\n", re.DOTALL)
RENAME = {
    "sidebar_position": "order",
    "draft": "isDraft",
    "keywords": "tags",
}
DROP = {"slug", "id", "hide_table_of_contents", "pagination_next", "pagination_prev"}

for path in Path(sys.argv[1]).rglob("*.md"):
    text = path.read_text(encoding="utf-8")
    m = FM.match(text)
    if not m: continue
    lines = []
    for raw in m.group(1).splitlines():
        if ":" not in raw or raw.startswith(" "): lines.append(raw); continue
        key, val = raw.split(":", 1)
        if key in DROP: continue
        lines.append(f"{RENAME.get(key, key)}:{val}")
    path.write_text(f"---\n" + "\n".join(lines) + "\n---\n" + text[m.end():], encoding="utf-8")
```

This is a sketch. `scripts/migrate_docs_ordering.py` (in the Pennington repo) is the more careful reference — verifies before/after navigation flatten matches leaf-for-leaf, handles nested YAML, preserves line endings.

## Ordering: folder-local with `_meta.yml`

Source systems express sidebar order with one of: a JS/TS config (`sidebars.js`, `astro.config.mjs`, `.vitepress/config.ts`), per-folder JSON (`_category_.json`, `_meta.json`), or per-file numeric positions.

Pennington uses:

- Per-file `order: <small int>` in front matter — folder-local 1, 2, 3, not globally unique.
- Optional `_meta.yml` per folder declaring the folder's own position and display title. Schema: <xref:reference.front-matter.folder-sidecar>.

Conversions:

```json
// Docusaurus _category_.json
{"label": "Tutorials", "position": 1, "collapsible": true}
```
```yaml
# Pennington Content/tutorials/_meta.yml
title: Tutorials
order: 1
```

```js
// VitePress .vitepress/config.ts
sidebar: [{ text: 'Guide', items: [{ text: 'Intro', link: '/intro' }] }]
```

Delete the VitePress config. The folder structure under `Content/` IS the sidebar. Use `_meta.yml` only where you need to override a folder's title or position.

```js
// Astro Starlight sidebar in astro.config.mjs
sidebar: [{ label: 'Start', autogenerate: { directory: 'start' } }]
```

Drop it. Pennington's discovery is already `autogenerate: { directory: 'start' }` for every folder.

Don't translate a sidebar config into a Pennington nav config. There isn't one. The filesystem is the source of truth.

## When the filesystem isn't enough — write an `IContentService`

Two patterns where forcing things into a markdown directory will hurt:

- **Versioned docs (`/v1/`, `/v2/`):** Multiple `AddMarkdownContent<T>` calls with different roots, or multiple `ContentArea` entries. See `examples/VersionedDocSiteExample` and <xref:how-to.versioning.docsite>.
- **Generated reference** (API docs from a compiled assembly, OpenAPI dumps, type catalogs, taxonomy pages): Implement `IContentService` directly. See <xref:how-to.content-services.custom-content-service> and `examples/ExtensibilityLabExample`. For API metadata specifically, `Pennington.ApiMetadata.Reflection` already does this — see <xref:how-to.content-services.auto-api-reference>.

Write the service. Don't try to model these as a markdown directory tree.

## Don't do these

- Don't write a custom Markdown renderer. Configure Markdig via `options.ConfigureMarkdownPipeline`. To add a new fenced-block handler, see <xref:how-to.markdown-pipeline.code-block-preprocessor>.
- Don't write a `getStaticPaths` equivalent. File discovery is automatic via `MarkdownContentService`.
- Don't wire Algolia or lunr.js. `AddSearch()` ships DeweySearch.
- Don't preprocess MDX before the pipeline sees it. Mdazor parses `<Component>` inline; just register the component once.
- Don't write a build CLI. `dotnet run -- build` writes `wwwroot-build/`.
- Don't model versions as nested folders inside a single area.
- Don't carry framework-specific front-matter keys (`hide_table_of_contents`, `pagination_label`, `displayed_sidebar`) — they're inert.
- Don't add `// removed for Pennington` comments. Delete code and move on.
- Don't pre-stagger `order:` values like `10, 20, 30` across all folders globally. Use folder-local `1, 2, 3` plus `_meta.yml`.
- Don't add a sidebar config file. There is no sidebar config file.
- Don't add `npm`, `postcss`, or any Node toolchain for CSS. MonorailCSS generates the stylesheet at runtime from compiled IL.
- Don't author a `tailwind.config.js`. The configuration surface is `MonorailCssOptions` in C#.
- Don't reach for raw Tailwind color names (`bg-blue-500`). Use the semantic slots — `bg-primary-500`, `bg-accent-200`, `bg-base-50`.

## When to escalate to the human

Stop and ask, don't guess:

- The source site has more than ~5 custom components. Ask which to port first.
- The source has a custom design system or CSS framework. Ask whether to port to MonorailCSS or carry the existing CSS.
- Image optimization was load-bearing. Pennington doesn't ship one — confirm whether to add or accept plain `<img>`.
- Authentication-gated docs. Not yet a Pennington feature; flag it.
- Multi-version docs with cross-version xrefs. Confirm whether cross-version links are required.
- Live editing / instant previews specific to the source platform. Pennington has hot-reload (<xref:explanation.dev-experience.hot-reload>) but not the same model.
- Custom JSX widgets that drive interactive UI (search-augmented filters, live API explorers). Ask whether to port as a hydrated island (<xref:explanation.spa.islands>) or as a plain Razor component.

## Read these when you need depth

Fetch these pages — most are linked above too:

- <xref:tutorials.getting-started.first-site> — smallest working Pennington site
- <xref:tutorials.getting-started.navigation> — custom NavMenu shape using `NavigationBuilder`
- <xref:reference.front-matter.keys> — every recognised front-matter key, per record
- <xref:reference.front-matter.folder-sidecar> — `_meta.yml` schema
- <xref:explanation.routing.navigation-tree> — how the sidebar tree is built
- <xref:explanation.routing.cross-references> — how `uid:` / `<xref:>` resolution works
- <xref:explanation.core.content-pipeline> — the four-case union and how content flows
- <xref:explanation.core.front-matter-capabilities> — the capability-interface design
- <xref:how-to.content-services.custom-content-service> — when to write your own service
- <xref:how-to.markdown-pipeline.code-block-preprocessor>, <xref:how-to.markdown-pipeline.custom-highlighter>, <xref:how-to.markdown-pipeline.shortcodes> — extending Markdig
- <xref:how-to.discovery.search>, <xref:how-to.discovery.localization>, <xref:how-to.discovery.multiple-sources> — discovery features
- <xref:reference.markdown.extensions>, <xref:reference.markdown.code-block-args> — markdown surface
- <xref:reference.host.extensions>, <xref:reference.host.cli> — host wiring and CLI
- `examples/GettingStartedNavigationExample` — minimum viable nav menu
- `examples/DocSiteKitchenSinkExample` — broad surface for picking patterns
- `examples/ExtensibilityLabExample` — custom highlighter / preprocessor / IContentService / response processor in one project
- `examples/VersionedDocSiteExample` — two-version layout

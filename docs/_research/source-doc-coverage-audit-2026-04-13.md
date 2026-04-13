# Source vs Docs Coverage Audit — 2026-04-13

Scope:
- Source reviewed: `src/`
- Docs reviewed: `docs/Pennington.Docs/Content` and `docs/docs-toc.md`
- Excluded: `examples/`

## Summary

The docs tree covers most of the major architecture areas, but it does **not** yet cover the shipped feature set cleanly.

Main issues:
- **One runtime/documentation mismatch:** BlogSite uses `BlogSiteFrontMatter`, but several docs still teach or catalog `BlogFrontMatter` instead.
- **Several real feature pages are missing entirely.**
- **The docs tree has heavy naming/link drift:** `110` broken internal links and `8` TOC URLs that do not map to actual pages.

## Findings

### 1. BlogSite front matter is documented against the wrong type

Severity: high

Source:
- `src/Pennington.BlogSite/BlogSiteServiceExtensions.cs`
- `src/Pennington.BlogSite/BlogSiteFrontMatter.cs`
- `src/Pennington.BlogSite/Components/Layout/BlogPost.razor`

Docs affected:
- `docs/Pennington.Docs/Content/tutorials/blogsite/first-post.md`
- `docs/Pennington.Docs/Content/reference/front-matter/built-in-types.md`
- `docs/Pennington.Docs/Content/reference/front-matter/keys.md`

What the source does:
- `AddBlogSite(...)` wires blog markdown through `AddMarkdownContent<BlogSiteFrontMatter>(...)`, not `BlogFrontMatter`.
- `BlogSiteFrontMatter` includes fields the current docs do not catalog, especially:
  - `Repository`
  - `RedirectUrl`
  - `Section`
- `Repository` is not theoretical: it is rendered in `Components/Layout/BlogPost.razor`.

Why this matters:
- A reader following the BlogSite tutorial learns the wrong front-matter record.
- The built-in front-matter reference omits the type actually used by BlogSite.
- The key reference omits `repository`, so the shipped post UI has a real feature with no key-level documentation.

Recommended fix:
- Replace BlogSite-facing uses of `BlogFrontMatter` with `BlogSiteFrontMatter`.
- Add `BlogSiteFrontMatter` to the built-in-type reference.
- Add `repository` to the key reference and document how it renders on blog posts.

### 2. UI components are promised but not actually documented

Severity: high

Source:
- `src/Pennington.UI/Components/Card.razor`
- `src/Pennington.UI/Components/CardGrid.razor`
- `src/Pennington.UI/Components/LinkCard.razor`
- `src/Pennington.UI/Components/Badge.razor`
- `src/Pennington.UI/Components/Step.razor`
- `src/Pennington.UI/Components/Steps.razor`
- `src/Pennington.UI/Components/CodeBlock.razor`
- `src/Pennington.UI/Components/BigTable.razor`

TOC promises:
- `/how-to/content-authoring/ui-components-in-markdown`
- `/reference/ui/content`

Current state:
- Neither page exists under `docs/Pennington.Docs/Content`.
- `BigTable` is public and shipped, but it is not even mentioned in `docs/docs-toc.md`.

Why this matters:
- Pennington ships a reusable UI component surface, but there is no user-facing doc page for either “how to use these in markdown” or “what parameters each component supports”.
- The TOC already tells readers these pages exist, so this is both a coverage miss and a navigation miss.

Recommended fix:
- Add the two promised pages.
- Decide whether `BigTable` is intended public surface. If yes, add it to both the TOC and the reference page; if not, make it internal.

### 3. Cross-reference authoring is missing, even though the feature exists and the explanation assumes it

Severity: high

Source:
- `src/Pennington/Infrastructure/XrefResolver.cs`
- `src/Pennington/Infrastructure/XrefHtmlRewriter.cs`
- `src/Pennington/Infrastructure/XrefResolvingService.cs`

Docs:
- Explanation exists: `docs/Pennington.Docs/Content/explanation/routing/cross-references.md`
- Reference support exists indirectly through response-processing docs
- Missing promised page: `/how-to/content-authoring/cross-references`

Why this matters:
- The conceptual “why/how it works” page exists, but the task-oriented “how do I author one” page does not.
- Other docs already link to the missing how-to.

Recommended fix:
- Add the missing how-to page for `uid:` and `xref:` authoring.

### 4. `ICodeBlockPreprocessor` is public and promised, but the how-to page is missing

Severity: high

Source:
- `src/Pennington/Markdown/Extensions/ICodeBlockPreprocessor.cs`
- `src/Pennington.Roslyn/Preprocessing/RoslynCodeBlockPreprocessor.cs`

TOC promise:
- `/how-to/extensibility/code-block-preprocessor`

Current state:
- No such page exists under `Content`.

Why this matters:
- This is a real extension point with a shipped production implementation (`RoslynCodeBlockPreprocessor`).
- The highlighting/reference docs mention it, but there is no task page for building one.

Recommended fix:
- Add the missing how-to page and cross-link it from code annotations and highlighting docs.

### 5. Several user-facing reference pages are missing even though other docs already link to them

Severity: medium

Missing pages with source backing:

- Structured-data types
  - Source: `src/Pennington/StructuredData/JsonLdTypes.cs`
  - Linked from: `reference/ui/utility.md`
  - Missing URL: `/reference/structured-data/types`

- Translation/localizer reference
  - Source: `src/Pennington/Localization/TranslationOptions.cs`
  - Source: `src/Pennington/Localization/PenningtonStringLocalizer.cs`
  - `reference/options/pennington-options.md` says this is documented on its own page
  - No such page exists

- Roslyn options reference
  - Source: `src/Pennington.Roslyn/RoslynOptions.cs`
  - `reference/host/extensions.md` points readers toward a `RoslynOptions` reference page
  - No such page exists

- SocialIcons built-ins reference
  - Source: `src/Pennington.BlogSite/Components/SocialIcons.razor`
  - `how-to/configuration/blogsite-socials.md` links to `/reference/blogsite-social-icons`
  - No such page exists

Why this matters:
- These are not speculative features. Other docs already treat them as documented surfaces.

### 6. Built-in BlogSite routes are only partially documented

Severity: medium

Source:
- `src/Pennington.BlogSite/Components/Pages/Archive.razor`
- `src/Pennington.BlogSite/Components/Pages/Tags.razor`
- `src/Pennington.BlogSite/Components/Pages/Tag.razor`

Observed routes:
- `/archive`
- `/tags`
- `/topics`
- `/tags/{TagEncodedName}`
- `/topics/{TagEncodedName}`

Current docs:
- `TagsPageUrl` is documented on `BlogSiteOptions`
- `/tags` is mentioned in the BlogSite scaffold tutorial
- `/archive` is only mentioned incidentally in a hero example string

Why this matters:
- These are built-in pages in the shipped blog template, not app-specific examples.
- Readers have no single page that explains what routes the BlogSite template gives them by default.

Recommended fix:
- Add a short reference page or expand BlogSite reference/tutorial docs to explicitly list built-in routes and their defaults.

### 7. The docs tree has substantial naming drift and broken internal links

Severity: high

Measured state:
- `110` broken internal links in `docs/Pennington.Docs/Content`
- `8` URLs promised by `docs/docs-toc.md` that do not exist as pages

Missing TOC pages:
- `/how-to/content-authoring/cross-references`
- `/how-to/content-authoring/ui-components-in-markdown`
- `/how-to/extensibility/code-block-preprocessor`
- `/reference/ui/content`
- `/tutorials/beyond-basics/custom-razor-component`
- `/tutorials/docsite/sections-and-areas`
- `/tutorials/getting-started/deploy-github-pages`
- `/tutorials/getting-started/styling`

Patterns in the broken links:
- Old section names such as `reference/generation/*`, `reference/api/*`, `reference/infrastructure/*`
- Old page names such as `blog-site-options`, `doc-site-options`, `monorailcss-options`
- Links to planned pages that never landed

Why this matters:
- Even when a feature is documented, readers may not be able to reach the page from neighboring docs.

## Recommended fix order

1. Correct the BlogSite front-matter docs (`BlogSiteFrontMatter`, `repository`, key tables, tutorial wording).
2. Add the missing UI component pages:
   - `/how-to/content-authoring/ui-components-in-markdown`
   - `/reference/ui/content`
3. Add the missing cross-reference authoring page.
4. Add the missing `ICodeBlockPreprocessor` how-to.
5. Add missing reference pages for:
   - structured-data types
   - translation/localizer
   - Roslyn options
   - SocialIcons built-ins
6. Sweep and fix internal links and stale URLs.

## Bottom line

The architecture docs are in decent shape, but the current docs do **not** fully cover the shipped source surface.

The most important misses are:
- BlogSite front matter is misdocumented
- UI components are shipped but effectively undocumented
- Cross-reference authoring and code-block preprocessors are shipped but missing task docs
- Several reference pages are assumed to exist by other docs, but do not exist

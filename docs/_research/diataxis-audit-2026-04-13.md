# Diataxis Audit — 2026-04-13

Scope: `docs/Pennington.Docs/Content`

Standard used for this pass:
- `docs/_research/diataxis-primer.md`
- The per-quadrant templates under `docs/Pennington.Docs/Content/*/_template.md`

## Status summary

- **Aligned and ready to expand:** the current outline set.
- **Needs tightening before expansion:** none after this pass.

## Quadrant review

### Tutorials

Aligned at outline level:
- `tutorials/getting-started/first-site.md`
- `tutorials/blogsite/scaffold.md`
- `tutorials/blogsite/first-post.md`
- `tutorials/blogsite/hero-projects-socials.md`
- `tutorials/beyond-basics/connect-roslyn.md`

Needs tightening before expansion:
- None after this pass.

### How-to Guides

Aligned at outline level:
- All pages under `how-to/content-authoring/`
- All pages under `how-to/deployment/`
- All pages under `how-to/extensibility/`
- `how-to/configuration/blogsite-hero.md`
- `how-to/configuration/blogsite-projects.md`
- `how-to/configuration/blogsite-socials.md`
- `how-to/configuration/fonts.md`
- `how-to/configuration/llms-txt.md`
- `how-to/configuration/localization.md`
- `how-to/configuration/monorail-css.md`
- `how-to/configuration/multiple-sources.md`
- `how-to/configuration/search.md`

Needs tightening before expansion:
- None after this pass.

### Explanations

Aligned at outline level:
- All pages under `explanation/`

Notes:
- The explanation quadrant is the cleanest part of the tree.
- `explanation/localization/urls-and-fallback.md` is dense but still properly explanation-shaped because it answers a single why/how-it-fits question and keeps the trade-offs explicit.

### Reference

Aligned at outline level:
- All pages under `reference/`

Needs tightening before expansion:
- None after this pass.

## Edit priorities

Priority 1:
- None after this pass.

Priority 2:
- None after this pass.

## Expansion rule for the next pass

Before expanding any page above, fix the outline first:
- Tutorials: remove architecture detours, keep one happy path, and keep the learner focused on the visible artifact.
- How-to guides: replace option-catalog bullets with action-first recipe bullets.
- Reference: remove "when should I use this" language and keep the page lookup-shaped.

## Patched in this pass

- `tutorials/docsite/first-doc-page.md`
- `tutorials/docsite/scaffold.md`
- `tutorials/getting-started/first-page.md`
- `tutorials/beyond-basics/add-a-locale.md`
- `how-to/configuration/blogsite.md`
- `how-to/configuration/docsite-options.md`
- `how-to/configuration/pennington-options.md`
- `reference/ui/utility.md`
- `reference/front-matter/built-in-types.md`

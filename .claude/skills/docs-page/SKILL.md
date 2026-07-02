---
name: docs-page
description: Author or revise a page on the Pennington docs site (docs/Pennington.Docs/Content). Use when writing or editing tutorials, how-to guides, reference pages, or explanations — picks the Diataxis quadrant, applies the docs voice, and wires front matter, xref links, and :symbol code embedding correctly.
---

# Write a docs page

Two canonical references — read both before writing:
- `docs/Pennington.Docs/CLAUDE.md` — quadrant rules, formatting, symbol-fence syntax, writing conventions
- `docs/docs-voice.md` — the full voice guide

This skill is the workflow around them.

## Workflow

1. **Pick exactly one Diataxis quadrant** (tutorial / how-to / reference / explanation) and obey its register. If a draft mixes quadrants — for example a how-to that walks a feature member-by-member — that content belongs in reference; split or re-frame around the reader's goal.
2. **Place the file** under the matching area in `docs/Pennington.Docs/Content/`. Folder ordering comes from `_meta.yml` sidecars with small folder-local `order` values (`1, 2, 3`), not wide global numbers.
3. **Front matter:** give the page a `uid` — other pages link to it via `[text](xref:uid)` / `<xref:uid>`, never hardcoded URL paths. No H1 in the body; the engine adds it from `title`.
4. **Embed code from `examples/`, don't hand-write it,** using `:symbol` fences (whole file, `path > Type.Member`, `,bodyonly`, `,imports`, `,signatures`, `:symbol-diff`). Full syntax and the when-to-use-which table are in `docs/Pennington.Docs/CLAUDE.md`. If the snippet you need doesn't exist in an example, extend the example (see the `new-example` skill) rather than inlining unverifiable code.
5. **Final polish:** delete AI scaffolding blockquotes (`> **In this page.** …`), check the never-use word list ("simply", "just", "easy", "please", Latin abbreviations), sentence-case headings, at most one or two admonitions.
6. **Verify:** `dotnet run --project docs/Pennington.Docs -- diag warnings` must come back clean (it catches broken xrefs and fences); `diag toc` to confirm the page landed where intended in the nav. For a new page, also check it renders: `dotnet run --project docs/Pennington.Docs` and fetch the route.

## Quadrant quick test

- Reader wants to *learn by doing* → tutorial (warm, every step shows a visible result)
- Reader has a *problem to solve now* → how-to (direct; title names the outcome, not the feature)
- Reader is *looking something up* → reference (neutral, systematic, one sentence per entry)
- Reader wants to *understand why* → explanation (discursive, tradeoffs, links to how-to for action)

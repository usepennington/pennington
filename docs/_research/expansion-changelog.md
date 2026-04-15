# Doc Expansion Changelog

Expanding italicized writing-instruction blocks in `docs/Pennington.Docs/Content/` into final prose, matching the Diataxis quadrant voice defined in `docs/docs-voice.md`.

Started 2026-04-14. One line per file. Retry notes are terse. `NEEDS REVIEW` means two retries failed and the file wants hand-triage.

## Round 22 — 2026-04-14 — **All quadrants complete**

- how-to/extensibility/island-renderer.md — PASS (6 steps; writer on Opus after Sonnet rate limit)
- how-to/extensibility/override-docsite-components.md — PASS (7 steps)
- how-to/extensibility/response-processor.md — PASS (5 steps) → **How-to 33/33**

## Final verification — 2026-04-14

- Italicized writing-instruction blocks remaining in Content/: **0 in content files** (39 matches across the 4 `_template.md` scaffolding files only, which are out of scope).
- Leading `> **In this page.** / > **Not in this page.**` callouts in content files: **0** (4 matches in templates, preserved as scaffold patterns).
- `dotnet build Pennington.slnx` — failed only on DLL file locks because the docs site (PID 65504) was running and serving the site live; the errors are `MSB3027 / MSB3021 "file locked"`, not code or YAML parse errors. All front matter is valid.

**Final tally: 86/86 content files expanded across 22 rounds.**

## Intro pass — 2026-04-14

Added a 1–2 sentence reader-facing intro paragraph between the YAML `---` and the first `##` heading on every content file (including the previously-complete `reference/options/pennington-options.md`). Four parallel Opus agents, one per quadrant. Voice matches the quadrant:

- Tutorials (12): second-person, forward-pointing ("You'll stand up...", "Let's give your host...").
- How-to (33): task-framed one-liners ("Drop GitHub-style alert blocks...", "Host a subpath deploy..."). One file (`deployment/self-host.md`) had existing pre-heading prose that was tightened rather than appended to.
- Reference (29, including `pennington-options.md`): tight descriptive fragments ("Complete property reference for X...", "Every key Pennington understands in page front matter.").
- Explanation (13): question-framing single sentence ("Why Pennington models each page as a four-case union..."). `routing/url-paths.md` had a pre-heading pointer paragraph; the new question-framing intro was inserted before it, preserving the original.
- Tutorials: 12/12 ✅
- Explanation: 13/13 ✅
- Reference: 29/29 ✅
- How-to: 33/33 ✅

## Round 21 — 2026-04-14

- how-to/deployment/static-build.md — PASS (5 steps)
- how-to/extensibility/html-rewriter.md — PASS (6 steps)
- reference/ui/navigation.md — PASS (18 cells; retry 1: removed reader-visible `- Background: TODO —` line in See also)
- reference/ui/utility.md — PASS (12 blocks)

## Round 20 — 2026-04-14

- how-to/deployment/github-pages.md — PASS (6 steps)
- how-to/deployment/self-host.md — PASS (5 steps)
- reference/options/roslyn-options.md — PASS (only the callout needed stripping; rest already clean)
- reference/ui/content.md — PASS (13 blocks)

## Round 19 — 2026-04-14

- how-to/content-authoring/tabbed-code.md — PASS (4 steps)
- how-to/content-authoring/ui-components-in-markdown.md — PASS (5 steps)
- reference/markdown/code-block-args.md — PASS (5 blocks)
- reference/structured-data/types.md — PASS (17 blocks)

## Round 18 — 2026-04-14

- how-to/content-authoring/linking.md — PASS (6 steps)
- how-to/content-authoring/redirects.md — PASS (4 steps)
- reference/front-matter/keys.md — PASS (14 table cells)
- reference/host/extensions.md — PASS (18 blocks)

## Round 17 — 2026-04-14

- how-to/configuration/sitemap.md — PASS (4 steps)
- how-to/content-authoring/images-and-assets.md — PASS (5 steps; 1 TODO on ExcludePaths xmldocid)
- reference/options/markdown-content-options.md — PASS (5 blocks)
- reference/options/translations.md — PASS (7 blocks; converted "Not in this page" info to a reader-facing Note blockquote)

## Round 16 — 2026-04-14

- how-to/configuration/search.md — PASS (4 steps)
- how-to/content-authoring/front-matter.md — PASS (5 steps; 1 TODO on AddMarkdownContent<T> step)
- reference/options/localization-options.md — PASS (8 blocks; fixed stray double-backtick typo)
- reference/options/monorail-css-options.md — PASS (8 blocks)

## Round 15 — 2026-04-14

- how-to/configuration/rss.md — PASS (4 steps)
- how-to/content-authoring/drafts-tags-ordering.md — PASS (3 steps)
- reference/host/cli.md — PASS (7 blocks)
- reference/markdown/extensions.md — PASS (18 blocks)

## Round 14 — 2026-04-14

- how-to/configuration/monorail-css.md — PASS (5 steps)
- how-to/configuration/multiple-sources.md — PASS (6 steps)
- reference/extension-points/response-processing.md — PASS (8 blocks)
- reference/extension-points/routing.md — PASS (17 blocks; one `e.g.` kept inside a backtick-quoted value example, acceptable)

## Round 13 — 2026-04-14 — **Explanation quadrant complete**

- how-to/configuration/localization.md — PASS (5 steps; 1 TODO xmldocid on inline `AddPennington` config with no single addressable symbol)
- how-to/content-authoring/diagrams.md — PASS (3 steps; 1 TODO carried over for JS-only `MermaidManager`)
- reference/extension-points/navigation.md — PASS (7 blocks)
- explanation/spa/islands.md — PASS (~750 words) → **Explanation 13/13**

## Round 12 — 2026-04-14 — **Tutorial quadrant complete**

- tutorials/beyond-basics/custom-razor-component.md — PASS (12 blocks) → **Tutorials 12/12**
- how-to/extensibility/custom-highlighter.md — PASS (6 steps)
- reference/front-matter/ifrontmatter.md — PASS (10 blocks; converted one lingering `[text](xref:...)` link to `<xref:uid>` inline form)
- explanation/core/response-processing.md — PASS (~850 words)

## Round 11 — 2026-04-14

- tutorials/beyond-basics/add-a-locale.md — PASS (13 blocks)
- how-to/content-authoring/customize-sidebar.md — PASS (4 steps; 1 TODO xmldocid on section-root proxy)
- reference/extension-points/islands.md — PASS (8 blocks)
- explanation/routing/url-paths.md — PASS (~750 words; filled external TODO with the canonical "Parse, don't validate" essay)

## Round 10 — 2026-04-14

- tutorials/blogsite/hero-projects-socials.md — PASS (9 blocks)
- how-to/extensibility/custom-content-service.md — PASS (7 steps)
- reference/options/blogsite-options.md — PASS (7 intros + 20 table cells)
- explanation/routing/navigation-tree.md — PASS (~1050 words)

## Round 9 — 2026-04-14

- tutorials/docsite/sections-and-areas.md — PASS (9 blocks + AI-reminder stripped)
- how-to/deployment/base-url.md — PASS (4 steps; left `<!-- TODO: xmldocid needed -->` on BaseUrlHtmlRewriter)
- reference/extension-points/highlighting.md — PASS (8 blocks; upgraded CodeBlockPreprocessResult table to the standard schema)
- explanation/core/front-matter-capabilities.md — PASS (~670 words)

## Round 8 — 2026-04-14

- tutorials/getting-started/styling.md — PASS (13 blocks + AI-reminder stripped)
- how-to/configuration/llms-txt.md — PASS (6 blocks; 4 steps)
- reference/diagnostics/request-context.md — PASS (12 blocks)
- explanation/core/docsite-positioning.md — PASS (10 blocks, ~820 words; dropped hardcoded ExtensibilityLabExample GitHub link — replaced with xref-friendly prose)

## Round 7 — 2026-04-14

- tutorials/docsite/first-doc-page.md — PASS (7 blocks + AI-reminder stripped)
- how-to/content-authoring/cross-references.md — PASS (7 blocks; 4 steps)
- reference/blogsite/social-icons.md — PASS (6 blocks; retry 1: tightened two table cells where icon description drifted into behavior narrative already covered by intro)
- explanation/rendering/monorail-css.md — PASS (13 blocks + AI-reminder stripped, ~950 words)

## Round 6 — 2026-04-14

- tutorials/blogsite/first-post.md — PASS (9 blocks)
- how-to/configuration/fonts.md — PASS (5 blocks; verifier hallucinated an "etc." that isn't in the file)
- reference/options/auxiliary-options.md — PASS (18 blocks)
- explanation/core/dev-vs-build.md — PASS (5 blocks, ~750 words; retry 1: softened "do not propose designs" imperative to a collegial observation)

## Round 5 — 2026-04-14

- tutorials/getting-started/first-page.md — PASS (13 blocks + trailing auto-nav reminder stripped)
- how-to/content-authoring/code-annotations.md — PASS (7 blocks; 5 steps)
- reference/front-matter/built-in-types.md — PASS (8 block groups)
- explanation/routing/cross-references.md — PASS (5 block groups, ~820 words)

## Round 4 — 2026-04-14

- tutorials/beyond-basics/connect-roslyn.md — PASS (22 blocks)
- how-to/extensibility/code-block-preprocessor.md — PASS (6 blocks + callout; 5 steps)
- reference/extension-points/content-pipeline.md — PASS (10 blocks)
- explanation/rendering/highlighting.md — PASS (11 blocks, ~1050 words)

## Round 3 — 2026-04-14

- tutorials/docsite/scaffold.md — PASS (callout stripped; flagged `xref:explanation.core.docsite-positioning` uncertain but that uid exists)
- how-to/deployment/adapt-for-other-hosts.md — PASS (8 blocks; 6 steps)
- reference/diagnostics/build-report.md — PASS (15 blocks; callout stripped)
- explanation/localization/urls-and-fallback.md — PASS (11 blocks, ~1215 words)

## Round 2 — 2026-04-14

- tutorials/blogsite/scaffold.md — PASS (callout stripped; fixed invalid xref `explanation.generation.dev-vs-build` → `explanation.core.dev-vs-build`)
- how-to/configuration/blogsite-homepage.md — PASS (5 blocks; callout stripped)
- reference/blogsite/routes.md — PASS (15 blocks; callout stripped; verifier false-positive on italicized link text in See-also — matches existing convention in fleshed-out files like `how-to/configuration/fonts.md`)
- explanation/dev-experience/hot-reload.md — PASS (8 blocks, ~700 words; retry 1: removed forbidden "just" adverb on line 66)

## Round 1 — 2026-04-14

- tutorials/getting-started/first-site.md — PASS (23 blocks resolved; verifier nit on post-fence `UsePennington` gloss, judged acceptable under the "one sentence of context" voice rule)
- how-to/content-authoring/alerts.md — PASS (11 blocks; verifier false-positive flagged the repo-standard "In this page / Not in this page" callout)
- reference/options/docsite-options.md — PASS (28 blocks, mostly property-table cells; verifier false-positive on the same callout + on-spec single-sentence Example framing)
- explanation/core/content-pipeline.md — PASS (12 blocks; ~870 words)

**Calibration (user correction):** the `> **In this page.** / > **Not in this page.**` leading callout is AI writing scaffolding, not reader-facing content, and must be stripped during expansion. Round 1 callouts removed from all four files above after the fact; also removed from the previously-"complete" `reference/options/pennington-options.md`. The first `##` section of each page (`## What you'll do` / `## When to use this` / `## Summary` / `## The question`) serves as the reader-facing intro — no pre-heading intro prose needed.

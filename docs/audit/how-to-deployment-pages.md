### docs/Pennington.Docs/Content/how-to/deployment/github-pages.md
**Form claimed:** how-to | **Actual:** how-to with tutorial drift — outcome title is correct but the body adds a lot of teaching framing.

- [voice] Chatty/editorial register over the how-to bar — quote: "feels approachable" (Assumptions bullet 4) and "the teaching surface; the rest of the example is outside scope here."
- [voice] Editorializing instead of stating the action — quote: "Drop in the canonical workflow" (Step 2 heading) — "canonical" is voice-weight that adds nothing.
- [voice] Condition-after-instruction in step 5 — "For sites at an org-level root or a custom apex domain, replace the `BASE_URL` env…" is correct shape, but Step 3's "For repos that host multiple buildable projects, add `actions/cache@v4`…" mixes a side-recipe into the primary instruction.
- [diataxis] Step 6 is labeled "(Optional)" and teaches CI semantics rather than completing the goal — belongs as a sub-bullet, a separate how-to, or in the related Explanation.
- [diataxis] Step 4 ("Keep `.nojekyll` in the artifact") teaches *why* Jekyll strips underscore paths — that paragraph is conceptual; trim to one sentence and link out.
- [clarity] The reader is told to "Commit the YAML below" but the YAML is embedded via `:path` reference — confirm the rendered output shows the file inline; if it doesn't, the step has no copy target.
- [clarity] "Assumptions" section runs four bullets and ends with a meta-paragraph about the example repo; the actual goal of the page is never restated after the intro before Step 1.
- [Q] Should Step 6 be split into a separate how-to (e.g., "Fail CI on broken links") so this page stops at the green deploy?

### docs/Pennington.Docs/Content/how-to/deployment/self-host.md
**Form claimed:** how-to | **Actual:** how-to — title is outcome-shaped and steps deliver, but several steps drift into teaching.

- [voice] Chatty asides — quote: "comfortable territory" (Assumptions) and "this page is for when that route is unavailable" (intro) read as filler.
- [diataxis] Step 3 ("Serve directory indexes for trailing-slash URLs") explains *why* Pennington emits `<slug>/index.html` before giving the directive — strip the rationale, link to an Explanation, leave the directives.
- [diataxis] Step 4 includes a paragraph teaching what `OutputGenerationService` does with `NotFoundGeneratorPath` — that's reference/explanation material; the action ("server returns 404.html on misses") is one sentence.
- [diataxis] Step 5 mixes three concerns (MIME types, cache headers for `_content/`, sitemap/llms.txt MIME) into one prose block — a how-to step should be a single action.
- [clarity] Two `:path` embeds (Nginx + IIS) are placed in the same step with no signposting telling the reader to pick the one for their server; an IIS reader has to skip Nginx prose to find their config.
- [Q] Should this be split into two pages ("Self-host behind Nginx" / "Self-host behind IIS") so each is a clean single-target recipe?

### docs/Pennington.Docs/Content/how-to/deployment/static-build.md
**Form claimed:** how-to | **Actual:** mixed — leans toward explanation/tutorial; title is outcome-shaped but the body teaches the build pipeline.

- [voice] Intro is two sentences of theory before any action — quote: "There is no separate build project — the same `Program.cs` that serves the site locally crawls itself over HTTP and writes the result to disk, so the locally tested site is exactly what ships." This belongs in Explanation; the how-to should open with the goal and a single orienting sentence.
- [diataxis] Step 3 ("Understand what the crawler does") is pure teaching with no action — it explains the crawler's behavior and links to the explanation page. Delete it; a how-to does not include "understand" steps.
- [diataxis] Step 1 ("Confirm the host calls `RunOrBuildAsync`") is a precondition, not an action — fold into Assumptions.
- [diataxis] Step 5 ("Fix what the report flags before shipping") teaches the meaning of `BrokenLinks`/`FailedPages` rather than giving a recipe — link to reference, drop the prose.
- [clarity] The goal ("produce a deployable `output/` directory") is in the description front matter but never restated as a one-line "use this when…" in the body.
- [Q] Should "Understand what the crawler does" be moved verbatim to the dev-vs-build explanation page that's already linked?

### docs/Pennington.Docs/Content/how-to/deployment/adapt-for-other-hosts.md
**Form claimed:** how-to | **Actual:** how-to with reference-table hybrid — title is outcome-shaped; structure works but several voice/structure issues.

- [voice] Chatty filler — quote: "comfortable territory — the snippets below are complete, not starting points" (Assumptions) and "this page is for when…" patterns repeat across deployment docs.
- [voice] Editorial weighting — quote: "the **authoritative diff**" and "canonical" used as voice-emphasis rather than information.
- [diataxis] Step 2 is a large diff table that is reference content embedded mid-recipe; the steps after it (3/4/5) are independent host-specific actions, not sequential. The `<Steps>` wrapper implies ordering that doesn't exist — a reader doing Cloudflare doesn't need Step 3 (Azure) or Step 4 (Netlify).
- [diataxis] Per the project CLAUDE.md: "Use `<Steps>` only when each step depends on the previous one being done." Steps 3, 4, 5 are alternatives — they should be H3 sections under a topical H2, not numbered steps.
- [diataxis] Step 6 ("Pass the right `baseUrl`…") teaches base-URL handling that's already its own how-to; it should be a one-line reminder linking out, not a step.
- [clarity] The intro tells the reader to read the GitHub Pages page first; the Assumptions repeat that. Pick one.
- [Q] Restructure as: brief intro + shared-values table + three independent H2 sections (Azure / Netlify / Cloudflare), each with a fenced config and a one-paragraph delta?

### docs/Pennington.Docs/Content/how-to/deployment/base-url.md
**Form claimed:** how-to | **Actual:** how-to with explanation drift — title is outcome-shaped but several steps teach instead of instructing.

- [voice] Intro buries the action — the first sentence is a 60-word conditional construction. The reader needs "To serve under a sub-path, pass `[baseUrl]` to `build`" before the discursion.
- [diataxis] Step 2 ("Know what the rewriter prefixes") is teaching — describes `Order => 30`, the rewriter chain, and `data-base-url` semantics. This is explanation/reference; the how-to needs only the directive "use root-relative links and the rewriter handles them."
- [diataxis] Step 3 ("Use root-relative links in your content") teaches what protocol-relative and page-relative links do — replace with the one-line rule.
- [clarity] Steps 1, 2, 3, 4 are not actually sequential — a reader could pass `--base-url` (Step 1) and be done; Steps 2-4 are background and an optional client-side detail. The `<Steps>` shape misrepresents the recipe.
- [clarity] "Sub-path the host will serve from" in Assumptions is the central input but isn't elevated — the reader has to derive it from prose.
- [Q] Should Step 4 (`data-base-url` from JS) split into its own short how-to so this page stops at "build with the prefix, serve, done"?

### docs/Pennington.Docs/Content/how-to/pages/redirects.md
**Form claimed:** how-to | **Actual:** how-to titled with a noun phrase — should be outcome-shaped.

- [diataxis] Title "Configure redirects" is feature-named, not outcome-shaped — voice guide and project CLAUDE.md both flag this. Better: "Redirect an old URL to a new one" or "Forward visitors from a renamed page."
- [diataxis] Step 3 ("Understand what the pipeline emits") is pure teaching with an embedded `M:` symbol — explains `RedirectSource` vs `MarkdownFileSource`. Delete or move to explanation; a how-to doesn't include "understand" steps.
- [diataxis] Step 2 ("Confirm the front-matter record implements `IRedirectable`") teaches the capability system inline — replace with a one-line conditional ("If using a custom front-matter record, add `IRedirectable`.").
- [diataxis] Step 4 ("Run the site and follow the old URL") is verification material, not an action — belongs in the existing Verify section, not as a step.
- [clarity] Intro sentence "the body is not rendered or indexed" appears before the reader has any context for *which* body — comes from skipped prose. Restate after the action.
- [Q] Is `Content/main/redirect-source.md` a clean example? Step 1 says "Open the markdown file at the old URL" but the fenced embed is showing a fixture path — verify the rendered output shows the front matter the reader needs.

### docs/Pennington.Docs/Content/how-to/pages/images-and-assets.md
**Form claimed:** how-to | **Actual:** how-to titled with an action that's almost outcome-shaped; structure is good but body has teaching drift.

- [voice] Title "Place images alongside the markdown that uses them" reads as half-outcome — the page actually covers two strategies (colocated AND shared); the title only captures one. Consider "Add images to a page."
- [diataxis] No `<Steps>` block — uses H3 alternatives under an H2 ("Colocated next to the markdown file" / "Shared in `wwwroot/`"). This is correct shape per project CLAUDE.md, good.
- [diataxis] "Colocated" section teaches what `MarkdownContentService` and `MarkdownLinkResolver` do — strip the internal-machinery sentences; the reader needs "drop the file next to the page and reference it with a relative path."
- [diataxis] "Excluded subtrees" H3 is a separate goal grafted onto the images page — likely belongs as its own how-to ("Exclude a folder from the build") or under content-discovery.
- [clarity] No example fixture path is fenced anywhere on the page — every other how-to in the set fences a real example. Reader has no copy target.
- [Q] Move "Excluded subtrees" to its own how-to under content-discovery?

### docs/Pennington.Docs/Content/how-to/pages/drafts-tags-ordering.md
**Form claimed:** how-to | **Actual:** how-to bundling three independent recipes — title is comma-list, not outcome.

- [voice] Title bundles three outcomes — "Mark drafts, tag pages, and control sort order" works, but each is a real separable goal; a reader searching "hide a draft page" might miss it.
- [diataxis] The applicability matrix table is reference content — useful but should be elevated to a reference page and linked, not embedded inline in a how-to. The how-to should say "this works on `DocSiteFrontMatter`, `BlogSiteFrontMatter`, and `DocFrontMatter`" and link out.
- [diataxis] Each H3 under "Options" is essentially a mini how-to — this is fine per project CLAUDE.md (H3-per-variant), but the "Order a page inside its section" H3 teaches "A section inherits its own sort key from the minimum `order:` among its children" — that's explanation, link out.
- [voice] "Spacing like 10/20/30 leaves room for later inserts between existing siblings" is the kind of teaching aside that belongs in explanation, not how-to.
- [clarity] Note that `order:` has no effect on `BlogSiteFrontMatter` is a critical gotcha but is buried in a sentence under the table — should be an admonition or a callout in the order H3 itself.
- [Q] Split into three separate how-to pages ("Hide a draft page", "Tag a page", "Order a page in the sidebar") so each is independently searchable?

### docs/Pennington.Docs/Content/how-to/pages/front-matter.md
**Form claimed:** how-to | **Actual:** tutorial in how-to clothing — walks a feature member-by-member; per project CLAUDE.md this is reference, not how-to.

- [diataxis] Title "Work with front matter" is feature-named, not outcome-shaped — voice guide flags this exact pattern. The page does not solve a problem; it teaches a system.
- [diataxis] Steps 1-5 walk the front-matter feature in order: declare YAML, pick a record, fill keys, define custom, register. This is a learning arc — the project CLAUDE.md explicitly says "Pages that walk a feature member-by-member are reference, not how-to; re-frame around the user's goal or move them."
- [diataxis] Step 3 ("Fill in only the keys needed") gives no action — it just shows the full record symbol and tells the reader "use what you need." No outcome.
- [diataxis] The page is doing the job that should be split across three pieces: (a) reference "Front matter key reference" (already linked), (b) explanation "front-matter capability system" (already linked), and (c) a real how-to like "Define custom front-matter keys" focused on the custom-record path.
- [clarity] A reader who knows nothing about Pennington's front matter doesn't have a problem to solve here — they need the tutorial or the reference. A reader who has a real problem ("I want a custom `apiVersion` key") has to wade through four steps of background to find Step 4.
- [Q] Repurpose this page as the how-to "Define custom front-matter keys" — keep Steps 4-5, drop 1-3 (which are tutorial/reference)?

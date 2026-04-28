# Docs review todo

Findings from a multi-agent review of `docs/Pennington.Docs/Content/` (tutorials, how-to, explanation, reference — API-generated pages excluded). Each item is sized to be picked up by someone who wasn't in the original review session.

Conventions referenced throughout:
- Voice rules: `docs/docs-voice.md` and `docs/Pennington.Docs/CLAUDE.md`.
- Internal links use `[text](xref:uid)` or `<xref:uid>`. Never hardcode URL paths.
- C# samples use `csharp:xmldocid` (or `,bodyonly`) referencing `examples/` projects. Markdown/Razor/HTML/CSS use `:path`.
- Banned in prose: "simply", "just", "easy", "obviously", "please", "as we discussed earlier", "e.g.", "i.e.".
- AI-scaffolding blockquotes (`> **In this page.**` / `> **Not in this page.**`) get deleted before publish.
- `<Steps>` is for ordered, dependent steps only. Independent variants use H2 + H3-per-variant.
- Tutorials must not hand-write next/prev nav — that's auto-generated from `order`.

---

## Phase 1 — Reader-visible bugs (do first)

These ship to readers right now.

- [x] **Resolve TODO marker in `how-to/content-authoring/front-matter.md:81`.** The page ends step 5 with `<!-- TODO: xmldocid needed -->` after instructing the reader to call `AddMarkdownContent<T>`. Either supply the xmldocid for the `MarkdownContentService` extension method (grep `src/Pennington/Content/` for the actual signature) or rewrite step 5 to not need a fenced declaration.

- [x] **Resolve TODO marker in `how-to/content-authoring/images-and-assets.md:48`.** Same `<!-- TODO: xmldocid needed -->` after `MarkdownContentServiceOptions.ExcludePaths`. Either fence `T:Pennington.Content.MarkdownContentServiceOptions` (or the specific property xmldocid) or remove the reference if the type was renamed.

- [x] **Resolve TODO marker in `how-to/deployment/base-url.md:86`.** Bullet says `<!-- TODO: xmldocid needed --> 'BaseUrlHtmlRewriter' API reference`. Confirm the type still exists in `src/Pennington/`, then either link to its API ref page via `xref:` or delete the bullet.

- [x] **Resolve TODO marker in `how-to/configuration/localization.md:105`.** Bullet says `TODO — Explanation page on locale routing and content fallback (not yet written in TOC)`. The page does exist (`explanation/localization/urls-and-fallback.md`); replace the TODO with `<xref:explanation.localization.urls-and-fallback>` (verify the actual uid in that file's front matter).

- [x] **Resolve TODO marker in `how-to/deployment/self-host.md:86`.** Bullet says `TODO — add link to the "Unified dev-and-build path" explanation page once published`. The page exists at `explanation/core/dev-vs-build.md`; replace the TODO with the corresponding `xref:` link (verify uid in front matter).

- [x] **Fix duplicate `order` between two extensibility how-tos.** Both `how-to/extensibility/razor-page-on-bare-host.md` and `how-to/extensibility/override-docsite-components.md` declare `order: 203070`. Pick distinct values per the project's tidy-sequential convention (10/20/30…). Verify the resulting sidebar order looks intentional after the change.

- [x] **Replace hardcoded GitHub URL in `how-to/extensibility/custom-highlighter.md:18`.** The file links `[examples/ExtensibilityLabExample](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample)`. Either drop the link entirely (the path in prose is enough) or, if a link is wanted, point at the in-repo example via the same syntax used elsewhere in the docs.

---

## Phase 2 — Verify reference components actually render

Reference pages invoke Mdazor components that may or may not be wired. If a component isn't registered, the page renders quietly empty and the reference looks complete but isn't.

For each item: build the docs site (`dotnet run --project docs/Pennington.Docs`), open the page in a browser, and confirm the component produces visible output. If it doesn't, either wire the component (see `docs/Pennington.Docs/Program.cs:96` for the pattern used for `FrontMatterKeys`) or replace the invocation with a static markdown table.

- [x] **Verify `<ExtensionMethods Receiver="…" />` renders in `reference/host/extensions.md`.** The page invokes the component three times (`Receiver="IServiceCollection"`, `"WebApplication"`, `"IEndpointRouteBuilder"`). Confirm a component by that name and contract exists. If not, swap to a markdown table.

- [x] **Verify `<FieldList>` and `<Field>` render in `reference/diagnostics/request-context.md:31-40`.** These wrap the positional parameters of the `Diagnostic` record. If unwired, replace with a `| Name | Type | Default | Description |` table.

- [x] **Confirm `<FrontMatterKeys />` in `reference/front-matter/keys.md:18` produces the expected catalog.** The component is registered (`docs/Pennington.Docs/Program.cs:96`), so this is a sanity check rather than a likely fix. _Caught a real bug: the index couldn't activate because `ApiReferenceOptions` is registered as a keyed singleton by `AddApiMetadataFromRoslyn`. Fixed by resolving the keyed option at registration time in `Program.cs`._

---

## Phase 3 — Diataxis fixes (structural rewrites)

Each of these moves content to the right quadrant or reshapes a page that's pretending to be one register while serving another.

- [ ] **Reshape `how-to/extensibility/custom-content-service.md` from member-walkthrough to goal-driven how-to.** Today the page walks each `IContentService` member (`DiscoverAsync`, `GetContentTocEntriesAsync`, `GetContentToCopyAsync`, `GetContentToCreateAsync`, `GetCrossReferencesAsync`) and explains it. That's reference territory. Pick one realistic outcome ("Source content from outside the file system" with a `ReleaseNotesContentService` style example), show one end-to-end implementation via `:xmldocid` against `examples/`, and link to the API reference for member-by-member detail. Move any deep member explanations into a reference page if they don't already exist there.

- [ ] **Reshape `how-to/content-authoring/cross-references.md` away from `<Steps>`.** Today four logically-independent sections sit inside one `<Steps>` block — steps 2 and 3 are alternative link forms (the reader picks one), step 4 is "how resolution works," not an instruction. Use `linking.md` (next door) as the template: H2 "Prerequisites" with the uid declaration, H2 "Link forms" with H3-per-form, H2 "How resolution works." Keep the existing Verify and Related sections.

- [ ] **Trim non-tutorial content from `tutorials/getting-started/styling.md:170-182`.** Two sections after the Summary teach reference/configuration material (`Pennington.UI` components and the listening-port option) without producing a verifiable result. Delete from this page; if the material isn't already in reference, add it there.

- [ ] **Replace concept-only step in `tutorials/blogsite/scaffold.md`, step 3 (lines ~78-82).** The "Contrast with `DocSite` defaults" step has no reader action and no visible result — a concept essay inside a tutorial. Either delete the step or rewrite it as an actual verify-step (build still succeeds, scaffold still serves), and link to an explanation page if the design contrast is worth surfacing.

- [ ] **Drop the false prerequisite in `tutorials/beyond-basics/custom-razor-component.md:22`.** The page lists `connect-roslyn` as a prerequisite, but a reader who completed the docsite scaffold can finish this tutorial without it. Remove that line; keep the scaffold prerequisite and the Razor familiarity note.

---

## Phase 4 — Drift in reference

- [ ] **Fix internal-API claims in `reference/markdown/code-block-args.md:10`.** The page cites `CodeBlockExtensions.GetArgumentPairs` and `CodeTransformer.Transform` as part of the public surface. Both are `internal` (verify by grepping `src/Pennington/Markdown/`). Reframe the sentence around the public boundary: Markdig's `FencedCodeBlock.Arguments` and the user-visible info-string contract. Don't expose internal types in a reference page.

---

## Phase 5 — Voice batch (mechanical, single pass)

Eight banned-word hits across seven files. Fix in one sweep — each is a single-line edit. After the edits, re-grep `docs/Pennington.Docs/Content/` for `\b(simply|just|easy|obviously|please|e\.g\.|i\.e\.)\b` to confirm nothing slipped back in.

- [ ] `explanation/core/content-source.md:60` — "just as clean" → "equally clean" (or rewrite to drop the comparator).
- [ ] `explanation/localization/urls-and-fallback.md:32` — "a cascade is easy to write and hard to reason about" → reframe without "easy" (suggested: "a cascade is quick to write but hard to reason about" — though that still hedges; prefer something like "a cascade hides coverage gaps").
- [ ] `explanation/dev-experience/hot-reload.md:38` — "i.e. during `dotnet run -- build`" → "that is, during `dotnet run -- build`" or restructure to drop the parenthetical.
- [ ] `explanation/spa/islands.md:52` — "expecting them to 'just work'" → "expecting drop-in compatibility".
- [ ] `how-to/deployment/static-build.md:86` — "the source is easy to locate" → "the source is straightforward to locate" (or just describe where it is).
- [ ] `how-to/extensibility/auto-api-reference.md:15,84,86` — three "e.g." → "for example".
- [ ] `how-to/extensibility/auto-api-reference.md:46` — "bumping the documented version is just a `<PackageReference Version=…>` change" → "bumping the documented version is a `<PackageReference Version=…>` change".

Bonus voice slips worth fixing in the same pass:
- [ ] `reference/diagnostics/request-context.md:14` — opening sentence is tour-guide ("the accumulator and record types live in namespace…"). Lead with what the types *do*; mention the namespace second.
- [ ] `reference/host/cli.md:51-52` — paragraph about BlogSite routes is out of scope for a CLI reference page. Delete or move to `reference/blogsite/routes.md`.
- [ ] `reference/blogsite/routes.md:46` — "A minimal BlogSite host…" → drop "minimal" (filler).

---

## Phase 6 — Cross-link audit (worth one careful pass)

- [ ] **Audit uid usage between `tutorials.getting-started.first-site` and `tutorials.getting-started.first-page`.** Multiple how-to pages cross-link to "the getting-started tutorial" using one or the other. They're distinct pages (`order: 101010` and `101020`). For each link, confirm the target answers the question the linking page is deferring. Files known to reference these uids include at least: `how-to/configuration/multiple-sources.md`, `how-to/configuration/llms-txt.md`, `how-to/configuration/fonts.md` — grep the full content tree for both uids before starting.

---

## Out of scope (intentionally not on the list)

The reviewers also flagged a number of "structurally sound but could be tightened" candidates — long preambles, slightly discursive tradeoff sections, repeated NOTE callouts, etc. These are within the natural prose budget of the relevant register and do not justify a sweep for sweep's sake. If you're already in one of these files for another reason, feel free to tighten while you're there:

- `explanation/core/docsite-positioning.md:56-62` — three restatements of the same design principle; collapsible to one paragraph.
- `explanation/routing/navigation-tree.md:20-21` — NOTE callout repeats what the body already explains.
- `how-to/configuration/multiple-sources.md` — verbose "Assumptions" section.
- `how-to/configuration/monorail-css.md:78-88` — "bare host escape" reads as an afterthought.
- `how-to/deployment/github-pages.md` step 6 — substantial guidance behind an "(Optional)" label.
- `reference/markdown/extensions.md:10-11` — preamble that the table below makes redundant.
- `reference/ui/navigation.md:37-39, 70-72` — "no `RenderFragment` slots" stated verbatim twice.

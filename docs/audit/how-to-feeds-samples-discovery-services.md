### docs/Pennington.Docs/Content/how-to/feeds/rss.md
**Form claimed:** how-to | **Actual:** how-to with mild explanation creep — solves a real outcome but the intro carries more theory than a recipe needs.

- [voice] Two informal/discursive phrases in one paragraph — quote: "kitchen-sink example wires" and "the two things that most often break a working feed are…"
- [voice] Conditions-before-instructions inverted in the lede — quote: "When subscribers should be able to follow the blog from a reader, `/rss.xml` is wired by `UseBlogSite` out of the box" buries the action under a long subordinate clause; the same sentence then chains a parenthetical and two diagnoses.
- [diataxis] H2 `## Options` followed by H3 variants matches the project's "enumerate variants" pattern, but the section "Where the feed is served and discovered" is pure explanation (dev vs. static-build behavior, browser RSS extensions) and belongs in Explanation or Reference.
- [diataxis] The `### Confirm \`EnableRss\` is on` subsection has no source-then-output unit; the prose says it defaults to `true` and then shows a kitchen-sink snippet that mixes three concerns. Reads as reference description rather than a problem-oriented step.
- [clarity] The `BuildBlogSiteOptions` snippet is referenced three times across pages (rss, sitemap, homepage) but the reader can't see what's inside without leaving — pasting a small focused fence beats reusing a 3-concern kitchen-sink fixture.
- [Q] Is `kitchen-sink` an intentional brand term, or should each option show only its own minimal snippet?

### docs/Pennington.Docs/Content/how-to/feeds/sitemap.md
**Form claimed:** how-to | **Actual:** mostly how-to with reference-flavored prose — the first H3 explains a no-op ("nothing to do") which is reference, not recipe.

- [voice] Banned phrase — quote: "There is no `AddSitemap(...)` call to make" reads fine, but the section title "Confirm `/sitemap.xml` is already wired" introduces a non-action; a how-to step should have an action.
- [voice] Discursive aside in a how-to — quote: "the sitemap has no per-request cost when nothing fetches it" is design-rationale, belongs in Explanation.
- [diataxis] Reference creep — the paragraph naming `IFrontMatter.IsDraft` and `IRedirectable.RedirectUrl` plus the linked builder signature reads like a member catalog; either trim to the user-facing front-matter keys or link to reference.
- [diataxis] The `### Confirm` section is a noun-phrase status check, not a recipe step — collapse into a one-sentence assumption or move to the page intro.
- [clarity] The `BuildBlogSiteOptions` xmldocid snippet under "Set `CanonicalBaseUrl`" is a BlogSite kitchen-sink fixture, but this page targets any `AddPennington` host — the reader sees three options at once when they wanted to see one property assignment.
- [Q] Why does this how-to include the `SitemapBuilder.Build(...)` signature inline? That's a reference asset; the user just needs the front-matter keys.

### docs/Pennington.Docs/Content/how-to/feeds/llms-txt.md
**Form claimed:** how-to | **Actual:** how-to with one explanation-creep paragraph and one decision-tree step that crowds the recipe.

- [voice] Long discursive intro to a subsection — quote: "`AddDocSite` already calls `AddLlmsTxt` internally and defaults `ContentSelector` to `#main-content`. On a DocSite host, per-page inclusion is controlled through front matter (below)…" — three sentences of orientation before any action.
- [voice] Sub-heading not an instruction — quote: "### Decide: DocSite front matter, or bare `AddLlmsTxt`?" — H3 phrased as a decision-tree question rather than an outcome.
- [diataxis] The "Decide:" H3 is meta-routing inside a how-to. Either split this into two pages (one per host) or push the decision into "Assumptions".
- [diataxis] The `humans-only` / `robots-only` subsection drifts into explanation — quote: "Reach for it when a widget, interactive demo, or layout flourish carries no information an LLM needs" — design rationale framed as guidance.
- [clarity] No fenced expected output of the per-page sidecar (`/_llms/<page>.md`) even though "Verify" references its YAML header; pasting one short sidecar would close the loop.
- [Q] Is `LlmsTxtOptions.ContentSelector` documented anywhere with the markdown-vs-HTTP-fetch distinction? The note is buried mid-paragraph in the "Decide" section.

### docs/Pennington.Docs/Content/how-to/feeds/blogsite-homepage.md
**Form claimed:** how-to | **Actual:** how-to but titled around a feature ("hero") rather than the reader's outcome.

- [diataxis] Title is feature-shaped, not outcome-shaped — "Wire the blog homepage hero" describes one of four surfaces, not the user goal. Something like "Populate the blog homepage" or "Configure the blog homepage surfaces" better matches the body, which covers hero + projects + socials + nav.
- [diataxis] The page categorisation under `sectionLabel: "Feeds & Indexes"` is wrong — hero/socials/nav are layout, not feeds. Likely a sidebar bug.
- [voice] Bracketed parenthetical that exists only to disclaim — quote: "This page is a recipe, not a tour, so it does not walk through the whole example." — cut; the how-to register makes this implicit.
- [voice] Italicized markdown link text inside xref labels — quote: "[_Add a hero, projects, and social links_]" — unusual styling for "Related" link text vs other pages on this audit list.
- [clarity] Each H3 references a separate `Stage1` / `Stage2` / `Stage3` snippet, but the reader doesn't know whether they should compose all three or pick one — clarify in the lede that the four surfaces are additive and independent.
- [Q] Should this page live under "Site layout" or "BlogSite configuration" rather than "Feeds & Indexes"?

### docs/Pennington.Docs/Content/how-to/code-samples/tabbed-code.md
**Form claimed:** how-to | **Actual:** how-to with one explanation-style section that should be trimmed or moved.

- [voice] Discursive aside — quote: "the language token before the attributes still drives syntax highlighting" — fine on its own, but composes with the next paragraph's "This works identically on `AddPennington`, `AddDocSite`, and `AddBlogSite` because each surface plumbs the same property through to the pipeline factory" which is design rationale, not recipe.
- [diataxis] Section "## What the renderer emits" is explanation creep — two paragraphs of HTML/CSS-class background plus the `with` expression rationale belong in a reference or explanation page; the recipe only needs "to override the class names, set `TabbedCodeBlockOptions`…" plus the snippet.
- [diataxis] Title is outcome-shaped (good), but "Group adjacent code fences into a tabbed sample" hides what most readers actually want — "Show language alternatives in tabs" or similar — minor.
- [clarity] First example uses three tabs (bash / PowerShell / csproj) which is more than needed to demonstrate the mechanic; two would be cleaner.
- [Q] Should the `TabbedCodeBlockRenderOptions` member type fence be folded into the override snippet rather than shown as a standalone `T:` fence?

### docs/Pennington.Docs/Content/how-to/code-samples/code-annotations.md
**Form claimed:** how-to | **Actual:** how-to, clean enumeration of variants.

- [diataxis] Title is feature-list rather than single outcome — "Highlight, diff, focus, or flag lines inside a code block" is closer to a reference catalog title; "Annotate specific lines in a code block" is more outcome-shaped.
- [diataxis] Section "## What the renderer emits" is explanation creep — quote: "The directive text never appears in rendered HTML. Comment-marker variants (`#`, `--`, `<!-- -->`, etc.) are recognised the same way, so the same directive set works across languages without per-language wiring." — design-rationale paragraph that belongs in Explanation.
- [voice] Latin abbreviation — quote: "(`#`, `--`, `<!-- -->`, etc.)" — `etc.` in prose; the page also uses it once in the bullet list under Assumptions.
- [clarity] No "Verify" section, unlike sibling how-tos — readers can't confirm they wired it right without inspecting rendered HTML.
- [Q] The Assumptions bullet "Authoring happens in plain markdown; directives travel through the fence as comments and are stripped at render time" is a fact about the feature, not a precondition — should be in the lede or cut.

### docs/Pennington.Docs/Content/how-to/code-samples/focused-code-samples.md
**Form claimed:** how-to | **Actual:** mixed — partly how-to, partly tutorial-style walkthrough of techniques. Long for the form.

- [voice] Banned-adjacent — quote: "no fence form will make it short and intelligible — the source itself is too large. Fix the source, not the fence" — confident but borders on prescriptive lecturing for a how-to.
- [voice] Multiple "reach for X" phrasings within one page — quote: "reach for `M:Type.Method(...)`", "reach for `:path` only when no xmldocid exists" — fine once, becomes a tic.
- [diataxis] Explanation creep around `,usings` — quote: "the assumption that a reader who has `<ImplicitUsings>enable</ImplicitUsings>` already has them" — design rationale.
- [diataxis] "Break a long method into named helpers" is teaching, not recipe — the reader who lands here wants to scope a fence; advising them to refactor their source code reads as authorial opinion. Belongs in Explanation/best-practices, or as a brief link-out.
- [diataxis] "Reach for `:path` only when no xmldocid exists" is a meta-decision rule, not a step — would be more honest as a one-line note under Assumptions.
- [clarity] Page covers five distinct techniques (`M:`, `,bodyonly`, `,usings`, split-into-helpers, `xmldocid-diff`, plus `:path` fallback) — too many for one outcome. Could be one how-to "Scope a fence to a member" plus a separate "Diff two implementations".
- [Q] Should the "Break a long method into named helpers" section move to an Explanation page about authoring conventions?

### docs/Pennington.Docs/Content/how-to/discovery/localization.md
**Form claimed:** how-to | **Actual:** how-to, but with one admonition that pushes the page just over the recommended limit and some reference-flavored prose.

- [voice] Admonition for a tutorial pointer — `> [!TIP]` admonition in the lede directs the reader to the tutorial; the project's how-to register says open with one sentence of context, often none. Cut or fold into a plain sentence (also: H3 lede already says "see the tutorial" elsewhere).
- [voice] Discursive — quote: "so shipping does not require a full translation pass" — design-rationale aside.
- [diataxis] Section "### Mirror your content tree…" mixes recipe with explanation about ContentResolver pairing and fallback semantics; the fallback behavior belongs in Explanation.
- [diataxis] Final section "### Surface the language switcher" doesn't depend on prior options and could read as standalone — fine, but its single snippet `<LanguageSwitcher />` (one element with no surrounding context) feels thin compared to the other H3s.
- [clarity] The `Translations.Add("en", "nav.home", "Home")` snippet doesn't show where `IStringLocalizer["nav.home"]` is then consumed in a component — closing the loop with a one-line component usage would help.
- [Q] Should "Confirm `UsePenningtonLocaleRouting` is in the pipeline" be in Assumptions instead, since template hosts get it for free?

### docs/Pennington.Docs/Content/how-to/discovery/multiple-sources.md
**Form claimed:** how-to | **Actual:** how-to but `<Steps>` is misused — the steps include a branch ("jump to step 4") and aren't strictly sequential.

- [diataxis] `<Steps>` with conditional branching — Step 1 says "continue to step 2" or "jump to step 4". The project guide explicitly says `<Steps>` implies linear ordering; this page should use H3-per-variant under topical H2s ("Split a DocSite via `Areas`" / "Chain `AddMarkdownContent` calls") instead.
- [diataxis] Step 1 is a decision tree, not an action — it should be the page lede or an "Assumptions" prerequisite, not Step 1 of 6.
- [voice] Italicized link text inconsistent with other how-to pages — quote: "[_When is DocSite the right starting point?_]" and "[_Your first Pennington site_]" — looks like markup artifacts.
- [voice] "_(confirm path)_" placeholders in Related links — these are author TODOs leaking into published prose.
- [clarity] The `RegisterOverlappingDocSource` snippet under "Optional" doesn't show the overlap warning text the reader is supposed to recognise — pasting one warning line would close the loop.
- [Q] Are the `_(confirm path)_` markers intentional, or are these xref uids unverified?

### docs/Pennington.Docs/Content/how-to/discovery/search.md
**Form claimed:** how-to | **Actual:** how-to, mostly clean but with reference-flavored body around `DefaultPriority`.

- [voice] Discursive aside — quote: "Per-source priority takes precedence: `MarkdownContentServiceOptions.SearchPriority` defaults to `10`, `RazorPageContentService` is `5`, and the llms.txt/SPA/redirect services report `0` so their artifacts never appear in results." — reads like a reference table inline.
- [diataxis] The default-priority paragraph above is reference-style coverage; in how-to register, link out to a reference table instead.
- [clarity] The example JSON `priority` field shows `10` but the prose just before talks about defaults `5`/`10`/`0`; readers may not connect that `10` corresponds to the markdown-source override.
- [clarity] "Verify" mentions `documents` array but the example JSON output above shows an unwrapped object — the shape between Result and Verify isn't consistent (`documents[]` wrapper vs. raw entry).
- [Q] Is the index JSON a top-level array of objects, or an object with a `documents` field? Result and Verify give different impressions.

### docs/Pennington.Docs/Content/how-to/content-services/custom-content-service.md
**Form claimed:** how-to | **Actual:** drifts toward tutorial — long teaching sections describing what each interface method does.

- [voice] Several discursive paragraphs — quote: "`ContentSource` is a union over `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`, and `EndpointSource` — implicit conversions make the case-name shorthand work, so `new EndpointSource()` and `new ContentSource(new EndpointSource())` are equivalent." — pure type-system explanation.
- [voice] Conditions-before-instructions inverted in lede — quote: "To source content from somewhere `MarkdownContentService<T>` can't reach… and have those pages appear in navigation, cross-references, search, and the static build the same way markdown pages do, implement `IContentService` directly." — long subordinate before the action; second sentence then introduces example name and a sibling how-to link, three concerns in one paragraph.
- [diataxis] Explanation creep — the "Implement the service" section's five-bullet enumeration of every member with its rationale is reference/explanation content, not recipe. A how-to should point at the example and call out the one or two non-obvious moves.
- [diataxis] "Model the source records" is a section for one immutable record — the reader who reaches for a custom content service knows what an immutable record is. Tutorial-handholding.
- [clarity] The `IContentService` example renders the whole `ReleaseNotesContentService` type with no callout to which parts are the load-bearing ones; the reader has to read 100+ lines of fenced output and identify what changed vs. boilerplate.
- [Q] Could the "Implement the service" prose move to an Explanation page on the content pipeline and leave a recipe-shaped step here?

### docs/Pennington.Docs/Content/how-to/content-services/emit-generated-artifacts.md
**Form claimed:** how-to | **Actual:** how-to with reference-style member enumeration mid-page.

- [diataxis] The bullet list under "Implement the service" enumerates every interface member with its return signature and rationale — that's reference content. A how-to should show the one member that matters and link out for the rest.
- [voice] Discursive — quote: "`DefaultSectionLabel` and `SearchPriority` are read by consumers that group discovered items; since this service discovers nothing, they do not matter — return `""` and `0`." — design-rationale aside.
- [voice] Subjective recommendation — quote: "Transient is the right lifetime for this shape" — fine in confident voice, but uses "the right" rather than naming a reason in one clause (e.g., "use transient — the service is stateless").
- [clarity] "Result" shows the static-build output and a footnote that the dev server returns 404; this surprises a reader who tested with `dotnet run`. Move the dev-server caveat earlier or into the lede so they don't get tripped up.
- [Q] Should this page cover both build-time emission and live serving via `MapGet`, or stay narrowly on static-build artifacts?

### docs/Pennington.Docs/Content/how-to/content-services/auto-api-reference.md
**Form claimed:** how-to | **Actual:** several how-tos stitched into one long page — different outcomes (Roslyn backend, reflection backend, customize prefix, multi-library, narrow scope, render components) each warrant their own page or clear separation.

- [voice] Latin abbreviation — quote: "(for example, a NuGet package)" is correct elsewhere, but the same page has "(for example, Spectre.Console.Cli reaching back into Spectre.Console)" — fine. However: quote "(`Properties`, `Constructors`, `Fields`, `Methods`, `Events`)" with parenthetical lists in body prose is reference-style.
- [voice] Discursive design-rationale paragraphs — quote: "No MSBuild workspace, no docfx, no source code. The backend uses `MetadataLoadContext` under the hood — it inspects metadata without running any of the assembly's code." — sales/positioning prose mid-recipe.
- [diataxis] Page covers six discrete outcomes — wire Roslyn backend, wire reflection backend, customize prefix, document multiple libraries, narrow scope, render fragments inline. Each is its own how-to. The page reads like a feature tour.
- [diataxis] Section "Render reference fragments inline" with five H3s for `<ApiSummary>`, `<ApiMemberTable>`, `<ApiParameterTable>`, `<ExtensionMethods>` is reference catalog material, not a how-to recipe. The live `<ApiSummary>` renders inside the page itself, suggesting the author treated it as a demo page.
- [diataxis] No "Verify" section per outcome — one Verify at the end covers only the default backend; the multi-library and narrow-scope outcomes have no verification.
- [clarity] The reflection-backend section uses `FromPackageReference("Spectre.Console")` without ever introducing what `FromPackageReference` is on first reading — it's named in passing, then explained, then used again.
- [clarity] Target reader (experienced C#/.NET dev wanting an API reference) hits six different decisions in one page; splitting per backend would let each page front-load its own assumptions and verification.
- [Q] Is this intentionally a "feature catalog" page, or should it split into "Generate an API reference from a Roslyn workspace", "…from a compiled assembly", "Document multiple libraries", and a reference page for the Mdazor components?

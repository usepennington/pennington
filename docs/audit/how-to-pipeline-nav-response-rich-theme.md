### docs/Pennington.Docs/Content/how-to/markdown-pipeline/code-block-preprocessor.md
**Form claimed:** how-to | **Actual:** how-to with reference creep — feature-tour shape, three back-to-back symbol fences feel like API reference rather than a goal-driven recipe.

- [voice] Sentence inside parenthetical loops back to "we" framing without need — quote: "(`AddPenningtonRoslyn` performs the equivalent registration for `RoslynCodeBlockPreprocessor`.)" — parenthetical aside is filler.
- [voice] Heading "Pick a Priority value" mixes case — `Priority` is a backtick'd identifier embedded in a sentence-case heading; acceptable, but the matching "Register the implementation" / "Result" / "Verify" headings imply ordering even though the page does not use `<Steps>`. Reader's-chair test: the page reads as steps but isn't structured as steps.
- [diataxis] Three `csharp:xmldocid` fences for `ICodeBlockPreprocessor`, the `TryProcess` body, and `CodeBlockPreprocessResult` shown sequentially is reference content embedded inline — the reader is taught the interface shape rather than handed a recipe. CLAUDE.md instructs how-to pages to link out to reference instead.
- [diataxis] Title "Add a custom fence syntax" is outcome-shaped (good), but the body pivots to "Implement the preprocessor" / "Pick a Priority value" / "Register" — that is feature-tour ordering, not a recipe centred on the reader's goal.
- [clarity] The reader is told `RoslynCodeBlockPreprocessor` uses priority 100 and `LineCountPreprocessor` uses 500, but never given a heuristic — "use a value above 100 unless you want Roslyn to win" would be the actionable guidance.
- [clarity] "the default highlighter does not run again on that block" appears in step 1 prose, then `SkipTransform` is introduced two paragraphs later as something different — the relationship between "no second highlighter pass" and `SkipTransform` is not crisp.
- [Q] Should this page exist at all if `xmldocid` fences already pull the interface and result types verbatim? The recipe portion is three sentences of prose plus one `AddSingleton` line — everything else is reference material that belongs in the highlighting interfaces reference.

### docs/Pennington.Docs/Content/how-to/markdown-pipeline/custom-highlighter.md
**Form claimed:** how-to | **Actual:** how-to wrapped in `<Steps>` — `<Steps>` is misused; the six steps are member descriptions of `ICodeHighlighter`, not sequential dependencies.

- [voice] "Use this approach for fences tagged with a language token — a DSL, config format, or domain notation — that TextMateSharp does not cover, when styled output is the goal but authoring a full TextMate grammar is not." — long opening sentence that buries the trigger condition.
- [voice] Banned phrasing — quote: "the same convention the built-in highlighters follow" reads fine, but elsewhere: "the highlighter is active for both `dotnet run` and `dotnet run -- build output`" — the doubled command repeats what step 5 already implies.
- [diataxis] `<Steps>` wraps the six interface members. CLAUDE.md is explicit: "Pages that walk a feature member-by-member are reference, not how-to" and "Use `<Steps>` only when each step depends on the previous one being done." Step 2 (declare `SupportedLanguages`) does not depend on step 3 (set `Priority`); they are sibling fields on the same interface.
- [diataxis] Step 1 says "The next three steps fence each of those members separately" — that confirms the page is touring members, not solving a problem.
- [diataxis] No `## Result` section, unlike its peer pages — inconsistent with the in-section template.
- [clarity] Step 4 says "Full implementation: `examples/ExtensibilityLabExample/PipelineHighlighter.cs`" but never shows even a single line of `Highlight` body inline — the reader has to leave the page to see what HTML their method should return.
- [Q] Recommend collapsing to topical H2/H3 ("Declare the languages", "Pick a priority", "Emit highlighted HTML", "Register") and dropping the `<Steps>` wrapper to match the in-doc convention.

### docs/Pennington.Docs/Content/how-to/navigation/cross-references.md
**Form claimed:** how-to | **Actual:** how-to drifting into explanation — two paragraphs spend their words on internal pipeline phases that the reader does not need to act.

- [voice] "How resolution works" subsection — quote: "Both phases run inside `XrefHtmlRewriter` (`Order => 10`), which executes before `LocaleLinkHtmlRewriter` and `BaseUrlHtmlRewriter` so later rewriters see canonical paths — identically in dev serve and `build`." — this is explanation territory, not how-to.
- [voice] "the value of a uid is that it survives a move" — slightly preachy aside; can be cut without losing the instruction.
- [diataxis] Section "How resolution works" plus the `XrefResolvingService` and `XrefHtmlRewriter` symbol fences are explanation content; CLAUDE.md says "No concept teaching in the body; link to Explanation for background." A "Related" link to `explanation.routing.cross-references` already exists — fold the explanation prose there.
- [diataxis] Three sequential `csharp:xmldocid` fences (`IFrontMatter.Uid`, `ResolveXrefTagsAsync`, `ResolveXrefLinksAsync`, `XrefHtmlRewriter`) bloat the page into reference + explanation rather than a recipe.
- [clarity] The recipe core is short: "give the page a `uid:`, link with `<xref:uid>` or `[text](xref:uid)`." That answer is buried under interface tours.
- [clarity] No `## Assumptions` heading mismatch with the doc set — uses "Assumptions" here, "Before you begin" on most other how-tos. Pick one.
- [Q] Does the reader who lands on this page actually need to see `ResolveXrefTagsAsync`'s xmldocid? The recipe works without ever looking at it.

### docs/Pennington.Docs/Content/how-to/navigation/customize-sidebar.md
**Form claimed:** how-to | **Actual:** how-to — closest to a clean topical-options shape in the set.

- [voice] "the value of a uid is that it survives a move" appears in the sibling cross-references page; this page has its own version — quote: "Use 10/20/30 spacing so later inserts land between siblings without renumbering every file." That is fine and instructional; flag only as a style note.
- [diataxis] Each H3 ends with a `csharp:xmldocid` symbol fence pointing at the backing property. That is reference content lifted inline. The "Backing symbol …" prose tag in front of each fence reads like a reference-style annotation, not a recipe step.
- [clarity] The cross-link to "the extensibility guide for overriding DocSite components" in the intro is not wired as a `xref:` — just prose. If the target is `how-to.response-pipeline.override-docsite-components`, link it.
- [clarity] "the section's aggregate sort key is the minimum `order:` of its direct children" — useful but explanation-flavoured; one sentence is fine, two would be drift.
- [Q] Should the backing-symbol fences move out of how-to entirely and live only in the front-matter key reference?

### docs/Pennington.Docs/Content/how-to/navigation/linking.md
**Form claimed:** how-to | **Actual:** how-to — well-shaped, the cleanest in the navigation cluster.

- [voice] Title "Link between pages without hardcoding URLs" includes an outcome (good), but the section headers `### Relative path to a sibling page` / `### Absolute path to a page in another area` are descriptors not outcomes — minor inconsistency with how-to register, but acceptable for a variants page.
- [clarity] The "Sub-path deployment" variant says "See <xref:reference.api.i-response-processor> for the rewriter chain" but does not actually tell the reader how/where to set `OutputOptions.BaseUrl` — the C# call site is implicit.
- [clarity] "Avoid hard-coding the prefix in markdown." — passive prescription without showing the wrong shape and the right shape side-by-side; for an authoring how-to a before/after fence would land harder.
- [Q] "External site" variant says "Add `rel=\"noopener\"` or `target=\"_blank\"` through a custom Markdig extension when a hosting policy requires it; none of the built-in rewriters add these attributes." — that is a pointer with no follow-up link. Is there a how-to for custom Markdig extensions to point at?

### docs/Pennington.Docs/Content/how-to/response-pipeline/html-rewriter.md
**Form claimed:** how-to | **Actual:** how-to with reference creep — interface-tour pattern repeats from the preprocessor page.

- [voice] Long opening sentence — quote: "Pennington's `HtmlResponseRewritingProcessor` parses each response body with AngleSharp exactly once and invokes every registered rewriter against that shared `IDocument`, so the work composes with the built-in xref, locale, and base-URL passes." — that is explanation framing inside the lead; trim to "Implement `IHtmlResponseRewriter`; every rewriter shares one parse."
- [voice] "to avoid paying for an allocation on every response" — minor, but "paying for an allocation" is jargon-y for a how-to register.
- [diataxis] Four sequential `csharp:xmldocid` fences (`ShouldApply`, `PreParseAsync`, `ApplyAsync`, `Order`) tour the interface members in order. Same pattern as `custom-highlighter.md` — this is a reference walk, not a recipe.
- [diataxis] "The three shipped rewriters run at 10 / 20 / 30" — exposes built-in `Order` values inline; that table belongs in reference, not in the recipe (also duplicated on `response-processor.md`).
- [clarity] The reader is never shown what `ApplyAsync` body actually does — three sentences of prose ("query with `QuerySelectorAll`, mutate attributes …") and an xmldocid pull. A small `QuerySelectorAll` snippet would beat the symbol fence.
- [Q] Pages for `IHtmlResponseRewriter`, `IResponseProcessor`, `ICodeBlockPreprocessor`, and `ICodeHighlighter` all follow the same shape: lead → "Implement the X" → tour members → "Pick Order" → "Register" → "Result" → "Verify". Is this template-driven duplication or recipe per extension point? If the latter, the per-page variation needs to be more than the type name.

### docs/Pennington.Docs/Content/how-to/response-pipeline/override-docsite-components.md
**Form claimed:** how-to | **Actual:** how-to — covers four seams plus a reference-shaped table at the end.

- [voice] "Awareness that …" appears twice in "Before you begin" as a bullet form — quote: "Awareness that `ExtraStyles` is appended to the generated `/styles.css`…" and "Awareness that these seams are set at host-build time…" — "Awareness" as a bullet noun is stiff; rephrase to a condition.
- [voice] "raw HTML string rendered inside every page's `<head>`, making it the right seam for meta tags, preconnect hints, analytics snippets, and font `<link>` elements that MonorailCSS does not know about" — list-in-prose runs long; would carry as a one-line sentence.
- [diataxis] "Other DocSite extension points" table at the bottom is reference inside a how-to. CLAUDE.md flags "embedded reference tables" as drift.
- [diataxis] Title "Replace the docsite header or footer" advertises a narrower outcome than the body covers (head injection, extra styles, routing assemblies). Either narrow the body or broaden the title; right now H1 mismatches scope.
- [clarity] "Edits made in the `DocSiteOptions` factory passed to `AddDocSite`, not the DocSite source — forking the template is out of scope" — the negative assumption competes with a positive instruction; reword as "All edits go through the `DocSiteOptions` factory."
- [Q] The "Other DocSite extension points" table cites `how-to.discovery.multiple-sources`, `how-to.content-services.custom-content-service`, and `explanation.spa.islands` — would those be better as a "Related" cluster than an embedded reference table?

### docs/Pennington.Docs/Content/how-to/response-pipeline/response-processor.md
**Form claimed:** how-to | **Actual:** how-to — same interface-tour pattern as `html-rewriter.md`.

- [voice] "When the work is DOM-shaped (anchor rewrites, attribute additions, element injection at a CSS selector), implement `IHtmlResponseRewriter` instead so every rewriter shares one AngleSharp parse." — clean sentence, but the lead packs the trigger condition, the alternative, and a cross-link into one breath. Split.
- [voice] "letting static assets, JSON endpoints, and redirects pass through untouched" — fine, but the next sentence's "an empty return empties the response" is borderline jokey for the register.
- [diataxis] Two `csharp:xmldocid` fences for `ShouldProcess` and `ProcessAsync` walk the interface members — reference walk inside a how-to. Same critique as `html-rewriter.md`.
- [diataxis] "The built-ins occupy 10 (`HtmlResponseRewritingProcessor`), 20 (`LiveReloadScriptProcessor`, dev-only), and 30 (`DiagnosticOverlayProcessor`, dev-only)." — built-in `Order` values listed inline; this is reference content (and duplicates the version in `html-rewriter.md`'s "Pick an Order value").
- [clarity] The `ProcessAsync` symbol fence dump shows the implementation behind a wrapper but never the literal `LastIndexOf` + splice pattern inline as a teaching pattern.
- [Q] How does the reader pick between `IResponseProcessor` and `IHtmlResponseRewriter`? Lead sentence gives a rule of thumb but the practical "if you ever touch the DOM, use rewriter" is buried.

### docs/Pennington.Docs/Content/how-to/response-pipeline/razor-page-on-bare-host.md
**Form claimed:** how-to | **Actual:** how-to — short and goal-shaped.

- [voice] "The component owns the document — `<html>`, `<head>`, `<body>` — so the response is a complete HTML page without any DocSite or BlogSite layout machinery in between." — fine framing; the next sentence "This is the pattern to reach for when…" tips into explanation register.
- [voice] "too rich for string-interpolated HTML" — colloquial; "too complex for inline HTML strings" is straighter.
- [diataxis] Section "Publish the routes through `IContentService`" has prose but no code — it pivots to "see <xref:…>" for the actual recipe; that hands the reader off without telling them what shape `EndpointSource` needs in this scenario. Either show it or drop the section.
- [clarity] The reader is told to register `IHttpContextAccessor` "so cascading values can resolve" but never told what cascading value the example uses or doesn't use — that hint dangles.
- [clarity] "Pennington appends a `<script>` block for live reload, a `<meta name="x-pennington-host">` fingerprint, and a `<link rel="canonical">`" — useful, but the "those are stripped from build output" is a Verify-only observation that adds dev-vs-build mental load to a recipe page.
- [Q] Is "Render a Razor component as a page on a bare host" the same as the bare-host Mermaid wiring in `diagrams.md`? If yes, the two pages should cross-reference each other.

### docs/Pennington.Docs/Content/how-to/rich-content/alerts.md
**Form claimed:** how-to | **Actual:** how-to — variants-under-H3 shape matches the CLAUDE.md prescription cleanly.

- [voice] "The five built-in kinds — `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION` — fix the visual treatment; pick the one whose signal strength matches the message." — clean.
- [voice] "fires only when the marker is the first inline of the first paragraph, so no leading text before it." — slightly awkward; "the marker must be the first inline of the first paragraph" reads cleaner.
- [diataxis] "What the renderer emits" is one paragraph of explanation embedded in the how-to. Short enough to pass, but watch the pattern — the same heading appears in `diagrams.md` and `ui-components-in-markdown.md` and starts to feel like a stealth explanation slot.
- [clarity] The page asserts five alert kinds and the `[!INFO]` fallback behaviour; no link to a list of kinds in reference. Probably fine if reference catalogs them.
- [Q] The intro line "Pennington recognises five kinds and paints each one differently" in the description front-matter — "paints" is a small affectation. Acceptable; flagging only because the voice guide says cut warmth that teaches nothing.

### docs/Pennington.Docs/Content/how-to/rich-content/diagrams.md
**Form claimed:** how-to | **Actual:** how-to with embedded conceptual walkthrough.

- [voice] "this page does not teach Mermaid" — bullet says it, then the body still teaches Mermaid sub-syntaxes (flowchart vs sequence). Pick one.
- [voice] "What the renderer emits" paragraph — quote: "the body is verbatim — Pennington does not transform it server-side. At page load, `MermaidManager` walks the DOM, dynamically imports Mermaid from `cdn.jsdelivr.net`…" — that is explanation prose narrating the runtime sequence, not a recipe.
- [diataxis] "Bare-host wiring" is an alternative-setup how-to embedded inside a how-to. It is small enough to live here, but the `## Diagram syntaxes` section that follows is variants, and the embedded setup section breaks the variants pattern. Consider hoisting bare-host wiring into its own how-to or moving it under "Assumptions".
- [diataxis] "For per-diagram theme overrides, use Mermaid's inline `%%{init: { 'theme': '…' } }%%` directive" — teaches Mermaid after the page disclaimed teaching Mermaid.
- [clarity] The reader sees flowchart and sequence diagrams as variants but no guidance that "any valid Mermaid renders" — until paragraph "Pennington does not preprocess the body, so anything valid in Mermaid works as is" which is buried.
- [Q] Should the CDN dependency be more prominent? A site that builds offline or behind a firewall will fail silently — currently mentioned only as "dynamically imports Mermaid from CDN".

### docs/Pennington.Docs/Content/how-to/rich-content/ui-components-in-markdown.md
**Form claimed:** how-to | **Actual:** how-to — variants-under-H3 shape is appropriate.

- [voice] "Mdazor matches the tag against the registered component types, binds attribute values to `[Parameter]` properties by case-insensitive name, and renders inner content through the markdown pipeline." — borderline explanation in the lead.
- [voice] "case-sensitive on the leading character (`<Card>`, not `<card>`)" — fine instructional aside; flagging the parenthetical density across the page generally — many sentences have inline parentheticals that slow scanning.
- [diataxis] "The seven built-in components" section says "Each H3 below shows the source markdown above the rendered output for the most common authoring shapes." but then only three H3s follow — "Inline a built-in tag", "Pass markdown as `ChildContent`", "Bind primitive attributes". The introduction promises a roster of seven and delivers a feature tour. Either list the seven or rename the section.
- [diataxis] "What the renderer emits" paragraph leans explanation; same critique as alerts and diagrams pages.
- [clarity] "Only primitive parameter types (strings, numbers, booleans) bind from markdown attributes — the value arrives as a raw string and Mdazor converts it via reflection. For complex data, pack it into a delimited string and parse inside the component, or use `ChildContent` for rich content." — the "delimited string and parse inside the component" workaround is suggested but not shown; readers facing this will need a follow-up example.
- [clarity] "Register components on a bare host" pulls `examples/DocSiteKitchenSinkExample/Program.cs` whole — kitchen-sink files routinely contain a lot of unrelated wiring; the relevant `AddMdazorComponent<T>()` line is not isolated.
- [Q] Should there be a list-of-seven section before variants — a quick reference for which built-ins exist? Currently the reader sees only `<Badge>` and `<Card>` in the examples.

### docs/Pennington.Docs/Content/how-to/theming/fonts.md
**Form claimed:** how-to | **Actual:** how-to — clean recipe shape with topical H3 variants.

- [voice] "When a DocSite needs custom display and body typefaces instead of the defaults, and those faces should load without a flash of fallback text on first paint, the knobs below cover it." — long opening; would split into two sentences.
- [voice] "(the example does not ship font binaries)" — useful parenthetical, but appears in the second sentence after the reader has already started thinking about kitchen-sink references; reorder so the disclaimer leads.
- [voice] "the perceptible delay drops by ~40 ms on a cold load" — specific number with no source; if measured, fine; if rough, this is voice-of-authority over-claim.
- [voice] Italicised link text — quote: `[_`DocSiteOptions`_](xref:reference.api.doc-site-options)` — Related-section links use underscored italics inside backticks; inconsistent with how every other how-to in the set writes Related links as plain `[…](xref:…)`.
- [diataxis] "(Optional) Match MonorailCSS utilities to your stacks" — well placed; minor note that "(Optional)" as a heading prefix is not a convention used elsewhere in the doc set.
- [Q] The bare-`AddPennington` host case isn't covered — the page assumes DocSite. If `Pennington` core surfaces a font-preload story, link it; if not, say so.

### docs/Pennington.Docs/Content/how-to/theming/monorail-css.md
**Form claimed:** how-to | **Actual:** how-to — solid topical-options shape; flag a couple of reference-creep moments.

- [voice] Title "Recolor the site" advertises a narrower outcome than the page delivers; the body also covers syntax-highlight theme, `ExtraStyles`, and prose rules through `CustomCssFrameworkSettings`. Either rename to "Restyle the site" / "Customize MonorailCSS" or narrow the body.
- [voice] "When customisations outside DocSite's scope are needed, drop to bare `AddPennington` + `AddMonorailCss`; see <xref:explanation.positioning.docsite-positioning> for the authoritative breakdown." — "drop to" is mildly colloquial; "switch to" or "use" reads straighter.
- [diataxis] Five back-to-back `csharp:xmldocid` fences for `NamedColorScheme`, `BuildColorScheme`, `DocSiteOptions.ColorScheme`, `SyntaxTheme`, `BuildExtraStyles`, `DocSiteOptions.ExtraStyles`, `DocSiteOptions.CustomCssFrameworkSettings`, `MonorailCssOptions`, `MonorailCssCustomization.BuildOptions` — that is a lot of reference pulled inline. Same critique as `cross-references.md` and the response-pipeline pair.
- [diataxis] "Familiarity with the `NamedColorScheme` defaults baked into `MonorailCssOptions`" in Assumptions then sends the reader to reference "if needed" — this is fine, but the recipe could land harder if it skipped the symbol fence for `NamedColorScheme` and just showed the three `*ColorName` assignments inline.
- [clarity] The page sets up two color-scheme paths (`NamedColorScheme`, `AlgorithmicColorScheme`) and then assigns the scheme via `DocSiteOptions.ColorScheme` — but never shows the constructor call shape for either, only the type fence. The reader has to read the symbol output to know how to instantiate.
- [Q] Three different "bare-host" footnotes appear: `AddDocSite or AddBlogSite host … wiring AddPennington directly requires a separate AddMonorailCss call`, then later a `MonorailCssCustomization.BuildOptions` fence. Could these be consolidated into one "Bare-host wiring" subsection?

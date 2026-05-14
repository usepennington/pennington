# Pennington docs audit worklist

Findings from a tone-and-form pass over every doc in `docs/Pennington.Docs/Content/`. Two lenses:

- **Voice** — `docs/docs-voice.md`. Confident, warm-not-chatty; mode-specific registers for tutorial / how-to / reference / explanation.
- **Form** — Diátaxis. Each page should serve exactly one of the four modes; mixed forms get flagged for splitting or linking out.

Target reader assumed throughout: an experienced C# / .NET developer adding documentation to their project, or writing a blog. They know C# and ASP.NET. They don't know Pennington.

Each entry follows:

```
**Form claimed:** <kind> | **Actual:** <assessment>

- [voice]    voice-guide violation (quoted where applicable)
- [diataxis] form drift (explanation in a tutorial, how-to in reference, etc.)
- [clarity]  gap that blocks the target reader
- [Q]        question or concern for the author
```

`Clean.` means no findings under any bucket.

---

## Cross-cutting findings

These patterns repeat across the doc set. Each is more useful to address holistically than one page at a time.

1. **Feature-tour anti-pattern in how-tos.** Pages that walk an interface member-by-member (declare it, fence its signature, repeat) are reference content dressed as recipe. Strongest examples: `markdown-pipeline/custom-highlighter.md`, `markdown-pipeline/code-block-preprocessor.md`, `response-pipeline/html-rewriter.md`, `response-pipeline/response-processor.md`, `content-services/custom-content-service.md`, `content-services/emit-generated-artifacts.md`, `pages/front-matter.md`. CLAUDE.md already calls this out — these pages should reduce to "show the one or two non-obvious moves" and link to reference for the surface.

2. **`<Steps>` used for non-sequential or single-step content.** `<Steps>` implies dependency between steps; the doc set routinely uses it for variants (`adapt-for-other-hosts.md`, `multiple-sources.md`, `custom-highlighter.md`), single-step units (`tutorials/blogsite/hero-projects-socials.md` — all four units; `tutorials/getting-started/styling.md` units 3 & 4), or decision trees (`multiple-sources.md` step 1). Convert to H3-per-variant under topical H2s, per the project's own guidance.

3. **Non-result tutorial steps.** Tutorial steps verbed "See the X state", "Review the X contract", "Confirm the X looks like Y" appear across `first-site.md`, `getting-started/first-page.md`, `docsite/scaffold.md`, `blogsite/scaffold.md`, `beyond-basics/add-a-locale.md`, `beyond-basics/connect-roslyn.md`. Diátaxis tutorials want a visible result per step; "see what's on the screen" isn't a step.

4. **"Build succeeds, runtime returns 404" checkpoints.** The getting-started tutorials in particular repeatedly tell the reader to confirm a non-event ("the bare host still 404s; that is expected"). Tutorial checkpoints should reward action with a visible result. Where the unit really doesn't produce a visible change, the unit's granularity is wrong — fold into the next one.

5. **Feature-name and noun-phrase titles.** A how-to title states an outcome ("Deploy to GitHub Pages"); a tutorial title states a learning result. Offenders: `pages/redirects.md` ("Configure redirects"), `pages/front-matter.md` ("Work with front matter"), `feeds/blogsite-homepage.md` ("Wire the blog homepage hero" — covers four surfaces), `theming/monorail-css.md` ("Recolor the site" — covers more), `response-pipeline/override-docsite-components.md` ("Replace the docsite header or footer" — covers more), `tutorials/getting-started/first-page.md` ("Using Blazor Pages"), `tutorials/blogsite/first-post.md` ("Author your first post with BlogSiteFrontMatter").

6. **"What the renderer emits" sections.** A stealth-explanation pattern that recurs across `rich-content/alerts.md`, `rich-content/diagrams.md`, `rich-content/ui-components-in-markdown.md`, `code-samples/tabbed-code.md`, `code-samples/code-annotations.md`. Individually short, collectively a drift trend. Either pull these into a single "How rendering works" explanation page and link out, or accept them as a deliberate section type — but then make them consistent.

7. **Reference tables inside how-tos.** Built-in `Order` values listed inline in both `html-rewriter.md` and `response-processor.md`; "Other DocSite extension points" table in `override-docsite-components.md`; applicability matrix in `drafts-tags-ordering.md`; default-priority paragraph in `discovery/search.md`. These belong in reference with a link.

8. **Reference pages with how-to leakage.** "Persistent chrome" (Mark/Listen/Read procedure) in `reference/spa/attributes.md`; Mdazor-registration paragraph in `reference/ui/content.md`; "Reference from `SocialLink.Icon`" with one-line syntax block in `reference/blogsite/social-icons.md`; middleware-order section in `reference/host/extensions.md`; explanation-paragraph leads on every H2 of `reference/markdown/extensions.md`. Reference should describe the surface, not advise on use.

9. **Reference pages with explanation creep.** `reference/host/extensions.md` is *mostly* explanation about why each middleware sits where it does. `reference/host/cli.md` Commands table embeds multi-step pipeline narrative in "Effect" cells. `reference/blogsite/routes.md` has a 60-word Note callout explaining why two URLs aren't templated. `reference/diagnostics/request-context.md` has narrative examples and gating advice.

10. **Inconsistent entry formatting within reference.** `reference/ui/utility.md` mixes inline `:path` fences with standalone parameter tables; `reference/diagnostics/request-context.md` uses bullets + `<FieldList>` + a table for three sibling type surfaces; `reference/markdown/extensions.md` uses a different shape for arguments per extension; `reference/spa/attributes.md` tables key differently per section. Pick one shape per page, apply it everywhere.

11. **Author TODOs in published prose.** `discovery/multiple-sources.md` ships `_(confirm path)_` placeholders in Related links — these are unverified xref uids.

12. **Italicized link text artifacts.** `[_Some text_](xref:…)` appears in `theming/fonts.md`, `feeds/blogsite-homepage.md`, `discovery/multiple-sources.md`. Most other how-tos use plain `[Some text](xref:…)`. Pick one — the underscores look like Markdown-rendering noise.

13. **"Assumptions" vs "Before you begin" used interchangeably.** Two heading conventions for the same idea across the how-to set. Pick one.

14. **Concept names introduced without links.** Recurring offenders across tutorials and how-tos: `DocFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`, `ContentResolver`, `NavigationBuilder`, `XrefResolver`, `class collector`, `FallbackNotice`, `Mdazor`, `LayoutComponentBase`, `LanguageSwitcher`, `RenderFragment`, `RoslynCodeBlockPreprocessor`, `SolutionWorkspaceService`, `ConfigurePennington`. First mention should link to reference or explanation.

15. **Banned-word and Latin-abbreviation hits.** Light: `just` in `tutorials/beyond-basics/custom-razor-component.md` ("styling is just utility-class swaps"); `etc.` in `code-samples/code-annotations.md`. The voice guide bans both.

16. **Admonition ceiling pressure.** `tutorials/getting-started/first-site.md` and `tutorials/beyond-basics/connect-roslyn.md` carry three admonitions across the page; the guide caps at two. `how-to/discovery/localization.md` opens with a TIP admonition pointing at a tutorial — usually unnecessary at the top of a how-to.

17. **Outcome-vs-body scope mismatches.** Titles advertising a narrower outcome than the body delivers: `override-docsite-components.md` (header/footer → also head injection, extra styles, routing assemblies), `theming/monorail-css.md` (recolor → also syntax theme, ExtraStyles, prose), `feeds/blogsite-homepage.md` (hero → four surfaces), `pages/images-and-assets.md` (colocated → also wwwroot/ and ExcludePaths).

18. **"Section labels" filing bugs.** `feeds/blogsite-homepage.md` is categorised under "Feeds & Indexes" but covers layout, not feeds.

19. **Single-doc structural questions to settle.**
    - `tutorials/docsite/first-doc-page.md` is half tutorial / half how-to comparing three link forms — split.
    - `tutorials/docsite/sections-and-areas.md` is half tutorial / half explanation of the sidebar model — split.
    - `explanation/core/content-source.md` is mostly how-to with explanation framing — repurpose.
    - `content-services/auto-api-reference.md` stitches six discrete how-tos into one page — split per backend / outcome.
    - `pages/drafts-tags-ordering.md` bundles three independent goals — split.
    - `deployment/self-host.md` covers Nginx + IIS in one recipe — consider splitting per server.
    - `pages/front-matter.md` walks a feature member-by-member — repurpose as "Define custom front-matter keys" (keep Steps 4-5).
    - `code-samples/focused-code-samples.md` covers five techniques — split into "Scope a fence to a member" + "Diff two implementations".

---

## Tutorials

### docs/Pennington.Docs/Content/tutorials/getting-started/first-site.md
**Form claimed:** tutorial | **Actual:** tutorial with explanation drift — mostly correct shape but the prose keeps explaining instead of walking.

- [voice] Tutorial register is missing the warmest "we'll" voice in spots; the second paragraph reads as explanation: "The tutorial covers how to wire…" rather than tutorial register.
- [voice] Phrase "for now, MapGet keeps the URL → markdown file → rendered HTML chain visible in one place" — explanation tone in tutorial; could be cut.
- [diataxis] The intro after the H1 ("The tutorial covers how to wire…") is meta-narration about what the tutorial does, not a tutorial opening that gets the reader moving. Diataxis tutorials should start doing, not summarising.
- [diataxis] Step 1.4 "Confirm the bare host runs" embeds an `xmldocid` snippet (`M:GettingStartedMinimalSiteExample.Stage1.Run`) for what should be the unmodified `dotnet new web` output — the reader has not been shown the file to "confirm" against and the body says "Before adding Pennington, `Program.cs` looks like this" which is more explanation than verification.
- [diataxis] Section 2 has a `<Checkpoint>` that explicitly says do NOT run the site — a tutorial checkpoint should produce a visible result; "build succeeds, do not run" is a non-result and confuses the reader expecting reward.
- [clarity] `DocFrontMatter` is referenced (`AddMarkdownContent<DocFrontMatter>`) but never explained or linked. The reader doesn't know what type that is or why it's the right choice for a non-doc site.
- [clarity] `RunOrBuildAsync` is introduced with "the same host that serves live today will generate static HTML tomorrow with no code change" — promise made but no link to the build doc; reader can't follow up.
- [clarity] Stage 2 example uses `Content/index.md` at `examples/GettingStartedMinimalSiteExample/Content/index.md`, embedded by path, but the reader is told to "add `index.md` with the contents below" — the rendered fence will show only the file contents, not the exact authoring shape with the YAML fence (front-matter markers) called out separately.
- [Q] The two `NOTE` admonitions about skipping to DocSite (one at the top, one near the bottom) is two callouts of the same idea — likely over the 2-admonition ceiling once the IMPORTANT alpha-version admonition is counted (three total).

### docs/Pennington.Docs/Content/tutorials/getting-started/first-page.md
**Form claimed:** tutorial | **Actual:** tutorial, but title is a feature name not an outcome and several units lean explanatory.

- [voice] Title "Using Blazor Pages" — gerund phrase, feature-name register; tutorials should describe a result ("Serve markdown through a Blazor catch-all" or similar). Guide isn't explicit about tutorial titles but it's a how-to-style title on a tutorial page.
- [voice] "that's a fine teaching shape, but it's not the shape a real app stays in" — chatty aside, second informal-aside on a single intro paragraph; voice rule is one per page.
- [voice] Heading casing: section 2 heading "Wire Pennington and Blazor in `Program.cs`" — fine. But the bullet list under it ("A walk-through of the calls:") drops into explanation register inside a tutorial unit — six bullets explaining each call instead of a one-line "what just happened."
- [diataxis] Section 2 reads as explanation, not tutorial — long bullet list documenting each service registration and middleware call. Diataxis tutorials minimise explanation and link to it; this content belongs in an explanation page.
- [diataxis] Section 2's `<Checkpoint>` again confirms a 404 is the expected output — same "non-result" issue as first-site.md.
- [diataxis] Step 3.3 explains `MarkdownPage.razor` injecting `IEnumerable<IContentService>`, `IContentParser`, `IContentRenderer` and walking them — that level of why-detail is explanation, not tutorial. Cut or link out.
- [clarity] `(MarkupString)` is mentioned without indicating why — Blazor folks know it but newer readers don't; either drop the rationale or link to Blazor docs.
- [clarity] Section 4 step 3 says "Rename it back to `about.md` before continuing so the next tutorial matches" — fine, but the next tutorial (styling) doesn't actually depend on the filename; this looks like a fragile coupling between tutorials.
- [Q] Section 4 step 3 ("Rename the file to see the URL follow it") teaches a property of the file watcher rather than producing a step toward the tutorial's stated end-state. Feels like demo, not tutorial step.

### docs/Pennington.Docs/Content/tutorials/getting-started/styling.md
**Form claimed:** tutorial | **Actual:** tutorial, generally aligned but voice slips into explanation in places.

- [voice] "Let's confirm the baseline before touching anything" — tutorial register, good. But "keeping DI registration separate from the endpoint wiring makes it easier to pinpoint problems" — explanation-register reasoning that doesn't progress the tutorial.
- [voice] "One line stands between here and a live stylesheet" — borderline chatty/clever; fine as one informal aside but check page-wide count (the intro and unit headings have similar flourishes).
- [diataxis] Unit 3 is a single-step "call this one method" unit — under `<Steps>` wrapper with one step. The guide doesn't require multi-step `<Steps>` blocks, but a one-step unit smells like the granularity is wrong; consider merging units 3 and 4, since registration without `UseMonorailCss` produces no visible result either time.
- [diataxis] Unit 3 checkpoint: "Pages still render unstyled… still 404s." A second "build succeeds, nothing renders" non-result checkpoint, same pattern as the other getting-started pages. Tutorials should produce visible progress per step.
- [clarity] `LayoutComponentBase` is mentioned once — fine for Blazor folks per the prereqs, but `class collector` (mentioned three times) is a Pennington concept without a link to an explanation page.
- [clarity] `NamedColorScheme` and `ColorName` are referenced without an authoritative reference link; "indigo/pink/slate; any combination works" leaves the reader to guess the surface.
- [Q] Step 5.2 has the reader add an inline `<p class="text-accent-600 italic">` to a markdown file — this works, but doesn't actually exercise the class collector against utility classes in *content* HTML (it's HTML in markdown); the tutorial promises "watch the stylesheet regenerate as new utility classes appear" but the realistic case is utility classes generated by rendered markdown chrome, not author-written HTML. Worth flagging.

### docs/Pennington.Docs/Content/tutorials/docsite/scaffold.md
**Form claimed:** tutorial | **Actual:** tutorial-with-reference-drift; sections 2 and 4 have entries that are documentation, not steps.

- [voice] Description "Stand up the DocSite template on an empty ASP.NET project and map content areas to top-level folders." — fragment, fine.
- [voice] "DocSite is a fast-path template — for the knobs it hard-codes, see…" appears in the intro and again in the summary — repetition, and "knobs it hard-codes" is jargon-y without prior introduction.
- [diataxis] Step 2.1 ("Add the registration call") has no code, no concrete action — just prose pointing at the reference. That's not a step.
- [diataxis] Step 2.2 ("Populate `DocSiteOptions`") again has no code in the step itself — the actual options block lives down at step 2.3 ("See the registration-only state"). The "see" verb is also non-tutorial register; tutorial steps tell the reader what to do, not what to look at.
- [diataxis] Step 4.1 ("Review the `ContentArea` contract") has a `csharp` code fence showing the record declaration — that's reference, not a step. The reader does not type or run anything.
- [diataxis] Step 4.3 ("Confirm the two-area `Areas` list") has no code and no action — just narrates that "the `Areas` block in the fully-wired host has exactly two `ContentArea` entries." Reads as explanation.
- [clarity] `ContentResolver`, `DocSiteArticleSlotRenderer`, `Pages.razor`, and the `/{*fileName:nonfile}` route constraint are all named but never explained or linked. The reader cannot map these to anything actionable.
- [clarity] Section 5 introduces "root `/` sits **outside** every area" and references `DocSiteFrontMatter` shape — the front-matter shape was not introduced in this tutorial or the prereq path; first-doc-page.md is the page that introduces it but is listed as the *next* tutorial.
- [Q] Three `xmldocid` Stage1/Stage2/Stage3 host snapshots is heavy for a tutorial; readers struggle to track three near-identical `Program.cs` versions. Are the intermediate stages doing enough work to justify their presence?

### docs/Pennington.Docs/Content/tutorials/docsite/first-doc-page.md
**Form claimed:** tutorial | **Actual:** tutorial leaning toward teaching — unit 4 in particular is explanation about link-rewriting strategies.

- [voice] "Let's drop two markdown files…" and "Let's rewrite it as a hub" — good tutorial register, consistent.
- [voice] Tip admonition "Relative paths are the right pick for tightly coupled siblings. Reach for the other two forms in the next two units when…" — this is teaching/explanation content inside a tutorial; should be linked to an explanation page about link forms.
- [voice] "handy on a hub that may itself migrate later" — soft hedging that the voice guide pushes against; "Confident not arrogant" prefers a direct statement.
- [diataxis] The whole tutorial is structured around teaching three link forms. The diataxis split says tutorials are "learning by doing" and minimise explanation — this page is teaching with checkpoints. Reframe as an outcome ("Build a Guides area with two cross-linked pages") and let the reader discover the link forms through doing rather than narrating them.
- [diataxis] Step 4.2's checkpoint instructs the reader to rename a file to confirm xref behavior, then "restore the filename when done experimenting." That's experimentation, not progression — feels like explanation in disguise.
- [clarity] `DocSiteFrontMatter`, `XrefResolver`, `NavigationInfo.SectionName` named but not linked. `uid:` is described inline; that's fine.
- [clarity] Summary's "Use it when the target file is likely to move or get renamed" — that's how-to/explanation register, not tutorial outcome.
- [Q] The page audits as much of a how-to comparing three link forms as a tutorial. Could split into a tutorial ("Add two doc pages") and a how-to ("Link between doc pages").

### docs/Pennington.Docs/Content/tutorials/docsite/sections-and-areas.md
**Form claimed:** tutorial | **Actual:** tutorial with strong explanation interleaved — at times reads like an explanation page with checkpoints bolted on.

- [voice] "Let's begin with a single page parked directly under an area folder" — good tutorial register.
- [voice] "The load-bearing rule: **the subfolder name is what creates the sidebar section**, not the `sectionLabel:` key." — "load-bearing rule" appears twice on the page (also in §3 "the navigation builder falls back to alphabetical ordering"). Repeated jargon flourish.
- [voice] "the tie-break surprise" — informal aside count: this plus "smaller numbers appear first, with ties broken alphabetically" plus "the 10/20/30 sequence is deliberate" — borderline chatty across the page.
- [diataxis] Step 2.1 has no code action — narrates "the load-bearing rule" before any rename happens. The actual rename ("Delete `Content/guides/install.md` and create `Content/guides/getting-started/installation.md`") is given as plain prose, no command, no file path verification.
- [diataxis] Step 2.2 contains paragraph-length explanation of `sectionLabel:` vs. `order:` — explanation in a tutorial step. Move to an explanation page and link.
- [diataxis] Section 3 step 1's last paragraph "The 10/20/30 sequence is deliberate — it leaves room to insert pages later without renumbering everything" — explanation.
- [diataxis] Section 3 step 2's last paragraph "Stagger `order:` values across sibling sections… so the two section headers sort in the intended order. When both sections start at `10`, the navigation builder falls back to alphabetical ordering of the folder names, and `advanced/` appears above *Getting Started*." — full why-explanation in a tutorial step.
- [clarity] `NavigationBuilder`, `NavigationInfo.SectionName`, `int.MaxValue` default for `order` are named without links.
- [clarity] The example uses six markdown files (index, two getting-started, two advanced, plus reference area) and the tutorial doesn't show any actual file content for them — every `:path` fence pulls from disk. The reader can follow without seeing what's inside any individual file; that's fine for shape but means the result on screen depends on disk content.
- [Q] This page genuinely teaches a model (subfolder = section, staggered order = predictability). That's explanation work. Should it be split: a short tutorial "Group your docs into sections" + an explanation "How DocSite builds the sidebar"?

### docs/Pennington.Docs/Content/tutorials/blogsite/scaffold.md
**Form claimed:** tutorial | **Actual:** tutorial with reference-style sub-sections.

- [voice] "Along the way, you'll see how to swap any plain Pennington host…" — tutorial register, OK; but in the same intro paragraph the "with a clear mental model of how `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, and `TagsPageUrl` work together" promises a *mental model*, which is explanation work.
- [voice] "The green diff markers show what's new; everything outside them is plain ASP.NET scaffolding." — meta-narration about the rendered output; flag this as drift since the tutorial register should hand the reader what to type, not describe what the diff looks like.
- [voice] "No DocSite experience is required — BlogSite is a separate template" — fine, but "Before starting, gather the following" is wordy.
- [diataxis] Section 2 explains "the options fall into three families" with three bullet groupings of fields — that's reference/explanation register inside a tutorial step. Move to a reference page or a BlogSiteOptions explanation page.
- [diataxis] Section 3 step 3 ("See the fully-wired host") — non-action step ("See the…"). The reader is not doing anything new in this step.
- [diataxis] Section 4 step 2 ("Walk the built-in routes") lists six URLs as a verification action — that's fine, but the implicit message is "now confirm the template's full route surface." That belongs in a reference page that lists built-in routes; the tutorial step could simply check `/` and `/rss.xml`.
- [diataxis] Section 2's checkpoint is again a "build succeeds, runtime returns default ASP.NET" non-result.
- [clarity] `BlogContentResolver`, `BlogSiteContentService`, `BlogSiteFrontMatter`, `DocFrontMatter` all mentioned without links from this page (BlogSiteFrontMatter is the subject of the next tutorial, which is fine).
- [clarity] "BlogSite binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` — not the core `BlogFrontMatter`" in the summary — this distinction (`BlogFrontMatter` vs `BlogSiteFrontMatter`) isn't introduced in the body; the reader meets `BlogFrontMatter` here for the first time, in the summary, in a contrast clause.
- [Q] The "`/topics` aliases" mention in the intro and again in step 4.2 is presented as a verify-each-URL item — does that match the depth a beginner needs in their first BlogSite tutorial?

### docs/Pennington.Docs/Content/tutorials/blogsite/first-post.md
**Form claimed:** tutorial | **Actual:** tutorial, mostly aligned, but section 2 step 1 batches up explanation.

- [voice] "Author your first post with BlogSiteFrontMatter" — title is feature-name register; tutorial titles in this set vary (the getting-started one is also feature-name). Closer to "Write a post that lights up every surface" or similar.
- [voice] "every `BlogSiteFrontMatter` field a post author touches comes into view" — overwrought; could be plain "every front-matter field the template reads."
- [voice] "Now to make the RSS wiring explicit" — slightly stilted register, but OK.
- [diataxis] Section 2 step 1 — six-bullet explanation of what each new YAML key does, then a single code embed. That bullet list is reference content; consider linking to <xref:reference.api.blog-site-front-matter> (which the page already links) and showing only one or two keys' effects inline.
- [diataxis] Section 3 step 1 — "Set `EnableRss = true` explicitly… mirrors the default… but makes the intent clear" — explaining defaults in a tutorial step is wasted motion. The unit's stated purpose ("turn on the built-in RSS feed") is also a no-op if `EnableRss` defaults to true; nothing actually changes.
- [diataxis] Section 3 step 2's checkpoint says "the browser either renders the feed (Firefox) or shows raw XML (Chrome/Edge)" — caveat-heavy result statement that hedges what success looks like.
- [clarity] `redirectUrl:` is included in section 2 with the note "stays empty here because this post has no previous home on the web" — leaving an empty value in YAML is itself a teaching moment; if it's empty, drop the line and link to a how-to.
- [clarity] Section 3's `EnableRss` step has no observable diff in the running site (default = true). The tutorial promises "turn on the built-in RSS feed" but nothing was actually off.
- [Q] Section 3 is essentially "make a no-op edit, then visit `/rss.xml`." Worth keeping as a unit, or fold the `/rss.xml` verification into section 2's checkpoint?

### docs/Pennington.Docs/Content/tutorials/blogsite/hero-projects-socials.md
**Form claimed:** tutorial | **Actual:** tutorial; one-step units throughout suggest the granularity is too fine.

- [voice] Title sentence-case OK. Description "Populate the four BlogSite homepage surfaces…" — fragment, fine.
- [voice] "Along the way, `HeroContent`, `Project`, `SocialLink`, and `HeaderLink` on `BlogSiteOptions` come into play, plus the four built-in icon `RenderFragment` fields from `SocialIcons` — all without a line of Razor." — feature-name parade in an intro, hard to parse for a first-time reader. Lead with the outcome, not the type roster.
- [voice] "The four homepage surfaces on `BlogSiteOptions` — hero, work, socials, header links — are in hand…" in the summary — "in hand" is a flourish.
- [diataxis] Every one of units 1–4 is a single-step unit wrapped in `<Steps><Step StepNumber="1">…</Step></Steps>`. The CLAUDE.md guidance is that `<Steps>` implies multi-step sequence; a one-step unit doesn't need the wrapper.
- [diataxis] Unit 4 step 1 ("Confirm the three header links resolve") — "this step exists to verify that the nav URLs line up with the routes BlogSite exposes out of the box" — that's not a step, it's a tautology. The unit could merge into unit 3.
- [diataxis] Unit 3 explains the rule for `RenderFragment` field references ("pass the field itself, not `typeof(...)` and not `<GithubIcon />`") — useful, but it's reference material about Mdazor / Blazor binding rather than a tutorial step.
- [clarity] `RenderFragment` is mentioned without a link; the audience is C#/.NET developers so the Blazor concept is fine, but the *Pennington* convention of passing the field is novel and would benefit from a reference link.
- [clarity] "the four built-in icon `RenderFragment` fields from `SocialIcons`" — `Pennington.BlogSite.Components.SocialIcons` is the type, no link to a reference page.
- [Q] Units 2, 3, and 4 each add roughly one option block to `BlogSiteOptions`. Could fold into one unit with three steps, since each step is a single property edit.

### docs/Pennington.Docs/Content/tutorials/beyond-basics/add-a-locale.md
**Form claimed:** tutorial | **Actual:** tutorial, generally well-shaped; some explanation pockets.

- [voice] "By the end of this tutorial you'll have a running DocSite…" — tutorial register, good.
- [voice] "A single `ConfigureLocalization` action on `DocSiteOptions` is the toggle that enables multi-locale behavior." in the intro and "That one change activates every piece of locale routing, link rewriting, and UI chrome downstream." in section 2 — same idea repeated; pick one.
- [voice] Step 4.2 "That URL rewriting is the switcher's entire job — no client-side state, no cookies involved." — voice is good ("Confident not arrogant"), but the trailing fact is explanation in a tutorial step.
- [diataxis] Section 2 step 2 ("Leave `UseDocSite()` alone") is a no-op step that exists to explain that no extra middleware is needed. Move to a callout or summary line; "leave X alone" is not a tutorial step.
- [diataxis] Section 4 step 1 ("Confirm the final host shape matches `Program.cs`") — "This is a sanity-check step, not a new code change." Explicitly admits to being non-progress.
- [diataxis] Section 1 step 1 ("Confirm the English-only host") again uses the "show what the host looks like with no change" pattern — same observed pattern as docsite/scaffold.md.
- [clarity] `LocalizationOptions.IsMultiLocale` is mentioned three times — first as "downstream," then with `xref` link, then bare. The first mention should be linked.
- [clarity] `FallbackNotice` is named but not linked or described — what does it look like? A reader who hasn't seen it can't predict the rendered banner.
- [clarity] "The `LanguageSwitcher` is already wired into DocSite chrome and stays hidden until a second locale is registered" — the conditional show/hide rule is helpful but the only `LanguageSwitcher` link comes much later (`<xref:reference.ui.utility>`); link earlier.
- [Q] The tutorial conflates "register a locale" and "translate content"; the prereq path doesn't say the reader needs Spanish copy ready. The example files supply it — should the intro flag that translations are provided in the example folder?

### docs/Pennington.Docs/Content/tutorials/beyond-basics/connect-roslyn.md
**Form claimed:** tutorial | **Actual:** tutorial fused with substantial reference/explanation about MSBuild requirements.

- [voice] Title "Connect to a Roslyn solution for live API snippets" — outcome-shaped, fine.
- [voice] "the workspace is hot and ready to resolve XmlDocIds" — chatty/clever, borderline; voice rule is one informal aside per page.
- [voice] "every `csharp:xmldocid` fence renders an error comment instead of source" — direct, OK. But the IMPORTANT admonition block goes deep into MSBuild internals ("BuildHost.dll not found", "Microsoft.Build.Framework reference (with runtime excluded)") in a tutorial. That's troubleshooting/reference.
- [voice] "we'll build a tiny one in unit 1" in prereqs — informal-aside register, fine, but combined with the rest of the page's tone count it's the second one.
- [diataxis] Section 2 has an `IMPORTANT` admonition explaining MSBuild workspace internals — that's reference. The admonition count for the page now includes this IMPORTANT and the NOTE in step 1.4 about `.slnx` vs `.sln` (two, the ceiling).
- [diataxis] Section 2 step 3 ("See the options surface") — non-action step describing `RoslynOptions` fields. Reference content in a tutorial.
- [diataxis] Section 2 step 4 ("See the registration-only state") — yet another `Stage2.Run` snapshot, same pattern; non-action verb "See."
- [diataxis] Section 1 step 2 ("Add a sibling `Sample` class library") — the instruction "Drop a `Sample/BeyondRoslynExample.Sample.csproj` folder next to the host csproj" doesn't tell the reader how to create the csproj (no `dotnet new classlib`, no template). For a tutorial that should be a complete recipe, this is a gap.
- [diataxis] Section 1 step 3 ("Add two small types to fence") asks the reader to "add" types but the snippets are `csharp:xmldocid` fences pointing into the *example's own* compiled solution — the reader cannot xmldocid-fence types they have not yet created. Chicken-and-egg in the rendered output.
- [clarity] `XmlDocId`, `SyntaxHighlighter`, `RoslynCodeBlockPreprocessor`, `SolutionWorkspaceService` named with xmldocid-style links but no doc-page links; the reader cannot follow these names to background.
- [clarity] The `DefaultItemExcludes` workaround is presented as an inline csproj fragment with no reference link; a beginner doesn't know this is a Pennington-specific quirk vs. a general MSBuild pattern.
- [clarity] Section 3 instructs "Add `Content/api-pulls.md` with a front-matter block (`title`, `description`, `order`) and a heading. The next step fences a type from the Sample library into it." — no example of the page header to verify against, leaving the reader to guess.
- [Q] Sections 1.2 + 1.3 collectively assume the reader can stand up a class library and define types from scratch; the page is in `beyond-basics/`, but that's a tutorial-mode register break — the page is more how-to-shaped than tutorial-shaped.

### docs/Pennington.Docs/Content/tutorials/beyond-basics/custom-razor-component.md
**Form claimed:** tutorial | **Actual:** tutorial, well-shaped overall; a few explanation/reference slips.

- [voice] "Two rules govern how the page works. Tag-name matching is case-sensitive on the leading character… Attribute-to-parameter binding is case-insensitive via reflection…" — explanation register inside a tutorial step.
- [voice] "Treat the snippet as the starting point and the disk file as the production-ready endpoint — the tutorial never re-fences the styled version because once the wiring works (next unit), styling is just utility-class swaps." — "just" is a banned word ("just utility-class swaps"), and the paragraph is meta-narration about the tutorial structure rather than a step.
- [diataxis] Section 1 step 2's final paragraph (about the styled disk file vs. the snippet starting form) is tutorial-meta-narration; cut.
- [diataxis] Section 2 step 2 ("Confirm the host still boots") — non-action verification step that admits "No markdown change has been made yet, so the site renders exactly as it did before — the new wiring stays invisible until a page consumes the tag." That's a step that explicitly produces no visible change.
- [diataxis] Section 4 (two steps editing the markdown) is largely a knob-tweak demo, not a tutorial outcome. Could be a single step or moved into section 3's checkpoint.
- [clarity] `AddMdazor()` is mentioned as "DocSite already calls" but never linked; reader doesn't know what it is or where it lives.
- [clarity] "Mdazor binds only primitive parameter types from markdown attributes; lists arrive as strings and are split inside the component" — important rule given without a link to Mdazor docs or a Pennington reference page.
- [clarity] "Boolean attribute values from markdown bind with case-insensitive `true` / `false`" — same rule-without-link issue.
- [Q] The 4-unit structure (author the component, register it, consume it, tweak parameters) has a strong "tutorial" arc. But units 2 and 4 each have a checkpoint that produces no new visible result beyond the unit before it. Consider folding units 2 + 3 (register + consume produce the first visible card together) and 4 into section 3's checkpoint.

---

## How-to: deployment + pages

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

---

## How-to: feeds + code-samples + discovery + content-services

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
- [diataxis] Section "Render reference fragments inline" with five H3s for `<ApiSummary>`, `<ApiMemberTable>`, `<ApiParameterTable>`, `<ExtensionMethods>` is a component catalog — useful, but a catalog of Mdazor components is reference material, not a how-to recipe. The components themselves are the right approach; the *placement* (six outcomes plus a component catalog on one page) is the issue.
- [diataxis] No "Verify" section per outcome — one Verify at the end covers only the default backend; the multi-library and narrow-scope outcomes have no verification.
- [clarity] The reflection-backend section uses `FromPackageReference("Spectre.Console")` without ever introducing what `FromPackageReference` is on first reading — it's named in passing, then explained, then used again.
- [clarity] Target reader (experienced C#/.NET dev wanting an API reference) hits six different decisions in one page; splitting per backend would let each page front-load its own assumptions and verification.
- [Q] Is this intentionally a "feature catalog" page, or should it split into "Generate an API reference from a Roslyn workspace", "…from a compiled assembly", "Document multiple libraries", and a reference page for the Mdazor components?

---

## How-to: markdown-pipeline + navigation + response-pipeline + rich-content + theming

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
- [diataxis] Title "Replace the docsite header or footer" advertises a narrower outcome than the body covers (head injection, extra styles, routing assemblies). Either narrow the body or broaden the title; right now the title mismatches scope.
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

---

## Reference

### docs/Pennington.Docs/Content/reference/host/cli.md
**Form claimed:** reference | **Actual:** reference with explanation creep — tables are consistent, but several rows and surrounding sentences narrate rationale instead of stating facts.

- [clarity] Opening sentence is ungrammatical: "The command-line surface `RunOrBuildAsync` dispatches on — one positional verb (`build`)…" reads as a sentence fragment; the em-dash interrupts what should be "the command-line surface that `RunOrBuildAsync` dispatches on".
- [diataxis] Commands table "Effect" cells embed multi-step prose ("Static build: `app.StartAsync()`, resolve `OutputGenerationService`, HTTP-crawl…, write…, print…, set `ExitCode = 1`, `app.StopAsync()`"). That is pipeline narrative belonging to an explanation page; the reference row should state what the command does, not retell the algorithm.
- [diataxis] "_anything else_" row carries authorial rationale: "guards against `dotnet test` / `dotnet watch` emitting stray positional args". Move to explanation or strip.
- [diataxis] Standalone paragraph "Pennington does not override any of these — the library adds middleware and endpoints on top of whatever URL Kestrel is told to listen on." is explanatory framing, not lookup material.
- [clarity] Positional argument descriptions say "Promoted to `args[2]`'s slot if `--base-url` was already supplied" without ever defining "promoted" or the parsing precedence elsewhere on the page. The reader can't predict behavior from this row alone.
- [Q] Environment-variable table includes `ASPNETCORE_ENVIRONMENT` only to note Pennington does *not* read it — is that worth a row, or should it move to a "What Pennington ignores" line?
- [Q] "Listening port" section duplicates the `ASPNETCORE_URLS` row already in "Environment variables". Pick one location.

### docs/Pennington.Docs/Content/reference/host/extensions.md
**Form claimed:** reference | **Actual:** mostly explanation/how-to dressed as reference — there is almost no looked-up information on this page, only narrative about ordering and an example.

- [diataxis] The "`UseDocSite` middleware order" section is pure explanation: six numbered steps, each justifying *why* that middleware is positioned there ("must run first so subsequent middleware sees…", "placement before `UseStaticFiles` lets antiforgery validation skip…"). Reference should list the order and link to an explanation page for the rationale.
- [diataxis] Sentence "Ordering within a `Use*` call chain is load-bearing; see each method's xmldoc for the invariant." is instructional editorial, not a lookup fact.
- [diataxis] "The same three-call shape holds for every template: `Add*` builds the service graph, `Use*` mounts the middleware and endpoints, `Run*Async` reads `args` and either serves or builds." — explanatory summary, not reference.
- [diataxis] "`UseBlogSite` follows the same shape with one difference: no `UsePenningtonLocaleRouting` (BlogSite is currently single-locale)." — narrative caveat plus a roadmap hint ("currently") that dates the page.
- [Q] Is the "Host runtime helpers" H2 carrying any reference content, or is it just a one-paragraph pointer to another page? It reads as filler.

### docs/Pennington.Docs/Content/reference/markdown/code-block-args.md
**Form claimed:** reference | **Actual:** reference with mild explanation seam.

- [diataxis] Second paragraph ("This page is the grammar spec. For task-oriented usage see…") is meta-navigation in body prose; replace with an admonition or a "See also" entry.
- [diataxis] Lead-in for "Attributes" includes editorial guidance — "A custom Markdig extension registered into the pipeline can read additional keys directly from `FencedCodeBlock.Arguments`." That is how-to leakage; reference should describe the attribute surface, not advise on extending it.
- [clarity] Suffix-form table's "Description" cells are written as instructions ("Embeds each symbol's declaration and body, concatenated in order", "Prepends the file-local `using` directives…"). Acceptable, but the row for `<lang>:xmldocid,bodyonly` adds "stripping the declaration line and enclosing braces" while `<lang>:path` doesn't say what file types are accepted — inconsistent depth across rows.
- [clarity] The `Attributes` table lists only `tabs` and `title`, while the grammar above implies arbitrary `key=value` pairs. The reader is left wondering which built-in extensions read which keys.
- [Q] EBNF rule `language := IDENT ; for example csharp, razor, text` — the inline `;` comment is non-standard BNF and could be read as part of the grammar. Move examples below the grammar block.

### docs/Pennington.Docs/Content/reference/markdown/extensions.md
**Form claimed:** reference | **Actual:** reference with how-to leakage — every section ships a "Minimal example" plus prose framing that does not belong on a lookup page.

- [diataxis] Each H2 opens with a one-paragraph narrative ("The tabs extension collapses a run of consecutive fenced code blocks…", "The alerts extension parses a GitHub-flavored…", "After syntax highlighting, each rendered line is scanned for a `[!code …]` directive…"). These are explanation paragraphs — quote: "Pennington registers its own `CustomAlertInlineParser` ahead of Markdig's built-in alert parser; the blockquote form is the only accepted syntax."
- [diataxis] "Minimal example" subsections appear in every H2 — that is how-to content. Reference's job is to enumerate, not demonstrate; either drop them or move to the linked how-to pages.
- [clarity] Inconsistent entry formatting. Tabs uses a `<FieldList>` for arguments, code annotations uses a `<FieldList>` for notations, alerts uses a prose paragraph plus a table, and cross-reference tags uses a one-paragraph "Arguments" section. Pick one shape and apply it across all four extensions.
- [clarity] "Emitted CSS classes" formatting differs across sections — Tabs has a configurable-class table keyed by *option name*; Alerts has a kind-to-class table; Code annotations splits into a long prose paragraph; Cross-reference tags says "no added class". Consider one consistent class-table-plus-note structure.
- [diataxis] Cross-reference section editorializes: "Unknown uids emit a diagnostic that surfaces in the dev overlay and in the static-build report." This is behavior, fine — but the surrounding "Two surface forms are supported: the tag form…is handled in a pre-parse string pass (it is not valid HTML), and the attribute form…" reads as implementation explanation. Trim or move.
- [Q] Code annotations table at top of `code-block-args.md` and the field list here cover the same directives — readers will hit one or the other. Which is canonical?

### docs/Pennington.Docs/Content/reference/front-matter/keys.md
**Form claimed:** reference | **Actual:** reference — `<FrontMatterKeys />` carries the catalog at render time; the surrounding prose is the audit target.

- [diataxis] "Notes" section is explanation ("YAML keys are matched case-insensitively under `CamelCaseNamingConvention`…unknown keys are silently ignored…"). The fourth bullet ("The concrete records…re-declare every default-member key explicitly…") is rationale about the implementation. Move to explanation.
- [diataxis] Lead paragraph ("The flat catalog of YAML keys parsed into the four shipped `IFrontMatter` records…via `FrontMatterParser` with `CamelCaseNamingConvention`. Keys are declared as `init`-only properties on records in `Pennington.FrontMatter`…") is implementation-tour narrative, not a reference-page opener.
- [clarity] "Example" section closes with a sentence pointing at a different example file ("the blog-only keys…are demonstrated in `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`") rather than embedding that example or referencing it in a "See also" line.

### docs/Pennington.Docs/Content/reference/ui/utility.md
**Form claimed:** reference | **Actual:** reference with three "Note" callouts and example narrative — drifts into how-to.

- [diataxis] Three blockquote `> **Note:**` callouts (`LanguageSwitcher`, `StructuredData`, `FallbackNotice`). House style caps callouts at two per page; reference register should rarely use any.
- [diataxis] Each "Example" subsection opens with prose pointing at the production wiring ("The DocSite `MainLayout`…shows the production wiring: guard on `LocalizationOptions.IsMultiLocale`, then pass the pre-computed `_langSwitcherItems` list."). That is instructional how-to voice — quote: "guard on `LocalizationOptions.IsMultiLocale`, then pass…".
- [clarity] Inconsistent entry shape. `LanguageSwitcher` has Parameters + nested `AlternateLanguageItem` table + example. `StructuredData` has a `:path` source fence inline before parameters. `FallbackNotice` has a `:path` source fence before parameters. Pick one — either always show the declaration via `:path`, or never.
- [diataxis] Description text in tables embeds rationale and behavior reasoning: "hides itself when fewer than two locales are available, and auto-computes the list from `LocaleContext` and `LocalizationOptions` when `AlternateLanguages` is null or empty" runs into the lead paragraph and the `AlternateLanguages` row, duplicating itself.
- [Q] The page description claims "their parameters and a one-line use-when row each" — but there is no "use-when" content. Either drop the claim or add a sentence per component (and ensure it's not when-to-use guidance, which belongs in how-to).

### docs/Pennington.Docs/Content/reference/ui/navigation.md
**Form claimed:** reference | **Actual:** reference plus a final explanation section that should be split or moved.

- [diataxis] "Binding to `NavigationInfo`" section is explanation prose about how `NavigationInfo` relates to the components ("`TableOfContentsNavigation.TableOfContents` is populated from the tree returned by…not from a `NavigationInfo`…`OutlineNavigation` does not read `NavigationInfo` at all — it is a client-side component bound to a DOM selector…"). Either split into a "Binding contract" entry per component or move to the explanation page on navigation.
- [diataxis] Lead paragraph ends with an editorial assertion: "`TableOfContentsNavigation` binds to an `ImmutableList<NavigationTreeItem>` produced by `NavigationBuilder`; `OutlineNavigation` binds to a client-side DOM selector at runtime. Neither accepts a `NavigationInfo` directly." This is rationale that belongs in the binding section, not the page intro.
- [clarity] Multiple `LinkColorClass` / `RootLinkColorClass` / `OutlineLinkColorClass` / `OutlineLinkStructureClass` rows show `Default | see source`. That breaks the "Default" column contract — the reader can't look up the default without leaving the page. Either inline the actual class string or drop the column for these rows and explain in a footnote.
- [diataxis] "Example" subsection for `TableOfContentsNavigation` is a one-paragraph narrative about how `MainLayout` uses the component ("instantiates `TableOfContentsNavigation` twice — once per area when…and once against the root tree otherwise"). Not a reference fact; move to how-to or strip.
- [clarity] Slots subsection consistently reads "This component has no `RenderFragment` slots; all customization is performed through the class-name parameters above." That sentence repeats. Consider collapsing to a single "No slots." line per component or removing the H3 entirely.

### docs/Pennington.Docs/Content/reference/ui/content.md
**Form claimed:** reference | **Actual:** mostly reference with one explanation patch and minor format drift.

- [diataxis] "Stylesheet" section is explanation ("The components ship in MonorailCSS utility classes — no separate stylesheet from the package. Sites that mount `UseMonorailCss`…get the components styled automatically: the class-collector picks up utility tokens…and the single `<link>` tag is sufficient. There is no `_content/Pennington.UI/styles.css` to load."). Worth keeping as a reference fact ("Stylesheet: none — utilities via MonorailCSS") but trim the paragraph.
- [diataxis] "Mdazor registration" closing paragraph instructs the reader: "For sites that do not use `AddDocSite` (for example, `AddBlogSite` or a hand-rolled `AddPennington` host), call `AddMdazorComponent<T>()` for each of the eight types to match the doc-site surface." — quote: "call `AddMdazorComponent<T>()` for each of the eight types to match the doc-site surface". That is how-to voice; either move to a how-to or rephrase as a fact ("Hosts without `AddDocSite` register these via `AddMdazorComponent<T>()` per component").
- [diataxis] Component lead sentences carry rationale, e.g. `Checkpoint` description: "Authored as a Mdazor component rather than a heading so the right-side outline nav lists only real section headings." — that is design rationale, not reference.
- [clarity] `Steps` has parameter `Type` with description "Declared parameter reserved for future theming; not currently applied to rendered markup." A reserved-for-future parameter is dead surface; either drop the row or flag with a `Deprecated`/`Reserved` tag and link to the issue.
- [clarity] `CodeBlock` `Language` row says `Default = ""` but description says "Required (`EditorRequired`)". Pick one — if it's required, default should read `(required)` or be omitted.
- [clarity] `Variant` and `Color` columns in `Badge` / `Card` / `LinkCard` use string literals (`"note"`, `"primary"`); consider a typed values list or a link to the color palette reference so the reader doesn't guess.

### docs/Pennington.Docs/Content/reference/spa/attributes.md
**Form claimed:** reference | **Actual:** reference contaminated with one how-to section and an explanatory lead paragraph.

- [diataxis] "Persistent chrome" section is a step-by-step how-to ("Mark", "Listen", "Read" rows describing the procedure for keeping elements outside the region system). That belongs in a how-to guide, not a reference page.
- [diataxis] Lead paragraph reasons through behavior: "The swap is synchronous: the engine waits for the response and any new stylesheets, then DOM replacement, scroll reset, and head update all happen in one block so the browser paints them as a single frame." That is design explanation; trim or move.
- [diataxis] "Anchor and stylesheet attributes" section closes with a prescriptive paragraph: "In production builds the stylesheet URL changes per content set, so `data-spa-reload` on a `<link>` is unnecessary and should be removed before deployment." — quote: "should be removed before deployment". That is instructional advice belonging in a how-to/explanation.
- [diataxis] "Boundary fallbacks" lead says "they are listed here so authors can recognise the behaviour." Meta-framing; drop.
- [clarity] Inconsistent entry shape. Region attributes table has `Name / Values / Description`. Anchor/stylesheet attributes table has `Selector / Attribute / Description` (no `Values` column). Document-root tuning has `Name / Type / Default / Description`. Lifecycle events has `Event / detail shape / When it fires`. Each table is keyed differently — fine in principle, but the missing `Default` on most tables breaks the audit-the-default workflow.
- [clarity] `data-spa-region-key` "Description" cell ends "Omit when the region's content is comparable across pages and scroll position should carry over." — that is when-to-use guidance, explicitly banned in reference register.

### docs/Pennington.Docs/Content/reference/diagnostics/request-context.md
**Form claimed:** reference | **Actual:** reference with inconsistent entry format and example narrative.

- [clarity] `DiagnosticContext` members are documented as a bulleted prose list, while `Diagnostic` parameters use a `<FieldList>` and `DiagnosticSeverity` values use a table. Three different shapes in one page for the same kind of information (a typed surface).
- [diataxis] Each member bullet adds usage commentary: "Used when the caller already has a `Diagnostic` instance (for example, one forwarded from a helper service)" and "Gate before enumerating the list to emit `X-Pennington-Diagnostic` headers." — quote: "Gate before enumerating the list to emit…". That is instructional guidance.
- [diataxis] Lead paragraph for `DiagnosticContext` editorializes: "backed by a private `List<Diagnostic>` with no thread-safety". That is implementation detail; either tag as a documented contract ("not thread-safe; resolve per request") or drop.
- [diataxis] "Example" section is narrative: "The canonical in-repo consumer is `XrefResolvingService`, which reports unresolved uids. Any service or response processor that resolves `DiagnosticContext` and calls `AddWarning` / `AddError` during request handling flows entries into the `X-Pennington-Diagnostic` response header and the dev overlay without further wiring." — pure how-to voice on a reference page.
- [clarity] Lead claims "two dev-mode transports" but the response-header row says "Every request that has `HasAny`" (not dev-mode-gated), while the overlay row gates on `DOTNET_WATCH`. The lead and the table contradict on whether the header is dev-mode-only.
- [Q] `DiagnosticSeverity` is described as "Two-value enum in ascending severity order" — does the underlying type ever surface to consumers (`int` cast, JSON serialization)? If so, document the storage type; if not, drop "in ascending severity order" as implementation noise.

### docs/Pennington.Docs/Content/reference/blogsite/routes.md
**Form claimed:** reference | **Actual:** reference with one large explanation Note callout that should be split out.

- [diataxis] The "Note on `TagsPageUrl` and `BlogBaseUrl`" blockquote is a 60-word explanation of why the `@page` directives are not templated, plus a workaround. That is explanation/how-to leakage: rationale for the design and the remediation step belong elsewhere — quote: "changing them away from the defaults requires supplying replacement Razor pages via `AdditionalRoutingAssemblies`".
- [diataxis] Routes table "Description" cells embed implementation tour ("Homepage Razor page (`Home.razor`); renders `BlogSiteOptions.HeroContent`, the ten most recent posts via `BlogSummary`, and the sidebar modules fed by `MyWork`/`Socials`/`AuthorBio`."). The "ten most recent posts" is a magic number — surface as a configurable option or document the constant explicitly, not as a description aside.
- [diataxis] "Option-to-route matrix" cells reuse the same caveat as the Note callout ("the `@page` directives on `Tags.razor` and `Tag.razor` are fixed string literals, so tag URLs and page routes only align at the default `"/tags"` value"). The reader hits this explanation three times on one page (lead Note, `TagsPageUrl` row, `BlogBaseUrl` row).
- [diataxis] Lead under "Entry point" reads: "The `/sitemap.xml` endpoint is mounted by `UsePennington` via `SitemapService`, not by `UseBlogSite`." Fine as a fact, but the Routes table omits `/sitemap.xml` even though "Option-to-route matrix" includes a row for it. Page either covers sitemap or it doesn't.
- [diataxis] "Example" closing sentence is narrative-explanatory: "The example boots `Pennington.BlogSite` with scaffold options; all eight routes listed above — including `/rss.xml`, because `EnableRss` defaults to `true` — are live in dev and in the static build." Drop or move to how-to.

### docs/Pennington.Docs/Content/reference/blogsite/social-icons.md
**Form claimed:** reference | **Actual:** reference with mild instructional voice; otherwise close to clean.

- [diataxis] "Reference from `SocialLink.Icon`" section gives a prescription: "pass the static field directly — `SocialIcons.GithubIcon` — not as a component tag `<SocialIcons.GithubIcon />`." Acceptable as a "non-obvious gotcha" sentence, but the trailing "One-line syntax:" plus a copy-pasteable code block reads as how-to.
- [diataxis] "Example" subsection is narrative wrapping an embedded snippet: "Excerpt from `BlogSiteHeroProjectsSocialsExample.Stage3.Run`, which populates `BlogSiteOptions.Socials` with all four built-in fragments." That is example walkthrough, not a reference fact.
- [clarity] Icons table column "Notes" mixes physical description ("Single-path Octocat silhouette") with stroke-width detail ("Two-path elephant-trunk mark using `stroke-width="1.5"`"). Inconsistent — either describe glyph shape uniformly or surface the differing stroke widths as a separate column.
- [Q] Is the descriptive name "Octocat silhouette" accurate / acceptable? GitHub's mark is trademarked and the silhouette label may need legal review. Less freighted phrasing: "GitHub mark, single path".

---

## Explanation

### docs/Pennington.Docs/Content/explanation/core/content-pipeline.md
**Form claimed:** explanation | **Actual:** explanation — strong "why" framing; union design choice argued through alternatives.

- [diataxis] The case-record list under "The union shape" uses YAML-style definition lists (`DiscoveredItem` `:   Carries Route and Source.`) — that's a reference-style enumeration of fields. Either tighten into prose ("`DiscoveredItem` carries route and source; `ParsedItem` adds metadata and raw markdown…") or move to the reference page and link.
- [clarity] "C# 15 discriminated unions offer a third path" is the first time unions are named, but the reader who arrives from a search engine has no link or sidebar pointer to "what is the C# 15 `union` keyword?" — a one-line aside or an external link would help, especially since the next page (`content-source.md`) leans on the same feature.
- [Q] The doc claims `Route` is the only property lifted onto the union — is that still true after the recent `Source` accessor work, or does `DiscoveredItem` expose `Source` directly off the union? Worth double-checking against current source so the invariant claim holds.

### docs/Pennington.Docs/Content/explanation/core/content-source.md
**Form claimed:** explanation | **Actual:** how-to with explanation framing — most of the body is "how to construct" and "how to read" recipes.

- [diataxis] Opens by naming itself: "this page focuses on the two questions that come up the moment you write a custom `IContentService`: *how do I build one of these?* and *how do I read one back out?*" — those are how-to questions. Either re-shape around *why* the union has `.Value` and *why* the five cases are split the way they are, or split out a how-to ("Construct and pattern-match `ContentSource`") and leave the explanation focused on the `.Value` polyfill design and the sitemap-exclusion rationale.
- [diataxis] The "Which case to use" table is a reference table inside an explanation. It belongs in a reference page or in a how-to about choosing a source type; the explanation should compare the cases in prose ("`RedirectSource` and `EndpointSource` both…").
- [diataxis] Code examples are instructional, not illustrative — `yield return new DiscoveredItem(route, new MarkdownFileSource(filePath));` etc. tell the reader what to type. Per voice guide, explanation code shows *how something works*, not *what to type*. Move the construction recipes to a how-to.
- [clarity] The `.Value` polyfill rationale is the most genuinely-explanatory section ("Why `.Value` and not the case type directly") and it's buried at the bottom. If this page stays as explanation, lead with the polyfill story.
- [Q] Should this page exist at all as explanation, or should it become how-to "Construct and consume a `ContentSource`" with the polyfill nuance folded into `content-pipeline.md` as a Trade-off bullet?

### docs/Pennington.Docs/Content/explanation/core/dev-vs-build.md
**Form claimed:** explanation | **Actual:** explanation — clean rationale for the one-host invariant, alternatives discussed.

Clean.

### docs/Pennington.Docs/Content/explanation/core/front-matter-capabilities.md
**Form claimed:** explanation | **Actual:** explanation — universal-vs-selective split argued clearly.

- [diataxis] The "Writing your own front-matter type" section is a single paragraph of how-to instructions ("Declare a `record`, implement `IFrontMatter`, add whichever capability interfaces…"). This is the imperative voice of a how-to and reads as leakage. Either drop it (the how-to link at the bottom covers it) or rephrase as discussion of what a custom type buys versus what the defaults give for free.
- [clarity] The Trade-offs bullet "Default members are an interface feature… Consumers that access `IFrontMatter` through reflection or multi-target older TFMs should keep that in mind" trails off without saying *what* to keep in mind or what breaks. Either expand or cut.

### docs/Pennington.Docs/Content/explanation/core/response-processing.md
**Form claimed:** explanation | **Actual:** explanation — two-tier split well argued; ordering rationale lands.

- [clarity] "Tier A" and "Tier B" are introduced as headings without ever being defined as terms. The reader has to infer that A = generic body processors, B = HTML rewriters. A single sentence at the top of "How it works" naming the tiers and what each owns would orient the section-jumping reader.
- [Q] Is there a reason `HtmlResponseRewritingProcessor` itself (at Order 10) isn't mentioned as a built-in `IResponseProcessor` until midway through? Naming it alongside `LiveReloadScriptProcessor` and `DiagnosticOverlayProcessor` in one sentence would make the bridge between tiers visible earlier.

### docs/Pennington.Docs/Content/explanation/dev-experience/hot-reload.md
**Form claimed:** explanation | **Actual:** mostly explanation, but the body reads as a mechanism walkthrough rather than a *why* discussion — light on tradeoff framing.

- [diataxis] No "Context" / "why this exists" framing at the top. Opens straight into mechanism ("Content files… are not part of the .NET compilation. Restarting the host for every markdown typo would…"). That's a reasonable lead but it never widens into "why this shape vs alternatives" before diving into `FileWatcher` internals. The Trade-offs section does the comparison work, but it's at the bottom rather than weaving through the body.
- [diataxis] The four subsections under "How it works" are sequential ("files change → caches drop → debounce → browser reloads") and read as a step-by-step trace of one event. That ordering is fine for explanation, but the prose ("The mechanism is a single chain: files change, cached services drop their state, a debounce window elapses, and the browser reloads") leans toward how-it-runs description rather than why-it's-shaped-this-way reasoning.
- [clarity] The opening paragraph mentions "a debounced WebSocket channel" as the answer but never says *why* WebSocket over Server-Sent Events or polling. A sentence on that tradeoff would round out the design rationale.

### docs/Pennington.Docs/Content/explanation/localization/urls-and-fallback.md
**Form claimed:** explanation | **Actual:** explanation — invariant clearly stated, alternatives weighed, tradeoffs concrete.

Clean.

### docs/Pennington.Docs/Content/explanation/positioning/docsite-positioning.md
**Form claimed:** explanation | **Actual:** explanation — positioning argument lands; "when to drop a level" is appropriate explanation territory.

- [diataxis] The "Signals that point toward bare AddPennington" bullet list is reference-shaped (a checklist of conditions). It works as explanation because the prose around it frames it discursively, but consider whether any of these five conditions wants its own how-to it can link to (e.g., "Use multiple content sources" is already linked — good; the others aren't).
- [clarity] "It takes either the `ConfigurePennington` escape hatch, which hands back the underlying `PenningtonOptions` after DocSite's defaults land, or dropping to bare `AddPennington` outright." — `ConfigurePennington` is not introduced or linked on first mention. A reader who landed here from search has no anchor for what that escape hatch looks like in code. A short illustrative snippet or a reference link would help.
- [Q] The page mentions `ExtensibilityLabExample` as the canonical bare-host reference but does not link to it (the example lives under `examples/ExtensibilityLabExample/Program.cs`). Is there a docs route for it, or should this be a GitHub link?

### docs/Pennington.Docs/Content/explanation/rendering/highlighting.md
**Form claimed:** explanation | **Actual:** explanation — cascade design defended with concrete alternatives.

Clean.

### docs/Pennington.Docs/Content/explanation/rendering/monorail-css.md
**Form claimed:** explanation | **Actual:** explanation — discovery rationale and OKLCH choice both well argued.

- [diataxis] The "Color schemes: named vs algorithmic" section drifts into a feature-tour register: it lists what each scheme's parameters are (`PrimaryHue`, `Chroma`, `CoordinatingScheme` enum values) rather than discussing *why* two schemes exist or *when* each fits. The "designer-versus-programmer axis" framing at the end is the explanation; the parameter enumeration above it is reference. Tighten or move the parameter list.
- [clarity] OKLCH is introduced without a one-line "what is OKLCH" for the C# developer who has never opened a color-science page. The perceptual-uniformity paragraph alludes to it but never says "OKLCH = an OK-Lab cylindrical coordinate system designed for perceptual uniformity"; a single sentence would orient the reader before the curve-family discussion.

### docs/Pennington.Docs/Content/explanation/routing/cross-references.md
**Form claimed:** explanation | **Actual:** explanation — two-phase resolver justified well, broken-xref diagnostic loop closed.

Clean.

### docs/Pennington.Docs/Content/explanation/routing/navigation-tree.md
**Form claimed:** explanation | **Actual:** explanation — fold algorithm explained, the folder-vs-sectionLabel distinction is the highlight.

- [diataxis] Two `csharp:xmldocid` fences embed `T:Pennington.Content.ContentTocItem` and `T:Pennington.Navigation.NavigationTreeItem` declarations. Per voice guide, explanation code is illustrative — embedding the full type declaration shows the reader the API surface (reference territory) rather than illustrating how the algorithm uses it. Consider linking to reference and keeping only the algorithm-method fence (`BuildTree`) since that one *is* illustrating the recursion shape under discussion.
- [Q] The NOTE admonition early on says "Renaming the folder changes the sidebar header; changing `sectionLabel:` does not." That's exactly the same point the "Sections without a direct content file" section makes in prose three paragraphs later. Is the admonition load-bearing or duplicative? If the prose section already lands the distinction, the callout reads as belt-and-suspenders.

### docs/Pennington.Docs/Content/explanation/routing/url-paths.md
**Form claimed:** explanation | **Actual:** explanation — parse-don't-validate argument applied cleanly; alternatives weighed.

Clean.

### docs/Pennington.Docs/Content/explanation/spa/islands.md
**Form claimed:** explanation | **Actual:** explanation — single-render-path rationale is the design story; tradeoff bullets concrete.

- [clarity] The opening "Why does in-site navigation fetch the same URL the address bar shows and parse it client-side, instead of round-tripping a small JSON envelope or letting the browser do a full reload?" assumes the reader already knows Pennington has an SPA story. A new arrival from search who is evaluating Pennington for a docs site might not have hit `AddDocSite`'s SPA wiring yet — one sentence locating this feature ("DocSite ships a small SPA navigation engine; this page covers the design choice behind it") would orient them.
- [Q] The page is titled "SPA navigation through region swaps" and tagged under `explanation.spa.islands` — the word "islands" appears only in the external-link bullet at the very end, where the author acknowledges Pennington's regions are a "degenerate case" of island architecture. Is "islands" the right uid for a page that explicitly tells the reader its mechanism isn't really islands? Consider `explanation.spa.region-swaps` or similar to match the title.

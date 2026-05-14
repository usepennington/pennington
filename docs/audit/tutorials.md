### docs/Pennington.Docs/Content/tutorials/getting-started/first-site.md
**Form claimed:** tutorial | **Actual:** tutorial with explanation drift — mostly correct shape but the prose keeps explaining instead of walking.

- [voice] Title "Create your first Pennington site" duplicates page H1 (engine adds title); guide says no top-level H1 — borderline since it's front matter, but description "Stand up a minimal ASP.NET host…" is fragment register, fine.
- [voice] Banned-word violations — "no manual setup required" is OK, but the tutorial register is missing the warmest "we'll" voice in spots; the second paragraph reads as explanation register: "The tutorial covers how to wire…" rather than tutorial register.
- [voice] Phrase "for now, MapGet keeps the URL → markdown file → rendered HTML chain visible in one place" — explanation tone in tutorial; could be cut.
- [diataxis] The intro after the H1 ("The tutorial covers how to wire…") is meta-narration about what the tutorial does, not a tutorial opening that gets the reader moving. Diataxis tutorials should start doing, not summarising.
- [diataxis] Step 1.4 "Confirm the bare host runs" embeds an `xmldocid` snippet (`M:GettingStartedMinimalSiteExample.Stage1.Run`) for what should be the unmodified `dotnet new web` output — the reader has not been shown the file to "confirm" against and the body says "Before adding Pennington, `Program.cs` looks like this" which is more explanation than verification.
- [diataxis] Section 2 has a `<Checkpoint>` that explicitly says do NOT run the site — a tutorial checkpoint should produce a visible result; "build succeeds, do not run" is a non-result and confuses the reader expecting reward.
- [clarity] `DocFrontMatter` is referenced (`AddMarkdownContent<DocFrontMatter>`) but never explained or linked. The reader doesn't know what type that is or why it's the right choice for a non-doc site.
- [clarity] `RunOrBuildAsync` is introduced with "the same host that serves live today will generate static HTML tomorrow with no code change" — promise made but no link to the build doc; reader can't follow up.
- [clarity] Stage 2 example uses `Content/index.md` at `examples/GettingStartedMinimalSiteExample/Content/index.md`, embedded by path, but the reader is told to "add `index.md` with the contents below" — the rendered fence will show only the file contents, not the exact authoring shape with the YAML fence (front-matter markers) called out separately.
- [Q] The two `NOTE` admonitions about skipping to DocSite (one at the top, one near the bottom) is two callouts of the same idea — likely over the 2-admonition ceiling once the IMPORTANT alpha-version admonition is counted (three total).

---

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

---

### docs/Pennington.Docs/Content/tutorials/getting-started/styling.md
**Form claimed:** tutorial | **Actual:** tutorial, generally aligned but voice slips into explanation in places.

- [voice] "Let's confirm the baseline before touching anything" — tutorial register, good. But "keeping DI registration separate from the endpoint wiring makes it easier to pinpoint problems" — explanation-register reasoning that doesn't progress the tutorial.
- [voice] "One line stands between here and a live stylesheet" — borderline chatty/clever; fine as one informal aside but check page-wide count (the intro and unit headings have similar flourishes).
- [diataxis] Unit 3 is a single-step "call this one method" unit — under `<Steps>` wrapper with one step. The guide doesn't require multi-step `<Steps>` blocks, but a one-step unit smells like the granularity is wrong; consider merging units 3 and 4, since registration without `UseMonorailCss` produces no visible result either time.
- [diataxis] Unit 3 checkpoint: "Pages still render unstyled… still 404s." A second "build succeeds, nothing renders" non-result checkpoint, same pattern as the other getting-started pages. Tutorials should produce visible progress per step.
- [clarity] `LayoutComponentBase` is mentioned once — fine for Blazor folks per the prereqs, but `class collector` (mentioned three times) is a Pennington concept without a link to an explanation page.
- [clarity] `NamedColorScheme` and `ColorName` are referenced without an authoritative reference link; "indigo/pink/slate; any combination works" leaves the reader to guess the surface.
- [Q] Step 5.2 has the reader add an inline `<p class="text-accent-600 italic">` to a markdown file — this works, but doesn't actually exercise the class collector against utility classes in *content* HTML (it's HTML in markdown); the tutorial promises "watch the stylesheet regenerate as new utility classes appear" but the realistic case is utility classes generated by rendered markdown chrome, not author-written HTML. Worth flagging.

---

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

---

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

---

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

---

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

---

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

---

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

---

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

---

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

---

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

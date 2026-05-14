# Explanation docs audit

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

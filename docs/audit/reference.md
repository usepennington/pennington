# Reference docs audit

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
- [clarity] The page promises an "index of every…extension method" but actually defers the entire surface to two `<ExtensionMethods Receiver="…" />` component invocations; a reviewer cannot see if every method is covered without rendering the site. Auditing the contract requires opening source.
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
**Form claimed:** reference | **Actual:** reference with most of the surface hidden behind a custom component — the page itself contains almost no looked-up content.

- [clarity] The entire key catalog is delegated to `<FrontMatterKeys />`. A reader auditing the doc, or one viewing it offline / on GitHub, sees zero keys. The page promises "every built-in YAML front-matter key" and then renders an opaque component.
- [diataxis] "Notes" section is explanation ("YAML keys are matched case-insensitively under `CamelCaseNamingConvention`…unknown keys are silently ignored…"). The fourth bullet ("The concrete records…re-declare every default-member key explicitly…") is rationale about the implementation. Move to explanation.
- [diataxis] Lead paragraph ("The flat catalog of YAML keys parsed into the four shipped `IFrontMatter` records…via `FrontMatterParser` with `CamelCaseNamingConvention`. Keys are declared as `init`-only properties on records in `Pennington.FrontMatter`…") is implementation-tour narrative, not a reference-page opener.
- [clarity] "Example" section closes with a sentence pointing at a different example file ("the blog-only keys…are demonstrated in `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`") rather than embedding that example or referencing it in a "See also" line.
- [Q] Does `<FrontMatterKeys />` emit the same type/default columns the prose claims? Worth asserting in a snapshot test so the reference promise (type, default, source interface) stays honest.

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

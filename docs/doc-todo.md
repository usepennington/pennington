# Docs migration TODO — xmldoc gaps

Tracks members whose `<summary>` xmldoc is missing or weak, surfaced by the `<ApiMemberTable>` / `<ApiMemberList>` migration in `docs/Pennington.Docs/Content/reference/`. The `ApiMember*` components pull descriptions from xmldoc; wherever a table cell renders blank, it means the source needs a `<summary>`.

Entries are grouped by source file. Each item lists a suggested description drawn from the previous hand-written markdown table — these are starting points, not verbatim text; tighten and verify against current behavior before committing.

Workflow: pick a type, add `<summary>` blocks in the source, rebuild, reload the affected reference page, confirm the table cells now populate. Check the box here and move on.

## src/Pennington.DocSite/DocSiteOptions.cs

Surfaced by `docs/Pennington.Docs/Content/reference/options/docsite-options.md` (migrated 2026-04-15).

- [ ] `SiteTitle` (line 13) — Site name shown in the header, `<title>` suffix, and RSS/sitemap/structured-data metadata.
- [ ] `Description` (line 14) — Site-wide description used as the default `<meta name="description">` and in structured data when a page does not supply its own.
- [ ] `ColorScheme` (line 15) — MonorailCSS color scheme forwarded to `MonorailCssOptions.ColorScheme`; accepts `NamedColorScheme` or `AlgorithmicColorScheme`.
- [ ] `CanonicalBaseUrl` (line 16) — Absolute canonical origin used for social meta tags, sitemap entries, RSS links, and structured-data URLs.
- [ ] `ContentRootPath` (line 17) — Root directory scanned for markdown content; each `ContentArea.Slug` resolves as a subfolder of this path.
- [ ] `HeaderIcon` (line 18) — Raw SVG or HTML markup rendered as the header logo/icon.
- [ ] `HeaderContent` (line 19) — Raw HTML rendered inside the site header region via `MarkupString`.
- [ ] `FooterContent` (line 20) — Raw HTML rendered as the page footer via `MarkupString`.
- [ ] `GitHubUrl` (line 21) — Repository URL linked from the header GitHub icon; `null` hides the icon.
- [ ] `SocialImageUrl` (line 22) — URL used as the default Open Graph and Twitter card image on pages that do not supply a per-page override.
- [ ] `DisplayFontFamily` (line 23) — CSS `font-family` value applied to heading and display text.
- [ ] `BodyFontFamily` (line 24) — CSS `font-family` value applied to body text.
- [ ] `ExtraStyles` (line 25) — Raw CSS emitted above the generated MonorailCSS stylesheet, forwarded to `MonorailCssOptions.ExtraStyles`.
- [ ] `AdditionalHtmlHeadContent` (line 26) — Raw HTML string injected into every page's `<head>` element, rendered as `MarkupString`.
- [ ] `FontPreloads` (line 27) — Font files emitted as `<link rel="preload">` hints in the `<head>`; each entry carries an `Href` and MIME `Type` (default `font/woff2`).
- [ ] `AdditionalRoutingAssemblies` (line 28) — Extra assemblies scanned for Razor `@page` components so custom pages outside `Pennington.DocSite` participate in routing.

## src/Pennington/FrontMatter/IFrontMatter.cs

Surfaced by `docs/Pennington.Docs/Content/reference/front-matter/ifrontmatter.md` (migrated 2026-04-15). All seven properties on the interface are missing `<summary>`.

- [ ] `Title` — The only abstract member; every implementation must supply a human-readable page title surfaced in `<title>`, navigation, breadcrumbs, and feed entries.
- [ ] `IsDraft` — When `true`, `ContentPipeline.GenerateAsync` skips the page so it does not appear in the output tree, sitemap, search index, or llms.txt.
- [ ] `Search` — When `false`, `SearchIndexBuilder` excludes the page from the per-locale search index JSON emitted at `/search-index-{code}.json`.
- [ ] `Llms` — When `false`, `LlmsTxtService` excludes the page from both the `llms.txt` index and the stripped-markdown sidecar output.
- [ ] `Uid` — Stable cross-reference identifier; when set, the page is registered with `XrefResolver` and can be linked via `<xref:uid>` or `[text](xref:uid)`.
- [ ] `Description` — Short summary emitted into `<meta name="description">`, social cards, and feed entries.
- [ ] `Date` — Publication timestamp consumed by RSS, sitemap, and blog post ordering; when `null` the page is treated as undated.

## src/Pennington/FrontMatter/Capabilities.cs

Surfaced by `docs/Pennington.Docs/Content/reference/front-matter/ifrontmatter.md`. Each capability interface has a type-level summary but no property-level summary.

- [ ] `ITaggable.Tags` — Non-null array of tag slugs used to group content in tag-index pages and surface tags in RSS and structured data.
- [ ] `IRedirectable.RedirectUrl` — When non-null, the page is emitted as a meta-refresh stub pointing at the target URL with `<meta name="robots" content="noindex">` applied.
- [ ] `ISectionable.SectionLabel` — Human-readable section label surfaced on breadcrumbs and prev/next navigation; it does not drive sidebar grouping (the content subfolder does).
- [ ] `IOrderable.Order` — Sort key consumed by `NavigationBuilder.BuildTree` to order siblings within a sidebar group; lower values render first, ties fall back to alphabetic title order. Implementations typically default to `int.MaxValue` so unset pages sort last.

## src/Pennington/FrontMatter/DocFrontMatter.cs

Surfaced by `docs/Pennington.Docs/Content/reference/front-matter/built-in-types.md` (migrated 2026-04-15). Type-level summary is fine; all 9 properties need `<summary>`.

- [ ] `Title`, `Description`, `IsDraft`, `Tags`, `SectionLabel`, `Uid`, `Order`, `Search`, `Llms` — port descriptions from the previous manual table (Description used for `<meta>` tags, Tags drives tag indexes, Order is sidebar sort key, etc.).

## src/Pennington/FrontMatter/BlogFrontMatter.cs

Surfaced by `docs/Pennington.Docs/Content/reference/front-matter/built-in-types.md`. All 10 properties need `<summary>`.

- [ ] `Title`, `Description`, `IsDraft`, `Tags`, `Date`, `Author`, `Series`, `Uid`, `Search`, `Llms` — port descriptions from the manual table (Date → RSS `<pubDate>`, Author → RSS `<author>`, Series groups multi-part posts, etc.).

## src/Pennington.DocSite/DocSiteFrontMatter.cs

Surfaced by `docs/Pennington.Docs/Content/reference/front-matter/built-in-types.md`. All 10 properties need `<summary>`.

- [ ] `Title`, `Description`, `IsDraft`, `Tags`, `Order`, `RedirectUrl`, `SectionLabel`, `Uid`, `Search`, `Llms`.

## src/Pennington.BlogSite/BlogSiteFrontMatter.cs

Surfaced by `docs/Pennington.Docs/Content/reference/front-matter/built-in-types.md`. All 13 properties need `<summary>`.

- [ ] `Title`, `Author`, `Description`, `Repository`, `Date`, `IsDraft`, `Tags`, `Series`, `RedirectUrl`, `SectionLabel`, `Uid`, `Search`, `Llms` — port descriptions from the manual table (Author → footer + RSS, Repository → source-link card, Title default is `"Empty title"` not `""`).

## src/Pennington.BlogSite/BlogSiteOptions.cs

Surfaced by `docs/Pennington.Docs/Content/reference/options/blogsite-options.md` (migrated 2026-04-15). The thematically-grouped tables were flattened to one alphabetical `<ApiMemberTable>`. All 25 properties are missing `<summary>`.

- [ ] `SiteTitle`, `Description`, `CanonicalBaseUrl`, `ColorScheme`, `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`, `ExtraStyles`, `DisplayFontFamily`, `BodyFontFamily`, `AdditionalHtmlHeadContent`, `FontPreloads`, `AdditionalRoutingAssemblies`, `AuthorName`, `AuthorBio`, `EnableRss`, `EnableSitemap`, `HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`, `SocialMediaImageUrlFactory` — port descriptions from the old thematic tables (Metadata / Content paths / Styling / Author chrome / Homepage data / Feature toggles / Integration hooks) in blogsite-options.md history.
- [ ] Helper records `HeroContent(Title, Description)`, `Project(Title, Description, Url)`, `SocialLink(Icon, Url)`, `HeaderLink(Title, Url)` — no xmldoc on records or their primary-constructor params. Add `<param>` tags to each record.

## src/Pennington/Routing/* — deferred full migration

`docs/Pennington.Docs/Content/reference/extension-points/routing.md` currently mixes a migrated `<ApiMemberTable>` for `ContentRoute.Properties` with hand-written method-by-method prose for `UrlPath`, `FilePath`, and `ContentRouteFactory`. The hand-written prose is richer than any xmldoc available today, so a full migration was deliberately deferred.

Before sweeping this file into `<ApiMemberList>` calls, add xmldoc to:

- [ ] `src/Pennington/Routing/UrlPath.cs` — type-level summary + operators (`/`, implicit) + methods (`EnsureLeadingSlash`, `EnsureTrailingSlash`, `RemoveTrailingSlash`, `RemoveLeadingSlash`, `Matches`). The manual page already describes each one in detail — port those as the source of truth.
- [ ] `src/Pennington/Routing/FilePath.cs` — type-level summary + `/` operator + `Extension`, `FileName`, `FileNameWithoutExtension`, `Value`. One method has xmldoc today; the rest are bare.
- [ ] `src/Pennington/Routing/ContentRoute.cs` — properties (`CanonicalPath`, `OutputFile`, `SourceFile`, `Locale`, `IsFallback` already has one), methods (`WithBaseUrl`, `AbsoluteUrl` already has one, `IsDefaultLocale`).
- [ ] `src/Pennington/Routing/ContentRouteFactory.cs` — five factory methods (`FromMarkdownFile`, `FromRazorPage`, `FromUrl`, `FromCustom`, `ForRedirect`). All currently bare.

## Bugs found and fixed during the sweep

Discovered by running the migrated pages through the dev server and fixed in-branch on 2026-04-15:

- Xmldoc parser dropped boundary whitespace around inline elements. `<see cref="Foo"/>` inside prose rendered as `"…underlyingFooafter…"` instead of `"…underlying Foo after…"`. Fixed in `src/Pennington.Roslyn/Documentation/XmlDocParser.cs` (`NormalizeWhitespace` now samples boundary whitespace before stripping indentation; `CollapseText` concatenates without synthesized spaces; new `TrimBoundaryWhitespace` strips outer edges only).
- Cref renderer leaked generic-arity markers from Roslyn's xmldoc comment ids — `AddMarkdownContent``1` instead of `AddMarkdownContent`, `List`1` instead of `List`. Fixed in `src/Pennington.Roslyn/Documentation/XmlDocHtmlRenderer.cs` (`ShortenCref` now strips the `` `N `` / `` ``N `` suffix).
- `<ApiMemberList>` declaration code block duplicated the member's xmldoc (both in the rendered code and as the parsed-summary paragraph beneath it). Fixed by adding `includeLeadingTrivia` parameter to `ISymbolExtractionService.ExtractCodeFragmentAsync` (default `true` preserves existing `:xmldocid` fence behavior, including type-level xmldoc in type declarations); `ApiMemberList.razor` now passes `false`.
- `<ApiMemberTable>` and `<ApiMemberList>` had no required-member indicator; `required` init-only properties rendered identically to optional ones. Fixed by appending a `(required)` marker when `IPropertySymbol.IsRequired` is true, spanning both components.
- Interface default-implementation defaults (`bool IsDraft => false;`) rendered as `—` because the expression body isn't a field initializer. Fixed in `ExtractPropertyDefault` (MemberEnumerator.cs) by emitting the literal when the containing type is an interface and the expression body is a `LiteralExpressionSyntax`. Concrete expression-bodied properties (computed getters like `IsDefaultLocale => string.IsNullOrEmpty(Locale)`) correctly still report no default.
- Auto-properties with no initializer rendered as `—` for the Default column even when the CLR default is obvious (`bool IsFallback { get; init; }` → `false`, `string? Foo { get; init; }` → `null`). Fixed in `ExtractPropertyDefault` via a new `FallbackClrDefault` helper that emits the CLR default for nullable-annotated properties and common value-type special types (bool, integers, floating-point, decimal). Required properties still report no default, preserving the `—`.

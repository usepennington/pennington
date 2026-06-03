# ExtensibilityLabExample

Bare `AddPennington` host that exercises every Pennington extension seam from one project. Each how-to under `docs/.../how-to/` fences into a self-contained file here. The canonical reference for "Pennington without DocSite."

## Extension points wired

- **Custom highlighter** — `PipelineHighlighter` claims `pipeline` fences (`how-to/markdown-pipeline/custom-highlighter.md`)
- **Code-block preprocessor** — `LineCountPreprocessor` claims `linecount` fences (`how-to/markdown-pipeline/code-block-preprocessor.md`)
- **Custom shortcode** — `GitHubRepoShortcode` expands `<?# GitHubRepo "owner/repo" /?>` to a link (`how-to/markdown-pipeline/shortcodes.md`)
- **Tabbed-code class override** — `TabbedCodeBlockStyling` swaps the default `tab-*` classes (`how-to/code-samples/tabbed-code.md`)
- **Custom `IContentService`** — `ReleaseNotesContentService` (JSON-backed). Generates one page per release file under `Content/releases/v*.json`, routed at `/releases/<semver>/` (e.g. `/releases/1.0.0/`, `/releases/1.1.0/`). The slug is the raw SemVer string without a `v` prefix — `/releases/v1.0/` would 404. (`how-to/content-services/custom-content-service.md`)
- **Custom records in taxonomy, search & JSON-LD** — each `ReleaseEntry` implements `IFrontMatter` + `ITaggable` + `IHasSearchFacets` + `IHasStructuredData` and is attached as `DiscoveredItem.Metadata`. From that one record: a browse-by-channel taxonomy (`AddTaxonomy<ReleaseEntry, string>` → `/channel/`, term pages rendered by `ChannelIndex.razor`/`ChannelTerm.razor`), the custom `channel` search facet, and per-page JSON-LD (`SoftwareApplication`) injected into each release page's `<head>` — no separate index page, the same treatment markdown gets. (`how-to/content-services/custom-content-service.md`)
- **Emit-only `IContentService`** — `RobotsTxtContentService`. **Build-mode only** — at `dotnet run -- build` the service emits `output/robots.txt`. Under `dotnet run` the file has no runtime route, so `/robots.txt` is a 404 in dev. This is the central design difference between a discoverable `IContentService` and an emit-only one. (`how-to/content-services/emit-generated-artifacts.md`)
- **Response processor** — `FeedbackWidgetProcessor` (`how-to/response-pipeline/response-processor.md`)
- **Diagnostics-emitting processor** — `DiagnosticsEmittingProcessor` (`reference/diagnostics/request-context.md`)
- **HTML rewriter** — `AnchorLowercaseRewriter` (`how-to/response-pipeline/html-rewriter.md`)
- **MonorailCSS customization** — `MonorailCssCustomization.BuildOptions` (`how-to/theming/monorail-css.md`)
- **llms.txt on bare host** — `LlmsTxtConfiguration.Configure` (`how-to/feeds/llms-txt.md`)

## Positioning

Identified in `docs/.../explanation/positioning/docsite-positioning.md` as the canonical bare-host reference and the recommended starting skeleton for hosts whose shape the DocSite template doesn't fit.

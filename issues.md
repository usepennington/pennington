# Pennington Examples Validation Report

Generated: 2026-04-11T18:46:34Z
Examples discovered: 20
Examples built successfully: 8 (pass A) / 8 (pass B)
Total issues: 1482 errors, 1431 warnings, 28 info
Shown in report: 315 samples (capped at 10 per example/pass/code; full counts preserved in cross-example patterns below).

## How to read this report

- **Issue codes** group by category. `L.*` = link integrity, `T.*` = table of contents, `B.*` = base URL rewriting, `S.*` = search index, `M.*` = llms.txt, `X.*` = sitemap, `P.*` = SPA data, `R.*` = engine's own build report.
- **Rollup**: identical root causes (e.g. the same broken link appearing in every page's nav) are collapsed into a single issue marked `rolled up (N occurrences)` with a sample of affected pages. Cross-example pattern counts still show the raw underlying totals so the true scale is visible.
- **Cross-example patterns** at the top roll up counts by code across all examples. Codes appearing in many examples point to engine bugs; codes appearing in one example are usually example bugs.
- **Per-example sections** contain at most 10 representative samples per (pass, code). A truncation footer lists how many distinct groups were omitted beyond the cap.
- **Pass A** builds with base URL `/`. **Pass B** builds with base URL `/preview/` to exercise `BaseUrlRewritingProcessor`.

## Summary table

| Example | Pass A | Pass B | Errors | Warnings | Info |
| ------- | ------ | ------ | ------ | -------- | ---- |
| AlexBlogExample | fail | fail | 30 | 34 | 2 |
| BeaconDocsExample | fail | fail | 6 | 10 | 0 |
| BlogExample | fail | fail | 88 | 100 | 2 |
| ForgePortalExample | fail | fail | 22 | 18 | 2 |
| LocalizationExample | ok | ok | 0 | 0 | 0 |
| LocalizationTutorialExample | ok | ok | 0 | 0 | 0 |
| MaraBlogExample | fail | fail | 26 | 30 | 2 |
| MinimalExample | fail | fail | 12 | 16 | 2 |
| MultipleContentSourceExample | fail | fail | 14 | 0 | 2 |
| NorthwindHandbookExample | fail | fail | 140 | 0 | 2 |
| PrismDocsExample | ok | ok | 0 | 0 | 0 |
| RecipeExample | fail | fail | 46 | 42 | 2 |
| RoslynIntegrationExample | fail | fail | 10 | 16 | 2 |
| SearchExample | ok | ok | 4 | 2 | 0 |
| SpaNavigationExample | ok | ok | 0 | 0 | 2 |
| SpaNavigationTutorialExample | ok | ok | 0 | 0 | 2 |
| SpectreConsoleExample | fail | fail | 812 | 810 | 2 |
| TempoDocsExample | ok | ok | 0 | 0 | 0 |
| UserInterfaceExample | ok | ok | 0 | 1 | 2 |
| YogaStudioExample | fail | fail | 272 | 352 | 2 |

## Cross-example patterns

Counts below reflect **raw** issue totals (not the truncated samples shown in per-example sections). Codes that appear in multiple examples are likely engine bugs; codes unique to one example are likely example bugs.

- `R.BUILD_FAILED` (error) — 24 occurrences across 12 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+6 more)
- `L.BROKEN` (error) — 1304 raw occurrences rolled up into 180 distinct groups across 10 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+4 more)
- `R.PAGE_FAILED` (error) — 24 occurrences across 5 examples: BlogExample, MultipleContentSourceExample, NorthwindHandbookExample, RecipeExample, YogaStudioExample
- `T.DUP` (error) — 132 raw occurrences rolled up into 6 distinct groups across 1 example: NorthwindHandbookExample
- `M.NO_ENTRIES` (error) — 2 occurrences across 1 example: SearchExample
- `S.EMPTY` (error) — 2 occurrences across 1 example: SearchExample
- `S.MISSING_FIELD` (error) — 2 occurrences across 1 example: MinimalExample
- `X.BROKEN` (error) — 2 occurrences across 1 example: BeaconDocsExample
- `R.BROKEN_LINK` (warning) — 1418 raw occurrences rolled up into 192 distinct groups across 10 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+4 more)
- `X.EMPTY` (warning) — 2 occurrences across 1 example: SearchExample
- `B.MISSING_BODY_ATTR` (warning) — 1 occurrence across 1 example: UserInterfaceExample
- `M.MISSING` (info) — 28 occurrences across 14 examples: AlexBlogExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, MultipleContentSourceExample, … (+8 more)

## Issues by example

### AlexBlogExample

- Project: `examples/AlexBlogExample/AlexBlogExample.csproj`
- Pass A: build failed (exit 1), 3 HTML pages in output, 3.4s
- Pass B: build failed (exit 1), 3 HTML pages in output, 3.3s

#### ISSUE-0001 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/building-a-cli-part-1/index.html`
  - `blog/building-a-cli-part-2/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/cli` → missing target `/tags/cli`.
- Excerpt:
  ```
  <a href="/tags/cli" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">cli</a>
  ```

#### ISSUE-0002 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `blog/building-a-cli-part-1/index.html`
  - `blog/building-a-cli-part-2/index.html`
  - `blog/why-i-switched-to-linux/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/dotnet` → missing target `/tags/dotnet`.
- Excerpt:
  ```
  <a href="/tags/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0003 · `L.BROKEN` · error · pass A · rolled up (6 occurrences)
- Affects 3 pages:
  - `blog/building-a-cli-part-1/index.html`
  - `blog/building-a-cli-part-2/index.html`
  - `blog/why-i-switched-to-linux/index.html`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Selector: `div.relative > header.mx-auto > div.mx-auto > div.text-base-800 > a`
- Message: Link `/` → missing target `/`.
- Excerpt:
  ```
  <a href="/">Alex's Dev Blog</a>
  ```

#### ISSUE-0004 · `L.BROKEN` · error · pass A
- File: `blog/building-a-cli-part-2/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > p > a`
- Message: Link `/blog/2026/03/building-a-cli-part-1` → missing target `/blog/2026/03/building-a-cli-part-1`.
- Excerpt:
  ```
  <a href="/blog/2026/03/building-a-cli-part-1">Part 1</a>
  ```

#### ISSUE-0005 · `L.BROKEN` · error · pass A
- File: `blog/why-i-switched-to-linux/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/linux` → missing target `/tags/linux`.
- Excerpt:
  ```
  <a href="/tags/linux" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">linux</a>
  ```

#### ISSUE-0006 · `L.BROKEN` · error · pass A
- File: `blog/why-i-switched-to-linux/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/workflow` → missing target `/tags/workflow`.
- Excerpt:
  ```
  <a href="/tags/workflow" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">workflow</a>
  ```

#### ISSUE-0007 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
    17 broken links found:
      /blog/building-a-cli-part-1/ links to /rss.xml (Page not found)
      /blog/building-a-cli-part-1/ links to / (Page not found)
      /blog/building-a-cli-part-1/ links to /tags/dotnet (Page not found)
      /blog/building-a-cli-part-1/ links to /tags/cli (Page not found)
      /blog/building-a-cli-part-1/ links to / (Page not found)
      /blog/building-a-cli-part-2/ links to /rss.xml (Page not found)
      /blog/building-a-cli-part-2/ links to / (Page not found)
      /blog/building-a-cli-part-2/ links to /blog/2026/03/building-a-cli-part-1 (Page not found)
      /blog/building-a-cli-part-2/ links to /tags/dotnet (Page not found)
      /blog/building-a-cli-part-2/ links to /tags/cli (Page not found)
      /blog/building-a-cli-part-2/ links to / (Page not found)
      /blog/why-i-switched-to-linux/ links to /rss.xml (Page not found)
      /blog/why-i-switched-to-linux/ links to / (Page not found)
      /blog/why-i-switched-to-linux/ links to /tags/linux (Page not found)
      /blog/why-i-switched-to-linux/ links to /tags/workflow (Page not found)
      /blog/why-i-switched-to-linux/ links to /tags/dotnet (Page not found)
      /blog/why-i-switched-to-linux/ links to / (Page not found)
  
  
  ```

#### ISSUE-0008 · `R.BROKEN_LINK` · warning · pass A · rolled up (6 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
  - `/blog/why-i-switched-to-linux/`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to / (Page not found)
  ```

#### ISSUE-0009 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
  - `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0010 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
- Message: Engine reports broken link to `/tags/cli` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /tags/cli (Page not found)
  ```

#### ISSUE-0011 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
  - `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/tags/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /tags/dotnet (Page not found)
  ```

#### ISSUE-0012 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/building-a-cli-part-2/`
- Message: Engine reports broken link to `/blog/2026/03/building-a-cli-part-1` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to /blog/2026/03/building-a-cli-part-1 (Page not found)
  ```

#### ISSUE-0013 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/tags/linux` (Page not found).
- Excerpt:
  ```
  /blog/why-i-switched-to-linux/ links to /tags/linux (Page not found)
  ```

#### ISSUE-0014 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/tags/workflow` (Page not found).
- Excerpt:
  ```
  /blog/why-i-switched-to-linux/ links to /tags/workflow (Page not found)
  ```

#### ISSUE-0015 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0016 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/building-a-cli-part-1/index.html`
  - `blog/building-a-cli-part-2/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/cli` → missing target `/tags/cli`.
- Excerpt:
  ```
  <a href="/preview/tags/cli" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">cli</a>
  ```

#### ISSUE-0017 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `blog/building-a-cli-part-1/index.html`
  - `blog/building-a-cli-part-2/index.html`
  - `blog/why-i-switched-to-linux/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/dotnet` → missing target `/tags/dotnet`.
- Excerpt:
  ```
  <a href="/preview/tags/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0018 · `L.BROKEN` · error · pass B · rolled up (6 occurrences)
- Affects 3 pages:
  - `blog/building-a-cli-part-1/index.html`
  - `blog/building-a-cli-part-2/index.html`
  - `blog/why-i-switched-to-linux/index.html`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Selector: `div.relative > header.mx-auto > div.mx-auto > div.text-base-800 > a`
- Message: Link `/preview/` → missing target `/`.
- Excerpt:
  ```
  <a href="/preview/">Alex's Dev Blog</a>
  ```

#### ISSUE-0019 · `L.BROKEN` · error · pass B
- File: `blog/building-a-cli-part-2/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > p > a`
- Message: Link `/preview/blog/2026/03/building-a-cli-part-1` → missing target `/blog/2026/03/building-a-cli-part-1`.
- Excerpt:
  ```
  <a href="/preview/blog/2026/03/building-a-cli-part-1">Part 1</a>
  ```

#### ISSUE-0020 · `L.BROKEN` · error · pass B
- File: `blog/why-i-switched-to-linux/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/linux` → missing target `/tags/linux`.
- Excerpt:
  ```
  <a href="/preview/tags/linux" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">linux</a>
  ```

#### ISSUE-0021 · `L.BROKEN` · error · pass B
- File: `blog/why-i-switched-to-linux/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/workflow` → missing target `/tags/workflow`.
- Excerpt:
  ```
  <a href="/preview/tags/workflow" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">workflow</a>
  ```

#### ISSUE-0022 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
    17 broken links found:
      /blog/building-a-cli-part-1/ links to /preview/rss.xml (Page not found)
      /blog/building-a-cli-part-1/ links to /preview/ (Page not found)
      /blog/building-a-cli-part-1/ links to /preview/tags/dotnet (Page not found)
      /blog/building-a-cli-part-1/ links to /preview/tags/cli (Page not found)
      /blog/building-a-cli-part-1/ links to /preview/ (Page not found)
      /blog/building-a-cli-part-2/ links to /preview/rss.xml (Page not found)
      /blog/building-a-cli-part-2/ links to /preview/ (Page not found)
      /blog/building-a-cli-part-2/ links to /preview/blog/2026/03/building-a-cli-part-1 (Page not found)
      /blog/building-a-cli-part-2/ links to /preview/tags/dotnet (Page not found)
      /blog/building-a-cli-part-2/ links to /preview/tags/cli (Page not found)
      /blog/building-a-cli-part-2/ links to /preview/ (Page not found)
      /blog/why-i-switched-to-linux/ links to /preview/rss.xml (Page not found)
      /blog/why-i-switched-to-linux/ links to /preview/ (Page not found)
      /blog/why-i-switched-to-linux/ links to /preview/tags/linux (Page not found)
      /blog/why-i-switched-to-linux/ links to /preview/tags/workflow (Page not found)
      /blog/why-i-switched-to-linux/ links to /preview/tags/dotnet (Page not found)
      /blog/why-i-switched-to-linux/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0023 · `R.BROKEN_LINK` · warning · pass B · rolled up (6 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
  - `/blog/why-i-switched-to-linux/`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /preview/ (Page not found)
  ```

#### ISSUE-0024 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
  - `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0025 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
- Message: Engine reports broken link to `/preview/tags/cli` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /preview/tags/cli (Page not found)
  ```

#### ISSUE-0026 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-1/`
  - `/blog/building-a-cli-part-2/`
  - `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/preview/tags/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-1/ links to /preview/tags/dotnet (Page not found)
  ```

#### ISSUE-0027 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/building-a-cli-part-2/`
- Message: Engine reports broken link to `/preview/blog/2026/03/building-a-cli-part-1` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to /preview/blog/2026/03/building-a-cli-part-1 (Page not found)
  ```

#### ISSUE-0028 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/preview/tags/linux` (Page not found).
- Excerpt:
  ```
  /blog/why-i-switched-to-linux/ links to /preview/tags/linux (Page not found)
  ```

#### ISSUE-0029 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/preview/tags/workflow` (Page not found).
- Excerpt:
  ```
  /blog/why-i-switched-to-linux/ links to /preview/tags/workflow (Page not found)
  ```

#### ISSUE-0030 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### BeaconDocsExample

- Project: `examples/BeaconDocsExample/BeaconDocsExample.csproj`
- Pass A: build failed (exit 1), 10 HTML pages in output, 3.6s
- Pass B: build failed (exit 1), 10 HTML pages in output, 3.6s

#### ISSUE-0031 · `L.BROKEN` · error · pass A
- File: `setup/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > a`
- Message: Link `/getting-started/` → missing target `/getting-started/`.
- Excerpt:
  ```
  <a href="/getting-started/">Getting Started</a>
  ```

#### ISSUE-0032 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Production
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: B:\Penn\examples\BeaconDocsExample
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\BeaconDocsExample\Content with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 22 pages in 0.9s
    22 pages generated
  
  WARNINGS
    4 broken links found:
      /getting-started/index/ links to ./beacon-arch.png (Page not found)
      / links to /getting-started/ (Page not found)
      / links to /api/ (Page not found)
      /setup/ links to /getting-started/ (Page not found)
  
  
  ```

#### ISSUE-0033 · `X.BROKEN` · error · pass A
- File: `sitemap.xml`
- Message: sitemap <loc> `/https://beacon-docs.example.com` does not resolve to any output file.

#### ISSUE-0034 · `L.BROKEN` · warning · pass A
- File: `getting-started/index/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > img`
- Message: Image src `./beacon-arch.png` → missing target `/getting-started/index/beacon-arch.png`.
- Excerpt:
  ```
  <img src="./beacon-arch.png" alt="Beacon Architecture">
  ```

#### ISSUE-0035 · `R.BROKEN_LINK` · warning · pass A
- File: `/`
- Message: Engine reports broken link to `/api/` (Page not found).
- Excerpt:
  ```
  / links to /api/ (Page not found)
  ```

#### ISSUE-0036 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/setup/`
- Message: Engine reports broken link to `/getting-started/` (Page not found).
- Excerpt:
  ```
  / links to /getting-started/ (Page not found)
  ```

#### ISSUE-0037 · `R.BROKEN_LINK` · warning · pass A
- File: `/getting-started/index/`
- Message: Engine reports broken link to `./beacon-arch.png` (Page not found).
- Excerpt:
  ```
  /getting-started/index/ links to ./beacon-arch.png (Page not found)
  ```

#### ISSUE-0038 · `L.BROKEN` · error · pass B
- File: `setup/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > a`
- Message: Link `/preview/getting-started/` → missing target `/getting-started/`.
- Excerpt:
  ```
  <a href="/preview/getting-started/">Getting Started</a>
  ```

#### ISSUE-0039 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Production
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: B:\Penn\examples\BeaconDocsExample
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\BeaconDocsExample\Content with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 22 pages in 0.9s
    22 pages generated
  
  WARNINGS
    4 broken links found:
      /getting-started/index/ links to ./beacon-arch.png (Page not found)
      / links to /preview/getting-started/ (Page not found)
      / links to /preview/api/ (Page not found)
      /setup/ links to /preview/getting-started/ (Page not found)
  
  
  ```

#### ISSUE-0040 · `X.BROKEN` · error · pass B
- File: `sitemap.xml`
- Message: sitemap <loc> `/https://beacon-docs.example.com` does not resolve to any output file.

#### ISSUE-0041 · `L.BROKEN` · warning · pass B
- File: `getting-started/index/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > img`
- Message: Image src `./beacon-arch.png` → missing target `/getting-started/index/beacon-arch.png`.
- Excerpt:
  ```
  <img src="./beacon-arch.png" alt="Beacon Architecture">
  ```

#### ISSUE-0042 · `R.BROKEN_LINK` · warning · pass B
- File: `/`
- Message: Engine reports broken link to `/preview/api/` (Page not found).
- Excerpt:
  ```
  / links to /preview/api/ (Page not found)
  ```

#### ISSUE-0043 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/setup/`
- Message: Engine reports broken link to `/preview/getting-started/` (Page not found).
- Excerpt:
  ```
  / links to /preview/getting-started/ (Page not found)
  ```

#### ISSUE-0044 · `R.BROKEN_LINK` · warning · pass B
- File: `/getting-started/index/`
- Message: Engine reports broken link to `./beacon-arch.png` (Page not found).
- Excerpt:
  ```
  /getting-started/index/ links to ./beacon-arch.png (Page not found)
  ```

### BlogExample

- Project: `examples/BlogExample/BlogExample.csproj`
- Pass A: build failed (exit 1), 8 HTML pages in output, 3.3s
- Pass B: build failed (exit 1), 8 HTML pages in output, 3.3s

#### ISSUE-0045 · `L.BROKEN` · error · pass A · rolled up (15 occurrences)
- Affects 8 pages:
  - `about/index.html`
  - `blog/2024/03/chewing-magazine-review/index.html`
  - `blog/2024/03/top-five-gum-brands-analysis/index.html`
  - `blog/2024/04/gum-chewing-apparel-guide/index.html`
  - `blog/2024/04/mandibular-fitness-regime/index.html`
  - `blog/2024/05/bazooka-joe-interview/index.html`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Selector: `div.relative > header.mx-auto > div.mx-auto > div.text-base-800 > a`
- Message: Link `/` → missing target `/`.
- Excerpt:
  ```
  <a href="/">Calvin's Chewing Chronicles</a>
  ```

#### ISSUE-0046 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/chewing-magazine` → missing target `/tags/chewing-magazine`.
- Excerpt:
  ```
  <a href="/tags/chewing-magazine" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">chewing-magazine</a>
  ```

#### ISSUE-0047 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gum-culture` → missing target `/tags/gum-culture`.
- Excerpt:
  ```
  <a href="/tags/gum-culture" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-culture</a>
  ```

#### ISSUE-0048 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `blog/2024/03/chewing-magazine-review/index.html`
  - `blog/2024/03/top-five-gum-brands-analysis/index.html`
  - `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/reviews` → missing target `/tags/reviews`.
- Excerpt:
  ```
  <a href="/tags/reviews" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">reviews</a>
  ```

#### ISSUE-0049 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/analysis` → missing target `/tags/analysis`.
- Excerpt:
  ```
  <a href="/tags/analysis" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">analysis</a>
  ```

#### ISSUE-0050 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gum-brands` → missing target `/tags/gum-brands`.
- Excerpt:
  ```
  <a href="/tags/gum-brands" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-brands</a>
  ```

#### ISSUE-0051 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/science` → missing target `/tags/science`.
- Excerpt:
  ```
  <a href="/tags/science" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">science</a>
  ```

#### ISSUE-0052 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/apparel` → missing target `/tags/apparel`.
- Excerpt:
  ```
  <a href="/tags/apparel" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">apparel</a>
  ```

#### ISSUE-0053 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/equipment` → missing target `/tags/equipment`.
- Excerpt:
  ```
  <a href="/tags/equipment" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">equipment</a>
  ```

#### ISSUE-0054 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gear` → missing target `/tags/gear`.
- Excerpt:
  ```
  <a href="/tags/gear" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gear</a>
  ```

#### ISSUE-0055 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /blog/2024/03/chewing-magazine-review/ links to /tags/gum-culture (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to / (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /rss.xml (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to / (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /tags/interview (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /tags/bazooka-joe (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /tags/legend (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /tags/inspiration (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to / (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to /rss.xml (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to / (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to /tags/data-analytics (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to /tags/python (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to /tags/performance-tracking (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to /tags/optimization (Page not found)
      /blog/2024/05/chewing-data-analytics/ links to / (Page not found)
      /about/ links to /rss.xml (Page not found)
      /about/ links to / (Page not found)
  
  
  ```

#### ISSUE-0056 · `R.PAGE_FAILED` · error · pass A
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\BlogExample\root\about\index.html' because it is being used by another process.

#### ISSUE-0057 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/04/gum-chewing-apparel-guide/`
- Message: Engine reports broken link to `/tags/apparel` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/apparel (Page not found)
  ```

#### ISSUE-0058 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/04/gum-chewing-apparel-guide/`
- Message: Engine reports broken link to `/tags/equipment` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/equipment (Page not found)
  ```

#### ISSUE-0059 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/04/gum-chewing-apparel-guide/`
- Message: Engine reports broken link to `/tags/gear` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/gear (Page not found)
  ```

#### ISSUE-0060 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/03/chewing-magazine-review/`
- Message: Engine reports broken link to `/tags/reviews` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/reviews (Page not found)
  ```

#### ISSUE-0061 · `R.BROKEN_LINK` · warning · pass A · rolled up (15 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/03/chewing-magazine-review/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to / (Page not found)
  ```

#### ISSUE-0062 · `R.BROKEN_LINK` · warning · pass A · rolled up (8 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/03/chewing-magazine-review/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - … (+2 more)
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0063 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/tags/bubbles` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/bubbles (Page not found)
  ```

#### ISSUE-0064 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/tags/exercises` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/exercises (Page not found)
  ```

#### ISSUE-0065 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/tags/technique` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/technique (Page not found)
  ```

#### ISSUE-0066 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/mandibular-fitness-regime/`
- Message: Engine reports broken link to `/tags/training` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/training (Page not found)
  ```

#### ISSUE-0067 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0068 · `L.BROKEN` · error · pass B · rolled up (15 occurrences)
- Affects 8 pages:
  - `about/index.html`
  - `blog/2024/03/chewing-magazine-review/index.html`
  - `blog/2024/03/top-five-gum-brands-analysis/index.html`
  - `blog/2024/04/gum-chewing-apparel-guide/index.html`
  - `blog/2024/04/mandibular-fitness-regime/index.html`
  - `blog/2024/05/bazooka-joe-interview/index.html`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Selector: `div.relative > header.mx-auto > div.mx-auto > div.text-base-800 > a`
- Message: Link `/preview/` → missing target `/`.
- Excerpt:
  ```
  <a href="/preview/">Calvin's Chewing Chronicles</a>
  ```

#### ISSUE-0069 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/chewing-magazine` → missing target `/tags/chewing-magazine`.
- Excerpt:
  ```
  <a href="/preview/tags/chewing-magazine" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">chewing-magazi...
  ```

#### ISSUE-0070 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gum-culture` → missing target `/tags/gum-culture`.
- Excerpt:
  ```
  <a href="/preview/tags/gum-culture" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-culture</a>
  ```

#### ISSUE-0071 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `blog/2024/03/chewing-magazine-review/index.html`
  - `blog/2024/03/top-five-gum-brands-analysis/index.html`
  - `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/reviews` → missing target `/tags/reviews`.
- Excerpt:
  ```
  <a href="/preview/tags/reviews" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">reviews</a>
  ```

#### ISSUE-0072 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/analysis` → missing target `/tags/analysis`.
- Excerpt:
  ```
  <a href="/preview/tags/analysis" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">analysis</a>
  ```

#### ISSUE-0073 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gum-brands` → missing target `/tags/gum-brands`.
- Excerpt:
  ```
  <a href="/preview/tags/gum-brands" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-brands</a>
  ```

#### ISSUE-0074 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/science` → missing target `/tags/science`.
- Excerpt:
  ```
  <a href="/preview/tags/science" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">science</a>
  ```

#### ISSUE-0075 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/apparel` → missing target `/tags/apparel`.
- Excerpt:
  ```
  <a href="/preview/tags/apparel" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">apparel</a>
  ```

#### ISSUE-0076 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/equipment` → missing target `/tags/equipment`.
- Excerpt:
  ```
  <a href="/preview/tags/equipment" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">equipment</a>
  ```

#### ISSUE-0077 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gear` → missing target `/tags/gear`.
- Excerpt:
  ```
  <a href="/preview/tags/gear" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gear</a>
  ```

#### ISSUE-0078 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/tags/science (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/tags/reviews (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/ (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/rss.xml (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/ (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/tags/reviews (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/tags/chewing-magazine (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/tags/gum-culture (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/ (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/rss.xml (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/ (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/interview (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/bazooka-joe (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/legend (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/inspiration (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/ (Page not found)
      /about/ links to /preview/rss.xml (Page not found)
      /about/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0079 · `R.PAGE_FAILED` · error · pass B
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\BlogExample\prefixed\about\index.html' because it is being used by another process.

#### ISSUE-0080 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/data-analytics` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/data-analytics (Page not found)
  ```

#### ISSUE-0081 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/optimization` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/optimization (Page not found)
  ```

#### ISSUE-0082 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/performance-tracking` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/performance-tracking (Page not found)
  ```

#### ISSUE-0083 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/python` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/python (Page not found)
  ```

#### ISSUE-0084 · `R.BROKEN_LINK` · warning · pass B · rolled up (15 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/03/chewing-magazine-review/`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/ (Page not found)
  ```

#### ISSUE-0085 · `R.BROKEN_LINK` · warning · pass B · rolled up (8 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/03/chewing-magazine-review/`
  - … (+2 more)
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0086 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/preview/tags/bubbles` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/bubbles (Page not found)
  ```

#### ISSUE-0087 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/preview/tags/exercises` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/exercises (Page not found)
  ```

#### ISSUE-0088 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/preview/tags/technique` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/technique (Page not found)
  ```

#### ISSUE-0089 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/mandibular-fitness-regime/`
- Message: Engine reports broken link to `/preview/tags/training` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/training (Page not found)
  ```

#### ISSUE-0090 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

_Additional issues not shown (truncated to 10 samples per code):_

- `L.BROKEN` (pass A): 15 more similar issues
- `R.BROKEN_LINK` (pass A): 16 more similar issues
- `L.BROKEN` (pass B): 15 more similar issues
- `R.BROKEN_LINK` (pass B): 16 more similar issues

### ForgePortalExample

- Project: `examples/ForgePortalExample/ForgePortalExample.csproj`
- Pass A: build failed (exit 1), 10 HTML pages in output, 2.9s
- Pass B: build failed (exit 1), 10 HTML pages in output, 2.9s

#### ISSUE-0091 · `L.BROKEN` · error · pass A · rolled up (10 occurrences)
- Affects 10 pages:
  - `404.html`
  - `about/index.html`
  - `blog/q1-retro/index.html`
  - `blog/welcome/index.html`
  - `docs/api-keys/index.html`
  - `docs/getting-started/index.html`
  - … (+4 more)
- Selector: `html > body > div.min-h-screen > header.border-b > a.text-lg`
- Message: Link `/` → missing target `/`.
- Excerpt:
  ```
  <a href="/" class="text-lg font-bold text-neutral-900 hover:text-neutral-700 transition">Forge</a>
  ```

#### ISSUE-0092 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\ForgePortalExample\Content\pages with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 12 pages in 0.6s
    12 pages generated
  
  WARNINGS
    9 broken links found:
      /blog/welcome/ links to / (Page not found)
      /docs/api-keys/ links to / (Page not found)
      /docs/pipeline-config/ links to / (Page not found)
      /blog/q1-retro/ links to / (Page not found)
      /about/ links to / (Page not found)
      /releases/v2-0-0/ links to / (Page not found)
      /docs/getting-started/ links to / (Page not found)
      /releases/v2-0-1/ links to / (Page not found)
      /releases/v2-1-0/ links to / (Page not found)
  
  
  ```

#### ISSUE-0093 · `R.BROKEN_LINK` · warning · pass A · rolled up (9 occurrences)
- Affects 9 pages:
  - `/blog/welcome/`
  - `/docs/api-keys/`
  - `/docs/pipeline-config/`
  - `/blog/q1-retro/`
  - `/about/`
  - `/releases/v2-0-0/`
  - … (+3 more)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/welcome/ links to / (Page not found)
  ```

#### ISSUE-0094 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0095 · `L.BROKEN` · error · pass B · rolled up (10 occurrences)
- Affects 10 pages:
  - `404.html`
  - `about/index.html`
  - `blog/q1-retro/index.html`
  - `blog/welcome/index.html`
  - `docs/api-keys/index.html`
  - `docs/getting-started/index.html`
  - … (+4 more)
- Selector: `html > body > div.min-h-screen > header.border-b > a.text-lg`
- Message: Link `/preview/` → missing target `/`.
- Excerpt:
  ```
  <a href="/preview/" class="text-lg font-bold text-neutral-900 hover:text-neutral-700 transition">Forge</a>
  ```

#### ISSUE-0096 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\ForgePortalExample\Content\pages with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 12 pages in 0.6s
    12 pages generated
  
  WARNINGS
    9 broken links found:
      /releases/v2-0-0/ links to /preview/ (Page not found)
      /docs/api-keys/ links to /preview/ (Page not found)
      /about/ links to /preview/ (Page not found)
      /docs/pipeline-config/ links to /preview/ (Page not found)
      /releases/v2-0-1/ links to /preview/ (Page not found)
      /blog/q1-retro/ links to /preview/ (Page not found)
      /docs/getting-started/ links to /preview/ (Page not found)
      /blog/welcome/ links to /preview/ (Page not found)
      /releases/v2-1-0/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0097 · `R.BROKEN_LINK` · warning · pass B · rolled up (9 occurrences)
- Affects 9 pages:
  - `/releases/v2-0-0/`
  - `/docs/api-keys/`
  - `/about/`
  - `/docs/pipeline-config/`
  - `/releases/v2-0-1/`
  - `/blog/q1-retro/`
  - … (+3 more)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /releases/v2-0-0/ links to /preview/ (Page not found)
  ```

#### ISSUE-0098 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### LocalizationExample

- Project: `examples/LocalizationExample/LocalizationExample.csproj`
- Pass A: built ok, 26 HTML pages in output, 3.6s
- Pass B: built ok, 26 HTML pages in output, 3.8s
- No issues.

### LocalizationTutorialExample

- Project: `examples/LocalizationTutorialExample/LocalizationTutorialExample.csproj`
- Pass A: built ok, 7 HTML pages in output, 3.2s
- Pass B: built ok, 7 HTML pages in output, 3.1s
- No issues.

### MaraBlogExample

- Project: `examples/MaraBlogExample/MaraBlogExample.csproj`
- Pass A: build failed (exit 1), 3 HTML pages in output, 3.1s
- Pass B: build failed (exit 1), 3 HTML pages in output, 3.1s

#### ISSUE-0099 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/dotnet` → missing target `/topics/dotnet`.
- Excerpt:
  ```
  <a href="/topics/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0100 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/performance` → missing target `/topics/performance`.
- Excerpt:
  ```
  <a href="/topics/performance" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">performance</a>
  ```

#### ISSUE-0101 · `L.BROKEN` · error · pass A · rolled up (6 occurrences)
- Affects 3 pages:
  - `blog/allocation-traps/index.html`
  - `blog/config-pitfalls/index.html`
  - `blog/span-patterns/index.html`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Selector: `div.relative > header.mx-auto > div.mx-auto > div.text-base-800 > a`
- Message: Link `/` → missing target `/`.
- Excerpt:
  ```
  <a href="/">Mara Writes Code</a>
  ```

#### ISSUE-0102 · `L.BROKEN` · error · pass A
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/aspnet` → missing target `/topics/aspnet`.
- Excerpt:
  ```
  <a href="/topics/aspnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">aspnet</a>
  ```

#### ISSUE-0103 · `L.BROKEN` · error · pass A
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/configuration` → missing target `/topics/configuration`.
- Excerpt:
  ```
  <a href="/topics/configuration" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">configuration</a>
  ```

#### ISSUE-0104 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  
  WARNINGS
    15 broken links found:
      /blog/allocation-traps/ links to /rss.xml (Page not found)
      /blog/allocation-traps/ links to / (Page not found)
      /blog/allocation-traps/ links to /topics/performance (Page not found)
      /blog/allocation-traps/ links to /topics/dotnet (Page not found)
      /blog/allocation-traps/ links to / (Page not found)
      /blog/span-patterns/ links to /rss.xml (Page not found)
      /blog/span-patterns/ links to / (Page not found)
      /blog/span-patterns/ links to /topics/performance (Page not found)
      /blog/span-patterns/ links to /topics/dotnet (Page not found)
      /blog/span-patterns/ links to / (Page not found)
      /blog/config-pitfalls/ links to /rss.xml (Page not found)
      /blog/config-pitfalls/ links to / (Page not found)
      /blog/config-pitfalls/ links to /topics/aspnet (Page not found)
      /blog/config-pitfalls/ links to /topics/configuration (Page not found)
      /blog/config-pitfalls/ links to / (Page not found)
  
  
  ```

#### ISSUE-0105 · `R.BROKEN_LINK` · warning · pass A · rolled up (6 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
  - `/blog/config-pitfalls/`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to / (Page not found)
  ```

#### ISSUE-0106 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
  - `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0107 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/topics/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /topics/dotnet (Page not found)
  ```

#### ISSUE-0108 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/topics/performance` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /topics/performance (Page not found)
  ```

#### ISSUE-0109 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/topics/aspnet` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /topics/aspnet (Page not found)
  ```

#### ISSUE-0110 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/topics/configuration` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /topics/configuration (Page not found)
  ```

#### ISSUE-0111 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0112 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/dotnet` → missing target `/topics/dotnet`.
- Excerpt:
  ```
  <a href="/preview/topics/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0113 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/performance` → missing target `/topics/performance`.
- Excerpt:
  ```
  <a href="/preview/topics/performance" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">performance</a>
  ```

#### ISSUE-0114 · `L.BROKEN` · error · pass B · rolled up (6 occurrences)
- Affects 3 pages:
  - `blog/allocation-traps/index.html`
  - `blog/config-pitfalls/index.html`
  - `blog/span-patterns/index.html`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Selector: `div.relative > header.mx-auto > div.mx-auto > div.text-base-800 > a`
- Message: Link `/preview/` → missing target `/`.
- Excerpt:
  ```
  <a href="/preview/">Mara Writes Code</a>
  ```

#### ISSUE-0115 · `L.BROKEN` · error · pass B
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/aspnet` → missing target `/topics/aspnet`.
- Excerpt:
  ```
  <a href="/preview/topics/aspnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">aspnet</a>
  ```

#### ISSUE-0116 · `L.BROKEN` · error · pass B
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/configuration` → missing target `/topics/configuration`.
- Excerpt:
  ```
  <a href="/preview/topics/configuration" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">configuration</...
  ```

#### ISSUE-0117 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  
  WARNINGS
    15 broken links found:
      /blog/allocation-traps/ links to /preview/rss.xml (Page not found)
      /blog/allocation-traps/ links to /preview/ (Page not found)
      /blog/allocation-traps/ links to /preview/topics/performance (Page not found)
      /blog/allocation-traps/ links to /preview/topics/dotnet (Page not found)
      /blog/allocation-traps/ links to /preview/ (Page not found)
      /blog/config-pitfalls/ links to /preview/rss.xml (Page not found)
      /blog/config-pitfalls/ links to /preview/ (Page not found)
      /blog/config-pitfalls/ links to /preview/topics/aspnet (Page not found)
      /blog/config-pitfalls/ links to /preview/topics/configuration (Page not found)
      /blog/config-pitfalls/ links to /preview/ (Page not found)
      /blog/span-patterns/ links to /preview/rss.xml (Page not found)
      /blog/span-patterns/ links to /preview/ (Page not found)
      /blog/span-patterns/ links to /preview/topics/performance (Page not found)
      /blog/span-patterns/ links to /preview/topics/dotnet (Page not found)
      /blog/span-patterns/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0118 · `R.BROKEN_LINK` · warning · pass B · rolled up (6 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/config-pitfalls/`
  - `/blog/span-patterns/`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/ (Page not found)
  ```

#### ISSUE-0119 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/config-pitfalls/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0120 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/preview/topics/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/topics/dotnet (Page not found)
  ```

#### ISSUE-0121 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/preview/topics/performance` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/topics/performance (Page not found)
  ```

#### ISSUE-0122 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/topics/aspnet` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /preview/topics/aspnet (Page not found)
  ```

#### ISSUE-0123 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/topics/configuration` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /preview/topics/configuration (Page not found)
  ```

#### ISSUE-0124 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MinimalExample

- Project: `examples/MinimalExample/MinimalExample.csproj`
- Pass A: build failed (exit 1), 6 HTML pages in output, 2.6s
- Pass B: build failed (exit 1), 6 HTML pages in output, 2.6s

#### ISSUE-0125 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-one/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `sample-post` → missing target `/sub-folder/page-one/sample-post`.
- Excerpt:
  ```
  <a href="sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0126 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../` → missing target `/sub-folder/`.
- Excerpt:
  ```
  <a href="../">Home</a>
  ```

#### ISSUE-0127 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../sub-folder/sample-post` → missing target `/sub-folder/sub-folder/sample-post`.
- Excerpt:
  ```
  <a href="../sub-folder/sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0128 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `page-one` → missing target `/sub-folder/page-two/page-one`.
- Excerpt:
  ```
  <a href="page-one">Page One - Getting Started</a>
  ```

#### ISSUE-0129 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Hosting environment: Production
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: B:\Penn\examples\MinimalExample
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\MinimalExample\Content with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 8 pages in 0.5s
    8 pages generated
  
  WARNINGS
    6 broken links found:
      /sub-folder/page-one/ links to sample-post (Page not found)
      /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
      /sub-folder/page-two/ links to page-one (Page not found)
      /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
      /sub-folder/page-two/ links to ../ (Page not found)
  
  
  ```

#### ISSUE-0130 · `S.MISSING_FIELD` · error · pass A
- File: `search-index.json`
- Selector: `[1]`
- Message: search-index entry #1 missing/empty: title, body (title=, url=/sub-folder/page-1/).

#### ISSUE-0131 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0132 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/sample-post/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0133 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-one/`
- Message: Engine reports broken link to `sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-one/ links to sample-post (Page not found)
  ```

#### ISSUE-0134 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../ (Page not found)
  ```

#### ISSUE-0135 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../sub-folder/sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
  ```

#### ISSUE-0136 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `page-one` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to page-one (Page not found)
  ```

#### ISSUE-0137 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0138 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0139 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0140 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-one/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `sample-post` → missing target `/sub-folder/page-one/sample-post`.
- Excerpt:
  ```
  <a href="sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0141 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../` → missing target `/sub-folder/`.
- Excerpt:
  ```
  <a href="../">Home</a>
  ```

#### ISSUE-0142 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../sub-folder/sample-post` → missing target `/sub-folder/sub-folder/sample-post`.
- Excerpt:
  ```
  <a href="../sub-folder/sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0143 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `page-one` → missing target `/sub-folder/page-two/page-one`.
- Excerpt:
  ```
  <a href="page-one">Page One - Getting Started</a>
  ```

#### ISSUE-0144 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Hosting environment: Production
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: B:\Penn\examples\MinimalExample
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\MinimalExample\Content with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 8 pages in 0.5s
    8 pages generated
  
  WARNINGS
    6 broken links found:
      /sub-folder/page-two/ links to page-one (Page not found)
      /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
      /sub-folder/page-two/ links to ../ (Page not found)
      /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
      /sub-folder/page-one/ links to sample-post (Page not found)
  
  
  ```

#### ISSUE-0145 · `S.MISSING_FIELD` · error · pass B
- File: `search-index.json`
- Selector: `[1]`
- Message: search-index entry #1 missing/empty: title, body (title=, url=/sub-folder/page-1/).

#### ISSUE-0146 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0147 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/sample-post/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0148 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-one/`
- Message: Engine reports broken link to `sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-one/ links to sample-post (Page not found)
  ```

#### ISSUE-0149 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../ (Page not found)
  ```

#### ISSUE-0150 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../sub-folder/sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
  ```

#### ISSUE-0151 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `page-one` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to page-one (Page not found)
  ```

#### ISSUE-0152 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0153 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0154 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MultipleContentSourceExample

- Project: `examples/MultipleContentSourceExample/MultipleContentSourceExample.csproj`
- Pass A: build failed (exit 1), 9 HTML pages in output, 3.0s
- Pass B: build failed (exit 1), 9 HTML pages in output, 2.7s

#### ISSUE-0155 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
    /docs/home-organization-systems/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\home-organization-systems\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md
    /blog/mystery-of-missing-socks/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\mystery-of-missing-socks\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md
    /docs/coffee-brewing-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\coffee-brewing-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md
    /blog/best-pizza-toppings/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\best-pizza-toppings\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md
    /blog/office-plant-survival-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\office-plant-survival-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md
    /docs/indoor-herb-garden/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\indoor-herb-garden\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md
  
  
  ```

#### ISSUE-0156 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/best-pizza-toppings/`
- Message: Engine reported page generation failure for `/blog/best-pizza-toppings/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\best-pizza-toppings\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md

#### ISSUE-0157 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/mystery-of-missing-socks/`
- Message: Engine reported page generation failure for `/blog/mystery-of-missing-socks/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\mystery-of-missing-socks\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md

#### ISSUE-0158 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/office-plant-survival-guide/`
- Message: Engine reported page generation failure for `/blog/office-plant-survival-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\office-plant-survival-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md

#### ISSUE-0159 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/coffee-brewing-guide/`
- Message: Engine reported page generation failure for `/docs/coffee-brewing-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\coffee-brewing-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md

#### ISSUE-0160 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/home-organization-systems/`
- Message: Engine reported page generation failure for `/docs/home-organization-systems/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\home-organization-systems\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md

#### ISSUE-0161 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/indoor-herb-garden/`
- Message: Engine reported page generation failure for `/docs/indoor-herb-garden/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\indoor-herb-garden\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md

#### ISSUE-0162 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0163 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
    /docs/coffee-brewing-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\coffee-brewing-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md
    /docs/indoor-herb-garden/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\indoor-herb-garden\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md
    /blog/mystery-of-missing-socks/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\mystery-of-missing-socks\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md
    /docs/home-organization-systems/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\home-organization-systems\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md
    /blog/office-plant-survival-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\office-plant-survival-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md
    /blog/best-pizza-toppings/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\best-pizza-toppings\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md
  
  
  ```

#### ISSUE-0164 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/best-pizza-toppings/`
- Message: Engine reported page generation failure for `/blog/best-pizza-toppings/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\best-pizza-toppings\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md

#### ISSUE-0165 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/mystery-of-missing-socks/`
- Message: Engine reported page generation failure for `/blog/mystery-of-missing-socks/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\mystery-of-missing-socks\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md

#### ISSUE-0166 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/office-plant-survival-guide/`
- Message: Engine reported page generation failure for `/blog/office-plant-survival-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\office-plant-survival-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md

#### ISSUE-0167 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/coffee-brewing-guide/`
- Message: Engine reported page generation failure for `/docs/coffee-brewing-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\coffee-brewing-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md

#### ISSUE-0168 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/home-organization-systems/`
- Message: Engine reported page generation failure for `/docs/home-organization-systems/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\home-organization-systems\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md

#### ISSUE-0169 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/indoor-herb-garden/`
- Message: Engine reported page generation failure for `/docs/indoor-herb-garden/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\indoor-herb-garden\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md

#### ISSUE-0170 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### NorthwindHandbookExample

- Project: `examples/NorthwindHandbookExample/NorthwindHandbookExample.csproj`
- Pass A: build failed (exit 1), 11 HTML pages in output, 2.7s
- Pass B: build failed (exit 1), 11 HTML pages in output, 2.7s

#### ISSUE-0171 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
           at System.IO.File.WriteToFileAsync(String path, FileMode mode, ReadOnlyMemory`1 contents, Encoding encoding, CancellationToken cancellationToken)
           at Pennington.Generation.OutputGenerationService.<>c__DisplayClass13_0.<<FetchPagesAsync>b__0>d.MoveNext() in B:\Penn\src\Pennington\Generation\OutputGenerationService.cs:line 318
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 16 pages in 0.6s
    13 pages generated
    3 pages failed
  
  ERRORS
    /changelog/v2-1-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-1-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-1-0.md
    /changelog/v2-0-1/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-1\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md
    /changelog/v2-0-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md
  
  
  ```

#### ISSUE-0172 · `R.PAGE_FAILED` · error · pass A
- File: `/changelog/v2-0-0/`
- Message: Engine reported page generation failure for `/changelog/v2-0-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md

#### ISSUE-0173 · `R.PAGE_FAILED` · error · pass A
- File: `/changelog/v2-0-1/`
- Message: Engine reported page generation failure for `/changelog/v2-0-1/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-1\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md

#### ISSUE-0174 · `R.PAGE_FAILED` · error · pass A
- File: `/changelog/v2-1-0/`
- Message: Engine reported page generation failure for `/changelog/v2-1-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-1-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-1-0.md

#### ISSUE-0175 · `T.DUP` · error · pass A · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - `changelog/v2-1-0/index.html`
  - `development/coding-standards/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/changelog/v2-0-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/changelog/v2-0-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-4...
  ```

#### ISSUE-0176 · `T.DUP` · error · pass A · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - `changelog/v2-1-0/index.html`
  - `development/coding-standards/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/changelog/v2-0-1/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/changelog/v2-0-1/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-4...
  ```

#### ISSUE-0177 · `T.DUP` · error · pass A · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - `changelog/v2-1-0/index.html`
  - `development/coding-standards/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/changelog/v2-1-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/changelog/v2-1-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-4...
  ```

#### ISSUE-0178 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0179 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
           at System.IO.File.WriteToFileAsync(String path, FileMode mode, ReadOnlyMemory`1 contents, Encoding encoding, CancellationToken cancellationToken)
           at Pennington.Generation.OutputGenerationService.<>c__DisplayClass13_0.<<FetchPagesAsync>b__0>d.MoveNext() in B:\Penn\src\Pennington\Generation\OutputGenerationService.cs:line 318
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 16 pages in 0.6s
    13 pages generated
    3 pages failed
  
  ERRORS
    /changelog/v2-0-1/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-1\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md
    /changelog/v2-0-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md
    /changelog/v2-1-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-1-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-1-0.md
  
  
  ```

#### ISSUE-0180 · `R.PAGE_FAILED` · error · pass B
- File: `/changelog/v2-0-0/`
- Message: Engine reported page generation failure for `/changelog/v2-0-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md

#### ISSUE-0181 · `R.PAGE_FAILED` · error · pass B
- File: `/changelog/v2-0-1/`
- Message: Engine reported page generation failure for `/changelog/v2-0-1/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-1\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md

#### ISSUE-0182 · `R.PAGE_FAILED` · error · pass B
- File: `/changelog/v2-1-0/`
- Message: Engine reported page generation failure for `/changelog/v2-1-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-1-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-1-0.md

#### ISSUE-0183 · `T.DUP` · error · pass B · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - `changelog/v2-1-0/index.html`
  - `development/coding-standards/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/preview/changelog/v2-0-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/preview/changelog/v2-0-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:tex...
  ```

#### ISSUE-0184 · `T.DUP` · error · pass B · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - `changelog/v2-1-0/index.html`
  - `development/coding-standards/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/preview/changelog/v2-0-1/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/preview/changelog/v2-0-1/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:tex...
  ```

#### ISSUE-0185 · `T.DUP` · error · pass B · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - `changelog/v2-1-0/index.html`
  - `development/coding-standards/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/preview/changelog/v2-1-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/preview/changelog/v2-1-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:tex...
  ```

#### ISSUE-0186 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### PrismDocsExample

- Project: `examples/PrismDocsExample/PrismDocsExample.csproj`
- Pass A: built ok, 4 HTML pages in output, 4.2s
- Pass B: built ok, 4 HTML pages in output, 4.2s
- No issues.

### RecipeExample

- Project: `examples/RecipeExample/RecipeExample.csproj`
- Pass A: build failed (exit 1), 7 HTML pages in output, 7.2s
- Pass B: build failed (exit 1), 7 HTML pages in output, 6.6s

#### ISSUE-0187 · `L.BROKEN` · error · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `recipes/bacon-wrapped-jalapenos/index.html`
  - `recipes/beer-cheese/index.html`
  - `recipes/cajun-chicken-pasta/index.html`
  - `recipes/chex-mix/index.html`
  - `recipes/chicken-piccata/index.html`
  - `recipes/chili/index.html`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Selector: `header.bg-white > div.max-w-6xl > nav.flex > div.flex > a.flex`
- Message: Link `/` → missing target `/`.
- Excerpt:
  ```
  <a href="/" class="flex items-center space-x-3 text-xl text-primary-900 hover:text-primary-600 transition-colors duration-200"><div class="w-8 h-8 bg-primary-500 rounded-lg flex items-center justify-center"><svg class="w-5 h-5 text-white" f...
  ```

#### ISSUE-0188 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /recipes/bacon-wrapped-jalapenos links to / (Page not found)
      /recipes/bacon-wrapped-jalapenos links to / (Page not found)
      /recipes/bacon-wrapped-jalapenos links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
  
  
  ```

#### ISSUE-0189 · `R.PAGE_FAILED` · error · pass A
- File: `/sitemap.xml`
- Message: Engine reported page generation failure for `/sitemap.xml`: HTTP 500 fetching /sitemap.xml

#### ISSUE-0190 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `/recipes/chili`
  - `/recipes/bacon-wrapped-jalapenos`
  - `/recipes/zuppa-toscana`
  - `/recipes/chex-mix`
  - `/recipes/beer-cheese`
  - `/recipes/cajun-chicken-pasta`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /recipes/chili links to / (Page not found)
  ```

#### ISSUE-0191 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0192 · `L.BROKEN` · error · pass B · rolled up (21 occurrences)
- Affects 7 pages:
  - `recipes/bacon-wrapped-jalapenos/index.html`
  - `recipes/beer-cheese/index.html`
  - `recipes/cajun-chicken-pasta/index.html`
  - `recipes/chex-mix/index.html`
  - `recipes/chicken-piccata/index.html`
  - `recipes/chili/index.html`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Selector: `header.bg-white > div.max-w-6xl > nav.flex > div.flex > a.flex`
- Message: Link `/preview/` → missing target `/`.
- Excerpt:
  ```
  <a href="/preview/" class="flex items-center space-x-3 text-xl text-primary-900 hover:text-primary-600 transition-colors duration-200"><div class="w-8 h-8 bg-primary-500 rounded-lg flex items-center justify-center"><svg class="w-5 h-5 text-...
  ```

#### ISSUE-0193 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /recipes/zuppa-toscana links to /preview/ (Page not found)
      /recipes/zuppa-toscana links to /preview/ (Page not found)
      /recipes/zuppa-toscana links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0194 · `R.PAGE_FAILED` · error · pass B
- File: `/sitemap.xml`
- Message: Engine reported page generation failure for `/sitemap.xml`: HTTP 500 fetching /sitemap.xml

#### ISSUE-0195 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 7 pages:
  - `/recipes/chili`
  - `/recipes/zuppa-toscana`
  - `/recipes/beer-cheese`
  - `/recipes/chex-mix`
  - `/recipes/cajun-chicken-pasta`
  - `/recipes/chicken-piccata`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /recipes/chili links to /preview/ (Page not found)
  ```

#### ISSUE-0196 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### RoslynIntegrationExample

- Project: `examples/RoslynIntegrationExample/RoslynIntegrationExample.csproj`
- Pass A: build failed (exit 1), 6 HTML pages in output, 13.3s
- Pass B: build failed (exit 1), 6 HTML pages in output, 14.0s

#### ISSUE-0197 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-one/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `sample-post` → missing target `/sub-folder/page-one/sample-post`.
- Excerpt:
  ```
  <a href="sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0198 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../` → missing target `/sub-folder/`.
- Excerpt:
  ```
  <a href="../">Home</a>
  ```

#### ISSUE-0199 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../sub-folder/sample-post` → missing target `/sub-folder/sub-folder/sample-post`.
- Excerpt:
  ```
  <a href="../sub-folder/sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0200 · `L.BROKEN` · error · pass A
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `page-one` → missing target `/sub-folder/page-two/page-one`.
- Excerpt:
  ```
  <a href="page-one">Page One - Getting Started</a>
  ```

#### ISSUE-0201 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Adding file watch: B:\Penn with pattern *.sln
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.slnx
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.cs
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 8 pages in 10.9s
    8 pages generated
  
  WARNINGS
    6 broken links found:
      /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
      /sub-folder/page-one/ links to sample-post (Page not found)
      /sub-folder/page-two/ links to page-one (Page not found)
      /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
      /sub-folder/page-two/ links to ../ (Page not found)
  
  
  ```

#### ISSUE-0202 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0203 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/sample-post/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0204 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-one/`
- Message: Engine reports broken link to `sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-one/ links to sample-post (Page not found)
  ```

#### ISSUE-0205 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../ (Page not found)
  ```

#### ISSUE-0206 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../sub-folder/sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
  ```

#### ISSUE-0207 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `page-one` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to page-one (Page not found)
  ```

#### ISSUE-0208 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0209 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0210 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0211 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-one/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `sample-post` → missing target `/sub-folder/page-one/sample-post`.
- Excerpt:
  ```
  <a href="sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0212 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../` → missing target `/sub-folder/`.
- Excerpt:
  ```
  <a href="../">Home</a>
  ```

#### ISSUE-0213 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `../sub-folder/sample-post` → missing target `/sub-folder/sub-folder/sample-post`.
- Excerpt:
  ```
  <a href="../sub-folder/sample-post">Sample Post with Images</a>
  ```

#### ISSUE-0214 · `L.BROKEN` · error · pass B
- File: `sub-folder/page-two/index.html`
- Selector: `article > div.prose > ul > li > a`
- Message: Link `page-one` → missing target `/sub-folder/page-two/page-one`.
- Excerpt:
  ```
  <a href="page-one">Page One - Getting Started</a>
  ```

#### ISSUE-0215 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Adding file watch: B:\Penn with pattern *.sln
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.slnx
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.cs
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 8 pages in 11.6s
    8 pages generated
  
  WARNINGS
    6 broken links found:
      /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
      /sub-folder/page-two/ links to page-one (Page not found)
      /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
      /sub-folder/page-two/ links to ../ (Page not found)
      /sub-folder/page-one/ links to sample-post (Page not found)
  
  
  ```

#### ISSUE-0216 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0217 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/sample-post/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0218 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-one/`
- Message: Engine reports broken link to `sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-one/ links to sample-post (Page not found)
  ```

#### ISSUE-0219 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../ (Page not found)
  ```

#### ISSUE-0220 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `../sub-folder/sample-post` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to ../sub-folder/sample-post (Page not found)
  ```

#### ISSUE-0221 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/page-two/`
- Message: Engine reports broken link to `page-one` (Page not found).
- Excerpt:
  ```
  /sub-folder/page-two/ links to page-one (Page not found)
  ```

#### ISSUE-0222 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0223 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0224 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SearchExample

- Project: `examples/SearchExample/SearchExample.csproj`
- Pass A: built ok, 1001 HTML pages in output, 13.6s
- Pass B: built ok, 1001 HTML pages in output, 13.4s

#### ISSUE-0225 · `M.NO_ENTRIES` · error · pass A
- File: `llms.txt`
- Message: llms.txt contains no markdown link entries.

#### ISSUE-0226 · `S.EMPTY` · error · pass A
- File: `search-index.json`
- Message: search-index.json is an empty array.

#### ISSUE-0227 · `X.EMPTY` · warning · pass A
- File: `sitemap.xml`
- Message: sitemap.xml has no <loc> entries.

#### ISSUE-0228 · `M.NO_ENTRIES` · error · pass B
- File: `llms.txt`
- Message: llms.txt contains no markdown link entries.

#### ISSUE-0229 · `S.EMPTY` · error · pass B
- File: `search-index.json`
- Message: search-index.json is an empty array.

#### ISSUE-0230 · `X.EMPTY` · warning · pass B
- File: `sitemap.xml`
- Message: sitemap.xml has no <loc> entries.

### SpaNavigationExample

- Project: `examples/SpaNavigationExample/SpaNavigationExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 2.7s
- Pass B: built ok, 5 HTML pages in output, 2.9s

#### ISSUE-0231 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0232 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpaNavigationTutorialExample

- Project: `examples/SpaNavigationTutorialExample/SpaNavigationTutorialExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 3.1s
- Pass B: built ok, 6 HTML pages in output, 2.8s

#### ISSUE-0233 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0234 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpectreConsoleExample

- Project: `examples/SpectreConsoleExample/SpectreConsoleExample.csproj`
- Pass A: build failed (exit 1), 79 HTML pages in output, 5.6s
- Pass B: build failed (exit 1), 79 HTML pages in output, 20.2s

#### ISSUE-0235 · `L.BROKEN` · error · pass A · rolled up (237 occurrences)
- Affects 79 pages:
  - `cli/index.html`
  - `console/index.html`
  - `blog/spectre-console-0-47-0-command-usability-validation/index.html`
  - `blog/spectre-console-0-48-0-net8-localization-customization/index.html`
  - `blog/spectre-console-0-49-0-search-positioning-progress/index.html`
  - `blog/spectre-console-0-49-1-version-handling-refinements/index.html`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Selector: `header.sticky > div.container > div.flex > div.flex > a.flex`
- Message: Link `/` → missing target `/`.
- Excerpt:
  ```
  <a href="/" class="flex items-center space-x-2"><span class="text-2xl font-bold bg-gradient-to-r from-primary-600 to-tertiary-one-600 dark:from-primary-400 dark:to-tertiary-one-400 bg-clip-text text-transparent"> Spectre.Console </span></a>
  ```

#### ISSUE-0236 · `L.BROKEN` · error · pass A · rolled up (163 occurrences)
- Affects 79 pages:
  - `cli/index.html`
  - `console/index.html`
  - `blog/spectre-console-0-47-0-command-usability-validation/index.html`
  - `blog/spectre-console-0-48-0-net8-localization-customization/index.html`
  - `blog/spectre-console-0-49-0-search-positioning-progress/index.html`
  - `blog/spectre-console-0-49-1-version-handling-refinements/index.html`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Selector: `header.sticky > div.container > div.flex > nav.hidden > a.px-3`
- Message: Link `/blog` → missing target `/blog`.
- Excerpt:
  ```
  <a href="/blog" class="px-3 py-2 rounded-md text-sm font-medium text-base-600 dark:text-base-400 hover:text-base-900 dark:hover:text-base-100 hover:bg-base-100 dark:hover:bg-base-800 transition-colors"> Blog </a>
  ```

#### ISSUE-0237 · `L.BROKEN` · error · pass A
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `div.flex-1 > main.prose > ul > li > a`
- Message: Link `getting-started-building-rich-console-app.md` → missing target `/console/tutorials/interactive-prompt-and-dashboard-tutorial/getting-started-building-rich-console-app.md`.
- Excerpt:
  ```
  <a href="getting-started-building-rich-console-app.md">Getting Started tutorial</a>
  ```

#### ISSUE-0238 · `L.BROKEN` · error · pass A
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/organizing-layout-with-panels-and-grids.md` → missing target `/console/tutorials/how-to/organizing-layout-with-panels-and-grids.md`.
- Excerpt:
  ```
  <a href="../how-to/organizing-layout-with-panels-and-grids.md">Organizing Layout with Panels and Grids</a>
  ```

#### ISSUE-0239 · `L.BROKEN` · error · pass A
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/prompting-for-user-input.md` → missing target `/console/tutorials/how-to/prompting-for-user-input.md`.
- Excerpt:
  ```
  <a href="../how-to/prompting-for-user-input.md">Prompting for User Input</a>
  ```

#### ISSUE-0240 · `L.BROKEN` · error · pass A
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/showing-progress-bars-and-spinners.md` → missing target `/console/tutorials/how-to/showing-progress-bars-and-spinners.md`.
- Excerpt:
  ```
  <a href="../how-to/showing-progress-bars-and-spinners.md">Showing Progress Bars and Spinners</a>
  ```

#### ISSUE-0241 · `L.BROKEN` · error · pass A
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/styling-text-with-markup-and-color.md` → missing target `/console/tutorials/how-to/styling-text-with-markup-and-color.md`.
- Excerpt:
  ```
  <a href="../how-to/styling-text-with-markup-and-color.md">Styling Text with Markup and Color</a>
  ```

#### ISSUE-0242 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /console/how-to/advanced-console-control/ links to /blog (Page not found)
      /console/how-to/advanced-console-control/ links to / (Page not found)
      /console/how-to/advanced-console-control/ links to /blog (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to / (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to / (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to /blog (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to / (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to /blog (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to getting-started-building-rich-console-app.md (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/showing-progress-bars-and-spinners.md (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/organizing-layout-with-panels-and-grids.md (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/prompting-for-user-input.md (Page not found)
      /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/styling-text-with-markup-and-color.md (Page not found)
      /console/how-to/showing-progress-bars-and-spinners/ links to / (Page not found)
      /console/how-to/showing-progress-bars-and-spinners/ links to / (Page not found)
      /console/how-to/showing-progress-bars-and-spinners/ links to /blog (Page not found)
      /console/how-to/showing-progress-bars-and-spinners/ links to / (Page not found)
      /console/how-to/showing-progress-bars-and-spinners/ links to /blog (Page not found)
  
  
  ```

#### ISSUE-0243 · `R.BROKEN_LINK` · warning · pass A · rolled up (237 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/blog/spectre-console-0-50-0-aot-testing-cli-improvements/`
  - `/cli/`
  - `/cli/how--to/dependency-injection-in-cli-commands/`
  - `/console/widgets/rule/`
  - `/console/prompts/text-prompt/`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to / (Page not found)
  ```

#### ISSUE-0244 · `R.BROKEN_LINK` · warning · pass A · rolled up (163 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/blog/spectre-console-0-50-0-aot-testing-cli-improvements/`
  - `/cli/`
  - `/cli/how--to/dependency-injection-in-cli-commands/`
  - `/console/widgets/rule/`
  - `/console/prompts/text-prompt/`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Message: Engine reports broken link to `/blog` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to /blog (Page not found)
  ```

#### ISSUE-0245 · `R.BROKEN_LINK` · warning · pass A
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/organizing-layout-with-panels-and-grids.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/organizing-layout-with-panels-and-grids.md (Page not found)
  ```

#### ISSUE-0246 · `R.BROKEN_LINK` · warning · pass A
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/prompting-for-user-input.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/prompting-for-user-input.md (Page not found)
  ```

#### ISSUE-0247 · `R.BROKEN_LINK` · warning · pass A
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/showing-progress-bars-and-spinners.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/showing-progress-bars-and-spinners.md (Page not found)
  ```

#### ISSUE-0248 · `R.BROKEN_LINK` · warning · pass A
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/styling-text-with-markup-and-color.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/styling-text-with-markup-and-color.md (Page not found)
  ```

#### ISSUE-0249 · `R.BROKEN_LINK` · warning · pass A
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `getting-started-building-rich-console-app.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to getting-started-building-rich-console-app.md (Page not found)
  ```

#### ISSUE-0250 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0251 · `L.BROKEN` · error · pass B · rolled up (237 occurrences)
- Affects 79 pages:
  - `cli/index.html`
  - `console/index.html`
  - `blog/spectre-console-0-47-0-command-usability-validation/index.html`
  - `blog/spectre-console-0-48-0-net8-localization-customization/index.html`
  - `blog/spectre-console-0-49-0-search-positioning-progress/index.html`
  - `blog/spectre-console-0-49-1-version-handling-refinements/index.html`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Selector: `header.sticky > div.container > div.flex > div.flex > a.flex`
- Message: Link `/preview/` → missing target `/`.
- Excerpt:
  ```
  <a href="/preview/" class="flex items-center space-x-2"><span class="text-2xl font-bold bg-gradient-to-r from-primary-600 to-tertiary-one-600 dark:from-primary-400 dark:to-tertiary-one-400 bg-clip-text text-transparent"> Spectre.Console </s...
  ```

#### ISSUE-0252 · `L.BROKEN` · error · pass B · rolled up (163 occurrences)
- Affects 79 pages:
  - `cli/index.html`
  - `console/index.html`
  - `blog/spectre-console-0-47-0-command-usability-validation/index.html`
  - `blog/spectre-console-0-48-0-net8-localization-customization/index.html`
  - `blog/spectre-console-0-49-0-search-positioning-progress/index.html`
  - `blog/spectre-console-0-49-1-version-handling-refinements/index.html`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Selector: `header.sticky > div.container > div.flex > nav.hidden > a.px-3`
- Message: Link `/preview/blog` → missing target `/blog`.
- Excerpt:
  ```
  <a href="/preview/blog" class="px-3 py-2 rounded-md text-sm font-medium text-base-600 dark:text-base-400 hover:text-base-900 dark:hover:text-base-100 hover:bg-base-100 dark:hover:bg-base-800 transition-colors"> Blog </a>
  ```

#### ISSUE-0253 · `L.BROKEN` · error · pass B
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `div.flex-1 > main.prose > ul > li > a`
- Message: Link `getting-started-building-rich-console-app.md` → missing target `/console/tutorials/interactive-prompt-and-dashboard-tutorial/getting-started-building-rich-console-app.md`.
- Excerpt:
  ```
  <a href="getting-started-building-rich-console-app.md">Getting Started tutorial</a>
  ```

#### ISSUE-0254 · `L.BROKEN` · error · pass B
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/organizing-layout-with-panels-and-grids.md` → missing target `/console/tutorials/how-to/organizing-layout-with-panels-and-grids.md`.
- Excerpt:
  ```
  <a href="../how-to/organizing-layout-with-panels-and-grids.md">Organizing Layout with Panels and Grids</a>
  ```

#### ISSUE-0255 · `L.BROKEN` · error · pass B
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/prompting-for-user-input.md` → missing target `/console/tutorials/how-to/prompting-for-user-input.md`.
- Excerpt:
  ```
  <a href="../how-to/prompting-for-user-input.md">Prompting for User Input</a>
  ```

#### ISSUE-0256 · `L.BROKEN` · error · pass B
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/showing-progress-bars-and-spinners.md` → missing target `/console/tutorials/how-to/showing-progress-bars-and-spinners.md`.
- Excerpt:
  ```
  <a href="../how-to/showing-progress-bars-and-spinners.md">Showing Progress Bars and Spinners</a>
  ```

#### ISSUE-0257 · `L.BROKEN` · error · pass B
- File: `console/tutorials/interactive-prompt-and-dashboard-tutorial/index.html`
- Selector: `main.prose > ul > li > strong > a`
- Message: Link `../how-to/styling-text-with-markup-and-color.md` → missing target `/console/tutorials/how-to/styling-text-with-markup-and-color.md`.
- Excerpt:
  ```
  <a href="../how-to/styling-text-with-markup-and-color.md">Styling Text with Markup and Color</a>
  ```

#### ISSUE-0258 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /cli/reference/api-reference/ links to /preview/blog (Page not found)
      /cli/reference/api-reference/ links to /preview/ (Page not found)
      /cli/reference/api-reference/ links to /preview/blog (Page not found)
      /cli/how--to/testing-command-line-applications/ links to /preview/ (Page not found)
      /cli/how--to/testing-command-line-applications/ links to /preview/ (Page not found)
      /cli/how--to/testing-command-line-applications/ links to /preview/blog (Page not found)
      /cli/how--to/testing-command-line-applications/ links to /preview/ (Page not found)
      /cli/how--to/testing-command-line-applications/ links to /preview/blog (Page not found)
      /cli/how--to/customizing-help-text-and-usage/ links to /preview/ (Page not found)
      /cli/how--to/customizing-help-text-and-usage/ links to /preview/ (Page not found)
      /cli/how--to/customizing-help-text-and-usage/ links to /preview/blog (Page not found)
      /cli/how--to/customizing-help-text-and-usage/ links to /preview/ (Page not found)
      /cli/how--to/customizing-help-text-and-usage/ links to /preview/blog (Page not found)
      /console/explanation/understanding-rendering-model/ links to /preview/ (Page not found)
      /console/explanation/understanding-rendering-model/ links to /preview/ (Page not found)
      /console/explanation/understanding-rendering-model/ links to /preview/blog (Page not found)
      /console/explanation/understanding-rendering-model/ links to /preview/ (Page not found)
      /console/explanation/understanding-rendering-model/ links to /preview/blog (Page not found)
  
  
  ```

#### ISSUE-0259 · `R.BROKEN_LINK` · warning · pass B · rolled up (237 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/blog/spectre-console-0-50-0-aot-testing-cli-improvements/`
  - `/cli/tutorials/quick-start-your-first-cli-app/`
  - `/cli/explanation/spectre-console-cli-vs-spectre-cli-migration-guide/`
  - `/cli/`
  - `/console/widgets/tree/`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to /preview/ (Page not found)
  ```

#### ISSUE-0260 · `R.BROKEN_LINK` · warning · pass B · rolled up (163 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/blog/spectre-console-0-50-0-aot-testing-cli-improvements/`
  - `/cli/tutorials/quick-start-your-first-cli-app/`
  - `/cli/explanation/spectre-console-cli-vs-spectre-cli-migration-guide/`
  - `/cli/`
  - `/console/widgets/tree/`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/blog` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to /preview/blog (Page not found)
  ```

#### ISSUE-0261 · `R.BROKEN_LINK` · warning · pass B
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/organizing-layout-with-panels-and-grids.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/organizing-layout-with-panels-and-grids.md (Page not found)
  ```

#### ISSUE-0262 · `R.BROKEN_LINK` · warning · pass B
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/prompting-for-user-input.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/prompting-for-user-input.md (Page not found)
  ```

#### ISSUE-0263 · `R.BROKEN_LINK` · warning · pass B
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/showing-progress-bars-and-spinners.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/showing-progress-bars-and-spinners.md (Page not found)
  ```

#### ISSUE-0264 · `R.BROKEN_LINK` · warning · pass B
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `../how-to/styling-text-with-markup-and-color.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to ../how-to/styling-text-with-markup-and-color.md (Page not found)
  ```

#### ISSUE-0265 · `R.BROKEN_LINK` · warning · pass B
- File: `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
- Message: Engine reports broken link to `getting-started-building-rich-console-app.md` (Page not found).
- Excerpt:
  ```
  /console/tutorials/interactive-prompt-and-dashboard-tutorial/ links to getting-started-building-rich-console-app.md (Page not found)
  ```

#### ISSUE-0266 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### TempoDocsExample

- Project: `examples/TempoDocsExample/TempoDocsExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 3.5s
- Pass B: built ok, 5 HTML pages in output, 3.3s
- No issues.

### UserInterfaceExample

- Project: `examples/UserInterfaceExample/UserInterfaceExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.8s
- Pass B: built ok, 6 HTML pages in output, 2.7s

#### ISSUE-0267 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0268 · `B.MISSING_BODY_ATTR` · warning · pass B
- File: `index.html`
- Selector: `body`
- Message: Page body missing `data-base-url` attribute (expected `/preview/`).

#### ISSUE-0269 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### YogaStudioExample

- Project: `examples/YogaStudioExample/YogaStudioExample.csproj`
- Pass A: build failed (exit 1), 18 HTML pages in output, 2.8s
- Pass B: build failed (exit 1), 18 HTML pages in output, 2.8s

#### ISSUE-0270 · `L.BROKEN` · error · pass A
- File: `blog/index.html`
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/gen-z/blog/` → missing target `/gen-z/blog/`.
- Excerpt:
  ```
  <a href="/gen-z/blog/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0271 · `L.BROKEN` · error · pass A · rolled up (22 occurrences)
- Affects 8 pages:
  - `index.html`
  - `gen-z/about/index.html`
  - `gen-z/contact/index.html`
  - `gen-z/faq/index.html`
  - `gen-z/pricing/index.html`
  - `gen-z/blog/breathing-techniques/index.html`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/gen-z/` → missing target `/gen-z/`.
- Excerpt:
  ```
  <a href="/gen-z/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0272 · `L.BROKEN` · error · pass A · rolled up (4 occurrences)
- Affects 3 pages:
  - `index.html`
  - `schedule/index.html`
  - `blog/morning-yoga-routine/index.html`
- Total raw occurrences: 4 (some pages emit multiple matches)
- Selector: `main.flex-1 > section.yoga-section > div.yoga-container > div.grid > a.yoga-card`
- Message: Link `/schedule/mon-vinyasa-morning` → missing target `/schedule/mon-vinyasa-morning`.
- Excerpt:
  ```
  <a href="/schedule/mon-vinyasa-morning" class="yoga-card group"><div class="h-2 bg-gradient-to-r from-primary-500 to-primary-600"></div> <div class="yoga-card-body"><div class="flex items-center justify-between mb-3"><span class="yoga-sched...
  ```

#### ISSUE-0273 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
- Affects 2 pages:
  - `index.html`
  - `schedule/index.html`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Selector: `main.flex-1 > section.yoga-section > div.yoga-container > div.grid > a.yoga-card`
- Message: Link `/schedule/tue-ashtanga` → missing target `/schedule/tue-ashtanga`.
- Excerpt:
  ```
  <a href="/schedule/tue-ashtanga" class="yoga-card group"><div class="h-2 bg-gradient-to-r from-primary-500 to-primary-600"></div> <div class="yoga-card-body"><div class="flex items-center justify-between mb-3"><span class="yoga-schedule-tim...
  ```

#### ISSUE-0274 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
- Affects 2 pages:
  - `index.html`
  - `schedule/index.html`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Selector: `main.flex-1 > section.yoga-section > div.yoga-container > div.grid > a.yoga-card`
- Message: Link `/schedule/wed-vinyasa-morning` → missing target `/schedule/wed-vinyasa-morning`.
- Excerpt:
  ```
  <a href="/schedule/wed-vinyasa-morning" class="yoga-card group"><div class="h-2 bg-gradient-to-r from-primary-500 to-primary-600"></div> <div class="yoga-card-body"><div class="flex items-center justify-between mb-3"><span class="yoga-sched...
  ```

#### ISSUE-0275 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/alex-rivera` → missing target `/instructors/alex-rivera`.
- Excerpt:
  ```
  <a href="/instructors/alex-rivera" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font...
  ```

#### ISSUE-0276 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/james-okafor` → missing target `/instructors/james-okafor`.
- Excerpt:
  ```
  <a href="/instructors/james-okafor" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl fon...
  ```

#### ISSUE-0277 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/maya-chen` → missing target `/instructors/maya-chen`.
- Excerpt:
  ```
  <a href="/instructors/maya-chen" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font-b...
  ```

#### ISSUE-0278 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/priya-sharma` → missing target `/instructors/priya-sharma`.
- Excerpt:
  ```
  <a href="/instructors/priya-sharma" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl fon...
  ```

#### ISSUE-0279 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/sam-tanaka` → missing target `/instructors/sam-tanaka`.
- Excerpt:
  ```
  <a href="/instructors/sam-tanaka" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font-...
  ```

#### ISSUE-0280 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /schedule/ links to /schedule/tue-ashtanga (Page not found)
      /schedule/ links to /schedule/tue-hot-yoga (Page not found)
      /schedule/ links to /schedule/wed-vinyasa-morning (Page not found)
      /schedule/ links to /schedule/wed-meditation (Page not found)
      /schedule/ links to /schedule/wed-restorative (Page not found)
      /schedule/ links to /js/search.js (Page not found)
      /schedule/ links to /js/faq.js (Page not found)
      /instructors/ links to /gen-z/instructors/ (Page not found)
      /instructors/ links to /instructors/maya-chen (Page not found)
      /instructors/ links to /instructors/james-okafor (Page not found)
      /instructors/ links to /instructors/priya-sharma (Page not found)
      /instructors/ links to /instructors/alex-rivera (Page not found)
      /instructors/ links to /instructors/sam-tanaka (Page not found)
      /instructors/ links to /js/search.js (Page not found)
      /instructors/ links to /js/faq.js (Page not found)
      /blog/breathing-techniques/ links to /schedule/sun-breathwork (Page not found)
      /blog/breathing-techniques/ links to /js/search.js (Page not found)
      /blog/breathing-techniques/ links to /js/faq.js (Page not found)
  
  
  ```

#### ISSUE-0281 · `R.PAGE_FAILED` · error · pass A
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\root\about\index.html' because it is being used by another process.

#### ISSUE-0282 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 2 pages:
  - `/`
  - `/schedule/`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Message: Engine reports broken link to `/schedule/tue-ashtanga` (Page not found).
- Excerpt:
  ```
  / links to /schedule/tue-ashtanga (Page not found)
  ```

#### ISSUE-0283 · `R.BROKEN_LINK` · warning · pass A · rolled up (4 occurrences)
- Affects 3 pages:
  - `/blog/morning-yoga-routine/`
  - `/`
  - `/schedule/`
- Total raw occurrences: 4 (some pages emit multiple matches)
- Message: Engine reports broken link to `/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /blog/morning-yoga-routine/ links to /schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0284 · `R.BROKEN_LINK` · warning · pass A
- File: `/gen-z/blog/breathing-techniques/`
- Message: Engine reports broken link to `/gen-z/schedule/sun-breathwork` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/schedule/sun-breathwork (Page not found)
  ```

#### ISSUE-0285 · `R.BROKEN_LINK` · warning · pass A · rolled up (22 occurrences)
- Affects 8 pages:
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/about/`
  - `/`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/ (Page not found)
  ```

#### ISSUE-0286 · `R.BROKEN_LINK` · warning · pass A · rolled up (24 occurrences)
- Affects 7 pages:
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/about/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 24 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/blog` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/blog (Page not found)
  ```

#### ISSUE-0287 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/about/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/instructors` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/instructors (Page not found)
  ```

#### ISSUE-0288 · `R.BROKEN_LINK` · warning · pass A
- File: `/gen-z/blog/morning-yoga-routine/`
- Message: Engine reports broken link to `/gen-z/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0289 · `R.BROKEN_LINK` · warning · pass A · rolled up (22 occurrences)
- Affects 7 pages:
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/about/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/schedule` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/schedule (Page not found)
  ```

#### ISSUE-0290 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 18 pages:
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/blog/morning-yoga-routine/`
  - `/contact/`
  - `/gen-z/blog/yoga-for-beginners/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/js/faq.js` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /js/faq.js (Page not found)
  ```

#### ISSUE-0291 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 18 pages:
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/blog/morning-yoga-routine/`
  - `/contact/`
  - `/gen-z/blog/yoga-for-beginners/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/js/search.js` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /js/search.js (Page not found)
  ```

#### ISSUE-0292 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0293 · `L.BROKEN` · error · pass B
- File: `blog/index.html`
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/preview/gen-z/blog/` → missing target `/gen-z/blog/`.
- Excerpt:
  ```
  <a href="/preview/gen-z/blog/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0294 · `L.BROKEN` · error · pass B · rolled up (22 occurrences)
- Affects 8 pages:
  - `index.html`
  - `gen-z/about/index.html`
  - `gen-z/contact/index.html`
  - `gen-z/faq/index.html`
  - `gen-z/pricing/index.html`
  - `gen-z/blog/breathing-techniques/index.html`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/preview/gen-z/` → missing target `/gen-z/`.
- Excerpt:
  ```
  <a href="/preview/gen-z/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0295 · `L.BROKEN` · error · pass B · rolled up (4 occurrences)
- Affects 3 pages:
  - `index.html`
  - `schedule/index.html`
  - `blog/morning-yoga-routine/index.html`
- Total raw occurrences: 4 (some pages emit multiple matches)
- Selector: `main.flex-1 > section.yoga-section > div.yoga-container > div.grid > a.yoga-card`
- Message: Link `/preview/schedule/mon-vinyasa-morning` → missing target `/schedule/mon-vinyasa-morning`.
- Excerpt:
  ```
  <a href="/preview/schedule/mon-vinyasa-morning" class="yoga-card group"><div class="h-2 bg-gradient-to-r from-primary-500 to-primary-600"></div> <div class="yoga-card-body"><div class="flex items-center justify-between mb-3"><span class="yo...
  ```

#### ISSUE-0296 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
- Affects 2 pages:
  - `index.html`
  - `schedule/index.html`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Selector: `main.flex-1 > section.yoga-section > div.yoga-container > div.grid > a.yoga-card`
- Message: Link `/preview/schedule/tue-ashtanga` → missing target `/schedule/tue-ashtanga`.
- Excerpt:
  ```
  <a href="/preview/schedule/tue-ashtanga" class="yoga-card group"><div class="h-2 bg-gradient-to-r from-primary-500 to-primary-600"></div> <div class="yoga-card-body"><div class="flex items-center justify-between mb-3"><span class="yoga-sche...
  ```

#### ISSUE-0297 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
- Affects 2 pages:
  - `index.html`
  - `schedule/index.html`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Selector: `main.flex-1 > section.yoga-section > div.yoga-container > div.grid > a.yoga-card`
- Message: Link `/preview/schedule/wed-vinyasa-morning` → missing target `/schedule/wed-vinyasa-morning`.
- Excerpt:
  ```
  <a href="/preview/schedule/wed-vinyasa-morning" class="yoga-card group"><div class="h-2 bg-gradient-to-r from-primary-500 to-primary-600"></div> <div class="yoga-card-body"><div class="flex items-center justify-between mb-3"><span class="yo...
  ```

#### ISSUE-0298 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/alex-rivera` → missing target `/instructors/alex-rivera`.
- Excerpt:
  ```
  <a href="/preview/instructors/alex-rivera" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-...
  ```

#### ISSUE-0299 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/james-okafor` → missing target `/instructors/james-okafor`.
- Excerpt:
  ```
  <a href="/preview/instructors/james-okafor" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text...
  ```

#### ISSUE-0300 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/maya-chen` → missing target `/instructors/maya-chen`.
- Excerpt:
  ```
  <a href="/preview/instructors/maya-chen" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2x...
  ```

#### ISSUE-0301 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/priya-sharma` → missing target `/instructors/priya-sharma`.
- Excerpt:
  ```
  <a href="/preview/instructors/priya-sharma" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text...
  ```

#### ISSUE-0302 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/sam-tanaka` → missing target `/instructors/sam-tanaka`.
- Excerpt:
  ```
  <a href="/preview/instructors/sam-tanaka" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2...
  ```

#### ISSUE-0303 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      / links to /preview/js/search.js (Page not found)
      / links to /preview/js/faq.js (Page not found)
      /pricing/ links to /preview/js/search.js (Page not found)
      /pricing/ links to /preview/js/faq.js (Page not found)
      /blog/ links to /preview/gen-z/blog/ (Page not found)
      /blog/ links to /preview/js/search.js (Page not found)
      /blog/ links to /preview/js/faq.js (Page not found)
      /instructors/ links to /preview/gen-z/instructors/ (Page not found)
      /instructors/ links to /preview/instructors/maya-chen (Page not found)
      /instructors/ links to /preview/instructors/james-okafor (Page not found)
      /instructors/ links to /preview/instructors/priya-sharma (Page not found)
      /instructors/ links to /preview/instructors/alex-rivera (Page not found)
      /instructors/ links to /preview/instructors/sam-tanaka (Page not found)
      /instructors/ links to /preview/js/search.js (Page not found)
      /instructors/ links to /preview/js/faq.js (Page not found)
      /blog/morning-yoga-routine/ links to /preview/schedule/mon-vinyasa-morning (Page not found)
      /blog/morning-yoga-routine/ links to /preview/js/search.js (Page not found)
      /blog/morning-yoga-routine/ links to /preview/js/faq.js (Page not found)
  
  
  ```

#### ISSUE-0304 · `R.PAGE_FAILED` · error · pass B
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\prefixed\about\index.html' because it is being used by another process. Source: B:\Penn\examples\YogaStudioExample\Content\pages\about.md

#### ISSUE-0305 · `R.BROKEN_LINK` · warning · pass B · rolled up (22 occurrences)
- Affects 8 pages:
  - `/gen-z/faq/`
  - `/gen-z/pricing/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/contact/`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/gen-z/` (Page not found).
- Excerpt:
  ```
  /gen-z/faq/ links to /preview/gen-z/ (Page not found)
  ```

#### ISSUE-0306 · `R.BROKEN_LINK` · warning · pass B · rolled up (24 occurrences)
- Affects 7 pages:
  - `/gen-z/faq/`
  - `/gen-z/pricing/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 24 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/gen-z/blog` (Page not found).
- Excerpt:
  ```
  /gen-z/faq/ links to /preview/gen-z/blog (Page not found)
  ```

#### ISSUE-0307 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 7 pages:
  - `/gen-z/faq/`
  - `/gen-z/pricing/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/gen-z/instructors` (Page not found).
- Excerpt:
  ```
  /gen-z/faq/ links to /preview/gen-z/instructors (Page not found)
  ```

#### ISSUE-0308 · `R.BROKEN_LINK` · warning · pass B · rolled up (22 occurrences)
- Affects 7 pages:
  - `/gen-z/faq/`
  - `/gen-z/pricing/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/gen-z/schedule` (Page not found).
- Excerpt:
  ```
  /gen-z/faq/ links to /preview/gen-z/schedule (Page not found)
  ```

#### ISSUE-0309 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 18 pages:
  - `/pricing/`
  - `/faq/`
  - `/gen-z/faq/`
  - `/schedule/`
  - `/gen-z/pricing/`
  - `/gen-z/blog/breathing-techniques/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/js/faq.js` (Page not found).
- Excerpt:
  ```
  /pricing/ links to /preview/js/faq.js (Page not found)
  ```

#### ISSUE-0310 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 18 pages:
  - `/pricing/`
  - `/faq/`
  - `/gen-z/faq/`
  - `/schedule/`
  - `/gen-z/pricing/`
  - `/gen-z/blog/breathing-techniques/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/js/search.js` (Page not found).
- Excerpt:
  ```
  /pricing/ links to /preview/js/search.js (Page not found)
  ```

#### ISSUE-0311 · `R.BROKEN_LINK` · warning · pass B
- File: `/schedule/`
- Message: Engine reports broken link to `/preview/gen-z/schedule/` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/gen-z/schedule/ (Page not found)
  ```

#### ISSUE-0312 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/mon-power-noon` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/mon-power-noon (Page not found)
  ```

#### ISSUE-0313 · `R.BROKEN_LINK` · warning · pass B · rolled up (4 occurrences)
- Affects 3 pages:
  - `/schedule/`
  - `/`
  - `/blog/morning-yoga-routine/`
- Total raw occurrences: 4 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0314 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/mon-yin-evening` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/mon-yin-evening (Page not found)
  ```

#### ISSUE-0315 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

_Additional issues not shown (truncated to 10 samples per code):_

- `L.BROKEN` (pass A): 21 more similar issues
- `R.BROKEN_LINK` (pass A): 23 more similar issues
- `L.BROKEN` (pass B): 21 more similar issues
- `R.BROKEN_LINK` (pass B): 23 more similar issues


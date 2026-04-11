# Pennington Examples Validation Report

Generated: 2026-04-11T19:09:33Z
Examples discovered: 20
Examples built successfully: 8 (pass A) / 8 (pass B)
Total issues: 1453 errors, 1399 warnings, 28 info
Shown in report: 256 samples (capped at 10 per example/pass/code; full counts preserved in cross-example patterns below).

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
| BeaconDocsExample | fail | fail | 4 | 4 | 0 |
| BlogExample | fail | fail | 88 | 100 | 2 |
| ForgePortalExample | fail | fail | 22 | 18 | 2 |
| LocalizationExample | ok | ok | 0 | 0 | 0 |
| LocalizationTutorialExample | ok | ok | 0 | 0 | 0 |
| MaraBlogExample | fail | fail | 26 | 30 | 2 |
| MinimalExample | fail | fail | 4 | 8 | 2 |
| MultipleContentSourceExample | fail | fail | 14 | 0 | 2 |
| NorthwindHandbookExample | fail | fail | 139 | 0 | 2 |
| PrismDocsExample | ok | ok | 0 | 0 | 0 |
| RecipeExample | fail | fail | 46 | 42 | 2 |
| RoslynIntegrationExample | fail | fail | 2 | 8 | 2 |
| SearchExample | ok | ok | 4 | 2 | 0 |
| SpaNavigationExample | ok | ok | 0 | 0 | 2 |
| SpaNavigationTutorialExample | ok | ok | 0 | 0 | 2 |
| SpectreConsoleExample | fail | fail | 802 | 800 | 2 |
| TempoDocsExample | ok | ok | 0 | 0 | 0 |
| UserInterfaceExample | ok | ok | 0 | 1 | 2 |
| YogaStudioExample | fail | fail | 272 | 352 | 2 |

## Cross-example patterns

Counts below reflect **raw** issue totals (not the truncated samples shown in per-example sections). Codes that appear in multiple examples are likely engine bugs; codes unique to one example are likely example bugs.

- `R.BUILD_FAILED` (error) — 24 occurrences across 12 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+6 more)
- `L.BROKEN` (error) — 1276 raw occurrences rolled up into 152 distinct groups across 10 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+4 more)
- `R.PAGE_FAILED` (error) — 23 occurrences across 5 examples: BlogExample, MultipleContentSourceExample, NorthwindHandbookExample, RecipeExample, YogaStudioExample
- `T.DUP` (error) — 132 raw occurrences rolled up into 6 distinct groups across 1 example: NorthwindHandbookExample
- `M.NO_ENTRIES` (error) — 2 occurrences across 1 example: SearchExample
- `S.EMPTY` (error) — 2 occurrences across 1 example: SearchExample
- `S.MISSING_FIELD` (error) — 2 occurrences across 1 example: MinimalExample
- `X.BROKEN` (error) — 2 occurrences across 1 example: BeaconDocsExample
- `R.BROKEN_LINK` (warning) — 1386 raw occurrences rolled up into 162 distinct groups across 10 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+4 more)
- `X.EMPTY` (warning) — 2 occurrences across 1 example: SearchExample
- `B.MISSING_BODY_ATTR` (warning) — 1 occurrence across 1 example: UserInterfaceExample
- `M.MISSING` (info) — 28 occurrences across 14 examples: AlexBlogExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, MultipleContentSourceExample, … (+8 more)

## Issues by example

### AlexBlogExample

- Project: `examples/AlexBlogExample/AlexBlogExample.csproj`
- Pass A: build failed (exit 1), 3 HTML pages in output, 3.1s
- Pass B: build failed (exit 1), 3 HTML pages in output, 3.1s

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
      /blog/building-a-cli-part-2/ links to /rss.xml (Page not found)
      /blog/building-a-cli-part-2/ links to / (Page not found)
      /blog/building-a-cli-part-2/ links to /blog/2026/03/building-a-cli-part-1 (Page not found)
      /blog/building-a-cli-part-2/ links to /tags/dotnet (Page not found)
      /blog/building-a-cli-part-2/ links to /tags/cli (Page not found)
      /blog/building-a-cli-part-2/ links to / (Page not found)
      /blog/building-a-cli-part-1/ links to /rss.xml (Page not found)
      /blog/building-a-cli-part-1/ links to / (Page not found)
      /blog/building-a-cli-part-1/ links to /tags/dotnet (Page not found)
      /blog/building-a-cli-part-1/ links to /tags/cli (Page not found)
      /blog/building-a-cli-part-1/ links to / (Page not found)
      /blog/why-i-switched-to-linux/ links to /rss.xml (Page not found)
      /blog/why-i-switched-to-linux/ links to / (Page not found)
      /blog/why-i-switched-to-linux/ links to /tags/linux (Page not found)
      /blog/why-i-switched-to-linux/ links to /tags/workflow (Page not found)
      /blog/why-i-switched-to-linux/ links to /tags/dotnet (Page not found)
      /blog/why-i-switched-to-linux/ links to / (Page not found)
  
  
  ```

#### ISSUE-0008 · `R.BROKEN_LINK` · warning · pass A · rolled up (6 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-2/`
  - `/blog/building-a-cli-part-1/`
  - `/blog/why-i-switched-to-linux/`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to / (Page not found)
  ```

#### ISSUE-0009 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/building-a-cli-part-2/`
- Message: Engine reports broken link to `/blog/2026/03/building-a-cli-part-1` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to /blog/2026/03/building-a-cli-part-1 (Page not found)
  ```

#### ISSUE-0010 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-2/`
  - `/blog/building-a-cli-part-1/`
  - `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0011 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/building-a-cli-part-2/`
  - `/blog/building-a-cli-part-1/`
- Message: Engine reports broken link to `/tags/cli` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to /tags/cli (Page not found)
  ```

#### ISSUE-0012 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/building-a-cli-part-2/`
  - `/blog/building-a-cli-part-1/`
  - `/blog/why-i-switched-to-linux/`
- Message: Engine reports broken link to `/tags/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/building-a-cli-part-2/ links to /tags/dotnet (Page not found)
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
- Pass A: build failed (exit 1), 10 HTML pages in output, 3.3s
- Pass B: build failed (exit 1), 10 HTML pages in output, 3.4s

#### ISSUE-0031 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  info: Microsoft.Hosting.Lifetime[14]
        Now listening on: http://localhost:5000
  info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Production
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: B:\Penn\examples\BeaconDocsExample
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn\examples\BeaconDocsExample\Content with pattern *.*
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 22 pages in 0.8s
    22 pages generated
  
  WARNINGS
    1 broken links found:
      /getting-started/ links to /getting-started/beacon-arch.png (Page not found)
  
  
  ```

#### ISSUE-0032 · `X.BROKEN` · error · pass A
- File: `sitemap.xml`
- Message: sitemap <loc> `/https://beacon-docs.example.com` does not resolve to any output file.

#### ISSUE-0033 · `L.BROKEN` · warning · pass A
- File: `getting-started/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > img`
- Message: Image src `/getting-started/beacon-arch.png` → missing target `/getting-started/beacon-arch.png`.
- Excerpt:
  ```
  <img src="/getting-started/beacon-arch.png" alt="Beacon Architecture">
  ```

#### ISSUE-0034 · `R.BROKEN_LINK` · warning · pass A
- File: `/getting-started/`
- Message: Engine reports broken link to `/getting-started/beacon-arch.png` (Page not found).
- Excerpt:
  ```
  /getting-started/ links to /getting-started/beacon-arch.png (Page not found)
  ```

#### ISSUE-0035 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  info: Microsoft.Hosting.Lifetime[14]
        Now listening on: http://localhost:5000
  info: Microsoft.Hosting.Lifetime[0]
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
    1 broken links found:
      /getting-started/ links to /preview/getting-started/beacon-arch.png (Page not found)
  
  
  ```

#### ISSUE-0036 · `X.BROKEN` · error · pass B
- File: `sitemap.xml`
- Message: sitemap <loc> `/https://beacon-docs.example.com` does not resolve to any output file.

#### ISSUE-0037 · `L.BROKEN` · warning · pass B
- File: `getting-started/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > img`
- Message: Image src `/preview/getting-started/beacon-arch.png` → missing target `/getting-started/beacon-arch.png`.
- Excerpt:
  ```
  <img src="/preview/getting-started/beacon-arch.png" alt="Beacon Architecture">
  ```

#### ISSUE-0038 · `R.BROKEN_LINK` · warning · pass B
- File: `/getting-started/`
- Message: Engine reports broken link to `/preview/getting-started/beacon-arch.png` (Page not found).
- Excerpt:
  ```
  /getting-started/ links to /preview/getting-started/beacon-arch.png (Page not found)
  ```

### BlogExample

- Project: `examples/BlogExample/BlogExample.csproj`
- Pass A: build failed (exit 1), 8 HTML pages in output, 3.2s
- Pass B: build failed (exit 1), 8 HTML pages in output, 3.2s

#### ISSUE-0039 · `L.BROKEN` · error · pass A · rolled up (15 occurrences)
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

#### ISSUE-0040 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/chewing-magazine` → missing target `/tags/chewing-magazine`.
- Excerpt:
  ```
  <a href="/tags/chewing-magazine" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">chewing-magazine</a>
  ```

#### ISSUE-0041 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gum-culture` → missing target `/tags/gum-culture`.
- Excerpt:
  ```
  <a href="/tags/gum-culture" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-culture</a>
  ```

#### ISSUE-0042 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
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

#### ISSUE-0043 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/analysis` → missing target `/tags/analysis`.
- Excerpt:
  ```
  <a href="/tags/analysis" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">analysis</a>
  ```

#### ISSUE-0044 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gum-brands` → missing target `/tags/gum-brands`.
- Excerpt:
  ```
  <a href="/tags/gum-brands" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-brands</a>
  ```

#### ISSUE-0045 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/science` → missing target `/tags/science`.
- Excerpt:
  ```
  <a href="/tags/science" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">science</a>
  ```

#### ISSUE-0046 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/apparel` → missing target `/tags/apparel`.
- Excerpt:
  ```
  <a href="/tags/apparel" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">apparel</a>
  ```

#### ISSUE-0047 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/equipment` → missing target `/tags/equipment`.
- Excerpt:
  ```
  <a href="/tags/equipment" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">equipment</a>
  ```

#### ISSUE-0048 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gear` → missing target `/tags/gear`.
- Excerpt:
  ```
  <a href="/tags/gear" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gear</a>
  ```

#### ISSUE-0049 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/reviews (Page not found)
      /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/equipment (Page not found)
      /blog/2024/04/gum-chewing-apparel-guide/ links to / (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to /rss.xml (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to / (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to /tags/fitness (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to /tags/jaw-exercises (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to /tags/training (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to /tags/health (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to / (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /rss.xml (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to / (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /tags/reviews (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /tags/chewing-magazine (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /tags/gum-culture (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to / (Page not found)
      /about/ links to /rss.xml (Page not found)
      /about/ links to / (Page not found)
  
  
  ```

#### ISSUE-0050 · `R.PAGE_FAILED` · error · pass A
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\BlogExample\root\about\index.html' because it is being used by another process.

#### ISSUE-0051 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/bazooka-joe-interview/`
- Message: Engine reports broken link to `/tags/bazooka-joe` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/bazooka-joe-interview/ links to /tags/bazooka-joe (Page not found)
  ```

#### ISSUE-0052 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/bazooka-joe-interview/`
- Message: Engine reports broken link to `/tags/inspiration` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/bazooka-joe-interview/ links to /tags/inspiration (Page not found)
  ```

#### ISSUE-0053 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/bazooka-joe-interview/`
- Message: Engine reports broken link to `/tags/interview` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/bazooka-joe-interview/ links to /tags/interview (Page not found)
  ```

#### ISSUE-0054 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/bazooka-joe-interview/`
- Message: Engine reports broken link to `/tags/legend` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/bazooka-joe-interview/ links to /tags/legend (Page not found)
  ```

#### ISSUE-0055 · `R.BROKEN_LINK` · warning · pass A · rolled up (15 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to / (Page not found)
  ```

#### ISSUE-0056 · `R.BROKEN_LINK` · warning · pass A · rolled up (8 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - … (+2 more)
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0057 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/data-analytics` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/data-analytics (Page not found)
  ```

#### ISSUE-0058 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/optimization` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/optimization (Page not found)
  ```

#### ISSUE-0059 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/performance-tracking` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/performance-tracking (Page not found)
  ```

#### ISSUE-0060 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/python` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/python (Page not found)
  ```

#### ISSUE-0061 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0062 · `L.BROKEN` · error · pass B · rolled up (15 occurrences)
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

#### ISSUE-0063 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/chewing-magazine` → missing target `/tags/chewing-magazine`.
- Excerpt:
  ```
  <a href="/preview/tags/chewing-magazine" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">chewing-magazi...
  ```

#### ISSUE-0064 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gum-culture` → missing target `/tags/gum-culture`.
- Excerpt:
  ```
  <a href="/preview/tags/gum-culture" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-culture</a>
  ```

#### ISSUE-0065 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
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

#### ISSUE-0066 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/analysis` → missing target `/tags/analysis`.
- Excerpt:
  ```
  <a href="/preview/tags/analysis" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">analysis</a>
  ```

#### ISSUE-0067 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gum-brands` → missing target `/tags/gum-brands`.
- Excerpt:
  ```
  <a href="/preview/tags/gum-brands" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-brands</a>
  ```

#### ISSUE-0068 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/science` → missing target `/tags/science`.
- Excerpt:
  ```
  <a href="/preview/tags/science" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">science</a>
  ```

#### ISSUE-0069 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/apparel` → missing target `/tags/apparel`.
- Excerpt:
  ```
  <a href="/preview/tags/apparel" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">apparel</a>
  ```

#### ISSUE-0070 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/equipment` → missing target `/tags/equipment`.
- Excerpt:
  ```
  <a href="/preview/tags/equipment" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">equipment</a>
  ```

#### ISSUE-0071 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gear` → missing target `/tags/gear`.
- Excerpt:
  ```
  <a href="/preview/tags/gear" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gear</a>
  ```

#### ISSUE-0072 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/legend (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/inspiration (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/ (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/rss.xml (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/ (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/tags/analysis (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/tags/gum-brands (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/tags/science (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/tags/reviews (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /preview/ (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/rss.xml (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/ (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/tags/reviews (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/tags/chewing-magazine (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/tags/gum-culture (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /preview/ (Page not found)
      /about/ links to /preview/rss.xml (Page not found)
      /about/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0073 · `R.PAGE_FAILED` · error · pass B
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\BlogExample\prefixed\about\index.html' because it is being used by another process.

#### ISSUE-0074 · `R.BROKEN_LINK` · warning · pass B · rolled up (15 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/ (Page not found)
  ```

#### ISSUE-0075 · `R.BROKEN_LINK` · warning · pass B · rolled up (8 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - … (+2 more)
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0076 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/data-analytics` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/data-analytics (Page not found)
  ```

#### ISSUE-0077 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/optimization` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/optimization (Page not found)
  ```

#### ISSUE-0078 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/performance-tracking` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/performance-tracking (Page not found)
  ```

#### ISSUE-0079 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/python` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/python (Page not found)
  ```

#### ISSUE-0080 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/preview/tags/bubbles` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/bubbles (Page not found)
  ```

#### ISSUE-0081 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/preview/tags/exercises` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/exercises (Page not found)
  ```

#### ISSUE-0082 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/preview/tags/technique` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/technique (Page not found)
  ```

#### ISSUE-0083 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/mandibular-fitness-regime/`
- Message: Engine reports broken link to `/preview/tags/training` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /preview/tags/training (Page not found)
  ```

#### ISSUE-0084 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

_Additional issues not shown (truncated to 10 samples per code):_

- `L.BROKEN` (pass A): 15 more similar issues
- `R.BROKEN_LINK` (pass A): 16 more similar issues
- `L.BROKEN` (pass B): 15 more similar issues
- `R.BROKEN_LINK` (pass B): 16 more similar issues

### ForgePortalExample

- Project: `examples/ForgePortalExample/ForgePortalExample.csproj`
- Pass A: build failed (exit 1), 10 HTML pages in output, 2.8s
- Pass B: build failed (exit 1), 10 HTML pages in output, 2.6s

#### ISSUE-0085 · `L.BROKEN` · error · pass A · rolled up (10 occurrences)
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

#### ISSUE-0086 · `R.BUILD_FAILED` · error · pass A
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
      /docs/getting-started/ links to / (Page not found)
      /about/ links to / (Page not found)
      /releases/v2-0-0/ links to / (Page not found)
      /docs/pipeline-config/ links to / (Page not found)
      /blog/q1-retro/ links to / (Page not found)
      /releases/v2-0-1/ links to / (Page not found)
      /releases/v2-1-0/ links to / (Page not found)
  
  
  ```

#### ISSUE-0087 · `R.BROKEN_LINK` · warning · pass A · rolled up (9 occurrences)
- Affects 9 pages:
  - `/blog/welcome/`
  - `/docs/api-keys/`
  - `/docs/getting-started/`
  - `/about/`
  - `/releases/v2-0-0/`
  - `/docs/pipeline-config/`
  - … (+3 more)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/welcome/ links to / (Page not found)
  ```

#### ISSUE-0088 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0089 · `L.BROKEN` · error · pass B · rolled up (10 occurrences)
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

#### ISSUE-0090 · `R.BUILD_FAILED` · error · pass B
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
      /blog/q1-retro/ links to /preview/ (Page not found)
      /blog/welcome/ links to /preview/ (Page not found)
      /docs/api-keys/ links to /preview/ (Page not found)
      /releases/v2-1-0/ links to /preview/ (Page not found)
      /releases/v2-0-1/ links to /preview/ (Page not found)
      /about/ links to /preview/ (Page not found)
      /docs/pipeline-config/ links to /preview/ (Page not found)
      /docs/getting-started/ links to /preview/ (Page not found)
      /releases/v2-0-0/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0091 · `R.BROKEN_LINK` · warning · pass B · rolled up (9 occurrences)
- Affects 9 pages:
  - `/blog/q1-retro/`
  - `/blog/welcome/`
  - `/docs/api-keys/`
  - `/releases/v2-1-0/`
  - `/releases/v2-0-1/`
  - `/about/`
  - … (+3 more)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/q1-retro/ links to /preview/ (Page not found)
  ```

#### ISSUE-0092 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### LocalizationExample

- Project: `examples/LocalizationExample/LocalizationExample.csproj`
- Pass A: built ok, 26 HTML pages in output, 3.4s
- Pass B: built ok, 26 HTML pages in output, 3.6s
- No issues.

### LocalizationTutorialExample

- Project: `examples/LocalizationTutorialExample/LocalizationTutorialExample.csproj`
- Pass A: built ok, 7 HTML pages in output, 3.3s
- Pass B: built ok, 7 HTML pages in output, 3.1s
- No issues.

### MaraBlogExample

- Project: `examples/MaraBlogExample/MaraBlogExample.csproj`
- Pass A: build failed (exit 1), 3 HTML pages in output, 3.1s
- Pass B: build failed (exit 1), 3 HTML pages in output, 3.0s

#### ISSUE-0093 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/dotnet` → missing target `/topics/dotnet`.
- Excerpt:
  ```
  <a href="/topics/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0094 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/performance` → missing target `/topics/performance`.
- Excerpt:
  ```
  <a href="/topics/performance" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">performance</a>
  ```

#### ISSUE-0095 · `L.BROKEN` · error · pass A · rolled up (6 occurrences)
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

#### ISSUE-0096 · `L.BROKEN` · error · pass A
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/aspnet` → missing target `/topics/aspnet`.
- Excerpt:
  ```
  <a href="/topics/aspnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">aspnet</a>
  ```

#### ISSUE-0097 · `L.BROKEN` · error · pass A
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/configuration` → missing target `/topics/configuration`.
- Excerpt:
  ```
  <a href="/topics/configuration" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">configuration</a>
  ```

#### ISSUE-0098 · `R.BUILD_FAILED` · error · pass A
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

#### ISSUE-0099 · `R.BROKEN_LINK` · warning · pass A · rolled up (6 occurrences)
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

#### ISSUE-0100 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
  - `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0101 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/topics/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /topics/dotnet (Page not found)
  ```

#### ISSUE-0102 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/topics/performance` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /topics/performance (Page not found)
  ```

#### ISSUE-0103 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/topics/aspnet` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /topics/aspnet (Page not found)
  ```

#### ISSUE-0104 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/topics/configuration` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /topics/configuration (Page not found)
  ```

#### ISSUE-0105 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0106 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/dotnet` → missing target `/topics/dotnet`.
- Excerpt:
  ```
  <a href="/preview/topics/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0107 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/performance` → missing target `/topics/performance`.
- Excerpt:
  ```
  <a href="/preview/topics/performance" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">performance</a>
  ```

#### ISSUE-0108 · `L.BROKEN` · error · pass B · rolled up (6 occurrences)
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

#### ISSUE-0109 · `L.BROKEN` · error · pass B
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/aspnet` → missing target `/topics/aspnet`.
- Excerpt:
  ```
  <a href="/preview/topics/aspnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">aspnet</a>
  ```

#### ISSUE-0110 · `L.BROKEN` · error · pass B
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/configuration` → missing target `/topics/configuration`.
- Excerpt:
  ```
  <a href="/preview/topics/configuration" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">configuration</...
  ```

#### ISSUE-0111 · `R.BUILD_FAILED` · error · pass B
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

#### ISSUE-0112 · `R.BROKEN_LINK` · warning · pass B · rolled up (6 occurrences)
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

#### ISSUE-0113 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/config-pitfalls/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0114 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/preview/topics/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/topics/dotnet (Page not found)
  ```

#### ISSUE-0115 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/preview/topics/performance` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /preview/topics/performance (Page not found)
  ```

#### ISSUE-0116 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/topics/aspnet` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /preview/topics/aspnet (Page not found)
  ```

#### ISSUE-0117 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/topics/configuration` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /preview/topics/configuration (Page not found)
  ```

#### ISSUE-0118 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MinimalExample

- Project: `examples/MinimalExample/MinimalExample.csproj`
- Pass A: build failed (exit 1), 6 HTML pages in output, 2.7s
- Pass B: build failed (exit 1), 6 HTML pages in output, 2.5s

#### ISSUE-0119 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Now listening on: http://localhost:5000
  info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
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
    2 broken links found:
      /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  
  
  ```

#### ISSUE-0120 · `S.MISSING_FIELD` · error · pass A
- File: `search-index.json`
- Selector: `[1]`
- Message: search-index entry #1 missing/empty: title, body (title=, url=/sub-folder/page-1/).

#### ISSUE-0121 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0122 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0123 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0124 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0125 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0126 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Now listening on: http://localhost:5000
  info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
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
    2 broken links found:
      /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  
  
  ```

#### ISSUE-0127 · `S.MISSING_FIELD` · error · pass B
- File: `search-index.json`
- Selector: `[1]`
- Message: search-index entry #1 missing/empty: title, body (title=, url=/sub-folder/page-1/).

#### ISSUE-0128 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0129 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0130 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0131 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0132 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MultipleContentSourceExample

- Project: `examples/MultipleContentSourceExample/MultipleContentSourceExample.csproj`
- Pass A: build failed (exit 1), 9 HTML pages in output, 2.5s
- Pass B: build failed (exit 1), 9 HTML pages in output, 2.7s

#### ISSUE-0133 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
    /blog/office-plant-survival-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\office-plant-survival-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md
    /docs/indoor-herb-garden/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\indoor-herb-garden\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md
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
  
  
  ```

#### ISSUE-0134 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/best-pizza-toppings/`
- Message: Engine reported page generation failure for `/blog/best-pizza-toppings/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\best-pizza-toppings\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md

#### ISSUE-0135 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/mystery-of-missing-socks/`
- Message: Engine reported page generation failure for `/blog/mystery-of-missing-socks/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\mystery-of-missing-socks\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md

#### ISSUE-0136 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/office-plant-survival-guide/`
- Message: Engine reported page generation failure for `/blog/office-plant-survival-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\office-plant-survival-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md

#### ISSUE-0137 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/coffee-brewing-guide/`
- Message: Engine reported page generation failure for `/docs/coffee-brewing-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\coffee-brewing-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md

#### ISSUE-0138 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/home-organization-systems/`
- Message: Engine reported page generation failure for `/docs/home-organization-systems/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\home-organization-systems\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md

#### ISSUE-0139 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/indoor-herb-garden/`
- Message: Engine reported page generation failure for `/docs/indoor-herb-garden/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\indoor-herb-garden\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md

#### ISSUE-0140 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0141 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
    /blog/best-pizza-toppings/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\best-pizza-toppings\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md
    /docs/coffee-brewing-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\coffee-brewing-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md
    /blog/office-plant-survival-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\office-plant-survival-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md
    /docs/home-organization-systems/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\home-organization-systems\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md
    /blog/mystery-of-missing-socks/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\mystery-of-missing-socks\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md
    /docs/indoor-herb-garden/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\indoor-herb-garden\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md
  
  
  ```

#### ISSUE-0142 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/best-pizza-toppings/`
- Message: Engine reported page generation failure for `/blog/best-pizza-toppings/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\best-pizza-toppings\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md

#### ISSUE-0143 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/mystery-of-missing-socks/`
- Message: Engine reported page generation failure for `/blog/mystery-of-missing-socks/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\mystery-of-missing-socks\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md

#### ISSUE-0144 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/office-plant-survival-guide/`
- Message: Engine reported page generation failure for `/blog/office-plant-survival-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\office-plant-survival-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md

#### ISSUE-0145 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/coffee-brewing-guide/`
- Message: Engine reported page generation failure for `/docs/coffee-brewing-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\coffee-brewing-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md

#### ISSUE-0146 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/home-organization-systems/`
- Message: Engine reported page generation failure for `/docs/home-organization-systems/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\home-organization-systems\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md

#### ISSUE-0147 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/indoor-herb-garden/`
- Message: Engine reported page generation failure for `/docs/indoor-herb-garden/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\indoor-herb-garden\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md

#### ISSUE-0148 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### NorthwindHandbookExample

- Project: `examples/NorthwindHandbookExample/NorthwindHandbookExample.csproj`
- Pass A: build failed (exit 1), 11 HTML pages in output, 2.8s
- Pass B: build failed (exit 1), 11 HTML pages in output, 2.6s

#### ISSUE-0149 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
           at System.IO.File.WriteToFileAsync(String path, FileMode mode, ReadOnlyMemory`1 contents, Encoding encoding, CancellationToken cancellationToken)
           at Pennington.Generation.OutputGenerationService.<>c__DisplayClass13_0.<<FetchPagesAsync>b__0>d.MoveNext() in B:\Penn\src\Pennington\Generation\OutputGenerationService.cs:line 318
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 16 pages in 0.7s
    13 pages generated
    3 pages failed
  
  ERRORS
    /changelog/v2-1-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-1-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-1-0.md
    /changelog/v2-0-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md
    /changelog/v2-0-1/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-1\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md
  
  
  ```

#### ISSUE-0150 · `R.PAGE_FAILED` · error · pass A
- File: `/changelog/v2-0-0/`
- Message: Engine reported page generation failure for `/changelog/v2-0-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md

#### ISSUE-0151 · `R.PAGE_FAILED` · error · pass A
- File: `/changelog/v2-0-1/`
- Message: Engine reported page generation failure for `/changelog/v2-0-1/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-0-1\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md

#### ISSUE-0152 · `R.PAGE_FAILED` · error · pass A
- File: `/changelog/v2-1-0/`
- Message: Engine reported page generation failure for `/changelog/v2-1-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\root\changelog\v2-1-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-1-0.md

#### ISSUE-0153 · `T.DUP` · error · pass A · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `development/index.html`
  - `operations/index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/changelog/v2-0-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/changelog/v2-0-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-4...
  ```

#### ISSUE-0154 · `T.DUP` · error · pass A · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `development/index.html`
  - `operations/index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/changelog/v2-0-1/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/changelog/v2-0-1/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-4...
  ```

#### ISSUE-0155 · `T.DUP` · error · pass A · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `development/index.html`
  - `operations/index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/changelog/v2-1-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/changelog/v2-1-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-4...
  ```

#### ISSUE-0156 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0157 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
           at Microsoft.Win32.SafeHandles.SafeFileHandle.CreateFile(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
           at Microsoft.Win32.SafeHandles.SafeFileHandle.Open(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
           at System.IO.File.OpenHandle(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize)
           at System.IO.File.WriteToFileAsync(String path, FileMode mode, ReadOnlyMemory`1 contents, Encoding encoding, CancellationToken cancellationToken)
           at Pennington.Generation.OutputGenerationService.<>c__DisplayClass13_0.<<FetchPagesAsync>b__0>d.MoveNext() in B:\Penn\src\Pennington\Generation\OutputGenerationService.cs:line 318
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 16 pages in 0.5s
    14 pages generated
    2 pages failed
  
  ERRORS
    /changelog/v2-0-0/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-0\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md
    /changelog/v2-0-1/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-1\index.html' because it is being used by another process.
      Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md
  
  
  ```

#### ISSUE-0158 · `R.PAGE_FAILED` · error · pass B
- File: `/changelog/v2-0-0/`
- Message: Engine reported page generation failure for `/changelog/v2-0-0/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-0\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-0.md

#### ISSUE-0159 · `R.PAGE_FAILED` · error · pass B
- File: `/changelog/v2-0-1/`
- Message: Engine reported page generation failure for `/changelog/v2-0-1/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\NorthwindHandbookExample\prefixed\changelog\v2-0-1\index.html' because it is being used by another process. Source: B:\Penn\examples\NorthwindHandbookExample\Content\changelog\v2-0-1.md

#### ISSUE-0160 · `T.DUP` · error · pass B · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `development/index.html`
  - `operations/index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/preview/changelog/v2-0-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/preview/changelog/v2-0-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:tex...
  ```

#### ISSUE-0161 · `T.DUP` · error · pass B · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `development/index.html`
  - `operations/index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/preview/changelog/v2-0-1/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/preview/changelog/v2-0-1/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:tex...
  ```

#### ISSUE-0162 · `T.DUP` · error · pass B · rolled up (22 occurrences)
- Affects 11 pages:
  - `404.html`
  - `index.html`
  - `development/index.html`
  - `operations/index.html`
  - `changelog/v2-0-0/index.html`
  - `changelog/v2-0-1/index.html`
  - … (+5 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Selector: `ul.flex > li.block > ul.mt-4 > li.block > a.block`
- Message: TOC lists `/preview/changelog/v2-1-0/` more than once.
- Excerpt:
  ```
  <a data-current="false" href="/preview/changelog/v2-1-0/" class="block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:tex...
  ```

#### ISSUE-0163 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### PrismDocsExample

- Project: `examples/PrismDocsExample/PrismDocsExample.csproj`
- Pass A: built ok, 4 HTML pages in output, 4.0s
- Pass B: built ok, 4 HTML pages in output, 3.9s
- No issues.

### RecipeExample

- Project: `examples/RecipeExample/RecipeExample.csproj`
- Pass A: build failed (exit 1), 7 HTML pages in output, 7.8s
- Pass B: build failed (exit 1), 7 HTML pages in output, 6.3s

#### ISSUE-0164 · `L.BROKEN` · error · pass A · rolled up (21 occurrences)
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

#### ISSUE-0165 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /recipes/bacon-wrapped-jalapenos links to / (Page not found)
      /recipes/bacon-wrapped-jalapenos links to / (Page not found)
      /recipes/bacon-wrapped-jalapenos links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
  
  
  ```

#### ISSUE-0166 · `R.PAGE_FAILED` · error · pass A
- File: `/sitemap.xml`
- Message: Engine reported page generation failure for `/sitemap.xml`: HTTP 500 fetching /sitemap.xml

#### ISSUE-0167 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `/recipes/chili`
  - `/recipes/bacon-wrapped-jalapenos`
  - `/recipes/chex-mix`
  - `/recipes/zuppa-toscana`
  - `/recipes/cajun-chicken-pasta`
  - `/recipes/beer-cheese`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /recipes/chili links to / (Page not found)
  ```

#### ISSUE-0168 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0169 · `L.BROKEN` · error · pass B · rolled up (21 occurrences)
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

#### ISSUE-0170 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/zuppa-toscana links to /preview/ (Page not found)
      /recipes/zuppa-toscana links to /preview/ (Page not found)
      /recipes/zuppa-toscana links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0171 · `R.PAGE_FAILED` · error · pass B
- File: `/sitemap.xml`
- Message: Engine reported page generation failure for `/sitemap.xml`: HTTP 500 fetching /sitemap.xml

#### ISSUE-0172 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 7 pages:
  - `/recipes/chili`
  - `/recipes/bacon-wrapped-jalapenos`
  - `/recipes/chicken-piccata`
  - `/recipes/cajun-chicken-pasta`
  - `/recipes/chex-mix`
  - `/recipes/beer-cheese`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /recipes/chili links to /preview/ (Page not found)
  ```

#### ISSUE-0173 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### RoslynIntegrationExample

- Project: `examples/RoslynIntegrationExample/RoslynIntegrationExample.csproj`
- Pass A: build failed (exit 1), 6 HTML pages in output, 12.6s
- Pass B: build failed (exit 1), 6 HTML pages in output, 13.4s

#### ISSUE-0174 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Adding file watch: B:\Penn\examples\RoslynIntegrationExample\Content with pattern *.*
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.csproj
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.sln
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.slnx
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.cs
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 8 pages in 10.4s
    8 pages generated
  
  WARNINGS
    2 broken links found:
      /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  
  
  ```

#### ISSUE-0175 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0176 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0177 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0178 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0179 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0180 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
        Adding file watch: B:\Penn\examples\RoslynIntegrationExample\Content with pattern *.*
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.csproj
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.sln
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.slnx
  info: Pennington.Infrastructure.FileWatcher[0]
        Adding file watch: B:\Penn with pattern *.cs
  info: Microsoft.Hosting.Lifetime[0]
        Application is shutting down...
  Build Complete — 8 pages in 11.2s
    8 pages generated
  
  WARNINGS
    2 broken links found:
      /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  
  
  ```

#### ISSUE-0181 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0182 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0183 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0184 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0185 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SearchExample

- Project: `examples/SearchExample/SearchExample.csproj`
- Pass A: built ok, 1001 HTML pages in output, 12.4s
- Pass B: built ok, 1001 HTML pages in output, 13.0s

#### ISSUE-0186 · `M.NO_ENTRIES` · error · pass A
- File: `llms.txt`
- Message: llms.txt contains no markdown link entries.

#### ISSUE-0187 · `S.EMPTY` · error · pass A
- File: `search-index.json`
- Message: search-index.json is an empty array.

#### ISSUE-0188 · `X.EMPTY` · warning · pass A
- File: `sitemap.xml`
- Message: sitemap.xml has no <loc> entries.

#### ISSUE-0189 · `M.NO_ENTRIES` · error · pass B
- File: `llms.txt`
- Message: llms.txt contains no markdown link entries.

#### ISSUE-0190 · `S.EMPTY` · error · pass B
- File: `search-index.json`
- Message: search-index.json is an empty array.

#### ISSUE-0191 · `X.EMPTY` · warning · pass B
- File: `sitemap.xml`
- Message: sitemap.xml has no <loc> entries.

### SpaNavigationExample

- Project: `examples/SpaNavigationExample/SpaNavigationExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 2.7s
- Pass B: built ok, 5 HTML pages in output, 2.8s

#### ISSUE-0192 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0193 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpaNavigationTutorialExample

- Project: `examples/SpaNavigationTutorialExample/SpaNavigationTutorialExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 3.2s
- Pass B: built ok, 6 HTML pages in output, 2.9s

#### ISSUE-0194 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0195 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpectreConsoleExample

- Project: `examples/SpectreConsoleExample/SpectreConsoleExample.csproj`
- Pass A: build failed (exit 1), 79 HTML pages in output, 4.2s
- Pass B: build failed (exit 1), 79 HTML pages in output, 4.6s

#### ISSUE-0196 · `L.BROKEN` · error · pass A · rolled up (237 occurrences)
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

#### ISSUE-0197 · `L.BROKEN` · error · pass A · rolled up (163 occurrences)
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

#### ISSUE-0198 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /cli/explanation/command-lifecycle-and-execution-flow/ links to /blog (Page not found)
      /cli/explanation/command-lifecycle-and-execution-flow/ links to / (Page not found)
      /cli/explanation/command-lifecycle-and-execution-flow/ links to /blog (Page not found)
      /console/widgets/table/ links to / (Page not found)
      /console/widgets/table/ links to / (Page not found)
      /console/widgets/table/ links to /blog (Page not found)
      /console/widgets/table/ links to / (Page not found)
      /console/widgets/table/ links to /blog (Page not found)
      /console/reference/border-styles-reference/ links to / (Page not found)
      /console/reference/border-styles-reference/ links to / (Page not found)
      /console/reference/border-styles-reference/ links to /blog (Page not found)
      /console/reference/border-styles-reference/ links to / (Page not found)
      /console/reference/border-styles-reference/ links to /blog (Page not found)
      /console/how-to/rendering-ascii-art-and-figlet-text/ links to / (Page not found)
      /console/how-to/rendering-ascii-art-and-figlet-text/ links to / (Page not found)
      /console/how-to/rendering-ascii-art-and-figlet-text/ links to /blog (Page not found)
      /console/how-to/rendering-ascii-art-and-figlet-text/ links to / (Page not found)
      /console/how-to/rendering-ascii-art-and-figlet-text/ links to /blog (Page not found)
  
  
  ```

#### ISSUE-0199 · `R.BROKEN_LINK` · warning · pass A · rolled up (237 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
  - `/cli/tutorials/quick-start-your-first-cli-app/`
  - `/cli/how--to/intercepting-command-execution/`
  - `/blog/spectre-console-0-49-0-search-positioning-progress/`
  - `/cli/reference/extensibility-points/`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to / (Page not found)
  ```

#### ISSUE-0200 · `R.BROKEN_LINK` · warning · pass A · rolled up (163 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
  - `/cli/tutorials/quick-start-your-first-cli-app/`
  - `/cli/how--to/intercepting-command-execution/`
  - `/blog/spectre-console-0-49-0-search-positioning-progress/`
  - `/cli/reference/extensibility-points/`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Message: Engine reports broken link to `/blog` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to /blog (Page not found)
  ```

#### ISSUE-0201 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0202 · `L.BROKEN` · error · pass B · rolled up (237 occurrences)
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

#### ISSUE-0203 · `L.BROKEN` · error · pass B · rolled up (163 occurrences)
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

#### ISSUE-0204 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /cli/tutorials/quick-start-your-first-cli-app/ links to /preview/blog (Page not found)
      /cli/tutorials/quick-start-your-first-cli-app/ links to /preview/ (Page not found)
      /cli/tutorials/quick-start-your-first-cli-app/ links to /preview/blog (Page not found)
      /cli/reference/attribute-and-parameter-reference/ links to /preview/ (Page not found)
      /cli/reference/attribute-and-parameter-reference/ links to /preview/ (Page not found)
      /cli/reference/attribute-and-parameter-reference/ links to /preview/blog (Page not found)
      /cli/reference/attribute-and-parameter-reference/ links to /preview/ (Page not found)
      /cli/reference/attribute-and-parameter-reference/ links to /preview/blog (Page not found)
      /console/widgets/json/ links to /preview/ (Page not found)
      /console/widgets/json/ links to /preview/ (Page not found)
      /console/widgets/json/ links to /preview/blog (Page not found)
      /console/widgets/json/ links to /preview/ (Page not found)
      /console/widgets/json/ links to /preview/blog (Page not found)
      /console/how-to/live-rendering-and-dynamic-updates/ links to /preview/ (Page not found)
      /console/how-to/live-rendering-and-dynamic-updates/ links to /preview/ (Page not found)
      /console/how-to/live-rendering-and-dynamic-updates/ links to /preview/blog (Page not found)
      /console/how-to/live-rendering-and-dynamic-updates/ links to /preview/ (Page not found)
      /console/how-to/live-rendering-and-dynamic-updates/ links to /preview/blog (Page not found)
  
  
  ```

#### ISSUE-0205 · `R.BROKEN_LINK` · warning · pass B · rolled up (237 occurrences)
- Affects 79 pages:
  - `/cli/how--to/working-with-multiple-command-hierarchies/`
  - `/console/widgets/table/`
  - `/cli/tutorials/building-a-multi-command-cli-tool/`
  - `/console/live/async-patterns/`
  - `/console/explanation/understanding-rendering-model/`
  - `/console/tutorials/getting-started-building-rich-console-app/`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /cli/how--to/working-with-multiple-command-hierarchies/ links to /preview/ (Page not found)
  ```

#### ISSUE-0206 · `R.BROKEN_LINK` · warning · pass B · rolled up (163 occurrences)
- Affects 79 pages:
  - `/cli/how--to/working-with-multiple-command-hierarchies/`
  - `/console/widgets/table/`
  - `/cli/tutorials/building-a-multi-command-cli-tool/`
  - `/console/live/async-patterns/`
  - `/console/explanation/understanding-rendering-model/`
  - `/console/tutorials/getting-started-building-rich-console-app/`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/blog` (Page not found).
- Excerpt:
  ```
  /cli/how--to/working-with-multiple-command-hierarchies/ links to /preview/blog (Page not found)
  ```

#### ISSUE-0207 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### TempoDocsExample

- Project: `examples/TempoDocsExample/TempoDocsExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 3.8s
- Pass B: built ok, 5 HTML pages in output, 3.2s
- No issues.

### UserInterfaceExample

- Project: `examples/UserInterfaceExample/UserInterfaceExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.7s
- Pass B: built ok, 6 HTML pages in output, 2.7s

#### ISSUE-0208 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0209 · `B.MISSING_BODY_ATTR` · warning · pass B
- File: `index.html`
- Selector: `body`
- Message: Page body missing `data-base-url` attribute (expected `/preview/`).

#### ISSUE-0210 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### YogaStudioExample

- Project: `examples/YogaStudioExample/YogaStudioExample.csproj`
- Pass A: build failed (exit 1), 18 HTML pages in output, 2.9s
- Pass B: build failed (exit 1), 18 HTML pages in output, 2.8s

#### ISSUE-0211 · `L.BROKEN` · error · pass A
- File: `blog/index.html`
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/gen-z/blog/` → missing target `/gen-z/blog/`.
- Excerpt:
  ```
  <a href="/gen-z/blog/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0212 · `L.BROKEN` · error · pass A · rolled up (22 occurrences)
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

#### ISSUE-0213 · `L.BROKEN` · error · pass A · rolled up (4 occurrences)
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

#### ISSUE-0214 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
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

#### ISSUE-0215 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
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

#### ISSUE-0216 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/alex-rivera` → missing target `/instructors/alex-rivera`.
- Excerpt:
  ```
  <a href="/instructors/alex-rivera" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font...
  ```

#### ISSUE-0217 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/james-okafor` → missing target `/instructors/james-okafor`.
- Excerpt:
  ```
  <a href="/instructors/james-okafor" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl fon...
  ```

#### ISSUE-0218 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/maya-chen` → missing target `/instructors/maya-chen`.
- Excerpt:
  ```
  <a href="/instructors/maya-chen" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font-b...
  ```

#### ISSUE-0219 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/priya-sharma` → missing target `/instructors/priya-sharma`.
- Excerpt:
  ```
  <a href="/instructors/priya-sharma" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl fon...
  ```

#### ISSUE-0220 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/sam-tanaka` → missing target `/instructors/sam-tanaka`.
- Excerpt:
  ```
  <a href="/instructors/sam-tanaka" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font-...
  ```

#### ISSUE-0221 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /instructors/ links to /instructors/james-okafor (Page not found)
      /instructors/ links to /instructors/priya-sharma (Page not found)
      /instructors/ links to /instructors/alex-rivera (Page not found)
      /instructors/ links to /instructors/sam-tanaka (Page not found)
      /instructors/ links to /js/search.js (Page not found)
      /instructors/ links to /js/faq.js (Page not found)
      /faq/ links to /js/search.js (Page not found)
      /faq/ links to /js/faq.js (Page not found)
      /contact/ links to /js/search.js (Page not found)
      /contact/ links to /js/faq.js (Page not found)
      /blog/breathing-techniques/ links to /schedule/sun-breathwork (Page not found)
      /blog/breathing-techniques/ links to /js/search.js (Page not found)
      /blog/breathing-techniques/ links to /js/faq.js (Page not found)
      /blog/yoga-for-beginners/ links to /js/search.js (Page not found)
      /blog/yoga-for-beginners/ links to /js/faq.js (Page not found)
      /blog/ links to /gen-z/blog/ (Page not found)
      /blog/ links to /js/search.js (Page not found)
      /blog/ links to /js/faq.js (Page not found)
  
  
  ```

#### ISSUE-0222 · `R.PAGE_FAILED` · error · pass A
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\root\about\index.html' because it is being used by another process.

#### ISSUE-0223 · `R.BROKEN_LINK` · warning · pass A · rolled up (22 occurrences)
- Affects 8 pages:
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/about/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/ (Page not found)
  ```

#### ISSUE-0224 · `R.BROKEN_LINK` · warning · pass A · rolled up (24 occurrences)
- Affects 7 pages:
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/about/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - … (+1 more)
- Total raw occurrences: 24 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/blog` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/blog (Page not found)
  ```

#### ISSUE-0225 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/about/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/instructors` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/instructors (Page not found)
  ```

#### ISSUE-0226 · `R.BROKEN_LINK` · warning · pass A
- File: `/gen-z/blog/breathing-techniques/`
- Message: Engine reports broken link to `/gen-z/schedule/sun-breathwork` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/schedule/sun-breathwork (Page not found)
  ```

#### ISSUE-0227 · `R.BROKEN_LINK` · warning · pass A · rolled up (22 occurrences)
- Affects 7 pages:
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/about/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - … (+1 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/schedule` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/schedule (Page not found)
  ```

#### ISSUE-0228 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 18 pages:
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/about/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/js/faq.js` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /js/faq.js (Page not found)
  ```

#### ISSUE-0229 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 18 pages:
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/about/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/js/search.js` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /js/search.js (Page not found)
  ```

#### ISSUE-0230 · `R.BROKEN_LINK` · warning · pass A
- File: `/gen-z/blog/morning-yoga-routine/`
- Message: Engine reports broken link to `/gen-z/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0231 · `R.BROKEN_LINK` · warning · pass A
- File: `/schedule/`
- Message: Engine reports broken link to `/gen-z/schedule/` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /gen-z/schedule/ (Page not found)
  ```

#### ISSUE-0232 · `R.BROKEN_LINK` · warning · pass A · rolled up (4 occurrences)
- Affects 3 pages:
  - `/schedule/`
  - `/blog/morning-yoga-routine/`
  - `/`
- Total raw occurrences: 4 (some pages emit multiple matches)
- Message: Engine reports broken link to `/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0233 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0234 · `L.BROKEN` · error · pass B
- File: `blog/index.html`
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/preview/gen-z/blog/` → missing target `/gen-z/blog/`.
- Excerpt:
  ```
  <a href="/preview/gen-z/blog/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0235 · `L.BROKEN` · error · pass B · rolled up (22 occurrences)
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

#### ISSUE-0236 · `L.BROKEN` · error · pass B · rolled up (4 occurrences)
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

#### ISSUE-0237 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
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

#### ISSUE-0238 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
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

#### ISSUE-0239 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/alex-rivera` → missing target `/instructors/alex-rivera`.
- Excerpt:
  ```
  <a href="/preview/instructors/alex-rivera" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-...
  ```

#### ISSUE-0240 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/james-okafor` → missing target `/instructors/james-okafor`.
- Excerpt:
  ```
  <a href="/preview/instructors/james-okafor" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text...
  ```

#### ISSUE-0241 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/maya-chen` → missing target `/instructors/maya-chen`.
- Excerpt:
  ```
  <a href="/preview/instructors/maya-chen" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2x...
  ```

#### ISSUE-0242 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/priya-sharma` → missing target `/instructors/priya-sharma`.
- Excerpt:
  ```
  <a href="/preview/instructors/priya-sharma" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text...
  ```

#### ISSUE-0243 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/sam-tanaka` → missing target `/instructors/sam-tanaka`.
- Excerpt:
  ```
  <a href="/preview/instructors/sam-tanaka" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2...
  ```

#### ISSUE-0244 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /gen-z/blog/yoga-for-beginners/ links to /preview/js/search.js (Page not found)
      /gen-z/blog/yoga-for-beginners/ links to /preview/js/faq.js (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/ (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/ (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/schedule (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/instructors (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/blog (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/ (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/schedule (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/instructors (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/blog (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/blog (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/schedule/sun-breathwork (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/schedule (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/instructors (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/gen-z/blog (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/js/search.js (Page not found)
      /gen-z/blog/breathing-techniques/ links to /preview/js/faq.js (Page not found)
  
  
  ```

#### ISSUE-0245 · `R.PAGE_FAILED` · error · pass B
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\prefixed\about\index.html' because it is being used by another process.

#### ISSUE-0246 · `R.BROKEN_LINK` · warning · pass B · rolled up (22 occurrences)
- Affects 8 pages:
  - `/`
  - `/gen-z/pricing/`
  - `/gen-z/faq/`
  - `/gen-z/about/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/contact/`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/gen-z/` (Page not found).
- Excerpt:
  ```
  / links to /preview/gen-z/ (Page not found)
  ```

#### ISSUE-0247 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/instructors/`
- Message: Engine reports broken link to `/preview/instructors/alex-rivera` (Page not found).
- Excerpt:
  ```
  / links to /preview/instructors/alex-rivera (Page not found)
  ```

#### ISSUE-0248 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/instructors/`
- Message: Engine reports broken link to `/preview/instructors/james-okafor` (Page not found).
- Excerpt:
  ```
  / links to /preview/instructors/james-okafor (Page not found)
  ```

#### ISSUE-0249 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/instructors/`
- Message: Engine reports broken link to `/preview/instructors/maya-chen` (Page not found).
- Excerpt:
  ```
  / links to /preview/instructors/maya-chen (Page not found)
  ```

#### ISSUE-0250 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/instructors/`
- Message: Engine reports broken link to `/preview/instructors/priya-sharma` (Page not found).
- Excerpt:
  ```
  / links to /preview/instructors/priya-sharma (Page not found)
  ```

#### ISSUE-0251 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/`
  - `/instructors/`
- Message: Engine reports broken link to `/preview/instructors/sam-tanaka` (Page not found).
- Excerpt:
  ```
  / links to /preview/instructors/sam-tanaka (Page not found)
  ```

#### ISSUE-0252 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 18 pages:
  - `/`
  - `/instructors/`
  - `/faq/`
  - `/pricing/`
  - `/gen-z/pricing/`
  - `/blog/`
  - … (+12 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/js/search.js` (Page not found).
- Excerpt:
  ```
  / links to /preview/js/search.js (Page not found)
  ```

#### ISSUE-0253 · `R.BROKEN_LINK` · warning · pass B · rolled up (4 occurrences)
- Affects 3 pages:
  - `/`
  - `/blog/morning-yoga-routine/`
  - `/schedule/`
- Total raw occurrences: 4 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  / links to /preview/schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0254 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 2 pages:
  - `/`
  - `/schedule/`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/tue-ashtanga` (Page not found).
- Excerpt:
  ```
  / links to /preview/schedule/tue-ashtanga (Page not found)
  ```

#### ISSUE-0255 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 2 pages:
  - `/`
  - `/schedule/`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/wed-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  / links to /preview/schedule/wed-vinyasa-morning (Page not found)
  ```

#### ISSUE-0256 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

_Additional issues not shown (truncated to 10 samples per code):_

- `L.BROKEN` (pass A): 21 more similar issues
- `R.BROKEN_LINK` (pass A): 23 more similar issues
- `L.BROKEN` (pass B): 21 more similar issues
- `R.BROKEN_LINK` (pass B): 23 more similar issues


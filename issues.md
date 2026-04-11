# Pennington Examples Validation Report

Generated: 2026-04-11T20:45:16Z
Examples discovered: 20
Examples built successfully: 9 (pass A) / 9 (pass B)
Total issues: 1307 errors, 1395 warnings, 28 info
Shown in report: 234 samples (capped at 10 per example/pass/code; full counts preserved in cross-example patterns below).

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
| BeaconDocsExample | fail | fail | 2 | 4 | 0 |
| BlogExample | fail | fail | 88 | 100 | 2 |
| ForgePortalExample | fail | fail | 22 | 18 | 2 |
| LocalizationExample | ok | ok | 0 | 0 | 0 |
| LocalizationTutorialExample | ok | ok | 0 | 0 | 0 |
| MaraBlogExample | fail | fail | 26 | 30 | 2 |
| MinimalExample | fail | fail | 2 | 8 | 2 |
| MultipleContentSourceExample | fail | fail | 14 | 0 | 2 |
| NorthwindHandbookExample | ok | ok | 0 | 0 | 2 |
| PrismDocsExample | ok | ok | 0 | 0 | 0 |
| RecipeExample | fail | fail | 46 | 42 | 2 |
| RoslynIntegrationExample | fail | fail | 2 | 8 | 2 |
| SearchExample | ok | ok | 0 | 0 | 0 |
| SpaNavigationExample | ok | ok | 0 | 0 | 2 |
| SpaNavigationTutorialExample | ok | ok | 0 | 0 | 2 |
| SpectreConsoleExample | fail | fail | 802 | 800 | 2 |
| TempoDocsExample | ok | ok | 0 | 0 | 0 |
| UserInterfaceExample | ok | ok | 0 | 1 | 2 |
| YogaStudioExample | fail | fail | 273 | 350 | 2 |

## Cross-example patterns

Counts below reflect **raw** issue totals (not the truncated samples shown in per-example sections). Codes that appear in multiple examples are likely engine bugs; codes unique to one example are likely example bugs.

- `R.BUILD_FAILED` (error) — 22 occurrences across 11 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+5 more)
- `L.BROKEN` (error) — 1276 raw occurrences rolled up into 152 distinct groups across 10 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+4 more)
- `R.PAGE_FAILED` (error) — 19 occurrences across 4 examples: BlogExample, MultipleContentSourceExample, RecipeExample, YogaStudioExample
- `R.BROKEN_LINK` (warning) — 1384 raw occurrences rolled up into 162 distinct groups across 10 examples: AlexBlogExample, BeaconDocsExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, … (+4 more)
- `B.MISSING_BODY_ATTR` (warning) — 1 occurrence across 1 example: UserInterfaceExample
- `M.MISSING` (info) — 28 occurrences across 14 examples: AlexBlogExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, MultipleContentSourceExample, … (+8 more)

## Issues by example

### AlexBlogExample

- Project: `examples/AlexBlogExample/AlexBlogExample.csproj`
- Pass A: build failed (exit 1), 3 HTML pages in output, 3.3s
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
- Pass A: build failed (exit 1), 10 HTML pages in output, 3.6s
- Pass B: build failed (exit 1), 10 HTML pages in output, 3.6s

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
  Build Complete — 22 pages in 0.9s
    22 pages generated
  
  WARNINGS
    1 broken links found:
      /getting-started/ links to /getting-started/beacon-arch.png (Page not found)
  
  
  ```

#### ISSUE-0032 · `L.BROKEN` · warning · pass A
- File: `getting-started/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > img`
- Message: Image src `/getting-started/beacon-arch.png` → missing target `/getting-started/beacon-arch.png`.
- Excerpt:
  ```
  <img src="/getting-started/beacon-arch.png" alt="Beacon Architecture">
  ```

#### ISSUE-0033 · `R.BROKEN_LINK` · warning · pass A
- File: `/getting-started/`
- Message: Engine reports broken link to `/getting-started/beacon-arch.png` (Page not found).
- Excerpt:
  ```
  /getting-started/ links to /getting-started/beacon-arch.png (Page not found)
  ```

#### ISSUE-0034 · `R.BUILD_FAILED` · error · pass B
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

#### ISSUE-0035 · `L.BROKEN` · warning · pass B
- File: `getting-started/index.html`
- Selector: `div.flex > article#main-content > main.prose > p > img`
- Message: Image src `/preview/getting-started/beacon-arch.png` → missing target `/getting-started/beacon-arch.png`.
- Excerpt:
  ```
  <img src="/preview/getting-started/beacon-arch.png" alt="Beacon Architecture">
  ```

#### ISSUE-0036 · `R.BROKEN_LINK` · warning · pass B
- File: `/getting-started/`
- Message: Engine reports broken link to `/preview/getting-started/beacon-arch.png` (Page not found).
- Excerpt:
  ```
  /getting-started/ links to /preview/getting-started/beacon-arch.png (Page not found)
  ```

### BlogExample

- Project: `examples/BlogExample/BlogExample.csproj`
- Pass A: build failed (exit 1), 8 HTML pages in output, 3.3s
- Pass B: build failed (exit 1), 8 HTML pages in output, 3.5s

#### ISSUE-0037 · `L.BROKEN` · error · pass A · rolled up (15 occurrences)
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

#### ISSUE-0038 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/chewing-magazine` → missing target `/tags/chewing-magazine`.
- Excerpt:
  ```
  <a href="/tags/chewing-magazine" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">chewing-magazine</a>
  ```

#### ISSUE-0039 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gum-culture` → missing target `/tags/gum-culture`.
- Excerpt:
  ```
  <a href="/tags/gum-culture" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-culture</a>
  ```

#### ISSUE-0040 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
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

#### ISSUE-0041 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/analysis` → missing target `/tags/analysis`.
- Excerpt:
  ```
  <a href="/tags/analysis" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">analysis</a>
  ```

#### ISSUE-0042 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gum-brands` → missing target `/tags/gum-brands`.
- Excerpt:
  ```
  <a href="/tags/gum-brands" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-brands</a>
  ```

#### ISSUE-0043 · `L.BROKEN` · error · pass A
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/science` → missing target `/tags/science`.
- Excerpt:
  ```
  <a href="/tags/science" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">science</a>
  ```

#### ISSUE-0044 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/apparel` → missing target `/tags/apparel`.
- Excerpt:
  ```
  <a href="/tags/apparel" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">apparel</a>
  ```

#### ISSUE-0045 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/equipment` → missing target `/tags/equipment`.
- Excerpt:
  ```
  <a href="/tags/equipment" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">equipment</a>
  ```

#### ISSUE-0046 · `L.BROKEN` · error · pass A
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/tags/gear` → missing target `/tags/gear`.
- Excerpt:
  ```
  <a href="/tags/gear" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gear</a>
  ```

#### ISSUE-0047 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/reviews (Page not found)
      /blog/2024/04/gum-chewing-apparel-guide/ links to /tags/equipment (Page not found)
      /blog/2024/04/gum-chewing-apparel-guide/ links to / (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /rss.xml (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to / (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /tags/analysis (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /tags/gum-brands (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /tags/science (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to /tags/reviews (Page not found)
      /blog/2024/03/top-five-gum-brands-analysis/ links to / (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /rss.xml (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to / (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /tags/reviews (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /tags/chewing-magazine (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to /tags/gum-culture (Page not found)
      /blog/2024/03/chewing-magazine-review/ links to / (Page not found)
      /about/ links to /rss.xml (Page not found)
      /about/ links to / (Page not found)
  
  
  ```

#### ISSUE-0048 · `R.PAGE_FAILED` · error · pass A
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\BlogExample\root\about\index.html' because it is being used by another process.

#### ISSUE-0049 · `R.BROKEN_LINK` · warning · pass A · rolled up (15 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to / (Page not found)
  ```

#### ISSUE-0050 · `R.BROKEN_LINK` · warning · pass A · rolled up (8 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
  - … (+2 more)
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0051 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/data-analytics` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/data-analytics (Page not found)
  ```

#### ISSUE-0052 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/optimization` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/optimization (Page not found)
  ```

#### ISSUE-0053 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/performance-tracking` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/performance-tracking (Page not found)
  ```

#### ISSUE-0054 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/tags/python` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /tags/python (Page not found)
  ```

#### ISSUE-0055 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/tags/bubbles` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/bubbles (Page not found)
  ```

#### ISSUE-0056 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/tags/exercises` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/exercises (Page not found)
  ```

#### ISSUE-0057 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/2024/05/tongue-exercises-bigger-bubbles/`
- Message: Engine reports broken link to `/tags/technique` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/technique (Page not found)
  ```

#### ISSUE-0058 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/04/mandibular-fitness-regime/`
- Message: Engine reports broken link to `/tags/training` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/tongue-exercises-bigger-bubbles/ links to /tags/training (Page not found)
  ```

#### ISSUE-0059 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0060 · `L.BROKEN` · error · pass B · rolled up (15 occurrences)
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

#### ISSUE-0061 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/chewing-magazine` → missing target `/tags/chewing-magazine`.
- Excerpt:
  ```
  <a href="/preview/tags/chewing-magazine" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">chewing-magazi...
  ```

#### ISSUE-0062 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/chewing-magazine-review/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gum-culture` → missing target `/tags/gum-culture`.
- Excerpt:
  ```
  <a href="/preview/tags/gum-culture" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-culture</a>
  ```

#### ISSUE-0063 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
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

#### ISSUE-0064 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/analysis` → missing target `/tags/analysis`.
- Excerpt:
  ```
  <a href="/preview/tags/analysis" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">analysis</a>
  ```

#### ISSUE-0065 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gum-brands` → missing target `/tags/gum-brands`.
- Excerpt:
  ```
  <a href="/preview/tags/gum-brands" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gum-brands</a>
  ```

#### ISSUE-0066 · `L.BROKEN` · error · pass B
- File: `blog/2024/03/top-five-gum-brands-analysis/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/science` → missing target `/tags/science`.
- Excerpt:
  ```
  <a href="/preview/tags/science" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">science</a>
  ```

#### ISSUE-0067 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/apparel` → missing target `/tags/apparel`.
- Excerpt:
  ```
  <a href="/preview/tags/apparel" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">apparel</a>
  ```

#### ISSUE-0068 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/equipment` → missing target `/tags/equipment`.
- Excerpt:
  ```
  <a href="/preview/tags/equipment" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">equipment</a>
  ```

#### ISSUE-0069 · `L.BROKEN` · error · pass B
- File: `blog/2024/04/gum-chewing-apparel-guide/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/tags/gear` → missing target `/tags/gear`.
- Excerpt:
  ```
  <a href="/preview/tags/gear" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">gear</a>
  ```

#### ISSUE-0070 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /blog/2024/04/mandibular-fitness-regime/ links to /preview/tags/health (Page not found)
      /blog/2024/04/mandibular-fitness-regime/ links to /preview/ (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/rss.xml (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/ (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/interview (Page not found)
      /blog/2024/05/bazooka-joe-interview/ links to /preview/tags/bazooka-joe (Page not found)
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
      /about/ links to /preview/rss.xml (Page not found)
      /about/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0071 · `R.PAGE_FAILED` · error · pass B
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\BlogExample\prefixed\about\index.html' because it is being used by another process.

#### ISSUE-0072 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/04/gum-chewing-apparel-guide/`
- Message: Engine reports broken link to `/preview/tags/apparel` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /preview/tags/apparel (Page not found)
  ```

#### ISSUE-0073 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/04/gum-chewing-apparel-guide/`
- Message: Engine reports broken link to `/preview/tags/equipment` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /preview/tags/equipment (Page not found)
  ```

#### ISSUE-0074 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/04/gum-chewing-apparel-guide/`
- Message: Engine reports broken link to `/preview/tags/gear` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /preview/tags/gear (Page not found)
  ```

#### ISSUE-0075 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/03/chewing-magazine-review/`
  - `/blog/2024/03/top-five-gum-brands-analysis/`
- Message: Engine reports broken link to `/preview/tags/reviews` (Page not found).
- Excerpt:
  ```
  /blog/2024/04/gum-chewing-apparel-guide/ links to /preview/tags/reviews (Page not found)
  ```

#### ISSUE-0076 · `R.BROKEN_LINK` · warning · pass B · rolled up (15 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/03/chewing-magazine-review/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - … (+2 more)
- Total raw occurrences: 15 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/ (Page not found)
  ```

#### ISSUE-0077 · `R.BROKEN_LINK` · warning · pass B · rolled up (8 occurrences)
- Affects 8 pages:
  - `/blog/2024/05/chewing-data-analytics/`
  - `/blog/2024/04/gum-chewing-apparel-guide/`
  - `/blog/2024/05/tongue-exercises-bigger-bubbles/`
  - `/blog/2024/03/chewing-magazine-review/`
  - `/blog/2024/04/mandibular-fitness-regime/`
  - `/blog/2024/05/bazooka-joe-interview/`
  - … (+2 more)
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0078 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/data-analytics` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/data-analytics (Page not found)
  ```

#### ISSUE-0079 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/optimization` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/optimization (Page not found)
  ```

#### ISSUE-0080 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/performance-tracking` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/performance-tracking (Page not found)
  ```

#### ISSUE-0081 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/2024/05/chewing-data-analytics/`
- Message: Engine reports broken link to `/preview/tags/python` (Page not found).
- Excerpt:
  ```
  /blog/2024/05/chewing-data-analytics/ links to /preview/tags/python (Page not found)
  ```

#### ISSUE-0082 · `M.MISSING` · info · pass B
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
- Pass B: build failed (exit 1), 10 HTML pages in output, 3.0s

#### ISSUE-0083 · `L.BROKEN` · error · pass A · rolled up (10 occurrences)
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

#### ISSUE-0084 · `R.BUILD_FAILED` · error · pass A
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
      /about/ links to / (Page not found)
      /docs/getting-started/ links to / (Page not found)
      /docs/pipeline-config/ links to / (Page not found)
      /blog/q1-retro/ links to / (Page not found)
      /releases/v2-1-0/ links to / (Page not found)
      /releases/v2-0-0/ links to / (Page not found)
      /releases/v2-0-1/ links to / (Page not found)
  
  
  ```

#### ISSUE-0085 · `R.BROKEN_LINK` · warning · pass A · rolled up (9 occurrences)
- Affects 9 pages:
  - `/blog/welcome/`
  - `/docs/api-keys/`
  - `/about/`
  - `/docs/getting-started/`
  - `/docs/pipeline-config/`
  - `/blog/q1-retro/`
  - … (+3 more)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/welcome/ links to / (Page not found)
  ```

#### ISSUE-0086 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0087 · `L.BROKEN` · error · pass B · rolled up (10 occurrences)
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

#### ISSUE-0088 · `R.BUILD_FAILED` · error · pass B
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
      /releases/v2-0-1/ links to /preview/ (Page not found)
      /releases/v2-1-0/ links to /preview/ (Page not found)
      /docs/pipeline-config/ links to /preview/ (Page not found)
      /releases/v2-0-0/ links to /preview/ (Page not found)
      /blog/q1-retro/ links to /preview/ (Page not found)
      /blog/welcome/ links to /preview/ (Page not found)
      /docs/api-keys/ links to /preview/ (Page not found)
      /docs/getting-started/ links to /preview/ (Page not found)
      /about/ links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0089 · `R.BROKEN_LINK` · warning · pass B · rolled up (9 occurrences)
- Affects 9 pages:
  - `/releases/v2-0-1/`
  - `/releases/v2-1-0/`
  - `/docs/pipeline-config/`
  - `/releases/v2-0-0/`
  - `/blog/q1-retro/`
  - `/blog/welcome/`
  - … (+3 more)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /releases/v2-0-1/ links to /preview/ (Page not found)
  ```

#### ISSUE-0090 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### LocalizationExample

- Project: `examples/LocalizationExample/LocalizationExample.csproj`
- Pass A: built ok, 26 HTML pages in output, 4.0s
- Pass B: built ok, 26 HTML pages in output, 3.9s
- No issues.

### LocalizationTutorialExample

- Project: `examples/LocalizationTutorialExample/LocalizationTutorialExample.csproj`
- Pass A: built ok, 7 HTML pages in output, 3.5s
- Pass B: built ok, 7 HTML pages in output, 3.5s
- No issues.

### MaraBlogExample

- Project: `examples/MaraBlogExample/MaraBlogExample.csproj`
- Pass A: build failed (exit 1), 3 HTML pages in output, 3.5s
- Pass B: build failed (exit 1), 3 HTML pages in output, 3.4s

#### ISSUE-0091 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/dotnet` → missing target `/topics/dotnet`.
- Excerpt:
  ```
  <a href="/topics/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0092 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/performance` → missing target `/topics/performance`.
- Excerpt:
  ```
  <a href="/topics/performance" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">performance</a>
  ```

#### ISSUE-0093 · `L.BROKEN` · error · pass A · rolled up (6 occurrences)
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

#### ISSUE-0094 · `L.BROKEN` · error · pass A
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/aspnet` → missing target `/topics/aspnet`.
- Excerpt:
  ```
  <a href="/topics/aspnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">aspnet</a>
  ```

#### ISSUE-0095 · `L.BROKEN` · error · pass A
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/topics/configuration` → missing target `/topics/configuration`.
- Excerpt:
  ```
  <a href="/topics/configuration" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">configuration</a>
  ```

#### ISSUE-0096 · `R.BUILD_FAILED` · error · pass A
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

#### ISSUE-0097 · `R.BROKEN_LINK` · warning · pass A · rolled up (6 occurrences)
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

#### ISSUE-0098 · `R.BROKEN_LINK` · warning · pass A · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
  - `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /rss.xml (Page not found)
  ```

#### ISSUE-0099 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/topics/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /topics/dotnet (Page not found)
  ```

#### ISSUE-0100 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/allocation-traps/`
  - `/blog/span-patterns/`
- Message: Engine reports broken link to `/topics/performance` (Page not found).
- Excerpt:
  ```
  /blog/allocation-traps/ links to /topics/performance (Page not found)
  ```

#### ISSUE-0101 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/topics/aspnet` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /topics/aspnet (Page not found)
  ```

#### ISSUE-0102 · `R.BROKEN_LINK` · warning · pass A
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/topics/configuration` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /topics/configuration (Page not found)
  ```

#### ISSUE-0103 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0104 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/dotnet` → missing target `/topics/dotnet`.
- Excerpt:
  ```
  <a href="/preview/topics/dotnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">dotnet</a>
  ```

#### ISSUE-0105 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `blog/allocation-traps/index.html`
  - `blog/span-patterns/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/performance` → missing target `/topics/performance`.
- Excerpt:
  ```
  <a href="/preview/topics/performance" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">performance</a>
  ```

#### ISSUE-0106 · `L.BROKEN` · error · pass B · rolled up (6 occurrences)
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

#### ISSUE-0107 · `L.BROKEN` · error · pass B
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/aspnet` → missing target `/topics/aspnet`.
- Excerpt:
  ```
  <a href="/preview/topics/aspnet" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">aspnet</a>
  ```

#### ISSUE-0108 · `L.BROKEN` · error · pass B
- File: `blog/config-pitfalls/index.html`
- Selector: `div.mx-auto > article > div.mt-8 > div.mt-2 > a.inline-flex`
- Message: Link `/preview/topics/configuration` → missing target `/topics/configuration`.
- Excerpt:
  ```
  <a href="/preview/topics/configuration" class="inline-flex items-center px-3 py-1 text-sm font-medium text-primary-600 dark:text-primary-400 bg-base-100 dark:bg-base-800 rounded-full hover:bg-base-200 dark:hover:bg-base-700">configuration</...
  ```

#### ISSUE-0109 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
  
  WARNINGS
    15 broken links found:
      /blog/span-patterns/ links to /preview/rss.xml (Page not found)
      /blog/span-patterns/ links to /preview/ (Page not found)
      /blog/span-patterns/ links to /preview/topics/performance (Page not found)
      /blog/span-patterns/ links to /preview/topics/dotnet (Page not found)
      /blog/span-patterns/ links to /preview/ (Page not found)
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
  
  
  ```

#### ISSUE-0110 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/topics/aspnet` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /preview/topics/aspnet (Page not found)
  ```

#### ISSUE-0111 · `R.BROKEN_LINK` · warning · pass B
- File: `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/topics/configuration` (Page not found).
- Excerpt:
  ```
  /blog/config-pitfalls/ links to /preview/topics/configuration (Page not found)
  ```

#### ISSUE-0112 · `R.BROKEN_LINK` · warning · pass B · rolled up (6 occurrences)
- Affects 3 pages:
  - `/blog/span-patterns/`
  - `/blog/allocation-traps/`
  - `/blog/config-pitfalls/`
- Total raw occurrences: 6 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/span-patterns/ links to /preview/ (Page not found)
  ```

#### ISSUE-0113 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 3 pages:
  - `/blog/span-patterns/`
  - `/blog/allocation-traps/`
  - `/blog/config-pitfalls/`
- Message: Engine reports broken link to `/preview/rss.xml` (Page not found).
- Excerpt:
  ```
  /blog/span-patterns/ links to /preview/rss.xml (Page not found)
  ```

#### ISSUE-0114 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/span-patterns/`
  - `/blog/allocation-traps/`
- Message: Engine reports broken link to `/preview/topics/dotnet` (Page not found).
- Excerpt:
  ```
  /blog/span-patterns/ links to /preview/topics/dotnet (Page not found)
  ```

#### ISSUE-0115 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `/blog/span-patterns/`
  - `/blog/allocation-traps/`
- Message: Engine reports broken link to `/preview/topics/performance` (Page not found).
- Excerpt:
  ```
  /blog/span-patterns/ links to /preview/topics/performance (Page not found)
  ```

#### ISSUE-0116 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MinimalExample

- Project: `examples/MinimalExample/MinimalExample.csproj`
- Pass A: build failed (exit 1), 6 HTML pages in output, 3.0s
- Pass B: build failed (exit 1), 6 HTML pages in output, 2.9s

#### ISSUE-0117 · `R.BUILD_FAILED` · error · pass A
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

#### ISSUE-0118 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0119 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0120 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0121 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0122 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0123 · `R.BUILD_FAILED` · error · pass B
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

#### ISSUE-0124 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0125 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0126 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0127 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0128 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MultipleContentSourceExample

- Project: `examples/MultipleContentSourceExample/MultipleContentSourceExample.csproj`
- Pass A: build failed (exit 1), 9 HTML pages in output, 3.6s
- Pass B: build failed (exit 1), 9 HTML pages in output, 3.3s

#### ISSUE-0129 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\home-organization-systems\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md
    /docs/indoor-herb-garden/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\indoor-herb-garden\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md
    /docs/coffee-brewing-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\coffee-brewing-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md
    /blog/office-plant-survival-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\office-plant-survival-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md
    /blog/mystery-of-missing-socks/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\mystery-of-missing-socks\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md
  
  WARNINGS
    Markdown content source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content' overlaps a more specific source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content\blog'. Both will discover files under 'blog', producing duplicate TOC entries and output-file races. Add `ExcludePaths = ["blog"]` to the outer source's options so the inner source owns that subtree.
    Markdown content source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content' overlaps a more specific source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content\docs'. Both will discover files under 'docs', producing duplicate TOC entries and output-file races. Add `ExcludePaths = ["docs"]` to the outer source's options so the inner source owns that subtree.
  
  
  ```

#### ISSUE-0130 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/best-pizza-toppings/`
- Message: Engine reported page generation failure for `/blog/best-pizza-toppings/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\best-pizza-toppings\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md

#### ISSUE-0131 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/mystery-of-missing-socks/`
- Message: Engine reported page generation failure for `/blog/mystery-of-missing-socks/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\mystery-of-missing-socks\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md

#### ISSUE-0132 · `R.PAGE_FAILED` · error · pass A
- File: `/blog/office-plant-survival-guide/`
- Message: Engine reported page generation failure for `/blog/office-plant-survival-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\blog\office-plant-survival-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md

#### ISSUE-0133 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/coffee-brewing-guide/`
- Message: Engine reported page generation failure for `/docs/coffee-brewing-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\coffee-brewing-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md

#### ISSUE-0134 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/home-organization-systems/`
- Message: Engine reported page generation failure for `/docs/home-organization-systems/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\home-organization-systems\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md

#### ISSUE-0135 · `R.PAGE_FAILED` · error · pass A
- File: `/docs/indoor-herb-garden/`
- Message: Engine reported page generation failure for `/docs/indoor-herb-garden/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\root\docs\indoor-herb-garden\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md

#### ISSUE-0136 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0137 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\home-organization-systems\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md
    /blog/office-plant-survival-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\office-plant-survival-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md
    /docs/indoor-herb-garden/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\indoor-herb-garden\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md
    /blog/best-pizza-toppings/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\best-pizza-toppings\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md
    /docs/coffee-brewing-guide/
      The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\coffee-brewing-guide\index.html' because it is being used by another process.
      Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md
  
  WARNINGS
    Markdown content source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content' overlaps a more specific source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content\blog'. Both will discover files under 'blog', producing duplicate TOC entries and output-file races. Add `ExcludePaths = ["blog"]` to the outer source's options so the inner source owns that subtree.
    Markdown content source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content' overlaps a more specific source rooted at 'B:\Penn\examples\MultipleContentSourceExample\Content\docs'. Both will discover files under 'docs', producing duplicate TOC entries and output-file races. Add `ExcludePaths = ["docs"]` to the outer source's options so the inner source owns that subtree.
  
  
  ```

#### ISSUE-0138 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/best-pizza-toppings/`
- Message: Engine reported page generation failure for `/blog/best-pizza-toppings/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\best-pizza-toppings\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\best-pizza-toppings.md

#### ISSUE-0139 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/mystery-of-missing-socks/`
- Message: Engine reported page generation failure for `/blog/mystery-of-missing-socks/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\mystery-of-missing-socks\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\mystery-of-missing-socks.md

#### ISSUE-0140 · `R.PAGE_FAILED` · error · pass B
- File: `/blog/office-plant-survival-guide/`
- Message: Engine reported page generation failure for `/blog/office-plant-survival-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\blog\office-plant-survival-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\blog\office-plant-survival-guide.md

#### ISSUE-0141 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/coffee-brewing-guide/`
- Message: Engine reported page generation failure for `/docs/coffee-brewing-guide/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\coffee-brewing-guide\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\coffee-brewing-guide.md

#### ISSUE-0142 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/home-organization-systems/`
- Message: Engine reported page generation failure for `/docs/home-organization-systems/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\home-organization-systems\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\home-organization-systems.md

#### ISSUE-0143 · `R.PAGE_FAILED` · error · pass B
- File: `/docs/indoor-herb-garden/`
- Message: Engine reported page generation failure for `/docs/indoor-herb-garden/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\MultipleContentSourceExample\prefixed\docs\indoor-herb-garden\index.html' because it is being used by another process. Source: B:\Penn\examples\MultipleContentSourceExample\Content\docs\indoor-herb-garden.md

#### ISSUE-0144 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### NorthwindHandbookExample

- Project: `examples/NorthwindHandbookExample/NorthwindHandbookExample.csproj`
- Pass A: built ok, 11 HTML pages in output, 3.0s
- Pass B: built ok, 11 HTML pages in output, 3.1s

#### ISSUE-0145 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0146 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### PrismDocsExample

- Project: `examples/PrismDocsExample/PrismDocsExample.csproj`
- Pass A: built ok, 4 HTML pages in output, 4.4s
- Pass B: built ok, 4 HTML pages in output, 4.2s
- No issues.

### RecipeExample

- Project: `examples/RecipeExample/RecipeExample.csproj`
- Pass A: build failed (exit 1), 7 HTML pages in output, 15.7s
- Pass B: build failed (exit 1), 7 HTML pages in output, 6.5s

#### ISSUE-0147 · `L.BROKEN` · error · pass A · rolled up (21 occurrences)
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

#### ISSUE-0148 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /recipes/chex-mix links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/chex-mix links to / (Page not found)
      /recipes/chili links to / (Page not found)
      /recipes/chili links to / (Page not found)
      /recipes/chili links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/beer-cheese links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/zuppa-toscana links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/chicken-piccata links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
      /recipes/cajun-chicken-pasta links to / (Page not found)
  
  
  ```

#### ISSUE-0149 · `R.PAGE_FAILED` · error · pass A
- File: `/sitemap.xml`
- Message: Engine reported page generation failure for `/sitemap.xml`: HTTP 500 fetching /sitemap.xml

#### ISSUE-0150 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `/recipes/bacon-wrapped-jalapenos`
  - `/recipes/chex-mix`
  - `/recipes/chili`
  - `/recipes/beer-cheese`
  - `/recipes/zuppa-toscana`
  - `/recipes/chicken-piccata`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /recipes/bacon-wrapped-jalapenos links to / (Page not found)
  ```

#### ISSUE-0151 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0152 · `L.BROKEN` · error · pass B · rolled up (21 occurrences)
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

#### ISSUE-0153 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/bacon-wrapped-jalapenos links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chex-mix links to /preview/ (Page not found)
      /recipes/chili links to /preview/ (Page not found)
      /recipes/chili links to /preview/ (Page not found)
      /recipes/chili links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/chicken-piccata links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/beer-cheese links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
      /recipes/cajun-chicken-pasta links to /preview/ (Page not found)
  
  
  ```

#### ISSUE-0154 · `R.PAGE_FAILED` · error · pass B
- File: `/sitemap.xml`
- Message: Engine reported page generation failure for `/sitemap.xml`: HTTP 500 fetching /sitemap.xml

#### ISSUE-0155 · `R.BROKEN_LINK` · warning · pass B · rolled up (21 occurrences)
- Affects 7 pages:
  - `/recipes/zuppa-toscana`
  - `/recipes/bacon-wrapped-jalapenos`
  - `/recipes/chex-mix`
  - `/recipes/chili`
  - `/recipes/chicken-piccata`
  - `/recipes/beer-cheese`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /recipes/zuppa-toscana links to /preview/ (Page not found)
  ```

#### ISSUE-0156 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### RoslynIntegrationExample

- Project: `examples/RoslynIntegrationExample/RoslynIntegrationExample.csproj`
- Pass A: build failed (exit 1), 6 HTML pages in output, 12.5s
- Pass B: build failed (exit 1), 6 HTML pages in output, 12.6s

#### ISSUE-0157 · `R.BUILD_FAILED` · error · pass A
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
  Build Complete — 8 pages in 10.2s
    8 pages generated
  
  WARNINGS
    2 broken links found:
      /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  
  
  ```

#### ISSUE-0158 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0159 · `L.BROKEN` · warning · pass A
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0160 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0161 · `R.BROKEN_LINK` · warning · pass A
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0162 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0163 · `R.BUILD_FAILED` · error · pass B
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
  Build Complete — 8 pages in 10.5s
    8 pages generated
  
  WARNINGS
    2 broken links found:
      /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
      /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  
  
  ```

#### ISSUE-0164 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` → missing target `/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg" alt="Unsplash Photo by Dan Cristian Padure">
  ```

#### ISSUE-0165 · `L.BROKEN` · warning · pass B
- File: `sub-folder/sample-post/index.html`
- Selector: `main.flex-1 > article > div.prose > p > img`
- Message: Image src `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` → missing target `/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg`.
- Excerpt:
  ```
  <img src="/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg" alt="Unsplash Photo by Kelly Sikkema">
  ```

#### ISSUE-0166 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/media/dan-cristian-padure-8cxJzVpGKk8-unsplash.jpg (Page not found)
  ```

#### ISSUE-0167 · `R.BROKEN_LINK` · warning · pass B
- File: `/sub-folder/sample-post/`
- Message: Engine reports broken link to `/preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg` (Page not found).
- Excerpt:
  ```
  /sub-folder/sample-post/ links to /preview/sub-folder/kelly-sikkema-rNdkGDOPJLc-unsplash.jpg (Page not found)
  ```

#### ISSUE-0168 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SearchExample

- Project: `examples/SearchExample/SearchExample.csproj`
- Pass A: built ok, 1001 HTML pages in output, 129.0s
- Pass B: built ok, 1001 HTML pages in output, 131.2s
- No issues.

### SpaNavigationExample

- Project: `examples/SpaNavigationExample/SpaNavigationExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 2.6s
- Pass B: built ok, 5 HTML pages in output, 2.5s

#### ISSUE-0169 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0170 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpaNavigationTutorialExample

- Project: `examples/SpaNavigationTutorialExample/SpaNavigationTutorialExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.7s
- Pass B: built ok, 6 HTML pages in output, 2.6s

#### ISSUE-0171 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0172 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpectreConsoleExample

- Project: `examples/SpectreConsoleExample/SpectreConsoleExample.csproj`
- Pass A: build failed (exit 1), 79 HTML pages in output, 4.1s
- Pass B: build failed (exit 1), 79 HTML pages in output, 4.5s

#### ISSUE-0173 · `L.BROKEN` · error · pass A · rolled up (237 occurrences)
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

#### ISSUE-0174 · `L.BROKEN` · error · pass A · rolled up (163 occurrences)
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

#### ISSUE-0175 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /cli/how--to/intercepting-command-execution/ links to /blog (Page not found)
      /cli/how--to/intercepting-command-execution/ links to / (Page not found)
      /cli/how--to/intercepting-command-execution/ links to /blog (Page not found)
      /console/widgets/padder/ links to / (Page not found)
      /console/widgets/padder/ links to / (Page not found)
      /console/widgets/padder/ links to /blog (Page not found)
      /console/widgets/padder/ links to / (Page not found)
      /console/widgets/padder/ links to /blog (Page not found)
      /console/widgets/panel/ links to / (Page not found)
      /console/widgets/panel/ links to / (Page not found)
      /console/widgets/panel/ links to /blog (Page not found)
      /console/widgets/panel/ links to / (Page not found)
      /console/widgets/panel/ links to /blog (Page not found)
      /console/how-to/displaying-tables-and-trees/ links to / (Page not found)
      /console/how-to/displaying-tables-and-trees/ links to / (Page not found)
      /console/how-to/displaying-tables-and-trees/ links to /blog (Page not found)
      /console/how-to/displaying-tables-and-trees/ links to / (Page not found)
      /console/how-to/displaying-tables-and-trees/ links to /blog (Page not found)
  
  
  ```

#### ISSUE-0176 · `R.BROKEN_LINK` · warning · pass A · rolled up (237 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-0-search-positioning-progress/`
  - `/cli/reference/api-reference/`
  - `/console/widgets/table/`
  - `/console/widgets/layout/`
  - `/console/live/live-display/`
  - `/cli/tutorials/quick-start-your-first-cli-app/`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Message: Engine reports broken link to `/` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-0-search-positioning-progress/ links to / (Page not found)
  ```

#### ISSUE-0177 · `R.BROKEN_LINK` · warning · pass A · rolled up (163 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-0-search-positioning-progress/`
  - `/cli/reference/api-reference/`
  - `/console/widgets/table/`
  - `/console/widgets/layout/`
  - `/console/live/live-display/`
  - `/cli/tutorials/quick-start-your-first-cli-app/`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Message: Engine reports broken link to `/blog` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-0-search-positioning-progress/ links to /blog (Page not found)
  ```

#### ISSUE-0178 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0179 · `L.BROKEN` · error · pass B · rolled up (237 occurrences)
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

#### ISSUE-0180 · `L.BROKEN` · error · pass B · rolled up (163 occurrences)
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

#### ISSUE-0181 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /console/how-to/testing-console-output/ links to /preview/blog (Page not found)
      /console/how-to/testing-console-output/ links to /preview/ (Page not found)
      /console/how-to/testing-console-output/ links to /preview/blog (Page not found)
      /console/live/status/ links to /preview/ (Page not found)
      /console/live/status/ links to /preview/ (Page not found)
      /console/live/status/ links to /preview/blog (Page not found)
      /console/live/status/ links to /preview/ (Page not found)
      /console/live/status/ links to /preview/blog (Page not found)
      /console/how-to/styling-text-with-markup-and-color/ links to /preview/ (Page not found)
      /console/how-to/styling-text-with-markup-and-color/ links to /preview/ (Page not found)
      /console/how-to/styling-text-with-markup-and-color/ links to /preview/blog (Page not found)
      /console/how-to/styling-text-with-markup-and-color/ links to /preview/ (Page not found)
      /console/how-to/styling-text-with-markup-and-color/ links to /preview/blog (Page not found)
      /console/explanation/extending-with-custom-renderables/ links to /preview/ (Page not found)
      /console/explanation/extending-with-custom-renderables/ links to /preview/ (Page not found)
      /console/explanation/extending-with-custom-renderables/ links to /preview/blog (Page not found)
      /console/explanation/extending-with-custom-renderables/ links to /preview/ (Page not found)
      /console/explanation/extending-with-custom-renderables/ links to /preview/blog (Page not found)
  
  
  ```

#### ISSUE-0182 · `R.BROKEN_LINK` · warning · pass B · rolled up (237 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/console/widgets/table/`
  - `/console/widgets/panel/`
  - `/console/widgets/calendar/`
  - `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
  - `/cli/how--to/handling-errors-and-exit-codes/`
  - … (+73 more)
- Total raw occurrences: 237 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to /preview/ (Page not found)
  ```

#### ISSUE-0183 · `R.BROKEN_LINK` · warning · pass B · rolled up (163 occurrences)
- Affects 79 pages:
  - `/blog/spectre-console-0-49-1-version-handling-refinements/`
  - `/console/widgets/table/`
  - `/console/widgets/panel/`
  - `/console/widgets/calendar/`
  - `/console/tutorials/interactive-prompt-and-dashboard-tutorial/`
  - `/cli/how--to/handling-errors-and-exit-codes/`
  - … (+73 more)
- Total raw occurrences: 163 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/blog` (Page not found).
- Excerpt:
  ```
  /blog/spectre-console-0-49-1-version-handling-refinements/ links to /preview/blog (Page not found)
  ```

#### ISSUE-0184 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### TempoDocsExample

- Project: `examples/TempoDocsExample/TempoDocsExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 3.1s
- Pass B: built ok, 5 HTML pages in output, 3.1s
- No issues.

### UserInterfaceExample

- Project: `examples/UserInterfaceExample/UserInterfaceExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.6s
- Pass B: built ok, 6 HTML pages in output, 2.6s

#### ISSUE-0185 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0186 · `B.MISSING_BODY_ATTR` · warning · pass B
- File: `index.html`
- Selector: `body`
- Message: Page body missing `data-base-url` attribute (expected `/preview/`).

#### ISSUE-0187 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### YogaStudioExample

- Project: `examples/YogaStudioExample/YogaStudioExample.csproj`
- Pass A: build failed (exit 1), 18 HTML pages in output, 4.0s
- Pass B: build failed (exit 1), 18 HTML pages in output, 3.1s

#### ISSUE-0188 · `L.BROKEN` · error · pass A
- File: `blog/index.html`
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/gen-z/blog/` → missing target `/gen-z/blog/`.
- Excerpt:
  ```
  <a href="/gen-z/blog/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0189 · `L.BROKEN` · error · pass A · rolled up (22 occurrences)
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

#### ISSUE-0190 · `L.BROKEN` · error · pass A · rolled up (4 occurrences)
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

#### ISSUE-0191 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
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

#### ISSUE-0192 · `L.BROKEN` · error · pass A · rolled up (3 occurrences)
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

#### ISSUE-0193 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/alex-rivera` → missing target `/instructors/alex-rivera`.
- Excerpt:
  ```
  <a href="/instructors/alex-rivera" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font...
  ```

#### ISSUE-0194 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/james-okafor` → missing target `/instructors/james-okafor`.
- Excerpt:
  ```
  <a href="/instructors/james-okafor" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl fon...
  ```

#### ISSUE-0195 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/maya-chen` → missing target `/instructors/maya-chen`.
- Excerpt:
  ```
  <a href="/instructors/maya-chen" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font-b...
  ```

#### ISSUE-0196 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/priya-sharma` → missing target `/instructors/priya-sharma`.
- Excerpt:
  ```
  <a href="/instructors/priya-sharma" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl fon...
  ```

#### ISSUE-0197 · `L.BROKEN` · error · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/instructors/sam-tanaka` → missing target `/instructors/sam-tanaka`.
- Excerpt:
  ```
  <a href="/instructors/sam-tanaka" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2xl font-...
  ```

#### ISSUE-0198 · `R.BUILD_FAILED` · error · pass A
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /about/ links to /js/faq.js (Page not found)
      /gen-z/about/ links to /gen-z/ (Page not found)
      /gen-z/about/ links to /gen-z/ (Page not found)
      /gen-z/about/ links to /gen-z/schedule (Page not found)
      /gen-z/about/ links to /gen-z/instructors (Page not found)
      /gen-z/about/ links to /gen-z/blog (Page not found)
      /gen-z/about/ links to /gen-z/ (Page not found)
      /gen-z/about/ links to /gen-z/schedule (Page not found)
      /gen-z/about/ links to /gen-z/instructors (Page not found)
      /gen-z/about/ links to /gen-z/blog (Page not found)
      /gen-z/about/ links to /gen-z/schedule (Page not found)
      /gen-z/about/ links to /gen-z/instructors (Page not found)
      /gen-z/about/ links to /gen-z/blog (Page not found)
      /gen-z/about/ links to /js/search.js (Page not found)
      /gen-z/about/ links to /js/faq.js (Page not found)
      /blog/ links to /gen-z/blog/ (Page not found)
      /blog/ links to /js/search.js (Page not found)
      /blog/ links to /js/faq.js (Page not found)
  
  
  ```

#### ISSUE-0199 · `R.PAGE_FAILED` · error · pass A
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\root\about\index.html' because it is being used by another process.

#### ISSUE-0200 · `R.PAGE_FAILED` · error · pass A
- File: `/contact/`
- Message: Engine reported page generation failure for `/contact/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\root\contact\index.html' because it is being used by another process. Source: B:\Penn\examples\YogaStudioExample\Content\pages\contact.md

#### ISSUE-0201 · `R.BROKEN_LINK` · warning · pass A
- File: `/gen-z/blog/breathing-techniques/`
- Message: Engine reports broken link to `/gen-z/schedule/sun-breathwork` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/breathing-techniques/ links to /gen-z/schedule/sun-breathwork (Page not found)
  ```

#### ISSUE-0202 · `R.BROKEN_LINK` · warning · pass A
- File: `/gen-z/blog/morning-yoga-routine/`
- Message: Engine reports broken link to `/gen-z/schedule/mon-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /gen-z/blog/morning-yoga-routine/ links to /gen-z/schedule/mon-vinyasa-morning (Page not found)
  ```

#### ISSUE-0203 · `R.BROKEN_LINK` · warning · pass A · rolled up (22 occurrences)
- Affects 8 pages:
  - `/gen-z/pricing/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/contact/`
  - … (+2 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/` (Page not found).
- Excerpt:
  ```
  /gen-z/pricing/ links to /gen-z/ (Page not found)
  ```

#### ISSUE-0204 · `R.BROKEN_LINK` · warning · pass A · rolled up (24 occurrences)
- Affects 7 pages:
  - `/gen-z/pricing/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 24 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/blog` (Page not found).
- Excerpt:
  ```
  /gen-z/pricing/ links to /gen-z/blog (Page not found)
  ```

#### ISSUE-0205 · `R.BROKEN_LINK` · warning · pass A · rolled up (21 occurrences)
- Affects 7 pages:
  - `/gen-z/pricing/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 21 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/instructors` (Page not found).
- Excerpt:
  ```
  /gen-z/pricing/ links to /gen-z/instructors (Page not found)
  ```

#### ISSUE-0206 · `R.BROKEN_LINK` · warning · pass A · rolled up (22 occurrences)
- Affects 7 pages:
  - `/gen-z/pricing/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/contact/`
  - … (+1 more)
- Total raw occurrences: 22 (some pages emit multiple matches)
- Message: Engine reports broken link to `/gen-z/schedule` (Page not found).
- Excerpt:
  ```
  /gen-z/pricing/ links to /gen-z/schedule (Page not found)
  ```

#### ISSUE-0207 · `R.BROKEN_LINK` · warning · pass A · rolled up (20 occurrences)
- Affects 18 pages:
  - `/gen-z/pricing/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/contact/`
  - … (+12 more)
- Total raw occurrences: 20 (some pages emit multiple matches)
- Message: Engine reports broken link to `/js/faq.js` (Page not found).
- Excerpt:
  ```
  /gen-z/pricing/ links to /js/faq.js (Page not found)
  ```

#### ISSUE-0208 · `R.BROKEN_LINK` · warning · pass A · rolled up (20 occurrences)
- Affects 18 pages:
  - `/gen-z/pricing/`
  - `/gen-z/blog/yoga-for-beginners/`
  - `/gen-z/blog/morning-yoga-routine/`
  - `/gen-z/blog/breathing-techniques/`
  - `/gen-z/faq/`
  - `/gen-z/contact/`
  - … (+12 more)
- Total raw occurrences: 20 (some pages emit multiple matches)
- Message: Engine reports broken link to `/js/search.js` (Page not found).
- Excerpt:
  ```
  /gen-z/pricing/ links to /js/search.js (Page not found)
  ```

#### ISSUE-0209 · `R.BROKEN_LINK` · warning · pass A
- File: `/instructors/`
- Message: Engine reports broken link to `/gen-z/instructors/` (Page not found).
- Excerpt:
  ```
  /instructors/ links to /gen-z/instructors/ (Page not found)
  ```

#### ISSUE-0210 · `R.BROKEN_LINK` · warning · pass A · rolled up (2 occurrences)
- Affects 2 pages:
  - `/instructors/`
  - `/`
- Message: Engine reports broken link to `/instructors/maya-chen` (Page not found).
- Excerpt:
  ```
  /instructors/ links to /instructors/maya-chen (Page not found)
  ```

#### ISSUE-0211 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0212 · `L.BROKEN` · error · pass B
- File: `blog/index.html`
- Selector: `div.flex > div.flex > details.relative > div.absolute > a.block`
- Message: Link `/preview/gen-z/blog/` → missing target `/gen-z/blog/`.
- Excerpt:
  ```
  <a href="/preview/gen-z/blog/" data-spa-reload="" data-locale="gen-z" class="block px-3 py-1.5 text-sm text-base-700 dark:text-base-300 hover:bg-base-100 dark:hover:bg-base-800">Gen Z</a>
  ```

#### ISSUE-0213 · `L.BROKEN` · error · pass B · rolled up (22 occurrences)
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

#### ISSUE-0214 · `L.BROKEN` · error · pass B · rolled up (4 occurrences)
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

#### ISSUE-0215 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
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

#### ISSUE-0216 · `L.BROKEN` · error · pass B · rolled up (3 occurrences)
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

#### ISSUE-0217 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/alex-rivera` → missing target `/instructors/alex-rivera`.
- Excerpt:
  ```
  <a href="/preview/instructors/alex-rivera" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-...
  ```

#### ISSUE-0218 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/james-okafor` → missing target `/instructors/james-okafor`.
- Excerpt:
  ```
  <a href="/preview/instructors/james-okafor" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text...
  ```

#### ISSUE-0219 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/maya-chen` → missing target `/instructors/maya-chen`.
- Excerpt:
  ```
  <a href="/preview/instructors/maya-chen" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2x...
  ```

#### ISSUE-0220 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/priya-sharma` → missing target `/instructors/priya-sharma`.
- Excerpt:
  ```
  <a href="/preview/instructors/priya-sharma" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text...
  ```

#### ISSUE-0221 · `L.BROKEN` · error · pass B · rolled up (2 occurrences)
- Affects 2 pages:
  - `index.html`
  - `instructors/index.html`
- Selector: `main.flex-1 > section.yoga-section-alt > div.yoga-container > div.grid > a.text-center`
- Message: Link `/preview/instructors/sam-tanaka` → missing target `/instructors/sam-tanaka`.
- Excerpt:
  ```
  <a href="/preview/instructors/sam-tanaka" class="text-center group"><div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary-200 to-accent-200 dark:from-primary-800 dark:to-accent-800 flex items-center justify-center text-2...
  ```

#### ISSUE-0222 · `R.BUILD_FAILED` · error · pass B
- Message: Build exited with code 1. Validators still ran against whatever made it to disk.
- Excerpt:
  ```
      /gen-z/blog/morning-yoga-routine/ links to /preview/gen-z/blog (Page not found)
      /gen-z/blog/morning-yoga-routine/ links to /preview/js/search.js (Page not found)
      /gen-z/blog/morning-yoga-routine/ links to /preview/js/faq.js (Page not found)
      /pricing/ links to /preview/js/search.js (Page not found)
      /pricing/ links to /preview/js/faq.js (Page not found)
      /instructors/ links to /preview/gen-z/instructors/ (Page not found)
      /instructors/ links to /preview/instructors/maya-chen (Page not found)
      /instructors/ links to /preview/instructors/james-okafor (Page not found)
      /instructors/ links to /preview/instructors/priya-sharma (Page not found)
      /instructors/ links to /preview/instructors/alex-rivera (Page not found)
      /instructors/ links to /preview/instructors/sam-tanaka (Page not found)
      /instructors/ links to /preview/js/search.js (Page not found)
      /instructors/ links to /preview/js/faq.js (Page not found)
      /contact/ links to /preview/js/search.js (Page not found)
      /contact/ links to /preview/js/faq.js (Page not found)
      /blog/ links to /preview/gen-z/blog/ (Page not found)
      /blog/ links to /preview/js/search.js (Page not found)
      /blog/ links to /preview/js/faq.js (Page not found)
  
  
  ```

#### ISSUE-0223 · `R.PAGE_FAILED` · error · pass B
- File: `/about/`
- Message: Engine reported page generation failure for `/about/`: The process cannot access the file 'B:\Penn\tmp\validate-examples\YogaStudioExample\prefixed\about\index.html' because it is being used by another process. Source: B:\Penn\examples\YogaStudioExample\Content\pages\about.md

#### ISSUE-0224 · `R.BROKEN_LINK` · warning · pass B
- File: `/schedule/`
- Message: Engine reports broken link to `/preview/gen-z/schedule/` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/gen-z/schedule/ (Page not found)
  ```

#### ISSUE-0225 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/mon-power-noon` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/mon-power-noon (Page not found)
  ```

#### ISSUE-0226 · `R.BROKEN_LINK` · warning · pass B · rolled up (4 occurrences)
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

#### ISSUE-0227 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/mon-yin-evening` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/mon-yin-evening (Page not found)
  ```

#### ISSUE-0228 · `R.BROKEN_LINK` · warning · pass B
- File: `/schedule/`
- Message: Engine reports broken link to `/preview/schedule/thu-power-morning` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/thu-power-morning (Page not found)
  ```

#### ISSUE-0229 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 2 pages:
  - `/schedule/`
  - `/`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/tue-ashtanga` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/tue-ashtanga (Page not found)
  ```

#### ISSUE-0230 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/tue-hot-yoga` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/tue-hot-yoga (Page not found)
  ```

#### ISSUE-0231 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/wed-meditation` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/wed-meditation (Page not found)
  ```

#### ISSUE-0232 · `R.BROKEN_LINK` · warning · pass B · rolled up (2 occurrences)
- Affects 1 page:
  - `/schedule/`
- Total raw occurrences: 2 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/wed-restorative` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/wed-restorative (Page not found)
  ```

#### ISSUE-0233 · `R.BROKEN_LINK` · warning · pass B · rolled up (3 occurrences)
- Affects 2 pages:
  - `/schedule/`
  - `/`
- Total raw occurrences: 3 (some pages emit multiple matches)
- Message: Engine reports broken link to `/preview/schedule/wed-vinyasa-morning` (Page not found).
- Excerpt:
  ```
  /schedule/ links to /preview/schedule/wed-vinyasa-morning (Page not found)
  ```

#### ISSUE-0234 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

_Additional issues not shown (truncated to 10 samples per code):_

- `L.BROKEN` (pass A): 21 more similar issues
- `R.BROKEN_LINK` (pass A): 23 more similar issues
- `L.BROKEN` (pass B): 21 more similar issues
- `R.BROKEN_LINK` (pass B): 23 more similar issues


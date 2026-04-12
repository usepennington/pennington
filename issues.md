# Pennington Examples Validation Report

Generated: 2026-04-12T00:41:49Z
Examples discovered: 20
Examples built successfully: 20 (pass A) / 20 (pass B)
Total issues: 0 errors, 9 warnings, 28 info
Shown in report: 37 samples (capped at 10 per example/pass/code; full counts preserved in cross-example patterns below).

## How to read this report

- **Issue codes** group by category. `L.*` = link integrity, `T.*` = table of contents, `B.*` = base URL rewriting, `S.*` = search index, `M.*` = llms.txt, `X.*` = sitemap, `P.*` = SPA data, `R.*` = engine's own build report.
- **Rollup**: identical root causes (e.g. the same broken link appearing in every page's nav) are collapsed into a single issue marked `rolled up (N occurrences)` with a sample of affected pages. Cross-example pattern counts still show the raw underlying totals so the true scale is visible.
- **Cross-example patterns** at the top roll up counts by code across all examples. Codes appearing in many examples point to engine bugs; codes appearing in one example are usually example bugs.
- **Per-example sections** contain at most 10 representative samples per (pass, code). A truncation footer lists how many distinct groups were omitted beyond the cap.
- **Pass A** builds with base URL `/`. **Pass B** builds with base URL `/preview/` to exercise `BaseUrlRewritingProcessor`.

## Summary table

| Example | Pass A | Pass B | Errors | Warnings | Info |
| ------- | ------ | ------ | ------ | -------- | ---- |
| AlexBlogExample | ok | ok | 0 | 2 | 2 |
| BeaconDocsExample | ok | ok | 0 | 0 | 0 |
| BlogExample | ok | ok | 0 | 2 | 2 |
| ForgePortalExample | ok | ok | 0 | 0 | 2 |
| LocalizationExample | ok | ok | 0 | 0 | 0 |
| LocalizationTutorialExample | ok | ok | 0 | 0 | 0 |
| MaraBlogExample | ok | ok | 0 | 2 | 2 |
| MinimalExample | ok | ok | 0 | 0 | 2 |
| MultipleContentSourceExample | ok | ok | 0 | 0 | 2 |
| NorthwindHandbookExample | ok | ok | 0 | 0 | 2 |
| PrismDocsExample | ok | ok | 0 | 0 | 0 |
| RecipeExample | ok | ok | 0 | 0 | 2 |
| RoslynIntegrationExample | ok | ok | 0 | 0 | 2 |
| SearchExample | ok | ok | 0 | 0 | 0 |
| SpaNavigationExample | ok | ok | 0 | 0 | 2 |
| SpaNavigationTutorialExample | ok | ok | 0 | 0 | 2 |
| SpectreConsoleExample | ok | ok | 0 | 0 | 2 |
| TempoDocsExample | ok | ok | 0 | 0 | 0 |
| UserInterfaceExample | ok | ok | 0 | 1 | 2 |
| YogaStudioExample | ok | ok | 0 | 2 | 2 |

## Cross-example patterns

Counts below reflect **raw** issue totals (not the truncated samples shown in per-example sections). Codes that appear in multiple examples are likely engine bugs; codes unique to one example are likely example bugs.

- `S.UNDERCOUNT` (warning) — 8 occurrences across 4 examples: AlexBlogExample, BlogExample, MaraBlogExample, YogaStudioExample
- `B.MISSING_BODY_ATTR` (warning) — 1 occurrence across 1 example: UserInterfaceExample
- `M.MISSING` (info) — 28 occurrences across 14 examples: AlexBlogExample, BlogExample, ForgePortalExample, MaraBlogExample, MinimalExample, MultipleContentSourceExample, … (+8 more)

## Issues by example

### AlexBlogExample

- Project: `examples/AlexBlogExample/AlexBlogExample.csproj`
- Pass A: built ok, 11 HTML pages in output, 3.2s
- Pass B: built ok, 11 HTML pages in output, 3.2s

#### ISSUE-0001 · `S.UNDERCOUNT` · warning · pass A
- File: `search-index.json`
- Message: search-index has 3 entries but output has 11 HTML pages — possible silent drops.

#### ISSUE-0002 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0003 · `S.UNDERCOUNT` · warning · pass B
- File: `search-index.json`
- Message: search-index has 3 entries but output has 11 HTML pages — possible silent drops.

#### ISSUE-0004 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### BeaconDocsExample

- Project: `examples/BeaconDocsExample/BeaconDocsExample.csproj`
- Pass A: built ok, 10 HTML pages in output, 3.6s
- Pass B: built ok, 10 HTML pages in output, 3.7s
- No issues.

### BlogExample

- Project: `examples/BlogExample/BlogExample.csproj`
- Pass A: built ok, 36 HTML pages in output, 3.5s
- Pass B: built ok, 36 HTML pages in output, 3.3s

#### ISSUE-0005 · `S.UNDERCOUNT` · warning · pass A
- File: `search-index.json`
- Message: search-index has 7 entries but output has 36 HTML pages — possible silent drops.

#### ISSUE-0006 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0007 · `S.UNDERCOUNT` · warning · pass B
- File: `search-index.json`
- Message: search-index has 7 entries but output has 36 HTML pages — possible silent drops.

#### ISSUE-0008 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### ForgePortalExample

- Project: `examples/ForgePortalExample/ForgePortalExample.csproj`
- Pass A: built ok, 11 HTML pages in output, 2.9s
- Pass B: built ok, 11 HTML pages in output, 2.9s

#### ISSUE-0009 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0010 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### LocalizationExample

- Project: `examples/LocalizationExample/LocalizationExample.csproj`
- Pass A: built ok, 26 HTML pages in output, 3.7s
- Pass B: built ok, 26 HTML pages in output, 3.7s
- No issues.

### LocalizationTutorialExample

- Project: `examples/LocalizationTutorialExample/LocalizationTutorialExample.csproj`
- Pass A: built ok, 7 HTML pages in output, 3.5s
- Pass B: built ok, 7 HTML pages in output, 3.8s
- No issues.

### MaraBlogExample

- Project: `examples/MaraBlogExample/MaraBlogExample.csproj`
- Pass A: built ok, 11 HTML pages in output, 3.4s
- Pass B: built ok, 11 HTML pages in output, 3.7s

#### ISSUE-0011 · `S.UNDERCOUNT` · warning · pass A
- File: `search-index.json`
- Message: search-index has 3 entries but output has 11 HTML pages — possible silent drops.

#### ISSUE-0012 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0013 · `S.UNDERCOUNT` · warning · pass B
- File: `search-index.json`
- Message: search-index has 3 entries but output has 11 HTML pages — possible silent drops.

#### ISSUE-0014 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MinimalExample

- Project: `examples/MinimalExample/MinimalExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.9s
- Pass B: built ok, 6 HTML pages in output, 2.9s

#### ISSUE-0015 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0016 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### MultipleContentSourceExample

- Project: `examples/MultipleContentSourceExample/MultipleContentSourceExample.csproj`
- Pass A: built ok, 9 HTML pages in output, 3.0s
- Pass B: built ok, 9 HTML pages in output, 3.0s

#### ISSUE-0017 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0018 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### NorthwindHandbookExample

- Project: `examples/NorthwindHandbookExample/NorthwindHandbookExample.csproj`
- Pass A: built ok, 11 HTML pages in output, 3.0s
- Pass B: built ok, 11 HTML pages in output, 2.9s

#### ISSUE-0019 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0020 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### PrismDocsExample

- Project: `examples/PrismDocsExample/PrismDocsExample.csproj`
- Pass A: built ok, 4 HTML pages in output, 4.2s
- Pass B: built ok, 4 HTML pages in output, 4.2s
- No issues.

### RecipeExample

- Project: `examples/RecipeExample/RecipeExample.csproj`
- Pass A: built ok, 8 HTML pages in output, 16.2s
- Pass B: built ok, 8 HTML pages in output, 15.7s

#### ISSUE-0021 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0022 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### RoslynIntegrationExample

- Project: `examples/RoslynIntegrationExample/RoslynIntegrationExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 13.3s
- Pass B: built ok, 6 HTML pages in output, 11.9s

#### ISSUE-0023 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0024 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SearchExample

- Project: `examples/SearchExample/SearchExample.csproj`
- Pass A: built ok, 1001 HTML pages in output, 126.0s
- Pass B: built ok, 1001 HTML pages in output, 125.1s
- No issues.

### SpaNavigationExample

- Project: `examples/SpaNavigationExample/SpaNavigationExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 2.5s
- Pass B: built ok, 5 HTML pages in output, 2.5s

#### ISSUE-0025 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0026 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpaNavigationTutorialExample

- Project: `examples/SpaNavigationTutorialExample/SpaNavigationTutorialExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.6s
- Pass B: built ok, 6 HTML pages in output, 2.6s

#### ISSUE-0027 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0028 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### SpectreConsoleExample

- Project: `examples/SpectreConsoleExample/SpectreConsoleExample.csproj`
- Pass A: built ok, 81 HTML pages in output, 4.2s
- Pass B: built ok, 81 HTML pages in output, 4.8s

#### ISSUE-0029 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0030 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### TempoDocsExample

- Project: `examples/TempoDocsExample/TempoDocsExample.csproj`
- Pass A: built ok, 5 HTML pages in output, 3.1s
- Pass B: built ok, 5 HTML pages in output, 3.1s
- No issues.

### UserInterfaceExample

- Project: `examples/UserInterfaceExample/UserInterfaceExample.csproj`
- Pass A: built ok, 6 HTML pages in output, 2.7s
- Pass B: built ok, 6 HTML pages in output, 2.7s

#### ISSUE-0031 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0032 · `B.MISSING_BODY_ATTR` · warning · pass B
- File: `index.html`
- Selector: `body`
- Message: Page body missing `data-base-url` attribute (expected `/preview/`).

#### ISSUE-0033 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

### YogaStudioExample

- Project: `examples/YogaStudioExample/YogaStudioExample.csproj`
- Pass A: built ok, 66 HTML pages in output, 2.7s
- Pass B: built ok, 66 HTML pages in output, 2.8s

#### ISSUE-0034 · `S.UNDERCOUNT` · warning · pass A
- File: `search-index.json`
- Message: search-index has 6 entries but output has 66 HTML pages — possible silent drops.

#### ISSUE-0035 · `M.MISSING` · info · pass A
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).

#### ISSUE-0036 · `S.UNDERCOUNT` · warning · pass B
- File: `search-index.json`
- Message: search-index has 6 entries but output has 66 HTML pages — possible silent drops.

#### ISSUE-0037 · `M.MISSING` · info · pass B
- File: `llms.txt`
- Message: No llms.txt present (may be intentional for this example).


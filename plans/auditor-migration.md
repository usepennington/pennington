# Auditor migration plan

Migrate Pennington's existing build-time checks into `IBuildAuditor`
implementations so they surface in the dev diagnostic overlay (per-page) and
the static build report (site-wide) from a single source.

Phases are independent. Each is meant to be picked up in a fresh agent
context — read this file's preamble, the phase, and the cited files; execute;
verify; mark complete.

---

## Background

Pennington has a build-auditor seam (`IBuildAuditor`) shipped in
`Pennington.Generation`. Auditors implement
`Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext, CancellationToken)`
and emit diagnostics that flow through `AuditCache` to two surfaces:

- **Dev mode (`dotnet run`).** `AuditRunner` is a hosted service that primes
  the cache at startup and re-runs every registered auditor on
  `IFileWatcher.SubscribeToChanges`. `AuditDiagnosticProcessor`
  (`IResponseProcessor`, Order=25) filters cached diagnostics to the current
  request route and pushes them into `DiagnosticContext`, which the existing
  diagnostic overlay (`DiagnosticOverlayProcessor`, Order=30) renders.
- **Build mode (`dotnet run -- build`).**
  `OutputGenerationService.GenerateAsync` reads the same `IAuditCache`
  snapshot after page generation and copies it into `BuildReportBuilder`.

Single classification engine, two surfaces. Several existing build-time
checks predate the seam and live as inline blocks in `OutputGenerationService`
or as separate services that only emit diagnostics at build time — this plan
migrates them so they get the dev overlay surface for free.

---

## Canonical reference

The translation auditor is the architectural template. Read these before
starting any phase:

- `src/Pennington.TranslationAudit/TranslationAuditor.cs` — `IBuildAuditor` impl
- `src/Pennington.TranslationAudit/TranslationAuditOptions.cs` — options class
- `src/Pennington.TranslationAudit/TranslationAuditExtensions.cs` — DI registration
- `src/Pennington/Generation/IBuildAuditor.cs` — the interface
- `src/Pennington/Generation/BuildAuditContext.cs` — what the auditor receives
- `src/Pennington/Generation/AuditRunner.cs` — runner & cache lifecycle
- `examples/BeyondTranslationAuditExample/` — verification harness

---

## Architectural invariants

Every auditor must satisfy these:

1. **Code identifier** — stable, dotted, lowercased (e.g. `content.overlap`).
   Used in the diagnostic source label as `"<code>/<bucket>/<context>"`.
2. **Pure read-only** — no file writes, no network calls, no side effects.
   Audits run on every content change in dev mode; non-determinism breaks the
   overlay.
3. **TOC-level by default** — operate on `BuildAuditContext.Pages`. That list
   is locale-aware and has drafts/redirects already filtered. For checks that
   need rendered HTML, see Phase 3.
4. **DI lifetime: transient** —
   `services.AddTransient<IBuildAuditor, MyAuditor>()`. Singletons capture
   stale dependencies; auditors are cheap to instantiate.
5. **Per-route diagnostics carry a Route.** `BuildDiagnostic.Route` set so
   `AuditDiagnosticProcessor` can filter by request path. Site-wide warnings
   pass null and are visible only in the startup console line and the build
   report.
6. **Failures isolated.** `AuditRunner` already catches per-auditor exceptions.
   Auditors should still degrade gracefully — return an empty list on bad
   input rather than throw.
7. **Repo style** — file-scoped namespace, xmldoc on every public member,
   records for data, `ImmutableList<T>` for collections. See `CLAUDE.md`.
8. **Where the code lives.** Migrations of existing core checks land in
   `src/Pennington/` (no new package). Optional packages like
   `Pennington.TranslationAudit` are reserved for things with heavy
   dependencies (LibGit2Sharp, etc.).

---

## Migration scope

| Check | Phase | Status |
|---|---|---|
| `MarkdownSourceOverlapDetector` | 1 | complete |
| Xref-resolution audit (new) | 2 | pending |
| `IRenderedAuditor` seam (decision) | 3 | pending |
| `LinkVerificationService` | 4 | pending |

Out of scope:

- **Duplicate-route detection** (currently inline in `OutputGenerationService`
  Phase 1). Stays inline because the dedup logic has a side effect — it
  removes the duplicate from the discovery list before Phase 6's parallel
  fetcher races. The diagnostic emission could move to an auditor, but the
  side effect can't, so splitting them creates two places that need to agree.
  Defer until a real reason appears.
- **Front-matter schema validation.** Out of scope until a schema definition
  exists in the codebase.

---

## Phase 1 — Migrate MarkdownSourceOverlapDetector

**Goal.** Move overlap warnings from the inline Phase 0 block in
`OutputGenerationService` into a dedicated `IBuildAuditor`. Net behavior:
identical warning text, now visible in the dev overlay too.

**Files to touch.**
- `src/Pennington/Content/MarkdownSourceOverlapDetector.cs` —
  unchanged. Already a pure static function, just wrap it.
- `src/Pennington/Generation/OverlapAuditor.cs` — new. Code: `content.overlap`.
- `src/Pennington/Generation/OutputGenerationService.cs:88-92` — delete the
  inline Phase 0 loop calling `MarkdownSourceOverlapDetector.DetectOverlaps`.
- `src/Pennington/Infrastructure/PenningtonExtensions.cs` — register the
  auditor next to the other audit pipeline registrations
  (`services.AddTransient<IBuildAuditor, OverlapAuditor>()`).

**Implementation shape.**

```csharp
public sealed class OverlapAuditor : IBuildAuditor
{
    private readonly IEnumerable<IContentService> _contentServices;
    public OverlapAuditor(IEnumerable<IContentService> contentServices) => _contentServices = contentServices;
    public string Code => "content.overlap";

    public Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext context, CancellationToken ct)
    {
        var sources = _contentServices.OfType<IMarkdownContentSource>();
        var diagnostics = MarkdownSourceOverlapDetector.DetectOverlaps(sources)
            .Select(msg => new BuildDiagnostic(DiagnosticSeverity.Warning, Route: null, Message: msg, SourceFile: $"{Code}"))
            .ToList();
        return Task.FromResult<IReadOnlyList<BuildDiagnostic>>(diagnostics);
    }
}
```

**Diagnostic shape.**
- Severity: Warning
- Route: null (overlap is about source configurations, not pages)
- Source: `"content.overlap"`
- Message: existing strings from `MarkdownSourceOverlapDetector`, unchanged

**Verification.**
- [ ] `dotnet build Pennington.slnx` clean
- [ ] `dotnet test tests/Pennington.Tests/Content/MarkdownSourceOverlapDetectorTests.cs` passes
- [ ] Configure two overlapping markdown sources in a scratch app; confirm
      warning appears in the `AuditRunner` startup line and in
      `dotnet run -- build` report
- [ ] Existing `OutputGenerationServiceTests` still pass (Phase 0 was tested
      via build report assertions there)

**Pitfalls.**
- Overlap diagnostics intentionally have null Route. The dev overlay only
  renders Route-bearing diagnostics on each page — that's correct for
  site-wide warnings. The startup `AuditRunner.LogSummary` console line still
  reports them.
- Don't change the message text — `MarkdownSourceOverlapDetectorTests`
  asserts against the existing strings.

---

## Phase 2 — Add xref-resolution auditor

**Goal.** Detect every unresolved `<xref:UID>` in markdown sources before
rendering, instead of letting it slip through to link verification as a
broken URL.

**Files to touch.**
- `src/Pennington/Generation/XrefAuditor.cs` — new. Code: `content.xref`.
- `src/Pennington/Infrastructure/PenningtonExtensions.cs` — register.
- Existing infrastructure: `XrefResolver` (file-watched singleton),
  `IContentService.DiscoverAsync()` for raw markdown source paths.

**Getting the markdown body.**
`BuildAuditContext.Pages` (TOC items) carries front matter, not body. Two
options:
- (a) Re-parse markdown via `IContentParser` per page.
- (b) Walk `IContentService.DiscoverAsync()`, pattern-match `MarkdownFileSource`,
  read the file directly.

Use (b). It's what the original Lunaria service did and it sidesteps the
parser dependency.

**Detection.**
- Match `<xref:UID>` and bare `xref:UID` tokens in the raw body. Reuse the
  same regex `XrefHtmlRewriter` uses if practical; otherwise a simple match
  is fine — false positives are tolerable since the resolver has the final
  say.
- For each unique UID per page, query `XrefResolver.Resolve(uid)`. If it
  returns null, emit a Warning.
- Dedupe within a single page so the same broken UID referenced three times
  emits one diagnostic.

**Diagnostic shape.**
- Severity: Warning
- Route: the page containing the unresolved xref
- Source: `"content.xref/<uid>"`
- Message: `"Cannot resolve <xref:{uid}> in {route.CanonicalPath.Value}: no content with this UID is registered."`

**Verification.**
- [ ] In `BeyondTranslationAuditExample`, add `<xref:does-not-exist>` to one
      markdown file
- [ ] `dotnet run`: visit that page; overlay shows the warning with the right
      message and source label
- [ ] `dotnet run -- build`: build report includes it under the page's route
- [ ] Remove the bad xref, save, reload — overlay clears (file watcher
      invalidates the cache)

**Pitfalls.**
- `XrefResolver` is file-watched. Resolve via the auditor's ctor parameter
  (DI re-resolves transient auditors) — don't capture in a singleton
  collaborator.
- Don't flag `xref:` in fenced code blocks. The simplest defense is to skip
  fenced blocks during the scan; the canonical regex in `XrefHtmlRewriter`
  already operates post-render, so the audit is doing pre-render extra work
  here. A naive `string.Contains` on the body will produce false positives
  inside `\`\`\`` fences.
- `xref:` appears inside `[text](xref:UID)` link syntax too; both forms must
  resolve.

---

## Phase 3 — Add IRenderedAuditor seam

**Goal.** Introduce a parallel seam for auditors that need post-pipeline
rendered HTML. Phase 4 (link verification) depends on this.

**Decision committed in this plan.** Add a separate interface
(`IRenderedAuditor`) rather than extend `BuildAuditContext`. Reasoning:
asymmetric cost. Body-aware audits are an order of magnitude more expensive
than structural ones (HTTP fetch per page) and consumers should pick the
cheaper interface when they can. Two interfaces is the smaller surface than
hiding the cost behind an opt-in callback on a single context.

**Files to add.**
- `src/Pennington/Generation/IRenderedAuditor.cs` —
  `Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(RenderedAuditContext, CancellationToken)`.
- `src/Pennington/Generation/RenderedAuditContext.cs` — record bundling
  `Pages`, `Localization`, plus `Func<ContentRoute, CancellationToken, Task<string?>> GetRenderedHtmlAsync`.
- `src/Pennington/Generation/AuditRunner.cs` — extend the existing
  `RunAsync` to also fan out to `IRenderedAuditor` instances. Rendered
  audits run after structural audits.

**Where rendered HTML comes from.**
- Both modes: `IInProcessHttpDispatcher` — fetch the route through the live
  pipeline. In build mode this duplicates Phase 6's work; that's acceptable
  in v1. Wire-up to reuse Phase 6's fetched HTML can come later if profiles
  show it matters.

**Two interfaces, one cache.**
Both `IBuildAuditor` and `IRenderedAuditor` write into `IAuditCache`. The dev
overlay processor doesn't care which auditor produced a diagnostic — it
filters by route either way.

**Cost knobs (deferred unless they prove necessary).**
- Debounce the runner: a flurry of saves in 50ms coalesces into one rebuild.
- Restrict rendered audits to changed routes only when triggered by a file
  watch. Today the structural runner re-audits everything; copy that pattern
  for v1 and revisit.

**Verification.**
- [ ] Unit test in `tests/Pennington.Tests/Generation/AuditRunnerTests.cs`
      registering a fake `IRenderedAuditor` that returns one diagnostic.
      Assert it lands in `IAuditCache.Diagnostics` after `StartAsync`.
- [ ] `dotnet build Pennington.slnx` clean
- [ ] `dotnet test tests/Pennington.Tests/` pass on both TFMs

**Pitfalls.**
- Rendered HTML in dev mode includes the diagnostic overlay markup. Audits
  scanning the body should target `<main>` / the article element, not raw
  `<body>` — otherwise the overlay's own DOM (with its inline `<script>`)
  can confuse a check.
- `IInProcessHttpDispatcher` returns null for routes that don't exist.
  `GetRenderedHtmlAsync` should propagate that as null, not throw, so
  consumers can filter.

---

## Phase 4 — Migrate LinkVerificationService

**Goal.** Move broken-link detection out of
`OutputGenerationService.GenerateAsync` Phase 9 into an `IRenderedAuditor`.
Net behavior: broken links surface in the dev overlay on the page that
contains them, not just at build time.

**Depends on Phase 3.**

**Files to touch.**
- `src/Pennington/Infrastructure/LinkVerificationService.cs` — keep. Expose
  a method that takes route + HTML and returns `IEnumerable<LinkCheckResult>`.
  The existing surface mostly does this already.
- `src/Pennington/Generation/LinkAuditor.cs` — new. Code: `content.links`.
- `src/Pennington/Generation/OutputGenerationService.cs:210-232` — delete
  Phase 9 (the loop that walks `contentResults.Concat(mapGetResults)` and
  invokes `linkVerifier.VerifyLinks`).
- `src/Pennington/Generation/BuildReport.cs` — see `BrokenLinks` decision
  below.

**Diagnostic shape.**
- Severity: Warning (matches today's behavior).
- Route: page containing the broken link.
- Source: `"content.links/<broken-url>"`.
- Message: existing `BrokenLink` strings, mirrored. Aim for
  `"Broken link to {url} ({reason})"`.

**Backward compatibility for `BuildReport.BrokenLinks`.**
`BrokenLinks` is part of the public report API. Two options:

- **Keep it.** The auditor populates both
  `BuildReportBuilder.AddBrokenLink(...)` and the diagnostics list. Users
  reading `report.BrokenLinks` continue to work.
- **Remove it.** Diagnostics are the canonical channel.

**This plan commits to keep.** Mark `BuildReport.BrokenLinks` and the
`AddBrokenLink` builder method `[Obsolete("Read Diagnostics filtered by source 'content.links/'.")]`
in xmldoc — schedule removal one release later.

**Verification.**
- [ ] Add `[broken](/this-does-not-exist)` to one markdown file in
      `BeyondTranslationAuditExample`
- [ ] `dotnet run`: overlay on that page shows broken-link warning
- [ ] `dotnet run -- build`: report includes it; `BuildReport.BrokenLinks`
      also populated for backward compat
- [ ] `tests/Pennington.Tests/Infrastructure/LinkVerificationServiceTests.cs`
      passes
- [ ] `tests/Pennington.Tests/Generation/OutputGenerationServiceTests.cs`
      that asserts on `BrokenLinks` still passes (the migration preserves
      population of that field)

**Pitfalls.**
- LinkVerification depends on a `copiedAssetPaths` list assembled in
  `OutputGenerationService` Phase 4 — the auditor needs an equivalent in dev
  mode. Either expose a `IStaticAssetCatalog` service that both phases
  query, or have the auditor walk the same content services and content-copy
  outputs to build its own catalog.
- The existing implementation also takes `OutputOptions.BaseUrl` for sub-path
  deployments. The auditor must inject `OutputOptions` and pass it in.
- `LinkVerificationService.FindLinksWithoutTrailingSlash` is also called from
  `ContentPipeline.cs:136`. Don't break that consumer.

---

## Per-phase checklist

Before starting any phase:

- [ ] Read this file's "Background", "Canonical reference", and
      "Architectural invariants" sections
- [ ] Open `src/Pennington.TranslationAudit/TranslationAuditor.cs` for shape
- [ ] Open `src/Pennington/Generation/IBuildAuditor.cs` and
      `BuildAuditContext.cs`
- [ ] Skim the phase's "Files to touch" list to understand the blast radius

During:

- [ ] File-scoped namespaces, xmldoc on every public member
- [ ] Auditor registered transient
- [ ] Diagnostics carry Route when route-specific, null otherwise
- [ ] Source label follows `<code>/<bucket>/<context>`
- [ ] Don't change existing message text unless the phase explicitly says to

After:

- [ ] `dotnet build Pennington.slnx` clean
- [ ] `dotnet test tests/Pennington.Tests/` pass on both TFMs
- [ ] Manual: `dotnet run --project examples/BeyondTranslationAuditExample/`
      shows expected overlay behavior
- [ ] Manual: `dotnet run --project examples/BeyondTranslationAuditExample/ -- build`
      shows expected report behavior
- [ ] Update the status column in this file to `complete`
- [ ] Commit with a focused message naming the phase

# Pennington Docs Authoring Agent Contract

This file is the single operational contract for any AI agent that writes or revises the Pennington documentation site.

It does not replace the source documents below. It tells the agent which sources are authoritative, in what order to trust them, and when to stop instead of inventing.

## Scope

The content site lives under `Pennington.Docs/Content/`.

The agent may draft, revise, or publish documentation pages only when the page is represented in `docs-manifest.yaml`.

If the manifest and a page disagree, the manifest wins for workflow state and the page wins for current prose.

## Authority order

Resolve conflicts in this order:

1. `docs-manifest.yaml`
2. `Pennington.Docs/CLAUDE.md`
3. `docs-voice.md`
4. `docs-toc.md`
5. `_research/site-architecture.md`
6. `_research/examples-inventory.md`
7. `_research/blockers.md`

Use this order deliberately:

- `docs-manifest.yaml` decides whether a page is writable now.
- `CLAUDE.md` decides allowed code-fence syntax.
- `docs-voice.md` decides tone and format.
- `docs-toc.md` decides page scope.
- `_research/site-architecture.md` decides product facts.
- `_research/examples-inventory.md` decides which examples are approved evidence.
- `_research/blockers.md` decides whether the page is blocked by missing product support or missing examples.

## Non-negotiable rules

- Do not claim a feature exists unless it is verified in `_research/site-architecture.md` or in production source.
- Do not cite a sample symbol unless it is listed in `_research/examples-inventory.md`.
- Do not invent support for a page whose manifest status is `blocked`.
- Do not change page scope without updating `docs-manifest.yaml`.
- Do not publish a page by flipping `isDraft: false` until the page passes the lint script and the manifest status is `ready_to_publish`.

## Allowed page statuses

- `outline_only`: skeleton exists but the page is not ready for drafting.
- `drafting`: agent may write or revise the page, but publication is not allowed.
- `needs_revision`: page exists but violates the contract and needs cleanup before publication.
- `blocked`: do not author the page beyond a stub; the manifest must explain the blocker.
- `ready_to_publish`: page is complete enough that the next pass should be publication cleanup.
- `published`: published and locked to revision work only.

## Writing workflow

For every page:

1. Read the page entry in `docs-manifest.yaml`.
2. Confirm the quadrant, URL, status, and evidence set.
3. Read the relevant source pages named in the manifest.
4. Draft only within the scope defined by the manifest and `docs-toc.md`.
5. Run `tools/docs-lint.ps1`.
6. Only then change publication-facing front matter such as `isDraft`, `search`, and `llms`.

## Evidence rules

Every substantive code or product claim must come from one of these buckets:

- Production source summarized in `_research/site-architecture.md`
- Approved sample project symbols or files listed in `_research/examples-inventory.md`
- Explicit product gaps listed in `_research/blockers.md`

When a page needs an example and no approved example exists:

- Mark or keep the page `blocked`, or
- Narrow the page scope in the manifest before drafting

Do not solve missing evidence with invented snippets.

## Fence syntax

Use only the fence forms documented in `Pennington.Docs/CLAUDE.md`:

- ```` ```<lang>:path ````
- ```` ```<lang>:xmldocid ````
- ```` ```<lang>:xmldocid,bodyonly ````
- ```` ```<lang>:xmldocid-diff ````

Never use:

- `file="..."`
- `raw-file="..."`
- ad hoc fence metadata not parsed by Pennington

## Quadrant requirements

Tutorial pages must include:

- `## What you'll do`
- `## Prerequisites`
- numbered units or steps with checkpoints
- `## Summary`

How-to pages must include:

- `## When to use this`
- `## Assumptions`
- `## Steps`
- `## Verify`
- `## Related`

Reference pages must include:

- `## Summary`
- `## Declaration`
- `## See also`

Explanation pages must include:

- `## The question`
- `## Context`
- `## How it works`
- `## Trade-offs`
- `## Further reading`

## Stop conditions

Stop and change the manifest instead of writing if any of these are true:

- The page depends on a blocked feature.
- The page needs a sample that is not approved in `_research/examples-inventory.md`.
- The page scope in `docs-toc.md` does not match current product behavior.
- The page requires unsupported fence syntax to be readable.

## Publication checklist

Before a page can move to `ready_to_publish` or `published`:

- No unsupported fence syntax
- No placeholders such as `_path_` or `_ExampleProjectName_`
- No outline artifacts such as `Bullets:` or `Fence slot:`
- No unresolved "link later" text
- Front matter has `title`, `description`, `section`, `order`, `uid`, `isDraft`, `search`, `llms`
- Related links point to real neighboring pages
- Examples are approved by `_research/examples-inventory.md`
- Claims about product behavior match `_research/site-architecture.md`

## Operational commands

Run the authoring lint pass from the docs root:

```powershell
./tools/docs-lint.ps1
```

Run a narrower pass against one page:

```powershell
./tools/docs-lint.ps1 -Page Pennington.Docs/Content/tutorials/getting-started/first-site.md
```

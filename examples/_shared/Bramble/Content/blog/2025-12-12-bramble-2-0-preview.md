---
title: "A preview of Bramble 2.0"
description: "An early look at Bramble 2.0's headline features — first-class tasks, trait coherence, and a new diagnostics engine — plus how to run the preview build today."
date: 2025-12-12
author: Bram Foley
tags: [release, preview, language, roadmap]
uid: bramble.blog.bramble-2-0-preview
---

The 2.0 preview branch has been building in the open for most of the year, and today we're ready to invite everyone to try it. This is a preview — not a release candidate, not production-ready — but it's stable enough to experiment with, and your feedback on it is the most useful thing you can give us right now.

## What's in the preview

### First-class tasks

In 1.2, `Task<T>` was a standard-library type and `spawn`/`await` were functions over it. In 2.0, tasks are a language construct. The user-visible change is small but meaningful:

```bramble
// Bramble 2.0: `?` propagates across `await`
fn fetch_title(url: Str) -> Task<Result<Str, HttpError>> {
    let response = http.get(url).await?
    let html     = response.text().await?
    Ok(extract_title(html))
}
```

The `?` operator now pierces `await` boundaries. In 1.2, you had to spell that out as a nested `match`. We know this was the most commonly cited friction in the async model, and closing it was worth the implementation work.

The scheduler is also now configurable at startup rather than requiring recompilation:

```toml
# bramble.toml
[runtime]
scheduler = "work-stealing"
worker-threads = 4
```

### Tightened trait coherence

The orphan rules are stricter in 2.0. If your codebase implements a trait from package A for a type from package B, the compiler will now reject it — the implementation needs to live in either A or B. The `sprig migrate` tool rewrites affected code automatically in most cases. Where it can't, it leaves a fixme comment explaining what to do.

This breaking change affects a small minority of packages on Thicket. We audited the top 100 packages before the preview and found nine affected crates, all of which have already published compatible versions.

### Diagnostics engine

The new diagnostics engine traces type mismatches back to the constraint that introduced them, not just the site where the conflict manifests. This is the improvement promised in the [2025 roadmap](xref:bramble.blog.roadmap-2025), and it's fully in place in the preview. The difference is most visible when the mismatch is several function calls away from the annotation that fixed the type.

## What's still rough

- Macro hygiene rules in 2.0 changed in a way that breaks a handful of patterns. The error messages are correct but not yet helpful.
- The `sprig fmt` output changed for multi-arm `match` expressions. Existing code reformats on first run, which produces noisy diffs.
- Compile times on large projects are slightly higher than 1.2 due to the coherence analysis pass. We expect to bring this back down before stable release.

## How to try it

```bash
# Install the preview toolchain alongside your existing installation
thicket toolchain install bramble@2.0-preview

# Run a project with the preview compiler
bramble +2.0-preview run .

# Or set it as the default for a specific project
echo 'toolchain = "2.0-preview"' >> bramble.toml
```

The preview toolchain installs alongside the stable one and doesn't affect projects that don't opt in. You can switch back to stable at any time with `bramble +stable`.

## The stability promise

Preview releases can change. We reserve the right to alter syntax, remove features, and change error message text in the 2.0-preview series without a deprecation notice. When we cut a release candidate, the stability promise kicks in and nothing changes without a migration path.

Stable 2.0 is targeted for Q2 2026. The [tutorials](xref:bramble.tutorials.index) and reference docs will be updated to reflect 2.0 semantics when we reach RC.

Try it, break it, tell us what you find.

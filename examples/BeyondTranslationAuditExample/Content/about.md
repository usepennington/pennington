---
title: About
description: About the translation audit example.
order: 20
---

# About

`Pennington.TranslationAudit` is a NuGet package that implements `IBuildAuditor`
and uses LibGit2Sharp to compare commit dates between source files and their
translations. It plugs into Pennington's audit cache — a singleton primed at
startup and refreshed on every content-tree change — so the dev overlay and the
build report always read the same data.

The seam is public. Implementing `IBuildAuditor` and registering it transient
gets your auditor the same plumbing: per-page diagnostics in dev, full report
in CI.

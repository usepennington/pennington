---
title: Inspect your site from the command line
description: One CLI built on System.CommandLine runs the dev server, the static build, and a read-only diag command group that prints what your site will emit, for you and for an AI assistant.
author: Phil Scott
date: 2026-05-29
isDraft: false
tags:
  - cli
  - tooling
---

Every Pennington project ends with `app.RunOrBuildAsync(args)`. That one call now
backs a proper command line: run the project to serve it, or pass a verb.

## The build verb

`dotnet run -- build` writes the static site and exits; `--base-url` and
`--output` set the sub-path and the output folder. The surface is built on
System.CommandLine, so `--help` works at the root and on every verb and prints
without booting the site. The [static-build how-to](xref:how-to.deployment.static-build)
and [base-URL how-to](xref:how-to.deployment.base-url) cover the build side.

## The diag verb

`dotnet run -- diag <command>` runs the site headless, prints plain text about
it, and exits. The commands answer the questions you'd otherwise dig through a
build to find:

- `info` prints the version, content root, page count, and enabled features.
- `toc` shows the table of contents as a tree.
- `routes` lists every emitted URL with its kind and source file.
- `warnings` reports current diagnostics and exits non-zero if any are errors.
- `translation`, `frontmatter`, and `llms` give locale coverage, the front-matter
  keys each content type accepts, and the generated llms.txt index.

It's built for two readers. For you, it's quicker than starting the server and
clicking around. For an AI assistant working in the repo, `diag routes` or `diag
warnings` is a scriptable way to see the site without crawling it. The output is
text and exit codes, not a format to parse. The [CLI
reference](xref:reference.host.cli) lists every command, and [dev mode and build
mode](xref:explanation.core.dev-vs-build) explains the headless run.

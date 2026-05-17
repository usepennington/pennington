---
title: API reference, generated from your XML docs
description: Pennington now builds API reference pages straight from Roslyn xmldocs — one page per type, Stripe-style definition lists, inherited members and union cases included.
author: Phil Scott
date: 2026-04-28
isDraft: false
tags:
  - api-reference
  - roslyn
---

API reference is the documentation that drifts fastest. Rename a parameter, add
an overload, change a default, and every hand-written reference page that
mentioned it is quietly wrong. Pennington can now generate those pages instead.

## Reference pages, generated from the source

Pennington builds API reference straight from the Roslyn workspace. Every public
type that carries an xmldoc gets its own page at `/reference/api/{type}/`, with
descriptions pulled from the `///` comments you already wrote. An index page
lists every discovered type, grouped by namespace.

Pennington's own reference replaced 20 hand-maintained markdown wrappers with a
single parameterised Razor page. The `<summary>` text, the parameter list, and
the return type all read from the compiler, so they can't disagree with the
code. Pointing the generator at your own class library takes one how-to:
[auto-generate an API reference
tree](xref:how-to.content-services.auto-api-reference).

## A layout built to scan

The pages use a Stripe-style definition list: the member name on the left, and
on the right a row of chips — required, type, default — followed by the prose
description. On a narrow screen it collapses to a single column. It's a format
for the way people read reference docs — hunting for one member, not reading top
to bottom.

Two things the generator handles that hand-written pages tend to skip. Interface
pages show members inherited from base interfaces under an "Inherited from"
heading. And C# 15 union types surface their cases as a "Cases" group at the top
of the page — tedious to keep up by hand, automatic when the compiler is the
source.

The pages also serve two readers at once: paired `.humans-only` and
`.robots-only` content gives the browser the visual layout and the
[llms.txt sidecar](xref:how-to.feeds.llms-txt) a plain HTML version. To wire your
solution in, start with [connecting a Roslyn
solution](xref:tutorials.beyond-basics.connect-roslyn).

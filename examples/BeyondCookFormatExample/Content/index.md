---
title: Cook Format Example
description: A bare Pennington host that registers Cooklang (.cook) recipes as a first-class content format alongside markdown.
---

# Cook Format Example

This page is **markdown**. The recipes below are **Cooklang** (`.cook`) files — a custom content
format registered with `AddContentFormat`. Both flow through the same discover → parse → render
pipeline; the dispatcher routes each URL to the parser and renderer for its format.

## Recipes

- [Chicken Piccata](/recipes/chicken-piccata/)
- [Chili](/recipes/chili/)
- [Zuppa Toscana](/recipes/zuppa-toscana/)
- [Cajun Chicken Pasta](/recipes/cajun-chicken-pasta/)
- [Beer Cheese](/recipes/beer-cheese/)
- [Bacon-Wrapped Jalapeños](/recipes/bacon-wrapped-jalapenos/)
- [Chex Mix](/recipes/chex-mix/)

See `how-to/content-services/custom-content-format.md` in the docs for a walkthrough of the four
pieces this example wires up: the front-matter type, the parser, the renderer, and the registration.

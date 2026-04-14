---
title: Chart island demo
description: Content page embedding a data-spa-island="chart" region rendered by ChartIslandRenderer.
---

# Chart island demo

The `<div>` below is marked as an SPA island. On first render the
server-side `ChartIslandRenderer` returns the HTML inside it. On
subsequent in-site navigation, `/_spa-data/chart-demo.json` carries the
same HTML in the `islands.chart` slot so the SPA engine can swap it
without a full reload.

<div data-spa-island="chart" data-extensibility-lab="chart-island">
  <noscript>Chart placeholder — JavaScript required for client navigation.</noscript>
</div>

The server-rendered markup is wired up in
`ChartIslandRenderer.BuildParametersAsync`.

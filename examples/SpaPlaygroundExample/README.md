# SpaPlaygroundExample

A minimal Pennington site whose only purpose is to make `spa-engine.js` visible. Two elements carry `data-spa-region` (`<header>` and `<main>`); the `<nav>` deliberately does not, so its DOM (including a navigation counter) survives every swap.

## Concepts

- `data-spa-region` regions vs. persistent chrome
- `MapStaticAssets` opting a bare host into RCL static assets (`/_content/Pennington.UI/spa-engine.js`)
- The three lifecycle events: `spa:before-navigate`, `spa:commit`, `spa:diagnostics`
- On-page event log so swaps are observable without DevTools

## Referenced from

Not currently referenced by any docs page — this example exists as a hands-on sandbox for SPA-engine behaviour.

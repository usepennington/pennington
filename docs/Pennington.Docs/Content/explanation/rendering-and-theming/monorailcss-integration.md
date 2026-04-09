---
title: "The MonorailCSS Integration"
description: "Why Pennington chose utility-first CSS — covering the two class collection strategies (runtime response scanning vs startup file scanning), why CSS must be fetched last during generation, the OKLCH palette generation algorithm, and dark mode mechanics"
uid: "penn.explanation.monorailcss-integration"
order: 20
---

Explain why Pennington chose utility-first CSS via MonorailCSS and how the integration works. Discuss the class collection problem: unlike Tailwind with a build step, Pennington generates CSS at runtime, so it needs to discover which utility classes are used. Cover the two collection strategies: (1) `CssClassCollectorProcessor` (a response processor) scans rendered HTML at request time and extracts `class="..."` values, and (2) `ContentPaths` configuration scans files at startup for classes that appear in JavaScript strings. Explain the critical constraint during static generation: the CSS endpoint must be fetched last because it needs to have observed all HTML pages first. Discuss the OKLCH palette generation algorithm in `AlgorithmicColorScheme` — how a single hue value generates a full palette with hue-specific adjustments for perceptual uniformity. Explain dark mode: the `dark:` CSS variant, how `prose-invert` works, and the FOUC (flash of unstyled content) prevention mechanism using a script that runs before body render.

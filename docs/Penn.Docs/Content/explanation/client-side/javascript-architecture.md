---
title: "JavaScript Architecture"
description: "Penn's minimal-JS philosophy — the client-side modules (SPA navigation, search modal with FlexSearch, live reload WebSocket, view transitions, copy-to-clipboard), delivery via Penn.UI static web assets, no build step required, and the data-spa-* attribute contract"
uid: "penn.explanation.javascript-architecture"
order: 10
---

Explain Penn's minimal-JavaScript philosophy: a content site shouldn't require a client-side framework, build tool, or bundler. All JavaScript is delivered as static files from the Penn.UI package via ASP.NET's static web assets mechanism. Walk through the client-side modules: `PageManager` as the entry point that conditionally initializes managers based on DOM presence, `ThemeManager` (localStorage persistence, Mermaid re-rendering on theme change), `OutlineManager` (scroll tracking with requestAnimationFrame throttle), `TabManager` (full ARIA tab semantics), `MermaidManager` (CDN lazy load, OKLCH-to-hex color conversion for theme-aware rendering), `MobileNavManager` and `SidebarToggleManager` (responsive navigation), `SearchManager` (FlexSearch lazy load, weighted scoring, snippet generation). Explain SPA integration: how managers respond to `spa:before-navigate` (cleanup) and `spa:commit` (reinitialize) events. Cover accessibility considerations: ARIA roles, keyboard navigation, focus management on SPA transitions.

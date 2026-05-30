---
title: Welcome
description: Translation audit example homepage.
order: 10
---

This site has two locales — English (default) and Spanish (`/es/`). Translation
status is reported through the standard Pennington diagnostic channels:

- **In dev (`dotnet run`):** the bottom-right overlay shows per-page warnings
  when the current page is missing a translation or its translation is older
  than the source's last commit.
- **In CI (`dotnet run -- build`):** the build report lists the same diagnostics
  alongside broken links and duplicate routes.

Visit `/getting-started/` to see the missing-translation overlay light up.

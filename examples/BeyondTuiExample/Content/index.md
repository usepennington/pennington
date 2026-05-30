---
title: Beyond TUI
order: 1
---

This example launches Pennington with the optional `Pennington.Tui` dev-time dashboard.
When the host starts, a full-screen terminal UI shows:

- The URL Kestrel bound to.
- The per-locale content tree discovered from `IContentService`.
- Warnings and broken links from a dry-run of the build pipeline.
- Recent file-watcher activity and captured console output.

Press `B` to open this page in your default browser, `R` to re-run the validator,
or `Q` to quit.

When this project is launched as `dotnet run -- build`, the dashboard no-ops and
the static build runs exactly as without the package reference.

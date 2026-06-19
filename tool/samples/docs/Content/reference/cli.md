---
title: CLI Reference
description: Verbs and flags accepted by the pennington tool.
order: 1
---

# CLI Reference

| Command | Effect |
| --- | --- |
| `pennington` | Serve the site live with hot reload. |
| `pennington build` | Generate the static site to `output/`. |
| `pennington build --base-url /docs --output dist` | Static build under a sub-path. |
| `pennington diag toc` | Print the table of contents (read-only inspection). |

The `--root=<folder>` option selects the site folder (defaults to the current directory).

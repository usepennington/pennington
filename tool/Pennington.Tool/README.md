# Pennington.Tool

A proof-of-concept .NET global tool that serves or builds a **doc site** or **blog site**
from a folder of markdown plus a single `pennington.toml` — no project file, no `Program.cs`.

It is a thin host over the existing `Pennington.DocSite` and `Pennington.BlogSite` templates:
the tool reads `pennington.toml`, projects it onto `DocSiteOptions` or `BlogSiteOptions`, and
forwards the verb (`serve` / `build` / `diag`) to the same engine the templates use.

## Install & run

```bash
# From the repo (PoC): pack and install as a global tool
dotnet pack tool/Pennington.Tool
dotnet tool install --global Pennington.Tool --add-source artifacts/package/release

# Or just run it without installing
dotnet run --project tool/Pennington.Tool -- build --root=./my-site
```

Once installed, `pennington` operates on the current directory by default:

```bash
cd my-site
pennington                         # serve live (hot reload) on http://localhost:5000
pennington build                   # generate the static site to ./output
pennington build --base-url /docs --output dist
pennington diag toc                # read-only inspection (info|toc|routes|warnings|…)
```

### Selecting the folder

The site folder defaults to the current directory. Point elsewhere with `--root`:

```bash
pennington --root=./my-site        # serve
pennington build --root=./my-site  # build
```

Use the **equals form** (`--root=PATH`) with `build`. The engine classifies its verb from the
raw process command line, and a space-separated `--root PATH` value can be misread there as a
positional base URL. `--root=PATH` (and plain `cd`) are always safe.

## `pennington.toml`

The top-level `template` key selects the template. A shared `[site]` table carries common
settings; docs add `[[area]]` entries and blogs add a `[blog]` table.

### Docs

```toml
template = "docs"

[site]
title = "Sample Docs"                  # required
description = "..."                    # required
canonical_base_url = "https://example.com"
content_root = "Content"               # default "Content"
github_url = "https://github.com/org/repo"
header = '<a href="/">Sample Docs</a>' # raw HTML brand area
footer = '<footer>…</footer>'          # raw HTML footer
social_image_url = "https://…/og.png"
additional_head = '<meta name="…" content="…">'
extra_styles = ".callout { … }"

[[area]]                               # optional content areas (top-level dirs)
title = "Guides"
slug  = "guides"

[[area]]
title = "Reference"
slug  = "reference"
```

Drop markdown under `Content/` (or `content_root`). A `Content/blog/` folder with posts
auto-activates the blog surface inside the doc site, exactly as the template does.

### Blog

```toml
template = "blog"

[site]
title = "Sample Blog"                  # required
description = "..."                    # required
canonical_base_url = "https://example.com"
content_root = "Content"

[blog]
content_path = "Blog"                  # posts under {content_root}/{content_path}
base_url = "/blog"
posts_per_page = 10
author_name = "Sample Author"
author_bio = "..."
enable_rss = true
enable_sitemap = true
```

Working examples live under [`tool/samples/`](../samples): a `docs` site and a `blog` site,
each a folder + `pennington.toml`.

## Scope

This is a proof of concept. It exposes the most common `DocSiteOptions` / `BlogSiteOptions`
fields — enough to stand up either template from config. Strongly-typed-only options (color
schemes, social-card renderers, custom Razor assemblies, localization callbacks) are out of
scope for a TOML surface and are left at their template defaults.

## Native AOT

The request mentioned native AOT. It is **not currently achievable** for this stack — see
[`AOT-NOTES.md`](./AOT-NOTES.md) for the specific blockers and what would have to change.

# BareHostSearchExample

Adds the Pennington.UI search modal to a bare `AddPennington` host — no DocSite. `AddPennington` already emits the search index at `/search/{locale}/index.json`; this example shows the small amount of wiring that lights up the modal UI on top of it.

## Concepts

- The search index ships on every Pennington host. `AddPennington` registers the artifact service, build emitter, and dev middleware, so `/search/{locale}/index.json` (term shards + per-page fragments) is served with no extra wiring.
- The modal UI ships in `Pennington.UI/wwwroot/scripts.js` (`SearchManager`). Referencing `Pennington.UI` serves it — and the `DeweySearch.Web` browser client — as static web assets under `/_content/`; `app.UseStaticFiles()` serves them.
- `scripts.js` self-initializes on load: it binds the click and the Ctrl/Cmd-K shortcut to the element with `id="search-input"` and reads `data-default-locale` (and, when present, `data-locales` / `data-base-url`) from `<body>`.
- The modal's CSS classes (`.search-modal*`, `.search-result*`) are safelisted as `@apply` blocks in `Pennington.MonorailCss` (`PenningtonApplies.SearchModalApplies`), so `AddMonorailCss` styles the modal with no hand-written CSS.
- Content is the shared `_shared/Bramble` corpus mounted at the root with `ExcludePaths = ["blog"]`, giving the index real heading-level documents across four areas (`tutorials`, `how-to`, `reference`, `explanation`) — which also populates the modal's area filter chips.

## Referenced from

- `docs/.../how-to/discovery/search-on-a-bare-host.md`

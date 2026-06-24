// _worker.js — Cloudflare Pages advanced-mode Worker.
// When a client sends `Accept: text/markdown` (e.g. Claude Code's WebFetch),
// serve the co-located Markdown twin at {route}/index.md. Otherwise serve HTML.
//
// Copied into the deployed output/ folder by the "Add Markdown content-negotiation
// worker" step in .github/workflows/deploy-docs.yml — the build wipes output/, so the
// source lives here in the repo, not in output/.
export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    const isRead = request.method === "GET" || request.method === "HEAD";
    const twin = isRead ? markdownTwin(url.pathname) : null; // null = not a page route

    if (twin && (request.headers.get("Accept") || "").includes("text/markdown")) {
      const md = await env.ASSETS.fetch(new Request(new URL(twin, url), request));
      if (md.status === 200) {
        const headers = new Headers(md.headers);
        headers.set("Content-Type", "text/markdown; charset=utf-8");
        headers.set("Vary", "Accept");
        return new Response(md.body, { status: 200, headers });
      }
      // No twin (e.g. a page with `llms: false`) — fall through to the HTML page.
    }

    const response = await env.ASSETS.fetch(request);
    if (!twin) return response;
    // Page routes vary by Accept, so caches don't hand HTML to a markdown client (or vice-versa).
    const headers = new Headers(response.headers);
    headers.append("Vary", "Accept");
    return new Response(response.body, { status: response.status, statusText: response.statusText, headers });
  },
};

// Map a page route to its Markdown twin, or null if it's a real asset.
function markdownTwin(pathname) {
  if (pathname.endsWith("/")) return pathname + "index.md";             // /guides/install/  -> /guides/install/index.md
  if (pathname.endsWith(".html")) return pathname.slice(0, -5) + ".md"; // /x/index.html     -> /x/index.md
  const last = pathname.slice(pathname.lastIndexOf("/") + 1);
  if (!last.includes(".")) return pathname + "/index.md";              // clean URL /x       -> /x/index.md
  return null;                                                          // .css/.png/.md/.txt/.json — leave alone
}

// _worker.js — Cloudflare Pages advanced-mode Worker.
// When a client sends `Accept: text/markdown` (e.g. Claude Code's WebFetch),
// serve the co-located Markdown twin at {route}.md (the home at /index.md). Otherwise serve HTML.
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
// The twin is the page URL with `.md` appended; the home is the one special case (/ -> /index.md).
function markdownTwin(pathname) {
  if (pathname === "/" || pathname === "/index.html") return "/index.md";                 // home            -> /index.md
  if (pathname.endsWith("/index.html")) return pathname.slice(0, -"/index.html".length) + ".md"; // /x/index.html -> /x.md
  if (pathname.endsWith("/")) return pathname.slice(0, -1) + ".md";    // /guides/install/  -> /guides/install.md
  const last = pathname.slice(pathname.lastIndexOf("/") + 1);
  if (!last.includes(".")) return pathname + ".md";                    // clean URL /x       -> /x.md
  return null;                                                          // .css/.png/.md/.txt/.json — leave alone
}

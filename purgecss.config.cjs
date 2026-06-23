// PurgeCSS config for the GitHub Pages build (see .github/workflows/publish-to-gh-pages.yml).
//
// MonorailCSS discovers utilities by scanning the IL of every referenced assembly, so the
// generated styles.css ships CSS for classes that exist as string literals in code but are
// never rendered on this site (~50% of the file — mostly unused dark: variants and component
// utilities from Pennington.BlogSite/.UI). This purges styles.css against the built HTML to
// drop them. It runs before the minify step.
// glob patterns must use forward slashes on every OS, so normalise __dirname.
const output = `${__dirname.replace(/\\/g, "/")}/docs/Pennington.Docs/output`;

module.exports = {
  css: [`${output}/styles.css`],
  content: [
    `${output}/**/*.html`,
    `${output}/_content/**/*.js`,
  ],

  // Whole-token extractor. The default extractor splits on `[`, `]`, `:` and `=`, which
  // shreds Monorail's arbitrary-variant classes (e.g. `data-[mobile-menu-open=true]:overflow-hidden`,
  // `h-[100dvh]`) so they never match. This keeps each class attribute token intact.
  defaultExtractor: (content) => content.match(/[^\s"'`<>]+/g) || [],

  // Classes/attributes toggled onto the DOM by JS at runtime never appear in the static
  // (light-mode) HTML. Without these, purge deletes every rule scoped under `.dark`
  // (`:where(.dark, .dark *)`), the mobile menu, tab state, etc. Sourced by grepping
  // classList.toggle / setAttribute in the shipped Pennington.UI scripts — keep in sync
  // when new runtime-toggled classes are added.
  safelist: {
    // bg-accent-500 / bg-base-400 are toggled onto the active nav item by scripts.js
    // (classList.toggle); they happen to also render statically today, but list them so
    // survival doesn't depend on that coincidence.
    standard: ["dark", "active", "is-active", "hljs", "hidden", "bg-accent-500", "bg-base-400"],
    greedy: [/^data-/, /^aria-/, /^opacity-(0|100)$/, /-state$/],
  },

  // @keyframes and @font-face are reached through MonorailCSS's `var()` indirection
  // (`animation: var(--animate-pulse)`, `font-family: var(--font-*)`), which PurgeCSS can't
  // follow — enabling their removal strips `@keyframes pulse`/`ping` and a web `@font-face`
  // while keeping the utilities that depend on them. Leave OFF (false is the default).
  keyframes: false,
  fontFace: false,

  // Variables are safe to purge: PurgeCSS keeps any custom property reached by a `var()` in
  // retained CSS, and dropping the unused palette entries saves ~2 KB brotli.
  variables: true,
};

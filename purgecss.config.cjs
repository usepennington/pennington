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
    standard: ["dark", "active", "is-active", "hljs", "hidden"],
    greedy: [/^data-/, /^aria-/, /^opacity-(0|100)$/, /-state$/],
  },

  // Never strip these — they are referenced indirectly (animation:, var(), @font-face)
  // rather than by a selector PurgeCSS can see in the HTML.
  keyframes: true,
  variables: true,
  fontFace: true,
};

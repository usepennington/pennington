# Pennington.BlogSite — Design system

Razor template for the blog site. Styling is utility-first via MonorailCSS (Tailwind-compatible, OKLCH). Styles live on the component — do not author `.css` rules when a utility composition works.

## Color slots

Use the **semantic palette**, never raw Tailwind colors (`gray-*`, `amber-*`, `bg-white`, …).

Built-in slots (from `Pennington.MonorailCss`):

- `primary` — brand (post CTAs, timestamp accent bar, hover states)
- `accent` — complementary
- `base` — neutrals (page/card backgrounds, borders, body text)

Utility shape: `bg-base-50 dark:bg-base-950`, `text-primary-500 dark:text-primary-400`, `border-base-200/50 dark:border-base-800/50`.

Shades on every palette: `50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950`.

Default scheme is `NamedColorScheme` Blue / Purple / Slate, overridable via `BlogSiteOptions.ColorScheme`. Any `IColorScheme` works: `NamedColorScheme`, `AlgorithmicColorScheme`, or a curated `ColorTheme` (seed-hue themes that also carry coordinating `syntax-*` highlight palettes and snap `base` to the nearest stock neutral). Extra slots (`accent-one`, `accent-two`, …) are **not** built in — they only exist when the consumer registers them through a custom `IColorScheme` (see `docs/Pennington.Docs/SnugglepussColorScheme.cs`).

## Dark mode

Every color-bearing utility pairs a `dark:` variant. The root wrapper carries `scheme-dark` to trigger it.

## Typography

- `font-sans` for body, `font-display` for headings.
- Post bodies render with `prose lg:prose-lg dark:prose-invert prose-headings:font-display` (see `Components/Pages/Home.razor`, `Components/Layout/BlogPost.razor`). Prose overrides themselves live in `src/Pennington.MonorailCss/MonorailCssOptions.cs` — edit there, not per-component.

## Layout anatomy

Header + centered `max-w-7xl` main + footer. No sidebar chrome; the home page has an inline two-column layout (recent posts + author sidebar).

- `Components/Layout/MainLayout.razor` — shell (header, nav, search, theme toggle, footer).
- `Components/Layout/ContentLayout.razor` / `ContentWithProseLayout.razor` — standalone content-page shells (the latter wraps the body in the prose classes).
- `Components/Pages/Pages.razor` — root catch-all content page (unmatched routes; also renders the built `output/404.html`).
- `Components/Layout/BlogArticleCard.razor` — post card; group-hover transitions, `primary-200` accent bar on the date.
- `Components/Layout/BlogPostsList.razor` — ordered list of cards, `divide-y divide-base-200 dark:divide-primary-900/50`.
- `Components/Layout/BlogPost.razor` — full post view with metadata.
- `Components/Layout/BlogSummary.razor` — recent-posts listing used on Home.
- `Components/Pages/{Home,Archive,Tags,Tag,Blog}.razor` — top-level pages.
- `Components/SocialIcons.razor` — byline/sidebar social strip.

No custom code-block or tab CSS — those come from the shared MonorailCSS prose rules.

## Rules

- Prefer utility classes over new CSS selectors.
- Semantic palette only — no direct colors.
- No decorative unicode glyphs (check marks, arrows, bullets) in rendered UI; keep strings ASCII.

# Pennington.DocSite — Design system

Razor template for the documentation site. Styling is utility-first via MonorailCSS (Tailwind-compatible, OKLCH). Styles live on the component — do not author `.css` rules when a utility composition works.

## Color slots

Use the **semantic palette**, never raw Tailwind colors (`gray-*`, `amber-*`, `bg-white`, …).

Built-in slots (from `Pennington.MonorailCss`):

- `primary` — brand
- `accent` — complementary
- `base` — neutrals (backgrounds, borders, body text)

Utility shape: `bg-primary-500`, `text-base-900 dark:text-base-50`, `border-accent-200`, `bg-primary-700/20`.

Shades on every palette: `50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950`.

Default scheme is `NamedColorScheme` Blue / Purple / Slate, overridable via `DocSiteOptions.ColorScheme`. Extra slots (`accent-one`, `accent-two`, …) are **not** built in — they only exist when the consumer registers them through a custom `IColorScheme`. Canonical example: `docs/Pennington.Docs/SnugglepussColorScheme.cs`.

## Dark mode

Every color-bearing utility pairs a `dark:` variant. The root wrapper carries `scheme-dark` to trigger it.

## Typography

- `font-sans` for body, `font-display` for headings.
- Prose customizations (links, blockquote, inline code, code blocks) are centralized in the MonorailCSS prose block in `src/Pennington.MonorailCss/MonorailCssOptions.cs`. Don't re-style prose at the component level — extend that block if a global change is needed.

## Layout anatomy

- `Components/Layout/MainLayout.razor` — left sidebar nav + article + right outline rail (three columns at `xl`).
- `Components/Layout/DocSiteHeader.razor` — sticky top bar with search, theme toggle, repo link.
- `Components/Layout/AreaNavigation.razor` — multi-area switcher with `aria-current` states.
- `Components/Layout/FullWidthLayout.razor` — landing/marketing pages that skip the sidebar.
- `Slots/Components/DocSiteArticle.razor` — article shell rendered via `DocSiteArticleSlotRenderer`.

## Rules

- Prefer utility classes over new CSS selectors.
- Semantic palette only — no direct colors.
- No decorative unicode glyphs (check marks, arrows, bullets) in rendered UI; keep strings ASCII.

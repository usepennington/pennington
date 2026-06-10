# BlogSiteSocialCardsExample

A BlogSite host that turns on **generated social cards** via
`BlogSiteOptions.SocialCards`. Referenced from the docs site at
`how-to/feeds/social-cards.md`.

## What it teaches

Pennington owns the *integration* for OpenGraph/Twitter card images; the host owns
only the *drawing*:

- **The hook** — `SocialCardOptions.Render` is a `Func<SocialCardRequest, IServiceProvider, CancellationToken, Task<byte[]?>>`.
  It receives a page's resolved metadata (title, description, date, tags, locale, the
  card's own absolute URL) plus the request's service provider (resolve font caches,
  theme options, ...) and returns PNG bytes, or `null` to skip the page.
- **Discovery + baking** — a `SocialCardContentService` discovers one card route per
  content page (`/social-cards/<page>.png`), so `dotnet run -- build` bakes a card for
  every page by fetching it.
- **On-demand rendering** — a `MapGet` endpoint renders each card live in dev and is the
  route the build crawler fetches.
- **Meta tags** — each post's `og:image` / `twitter:image` is pointed at its card URL
  automatically (absolute, using `CanonicalBaseUrl`). This wiring is template-agnostic:
  the same `SocialCards` option works on a DocSite, a bare `AddPennington` host with any
  `IFrontMatter` type, and Razor pages with `.razor.metadata.yml` sidecar files.
- **The home page card** — BlogSite projects a site-identity record for `/` (title and
  description from `BlogSiteOptions`), so the root URL gets a card at
  `/social-cards/index.png` with no extra configuration. The renderer here paints it a
  distinct color by switching on `request.CanonicalPath`.

`SocialCardPainter` here is a dependency-free PNG encoder that paints a solid placeholder
so the example needs no image library. In a real app, draw `request.Title` /
`request.Description` with an image library (ImageSharp, SkiaSharp) or screenshot an HTML
template with Playwright.

## Run

- `dotnet run --project examples/BlogSiteSocialCardsExample` — serve; open
  `/social-cards/blog/hello-card.png` to see a card rendered on demand, and
  `/social-cards/index.png` for the home page's card.
- `dotnet run --project examples/BlogSiteSocialCardsExample -- build` — generate the static
  site; each content page gets a baked `output/social-cards/.../*.png`.

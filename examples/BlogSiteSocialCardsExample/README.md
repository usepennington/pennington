# BlogSiteSocialCardsExample

A BlogSite host that turns on **generated social cards** via
`BlogSiteOptions.SocialCards`.

## What it teaches

Pennington owns the *integration* for OpenGraph/Twitter card images; the host owns
only the *drawing*:

- **The hook** — `SocialCardOptions.Render` is a `Func<SocialCardRequest, CancellationToken, Task<byte[]?>>`.
  It receives a page's resolved metadata (title, description, date, tags, locale, the
  card's own absolute URL) and returns PNG bytes, or `null` to skip the page.
- **Discovery + baking** — a `SocialCardContentService` discovers one card route per
  content page (`/social-cards/<page>.png`), so `dotnet run -- build` bakes a card for
  every page by fetching it.
- **On-demand rendering** — a `MapGet` endpoint renders each card live in dev and is the
  route the build crawler fetches.
- **Meta tags** — each post's `og:image` / `twitter:image` is pointed at its card URL
  automatically (absolute, using `CanonicalBaseUrl`).

`SocialCardPainter` here is a dependency-free PNG encoder that paints a solid placeholder
so the example needs no image library. In a real app, draw `request.Title` /
`request.Description` with an image library (ImageSharp, SkiaSharp) or screenshot an HTML
template with Playwright.

## Run

- `dotnet run --project examples/BlogSiteSocialCardsExample` — serve; open
  `/social-cards/blog/hello-card.png` to see a card rendered on demand.
- `dotnet run --project examples/BlogSiteSocialCardsExample -- build` — generate the static
  site; each content page gets a baked `output/social-cards/.../*.png`.

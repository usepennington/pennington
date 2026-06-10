using Pennington.BlogSite;
using Pennington.SocialCards;

var builder = WebApplication.CreateBuilder(args);

// Generated social cards. Pennington owns the integration — it discovers one card
// route per content page (so the static build bakes them), serves each on demand at
// `/social-cards/<page>.png`, and points every post's `og:image`/`twitter:image` at it.
// The host owns only the drawing, via `SocialCardOptions.Render`: it receives the page's
// resolved metadata (title, description, date, tags, the card's own absolute URL) and
// returns PNG bytes — or null to skip a page.
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "Social Cards Blog",
    SiteDescription = "A BlogSite host demonstrating generated OpenGraph social cards.",
    CanonicalBaseUrl = "https://example.com",
    AuthorName = "Author Name",

    SocialCards = new SocialCardOptions
    {
        // This sample paints a solid placeholder card with a dependency-free PNG encoder so the
        // example needs no image library. In a real app, draw `request.Title` /
        // `request.Description` onto the canvas with an image library (ImageSharp, SkiaSharp) or
        // screenshot an HTML template with Playwright — `request` carries everything you need,
        // and the IServiceProvider lets the renderer resolve registered services (font caches,
        // theme options, ...).
        Render = (request, services, _) =>
        {
            // The home page gets its own card too — BlogSite projects a site-identity record
            // for `/` (title/description from the options), rendered at /social-cards/index.png.
            // CanonicalPath is how a renderer varies the design per page: purple for the home
            // card here, slate blue for posts.
            var png = request.CanonicalPath.Value == "/"
                ? SocialCardPainter.SolidCard(request.Width, request.Height, 0x6D, 0x28, 0xD9)
                : SocialCardPainter.SolidCard(request.Width, request.Height);
            return Task.FromResult<byte[]?>(png);
        },
    },
});

var app = builder.Build();

app.UseBlogSite();

await app.RunBlogSiteAsync(args);

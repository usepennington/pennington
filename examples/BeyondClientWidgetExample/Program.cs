using BeyondClientWidgetExample;
using BeyondClientWidgetExample.Components;
using Mdazor;
using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// A DocSite whose only customization is one client-side widget: an image-gallery
// lightbox. GalleryWidget.BuildDocSiteOptions injects the GLightbox CDN assets
// and the local init script into <head>; AddMdazorComponent<ImageGallery>()
// registers the server-rendered tag the script enhances. Backs the how-to
// /how-to/rich-content/client-side-widget.
builder.Services.AddDocSite(GalleryWidget.BuildDocSiteOptions);
builder.Services.AddMdazorComponent<ImageGallery>();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

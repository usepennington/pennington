// gallery.js — the client half of the image-gallery widget.
//
// The server renders <a class="glightbox"> thumbnails (Components/ImageGallery.razor);
// this script finds them in the browser and upgrades them into a lightbox.
// GLightbox itself loads from a CDN <script> in <head> (see GalleryWidget.cs), so
// the global GLightbox function is available by the time this deferred script runs.
let lightbox = null;

function initGallery() {
    if (typeof GLightbox !== 'function') return;
    // When re-running after an in-site navigation, tear down the previous
    // instance first so its event listeners don't accumulate.
    lightbox?.destroy();
    lightbox = GLightbox({ selector: '.glightbox' });
}

// First full page load. A deferred script runs after the DOM is parsed, so the
// gallery markup is already present.
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initGallery);
} else {
    initGallery();
}

// Pennington swaps page content on in-site navigation without a full reload, so
// re-scan for galleries after each SPA commit. No-op if spa-engine.js is absent.
document.addEventListener('spa:commit', initGallery);

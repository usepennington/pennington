---
title: "Image gallery"
uid: guides.image-gallery
order: 2
---

The `<ImageGallery>` tag renders a grid of thumbnails on the server. Click any
one to open it in the GLightbox overlay; the arrow keys and swipe gestures move
through the set because every thumbnail shares the same `data-gallery` group.

<ImageGallery Images="peppermint-express.png, merry-mixer.png, indigo-inchworm.png, dusty-gusher.png, train-junction-sunset.png" Group="trains" />

The tag binds three primitive attributes — `Images`, `Group`, and an optional
`BasePath` — so the whole gallery is one line of markdown. The captions in the
lightbox are derived from the file names.

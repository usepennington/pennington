---
title: Old page URL
description: Redirects away to the new location.
redirectUrl: /main/front-matter/
order: 200
uid: kitchen-sink.main.redirect-source
---

# This page has moved

A visit to this URL is intercepted by the Pennington redirect middleware
and returns HTTP 301 with a meta-refresh body pointing at `redirectUrl`.
The static build captures the same 301 and writes the meta-refresh file
to disk at this page's output path — one code path for dev and publish.

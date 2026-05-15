---
title: Your docs site can have a blog now
description: Drop a Content/blog folder into a DocSite and a blog lights up — index, post pages, browse-by-tag pages, an RSS feed, and a header link, with no Program.cs changes.
author: Phil Scott
date: 2026-05-15
isDraft: false
tags:
  - announcements
  - blog
---

You're reading this on the feature it announces. This blog isn't a separate app
— it's a DocSite that found a `blog` folder. The docs explain how Pennington
works; the blog is where release notes like this one go.

## A folder is the switch

A DocSite turns on a blog the moment its content project contains a `blog` folder
with markdown posts in it. No flag, no `Program.cs` change, no service to
register. `AddDocSite` checks for the folder at startup, and if it's there, you
get:

- a `/blog` index, posts listed newest first
- a page per post
- browse-by-tag pages
- an RSS feed at `/rss.xml`
- a "Blog" link in the site header

Remove the folder and all of it goes away — no dead link, no empty route.

A post is a markdown file with front matter — title, description, author, date,
tags:

```markdown
---
title: Your docs site can have a blog now
author: Phil Scott
date: 2026-05-15
tags:
  - announcements
  - blog
---
```

## Convention over configuration

The presence of the folder is the configuration. You don't tell the engine you
want a blog — you write posts, and it picks them up. It's the same idea as locale
folders turning on localization, or a data file becoming a typed dataset: the
content says what you want, so there's no registration line to add.

If you have a DocSite, adding a blog is a folder away. The [add-a-blog
tutorial](xref:tutorials.docsite.add-a-blog) walks through it — first post, date
ordering, tags, and the [RSS feed](xref:how-to.feeds.rss).

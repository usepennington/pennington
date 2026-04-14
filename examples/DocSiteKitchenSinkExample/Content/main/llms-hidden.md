---
title: Not in llms.txt
description: This page is intentionally excluded from llms.txt.
section: authoring
order: 230
llms: false
uid: kitchen-sink.main.llms-hidden
---

# Not in llms.txt

This page carries `llms: false` in its front matter. It still appears
in the sidebar and in search results, but `/llms.txt` does **not**
list it. The content-stripping llms generator skips pages whose
`Llms` flag is `false` when assembling its index of documents.

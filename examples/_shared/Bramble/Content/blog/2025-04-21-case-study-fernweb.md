---
title: "Case study: how Fernweb ships with Bramble"
description: "How Fernweb replaced their Python data-pipeline scripts with Bramble, cut processing time by 40%, and stopped getting paged for null-pointer surprises at 3 AM."
date: 2025-04-21
author: Indira Cole
tags: [case-study, data-pipelines, production, community]
uid: bramble.blog.case-study-fernweb
---

Fernweb runs a content-intelligence platform: they ingest articles from hundreds of sources, score them for relevance and quality, and surface the results to editorial teams. The pipeline that does this work used to be a collection of Python scripts held together with subprocess calls and a shared filesystem. It worked until it didn't, and when it didn't, the failure mode was usually a `NoneType has no attribute 'strip'` error at 2:47 in the morning.

I talked to Fernweb's infrastructure lead, Casey Halvorsen, about their migration to Bramble over the first half of 2024.

## The problem they were solving

Fernweb's pipeline had three layers: fetch, normalize, and score. Each layer was a separate Python process reading from and writing to intermediate files. The glue between them was implicit — file naming conventions, undocumented field presence assumptions, one shared config file that had grown to 900 lines.

"The fetch layer would sometimes return a record with the `body` field missing, which was a valid representation of a fetch error. The normalize layer had a comment saying 'body may be None, handle it' but nobody had added the check everywhere it was needed," Casey told me. "Bramble doesn't give you the option to forget. If `body` is `Option<Str>`, you have to decide what to do with it before you can use it."

## The migration

They rewrote the pipeline stage by stage over about three months, starting with normalize — the layer with the worst error history. The new code modeled each record as a Bramble struct with explicit `Option` fields, and the type checker flagged every site where the old code had been optimistic:

```bramble
struct Article {
    id:    Str,
    url:   Str,
    body:  Option<Str>,
    score: Option<Float>,
}

fn normalize(raw: RawRecord) -> Result<Article, ParseError> {
    Ok(Article {
        id:    raw.id,
        url:   raw.url,
        body:  raw.body.map(|b| clean_whitespace(b)),
        score: None,
    })
}
```

The fetch and score layers followed over the next six weeks. The whole pipeline, previously spread across eight Python files totalling about 2,400 lines, compressed to roughly 900 lines of Bramble. Casey attributes some of that to consolidation and some to the fact that a lot of the Python was defensive null-checking that the type system now handles structurally.

## Outcomes

Fernweb measures their pipeline reliability by "unplanned wakeups" — pages that go out overnight for unexpected failures. In the six months before migration, they averaged 2.3 unplanned wakeups per month related to the pipeline. In the six months after: zero.

Processing throughput improved by roughly 40%, partly because the new code is faster and partly because they removed a layer of intermediate file I/O while they were rewriting. The Trellis build system made it straightforward to express the fetch/normalize/score dependency graph as actual build targets rather than a shell script, so parallelism became explicit.

"The thing I didn't expect," Casey said, "was how much the error-as-value model changed the way we think about failure modes. When every function that can fail returns a `Result`, you have to read through the whole error handling path when you write the function. You can't push it onto future-Casey. It turns out future-Casey appreciates that a lot."

## Getting started

If you're evaluating Bramble for a similar workload, the [web service tutorial](xref:bramble.tutorials.a-web-service) is a good first hands-on project, and the [concurrency how-to](xref:bramble.how-to.language.run-work-concurrently) covers the task model that Fernweb is starting to use for their fetch layer in Bramble 1.2.

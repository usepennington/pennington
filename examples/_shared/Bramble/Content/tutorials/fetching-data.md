---
title: "Fetching data"
description: "Make an HTTP GET request with std/http and parse the JSON response into a typed Bramble record with std/json."
uid: bramble.tutorials.fetching-data
order: 80
sectionLabel: "Tutorials"
tags: [http, json, network, records, result]
---

Bramble's sandbox allows network access only when the host explicitly grants it — but in a normal `bramble run` invocation on your own machine, outbound HTTP is permitted by default. This tutorial fetches a small public JSON API and maps the response onto a Bramble record.

## Making a GET request

`std/http` exposes a blocking `get(url)` function that returns `Result<Response, HttpError>`.

```bramble
import std/http
import std/io

fn main() {
    match http.get("https://api.bramble.dev/ping") {
        Ok(resp)  => io.println("Status: ${resp.status}"),
        Err(e)    => io.println("Request failed: ${e}"),
    }
}
```

`resp.status` is an `i32` (the HTTP status code). `resp.body` holds the raw response as `str`.

## Defining a record type

Records in Bramble are named structural types. Define one to represent the shape of the JSON you expect.

```bramble
record Post {
    id:    i64,
    title: str,
    body:  str,
}
```

Record fields are immutable by default. A record instance is constructed with `Post { id: 1, title: "...", body: "..." }` and its fields accessed with dot notation.

## Parsing JSON into a record

`std/json` provides `parse_as<T>(text)` which returns `Result<T, JsonError>`. The type parameter must be a record whose field names match the JSON keys.

```bramble
import std/http
import std/io
import std/json

record Post {
    id:    i64,
    title: str,
    body:  str,
}

fn fetch_post(id: i64) -> Result<Post, str> {
    let url  = "https://jsonplaceholder.bramble.dev/posts/${id}"
    let resp = http.get(url).map_err(|e| "HTTP error: ${e}")?
    let post = json.parse_as<Post>(resp.body).map_err(|e| "JSON error: ${e}")?
    Ok(post)
}

fn main() {
    match fetch_post(1) {
        Ok(post) => {
            io.println("Title: ${post.title}")
            io.println("Body preview: ${post.body.truncate(60)}")
        },
        Err(e) => io.println("Failed: ${e}"),
    }
}
```

```text
Title: Understanding Bramble ownership
Body preview: Ownership in Bramble is lightweight compared to systems langua
```

Two things worth noting: `map_err` transforms the error type so both `?` operators produce the same `str` error, and `.truncate(60)` is a `str` method that caps the output length.

## Fetching a list

When the endpoint returns a JSON array, use `parse_as<List<T>>`.

```bramble
let posts = json.parse_as<List<Post>>(resp.body)?
io.println("Fetched ${posts.len()} posts")
for post in posts {
    io.println("  - ${post.title}")
}
```

> [!TIP]
> For more advanced JSON work — optional fields, nested objects, custom deserialisation — see the [`std/json` reference](xref:bramble.reference.stdlib.json).

With network access and JSON parsing working together, you are ready to start thinking about quality. Continue to [Testing your code](xref:bramble.tutorials.testing-your-code) to learn how to write tests that keep your programs working as they grow.

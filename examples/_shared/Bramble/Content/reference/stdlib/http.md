---
title: "std/http"
description: "Reference for the std/http module, covering HTTP client functions, request and response types, headers, and status codes."
uid: bramble.reference.stdlib.http
order: 270
sectionLabel: "Standard library"
tags: [stdlib, http, networking, requests]
---

The `std/http` module provides an HTTP client for making network requests. All functions return `Result` types, and network access requires the `net` capability to be granted at runtime — see [The sandbox and security](xref:bramble.explanation.the-sandbox-and-security) for details.

## Importing

```bramble
import std/http
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `get` | `(url: str, opts: Option<RequestOptions>) -> Result<Response, HttpError>` | Sends a GET request to the given URL. |
| `post` | `(url: str, body: str, opts: Option<RequestOptions>) -> Result<Response, HttpError>` | Sends a POST request with a string body. |
| `put` | `(url: str, body: str, opts: Option<RequestOptions>) -> Result<Response, HttpError>` | Sends a PUT request with a string body. |
| `delete` | `(url: str, opts: Option<RequestOptions>) -> Result<Response, HttpError>` | Sends a DELETE request. |
| `request` | `(req: Request) -> Result<Response, HttpError>` | Sends a fully constructed `Request` value. |
| `Response.status` | `(self: Response) -> i64` | Returns the numeric HTTP status code. |
| `Response.body` | `(self: Response) -> str` | Returns the response body as a string. |
| `Response.header` | `(self: Response, name: str) -> Option<str>` | Returns the value of a named response header, if present. |

## Types

### Request

`Request` is a record type for constructing requests manually:

```bramble
let req = http.Request {
    method: "PATCH",
    url: "https://api.example.test/items/42",
    headers: {"Content-Type": "application/json", "Authorization": "Bearer ${token}"},
    body: Option.Some("{\"done\": true}")
}
```

### RequestOptions

`RequestOptions` holds optional configuration shared by the convenience functions:

| Field | Type | Description |
|---|---|---|
| `headers` | `{str: str}` | Additional request headers. |
| `timeout_ms` | `Option<i64>` | Request timeout in milliseconds. |
| `follow_redirects` | `bool` | Whether to follow HTTP redirects. Defaults to `true`. |

### HttpError

`HttpError` is a union type with variants `Timeout`, `ConnectionFailed(str)`, and `InvalidUrl(str)`. Non-2xx responses are **not** errors — check `Response.status` yourself.

## Example: fetching JSON

```bramble
import std/http
import std/json

fn fetch_item(id: i64) -> Result<Item, str> {
    let resp = http.get("https://api.example.test/items/${id}", Option.None)?
    if resp.status() != 200 {
        return Result.Err("unexpected status: ${resp.status()}")
    }
    let item = json.parse(resp.body())?
    Result.Ok(item)
}
```

```text
Item { id: 42, name: "Sprocket", done: false }
```

> [!WARNING]
> Running any `std/http` function without the `net` capability raises a runtime `CapabilityError` immediately. Grant the capability with `--allow-net` or via your host configuration before calling any HTTP function.

## Related

- [std/json](xref:bramble.reference.stdlib.json) — parse and serialize JSON bodies
- [Fetching data tutorial](xref:bramble.tutorials.fetching-data)

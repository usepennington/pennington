---
title: "A web service"
description: "Build a small HTTP service with std/http, define routes that return JSON, and run it with bramble run."
uid: bramble.tutorials.a-web-service
order: 120
sectionLabel: "Tutorials"
tags: [http, web-service, json, routes, capstone]
---

This is the capstone tutorial. You will build a small HTTP service that stores items in memory and exposes them over a JSON API. Along the way, you will see how Bramble's standard library composes everything you have learned — records, Result, collections, and JSON — into a working server.

## Setting up the project

Create a new directory and initialise a Thicket manifest.

```bash
mkdir bramble-notes-api
cd bramble-notes-api
thicket init
```

This tutorial uses only the standard library, so no additional dependencies are needed.

## Defining the data model

```bramble
import std/json

record Note {
    id:   i64,
    text: str,
}

// A shared mutable store — in a real service you'd use a database.
let mut notes: List<Note> = []
let mut next_id: i64 = 1
```

## Creating the router

`std/http` ships a lightweight router. You define routes by method and path, and each handler receives a `Request` and returns a `Response`.

```bramble
import std/http
import std/json
import std/io

fn list_notes(req: http.Request) -> http.Response {
    match json.stringify(notes) {
        Ok(body) => http.Response.json(body),
        Err(e)   => http.Response.internal_error("Serialise failed: ${e}"),
    }
}

fn create_note(req: http.Request) -> http.Response {
    match json.parse_as<Note>(req.body) {
        Ok(note) => {
            let stored = Note { id: next_id, text: note.text }
            notes = notes.push(stored)
            next_id = next_id + 1
            match json.stringify(stored) {
                Ok(body) => http.Response.created(body),
                Err(e)   => http.Response.internal_error(e),
            }
        },
        Err(e) => http.Response.bad_request("Invalid JSON: ${e}"),
    }
}

fn get_note(req: http.Request) -> http.Response {
    let id = req.params.get("id")
                       .and_then(|s| s.parse_i64())
                       .unwrap_or(-1)

    match notes.find(|n| n.id == id) {
        Some(note) => match json.stringify(note) {
            Ok(body) => http.Response.json(body),
            Err(e)   => http.Response.internal_error(e),
        },
        None => http.Response.not_found("Note not found"),
    }
}
```

## Wiring up main

```bramble
fn main() -> Result<(), str> {
    let router = http.Router.new()
        .get("/notes",      list_notes)
        .post("/notes",     create_note)
        .get("/notes/:id",  get_note)

    let server = http.Server { port: 8080, router: router }

    io.println("Listening on http://localhost:8080")
    server.serve().map_err(|e| "Server error: ${e}")
}
```

## Running and testing the service

Start the server in one terminal.

```bash
bramble run main.bramble
```

```text
Listening on http://localhost:8080
```

In a second terminal, exercise the API.

```bash
curl -s -X POST http://localhost:8080/notes \
     -H "Content-Type: application/json" \
     -d '{"text": "Learn Bramble"}'

curl -s http://localhost:8080/notes

curl -s http://localhost:8080/notes/1
```

```text
{"id":1,"text":"Learn Bramble"}

[{"id":1,"text":"Learn Bramble"}]

{"id":1,"text":"Learn Bramble"}
```

> [!NOTE]
> This server uses in-memory storage — data is lost when the process exits. For persistent storage, use `std/fs` to read and write a JSON file on each request, or reach for a database package on [thicket.dev](https://thicket.dev).

## Where to go next

You have completed the Bramble tutorial series. Here is a map of what to explore next depending on your goals.

- **Deeper language features** — the how-to guides cover [error handling](xref:bramble.how-to.language.handle-errors-with-result), [pattern matching on records](xref:bramble.how-to.language.pattern-match-on-records), and [concurrency](xref:bramble.how-to.language.run-work-concurrently).
- **Distributing your service** — [Packaging with Thicket](xref:bramble.tutorials.packaging-with-thicket) showed how to build a release binary and publish a package.
- **Understanding the runtime** — the explanation section covers the [bytecode VM](xref:bramble.explanation.the-bytecode-vm), [garbage collection](xref:bramble.explanation.garbage-collection), and the [sandbox and security model](xref:bramble.explanation.the-sandbox-and-security).

Bramble grows on you. Possibly invasive.

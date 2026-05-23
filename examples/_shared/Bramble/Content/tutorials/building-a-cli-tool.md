---
title: "Building a CLI tool"
description: "Parse command-line arguments with std/os, build a small to-do list CLI, and structure a multi-function Bramble program."
uid: bramble.tutorials.building-a-cli-tool
order: 100
sectionLabel: "Tutorials"
tags: [cli, args, program-structure, to-do, os]
---

You now know enough Bramble to build something genuinely useful. This tutorial walks through a small command-line to-do manager: `todo`. It reads and writes a JSON file, parses subcommands, and shows how to structure a program that does more than a single task.

## Parsing arguments

`std/os` exposes `os.args()` which returns `List<str>` — the raw command-line tokens starting from the first user-supplied argument (the binary name is excluded automatically).

```bramble
import std/os
import std/io

fn main() {
    let args = os.args()
    if args.len() == 0 {
        io.println("Usage: todo <add|list|done> [args]")
        return
    }
    io.println("Subcommand: ${args.first().unwrap()}")
}
```

## Defining the data model

The to-do list is stored as a JSON file. Define a record for each item.

```bramble
import std/json

record TodoItem {
    id:   i64,
    text: str,
    done: bool,
}

fn load_items(path: str) -> Result<List<TodoItem>, str> {
    match std/fs.read_text(path) {
        Ok(text) => json.parse_as<List<TodoItem>>(text)
                        .map_err(|e| "JSON parse error: ${e}"),
        Err(_)   => Ok([]),   // treat missing file as empty list
    }
}

fn save_items(path: str, items: List<TodoItem>) -> Result<(), str> {
    let text = json.stringify(items).map_err(|e| "Serialise error: ${e}")?
    std/fs.write_text(path, text).map_err(|e| "Write error: ${e}")
}
```

## Implementing the subcommands

Break each subcommand into its own function that takes the item list and any extra arguments, and returns an updated list.

```bramble
fn cmd_add(items: List<TodoItem>, text: str) -> List<TodoItem> {
    let next_id = items.last().map(|i| i.id + 1).unwrap_or(1)
    items.push(TodoItem { id: next_id, text: text, done: false })
}

fn cmd_list(items: List<TodoItem>) {
    if items.len() == 0 {
        io.println("Nothing to do!")
        return
    }
    for item in items {
        let marker = if item.done { "x" } else { " " }
        io.println("[${marker}] ${item.id}. ${item.text}")
    }
}

fn cmd_done(items: List<TodoItem>, id: i64) -> List<TodoItem> {
    items.map(|item| {
        if item.id == id { TodoItem { ..item, done: true } }
        else { item }
    })
}
```

`TodoItem { ..item, done: true }` is a record update expression — it copies all fields from `item` except the ones you explicitly override. This is a concise, safe way to produce a modified copy without mutating the original.

## Wiring it together in main

```bramble
import std/os
import std/io

fn main() -> Result<(), str> {
    let db    = "todo.json"
    let args  = os.args()
    let mut items = load_items(db)?

    match args.first().map(|s| s as str) {
        Some("add")  => {
            let text = args.drop(1).join(" ")
            items = cmd_add(items, text)
            save_items(db, items)?
        },
        Some("list") => cmd_list(items),
        Some("done") => {
            let id = args.get(1)
                         .and_then(|s| s.parse_i64())
                         .ok_or("done requires a numeric id")?
            items = cmd_done(items, id)
            save_items(db, items)?
        },
        _ => io.println("Usage: todo <add|list|done> [args]"),
    }

    Ok(())
}
```

## Trying it out

```bash
bramble run todo.bramble add "Write more Bramble"
bramble run todo.bramble add "Publish to Thicket"
bramble run todo.bramble list
bramble run todo.bramble done 1
bramble run todo.bramble list
```

```text
[ ] 1. Write more Bramble
[ ] 2. Publish to Thicket

[x] 1. Write more Bramble
[ ] 2. Publish to Thicket
```

> [!TIP]
> For a more structured argument-parsing experience — flags, option values, help text generation — reach for the `bramble-cli` package on [thicket.dev](https://thicket.dev). You will add a Thicket dependency in the next tutorial.

Continue to [Packaging with Thicket](xref:bramble.tutorials.packaging-with-thicket) to turn this program into a proper distributable package.

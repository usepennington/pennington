---
title: "Working with files"
description: "Read and write files using std/fs and std/io, and handle errors the Bramble way with Result and match."
uid: bramble.tutorials.working-with-files
order: 70
sectionLabel: "Tutorials"
tags: [files, io, result, error-handling, fs]
---

File operations can fail — the path might not exist, permissions might be wrong, the disk might be full. Bramble models this honestly: every fallible file operation returns a `Result`, and the compiler forbids you from ignoring it. This section shows the two most common patterns: reading a whole file and writing one.

## Reading a file

`std/fs` provides `read_text(path)` which returns `Result<str, IoError>`.

```bramble
import std/fs
import std/io

fn main() {
    match fs.read_text("notes.txt") {
        Ok(contents) => io.println(contents),
        Err(e)       => io.println("Could not read file: ${e}"),
    }
}
```

The `match` forces you to handle both cases. If `notes.txt` exists and is readable, `contents` is its text. If not, `e` carries the error — a value you can inspect, log, or propagate.

Create a test file and run this to see it in action.

```bash
echo "Hello from a file!" > notes.txt
bramble run read_file.bramble
```

```text
Hello from a file!
```

## Propagating errors with `?`

When you are inside a function that itself returns `Result`, the `?` operator lets you propagate errors without writing `match` at every step.

```bramble
import std/fs
import std/io

fn count_lines(path: str) -> Result<i64, str> {
    let text  = fs.read_text(path)?           // returns Err early if it fails
    let lines = text.lines().len()
    Ok(lines)
}

fn main() {
    match count_lines("notes.txt") {
        Ok(n)  => io.println("${n} lines"),
        Err(e) => io.println("Error: ${e}"),
    }
}
```

`?` unwraps the `Ok` value or returns the `Err` immediately from the enclosing function. The main function is where you typically handle the error, not deep inside helpers.

> [!IMPORTANT]
> `?` only works inside functions whose return type is `Result` (or `Option`). Using it in `main()` requires `main` to declare `-> Result<(), str>`. See [Your first program](xref:bramble.tutorials.your-first-program) for the `main` signature with a `Result` return.

## Writing a file

`fs.write_text(path, contents)` creates or overwrites a file.

```bramble
import std/fs
import std/io

fn main() {
    let log_entry = "Build succeeded at 2025-09-14T10:23:00Z\n"

    match fs.write_text("build.log", log_entry) {
        Ok(())  => io.println("Log written"),
        Err(e)  => io.println("Write failed: ${e}"),
    }
}
```

## Appending to a file

To append rather than overwrite, use `fs.open` with `OpenOptions`.

```bramble
import std/fs
import std/io

fn main() {
    let opts = fs.OpenOptions { append: true, create: true }
    match fs.open("events.log", opts) {
        Ok(mut file) => {
            file.write_line("event: user_login")?
            io.println("Appended.")
        },
        Err(e) => io.println("Open failed: ${e}"),
    }
}
```

The `?` inside the `Ok` arm works here because the enclosing lambda inherits the `Result` return type from `main`.

For a deeper look at how `Result` and `Option` work in Bramble, see the how-to guide on [handling errors with Result](xref:bramble.how-to.language.handle-errors-with-result). Next up: [Fetching data](xref:bramble.tutorials.fetching-data), where you will pull content from the network and parse it as JSON.

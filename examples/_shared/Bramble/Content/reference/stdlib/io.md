---
title: "std/io"
description: "Reference for the std/io module, covering console output, user input, and the standard stream handles."
uid: bramble.reference.stdlib.io
order: 240
sectionLabel: "Standard library"
tags: [stdlib, io, console]
---

The `std/io` module handles reading from stdin and writing to stdout and stderr. It is the primary interface for interactive programs and for emitting diagnostic output at runtime.

## Importing

```bramble
import std/io
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `print` | `(s: str) -> unit` | Writes `s` to stdout without a trailing newline. |
| `println` | `(s: str) -> unit` | Writes `s` to stdout followed by a newline. |
| `eprint` | `(s: str) -> unit` | Writes `s` to stderr without a trailing newline. |
| `eprintln` | `(s: str) -> unit` | Writes `s` to stderr followed by a newline. |
| `read_line` | `() -> Result<str, IoError>` | Reads one line from stdin, stripping the trailing newline. Returns `Err` on EOF or read failure. |
| `read_all` | `() -> Result<str, IoError>` | Reads all of stdin until EOF and returns it as a single string. |
| `flush` | `(stream: Stream) -> Result<unit, IoError>` | Flushes the write buffer for `stream`. |

## Stream constants

| Constant | Type | Description |
|---|---|---|
| `io.stdin` | `Stream` | Handle to the standard input stream. |
| `io.stdout` | `Stream` | Handle to the standard output stream. |
| `io.stderr` | `Stream` | Handle to the standard error stream. |

`Stream` is an opaque handle type. It is passed to `flush` and to lower-level write functions in [std/fs](xref:bramble.reference.stdlib.fs).

## Example

A simple read-evaluate-print loop that echoes lines back to the user until EOF.

```bramble
import std/io

fn main() {
    io.println("Enter lines (Ctrl-D to stop):")
    loop {
        match io.read_line() {
            Ok(line) -> io.println("you said: ${line}")
            Err(_)   -> break
        }
    }
    io.println("done")
}
```

```text
Enter lines (Ctrl-D to stop):
you said: hello
you said: bramble
done
```

## Flushing

Stdout is line-buffered when connected to a terminal and block-buffered when redirected to a file or pipe. Call `io.flush(io.stdout)` before reading from stdin or before a long-running operation when you need output to appear immediately.

```bramble
import std/io

fn prompt(msg: str) -> Result<str, io.IoError> {
    io.print(msg)
    io.flush(io.stdout)?
    io.read_line()
}
```

## Error type

`IoError` carries a `kind` field of enum type `IoErrorKind` with variants `EndOfFile`, `PermissionDenied`, `BrokenPipe`, and `Other(str)`. Inspect it with `match` for fine-grained error handling.

> [!WARNING]
> `read_line` strips only the platform-native line ending. On Windows-style streams the returned string may still contain a trailing `\r` if the stream was opened in binary mode.

## Related

- [std/fs](xref:bramble.reference.stdlib.fs)
- [Handling errors with Result](xref:bramble.how-to.language.handle-errors-with-result)

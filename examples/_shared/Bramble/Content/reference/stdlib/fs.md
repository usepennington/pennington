---
title: "std/fs"
description: "Reference for the std/fs module, covering file reading, writing, directory listing, and open options."
uid: bramble.reference.stdlib.fs
order: 250
sectionLabel: "Standard library"
tags: [stdlib, fs, filesystem]
---

The `std/fs` module provides filesystem operations: reading and writing files, querying existence, listing directories, and opening files with fine-grained control. Every function that touches the filesystem returns a `Result`, so failures propagate explicitly rather than through exceptions.

## Importing

```bramble
import std/fs
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `read_text` | `(path: str) -> Result<str, FsError>` | Reads the entire file at `path` as a UTF-8 string. |
| `read_bytes` | `(path: str) -> Result<[i64], FsError>` | Reads the entire file at `path` as a list of raw bytes. |
| `write_text` | `(path: str, content: str) -> Result<unit, FsError>` | Creates or overwrites the file at `path` with `content`. |
| `write_bytes` | `(path: str, data: [i64]) -> Result<unit, FsError>` | Creates or overwrites the file at `path` with raw bytes. |
| `append_text` | `(path: str, content: str) -> Result<unit, FsError>` | Appends `content` to the file at `path`, creating it if absent. |
| `exists` | `(path: str) -> bool` | Returns `true` if the path exists (file or directory). |
| `is_file` | `(path: str) -> bool` | Returns `true` if the path exists and is a regular file. |
| `is_dir` | `(path: str) -> bool` | Returns `true` if the path exists and is a directory. |
| `remove` | `(path: str) -> Result<unit, FsError>` | Deletes the file at `path`. |
| `remove_dir` | `(path: str) -> Result<unit, FsError>` | Deletes the empty directory at `path`. |
| `remove_dir_all` | `(path: str) -> Result<unit, FsError>` | Deletes the directory at `path` and all of its contents. |
| `list_dir` | `(path: str) -> Result<[DirEntry], FsError>` | Returns entries in the directory at `path` (non-recursive). |
| `create_dir` | `(path: str) -> Result<unit, FsError>` | Creates a new directory at `path`. |
| `create_dir_all` | `(path: str) -> Result<unit, FsError>` | Creates `path` and any missing parent directories. |
| `copy` | `(src: str, dst: str) -> Result<unit, FsError>` | Copies the file at `src` to `dst`. |
| `rename` | `(src: str, dst: str) -> Result<unit, FsError>` | Moves or renames `src` to `dst`. |
| `open` | `(path: str, opts: OpenOptions) -> Result<File, FsError>` | Opens the file at `path` with the given options, returning a `File` handle. |

## The `DirEntry` type

`list_dir` returns a list of `DirEntry` records:

| Field | Type | Description |
|---|---|---|
| `name` | `str` | File or directory name (not the full path). |
| `path` | `str` | Full path to the entry. |
| `is_file` | `bool` | `true` if the entry is a regular file. |
| `is_dir` | `bool` | `true` if the entry is a directory. |
| `size` | `i64` | Size in bytes (0 for directories). |

## `OpenOptions`

`OpenOptions` controls how `open` behaves. Construct one with the struct literal syntax and pass it to `fs.open`.

| Field | Type | Default | Description |
|---|---|---|---|
| `read` | `bool` | `true` | Open for reading. |
| `write` | `bool` | `false` | Open for writing. |
| `append` | `bool` | `false` | Seek to end before each write. |
| `create` | `bool` | `false` | Create the file if it does not exist. |
| `truncate` | `bool` | `false` | Truncate the file to zero length on open. |

## Example

List all `.brb` files in a directory and print their sizes.

```bramble
import std/fs
import std/strings

fn list_scripts(dir: str) -> Result<unit, fs.FsError> {
    let entries = fs.list_dir(dir)?
    for entry in entries {
        if entry.is_file and strings.ends_with(entry.name, ".brb") {
            std/io.println("${entry.name}  (${entry.size} bytes)")
        }
    }
    Ok(())
}

match list_scripts("./src") {
    Ok(_)    -> {}
    Err(e)   -> std/io.eprintln("error: ${e}")
}
```

```text
main.brb  (1042 bytes)
utils.brb  (388 bytes)
```

> [!WARNING]
> `remove_dir_all` is irreversible and operates recursively without confirmation. Validate the path before calling it, especially when the path is constructed from user input.

## Related

- [std/io](xref:bramble.reference.stdlib.io)
- [Working with files tutorial](xref:bramble.tutorials.working-with-files)
- [Handling errors with Result](xref:bramble.how-to.language.handle-errors-with-result)

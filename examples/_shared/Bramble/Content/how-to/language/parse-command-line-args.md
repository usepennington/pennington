---
title: "Parse command-line arguments"
description: "Read raw arguments from std/os and apply a flag-parsing pattern to handle options and positional arguments."
uid: bramble.how-to.language.parse-command-line-args
order: 180
sectionLabel: "Language"
tags: [cli, arguments, os, parsing]
---

`std/os` exposes the process's argument list as `os::args()`, which returns a `list<str>` starting from index 0 (the program name). Bramble's standard library does not ship a flag-parsing framework — the pattern below covers the common case without a dependency.

## Read raw arguments

```bramble
import std/os
import std/io

fn main() {
    let args = os::args()
    io::println("program: ${args[0]}")
    io::println("arg count: ${args.len() - 1}")
}
```

Running `bramble run . greet --name Alice` prints:

```text
program: greet
arg count: 2
```

## Parse flags and positional arguments

Iterate the argument list, consuming known flags with their values and collecting the remainder as positionals.

```bramble
import std/os

record Args {
    name: Option<str>,
    verbose: bool,
    positionals: list<str>,
}

fn parse_args(raw: list<str>) -> Result<Args, str> {
    let mut name: Option<str> = None
    let mut verbose = false
    let mut positionals: list<str> = []
    let mut i = 1  // skip program name

    while i < raw.len() {
        match raw[i] {
            "--name" => {
                i = i + 1
                if i >= raw.len() {
                    return Err("--name requires a value")
                }
                name = Some(raw[i])
            },
            "--verbose" | "-v" => {
                verbose = true
            },
            flag if flag.starts_with("--") => {
                return Err("unknown flag: ${flag}")
            },
            arg => {
                positionals.push(arg)
            },
        }
        i = i + 1
    }

    Ok(Args { name, verbose, positionals })
}
```

## Use the parsed struct

```bramble
import std/io

fn main() -> Result<(), str> {
    let args = parse_args(os::args())?

    let greeting_name = args.name.unwrap_or("stranger")
    if args.verbose {
        io::println("verbose mode on")
    }
    io::println("Hello, ${greeting_name}!")

    for path in args.positionals {
        io::println("processing: ${path}")
    }
    Ok(())
}
```

## Print a usage message

A simple `usage()` function that writes to stderr is enough for most tools.

```bramble
fn usage(program: str) {
    io::eprintln("Usage: ${program} [--name NAME] [-v] [files...]")
    io::eprintln("  --name NAME   name to greet")
    io::eprintln("  -v, --verbose enable verbose output")
}
```

Call it before returning an `Err` from `parse_args`, or when a `--help` flag is detected.

> [!NOTE]
> For a full-featured CLI application with subcommands, completion, and generated help text, see the `bramble-cli` package on Thicket. The pattern above is intentionally self-contained for scripts that cannot take dependencies.

The full tutorial for building a CLI tool end-to-end — including Thicket packaging — is at [building a CLI tool](xref:bramble.tutorials.building-a-cli-tool). The `os::args`, `os::env`, and process-exit functions are documented in the [os standard library reference](xref:bramble.reference.stdlib.os).

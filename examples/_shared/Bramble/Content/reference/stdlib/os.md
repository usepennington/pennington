---
title: "std/os"
description: "Reference for the std/os module, covering process arguments, environment variables, working directory, platform detection, and process exit."
uid: bramble.reference.stdlib.os
order: 320
sectionLabel: "Standard library"
tags: [stdlib, os, environment, cli]
---

The `std/os` module exposes information about the host process and operating environment. Most functions are always available, but those that read or modify environment variables require the `env` capability, and `cwd` requires the `fs` capability. See [The sandbox and security](xref:bramble.explanation.the-sandbox-and-security) for the full capability model.

## Importing

```bramble
import std/os
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `args` | `() -> [str]` | Returns the list of command-line arguments passed to the script, excluding the script name. |
| `env` | `() -> {str: str}` | Returns a snapshot of all environment variables as a map. Requires `env` capability. |
| `getenv` | `(name: str) -> Option<str>` | Returns the value of a single environment variable, or `Option.None`. Requires `env` capability. |
| `setenv` | `(name: str, value: str) -> ()` | Sets an environment variable for the current process. Requires `env` capability. |
| `cwd` | `() -> str` | Returns the current working directory as an absolute path string. Requires `fs` capability. |
| `platform` | `() -> str` | Returns a lowercase platform identifier: `"linux"`, `"macos"`, or `"windows"`. |
| `exit` | `(code: i64) -> never` | Terminates the process with the given exit code. |

## Example: reading arguments and an environment variable

```bramble
import std/os

fn main() {
    let args = os.args()
    if args.len() < 1 {
        println("usage: script <name>")
        os.exit(1)
    }

    let greeting = os.getenv("GREETING").unwrap_or("Hello")
    println("${greeting}, ${args[0]}!")
}
```

```text
$ GREETING=Hi bramble run greet.brb World
Hi, World!
```

## Example: platform-conditional behaviour

```bramble
import std/os

let sep = match os.platform() {
    "windows" => "\\"
    _ => "/"
}
println("path separator is: ${sep}")
```

## Capability summary

| Function | Required capability |
|---|---|
| `args` | none |
| `platform` | none |
| `exit` | none |
| `getenv`, `env`, `setenv` | `env` |
| `cwd` | `fs` |

Grant capabilities with the `--allow-env` and `--allow-fs` flags, or configure them in your host's `bramble.toml`.

## Related

- [Parse command-line args](xref:bramble.how-to.language.parse-command-line-args) — structured argument parsing patterns
- [Building a CLI tool tutorial](xref:bramble.tutorials.building-a-cli-tool)

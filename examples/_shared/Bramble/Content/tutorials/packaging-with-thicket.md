---
title: "Packaging with Thicket"
description: "Create a bramble.toml manifest, add a dependency from thicket.dev, build a release binary, and publish your own package."
uid: bramble.tutorials.packaging-with-thicket
order: 110
sectionLabel: "Tutorials"
tags: [thicket, packaging, dependencies, publish, bramble-toml]
---

Thicket is Bramble's package manager. It resolves dependencies, fetches them from the registry at [thicket.dev](https://thicket.dev), and records exact versions in a lockfile so builds are reproducible. This tutorial adds Thicket to the to-do CLI you built in the previous lesson.

## Initialising a manifest

Inside your project directory, run:

```bash
thicket init
```

Thicket creates `bramble.toml` with sensible defaults based on the directory name.

```toml
[package]
name    = "todo"
version = "0.1.0"
edition = "1.2"

[dependencies]
```

The `edition` field pins the language edition your code targets. Use `"1.2"` for all stable Bramble features covered in these tutorials.

## Adding a dependency

The `bramble-cli` package provides structured argument parsing. Add it with:

```bash
thicket add bramble-cli
```

Thicket fetches the latest compatible version, updates `bramble.toml`, and writes `thicket.lock`.

```toml
[dependencies]
bramble-cli = "^2.1"
```

The `^` prefix means "compatible with 2.1" — any `2.x` release where `x >= 1`. Now import it in your source file.

```bramble
import bramble_cli as cli

fn main() -> Result<(), str> {
    let app = cli.App {
        name:    "todo",
        version: "0.1.0",
        about:   "A tiny to-do manager",
    }

    let matches = app
        .subcommand(cli.sub("add",  "Add a new item"))
        .subcommand(cli.sub("list", "List all items"))
        .subcommand(cli.sub("done", "Mark an item complete").arg(cli.arg("id")))
        .parse(os.args())?

    // dispatch to cmd_* functions as before …
    Ok(())
}
```

> [!NOTE]
> After adding a dependency for the first time, use `bramble run` (not `bramble run <file>`) from the project root. Thicket's resolver ensures all imports are available before compilation begins.

## Building a release binary

To compile a standalone binary optimised for distribution:

```bash
bramble build --release
```

The output lands in `dist/todo` (or `dist\todo.exe` on Windows). This binary embeds the Bramble VM and has no external runtime dependency.

## Publishing your package

Authenticate with Thicket first.

```bash
thicket login
```

Then publish from the project root.

```bash
thicket publish
```

Thicket validates `bramble.toml`, runs your tests, and uploads the package. Your package is immediately available at `thicket.dev/your-name/todo`.

Other developers can then depend on it in their own `bramble.toml`.

```toml
[dependencies]
todo = { package = "your-name/todo", version = "^0.1" }
```

## The lockfile

`thicket.lock` records the exact resolved versions of every dependency, including transitive ones. Commit it to version control. When a teammate clones the repository and runs `thicket install`, they get byte-for-byte identical packages.

```text
[[package]]
name    = "bramble-cli"
version = "2.3.1"
hash    = "sha256:4e3f9a..."
```

For details on pinning, updating, and auditing the dependency tree, see the how-to guides on [pinning and updating versions](xref:bramble.how-to.thicket.pin-and-update-versions).

You are one tutorial away from completing the series. Head to [A web service](xref:bramble.tutorials.a-web-service) for the capstone project.

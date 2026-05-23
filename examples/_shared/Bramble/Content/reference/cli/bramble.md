---
title: "bramble"
description: "Reference for the bramble CLI, the primary entrypoint for running, building, testing, and inspecting Bramble programs."
uid: bramble.reference.cli.bramble
order: 410
sectionLabel: "CLI"
tags: [cli, bramble, run, build, repl]
---

The `bramble` CLI is the main toolchain entrypoint. It compiles and runs scripts, manages the REPL, serves the language server, and surfaces bytecode inspection utilities.

## Subcommands

| Subcommand | Purpose |
|---|---|
| `run` | Execute a Bramble script or compiled artifact |
| `build` | Compile a package to a bytecode artifact (`.bramblec`) |
| `test` | Discover and run test functions in the current package |
| `fmt` | Format source files (passthrough to `sprig fmt`) |
| `repl` | Start an interactive read-eval-print loop |
| `patch` | Apply a patch script against a running REPL session |
| `lsp` | Start the Language Server Protocol daemon |
| `inspect` | Examine a compiled bytecode artifact |
| `version` | Print version information and exit |

## Global flags

| Flag | Default | Description |
|---|---|---|
| `--color` | `auto` | Force colored output: `always`, `never`, or `auto` |
| `--quiet` / `-q` | false | Suppress informational output |
| `--verbose` / `-v` | false | Emit extra diagnostic detail |
| `--config <path>` | `bramble.toml` | Override the manifest path |
| `--no-sandbox` | false | Disable the default capability sandbox |

## bramble run

Runs a script file or a compiled `.bramblec` artifact. Passes remaining arguments to the script via `std/env.args()`.

```bash
bramble run main.br
bramble run --release dist/app.bramblec -- --port 8080
bramble run --cap net src/server.br
```

The `--cap` flag grants a named capability (`net`, `fs`, `env`, `proc`) to the sandboxed script. Capabilities can be repeated.

## bramble build

Compiles the current package or a named entry point. Output defaults to `dist/<package-name>.bramblec`.

```bash
bramble build
bramble build --release --out dist/myapp.bramblec
bramble build --target wasm32
```

| Flag | Description |
|---|---|
| `--release` | Enable optimizations and strip debug symbols |
| `--out <path>` | Override the output artifact path |
| `--target <triple>` | Cross-compile to a different target triple |

## bramble test

Discovers functions annotated with `@test` in `tests/` and the current package, runs them, and reports results. Integrates with the standard `std/testing` harness.

```bash
bramble test
bramble test --filter parse_
bramble test --jobs 4
```

## bramble repl

Opens an interactive session. The `--file` flag pre-loads a source file into the session scope.

```bash
bramble repl
bramble repl --file src/utils.br
```

## bramble lsp

Starts the LSP daemon on stdio (default) or a TCP socket. Editor plugins invoke this automatically; you rarely need to run it manually.

```bash
bramble lsp
bramble lsp --tcp 127.0.0.1:6008
```

## bramble inspect

Disassembles a compiled artifact and prints bytecode, constant pools, and GC metadata.

```bash
bramble inspect dist/app.bramblec
bramble inspect --section constants dist/app.bramblec
```

```text
[module] app
  [fn] main/0
    0000  LOAD_CONST   0   ; "hello"
    0002  CALL         1
    0004  RET
  [constants]
    0: "hello"
```

## bramble version

```bash
bramble version
```

```text
bramble 1.2.4 (stable)
vm: bytecode 3, gc: generational
build: 2025-11-03 x86_64-linux
```

> **Note:** The `--no-sandbox` flag disables all capability checks. Use it only in trusted, offline environments — scripts run with `--no-sandbox` can access the filesystem, network, and subprocesses without restriction.

See [bramble.toml](xref:bramble.reference.config.bramble-toml) for package-level build configuration and [the sandbox and security](xref:bramble.explanation.the-sandbox-and-security) for capability model details.

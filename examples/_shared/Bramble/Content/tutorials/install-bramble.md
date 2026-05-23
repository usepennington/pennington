---
title: "Install Bramble"
description: "Install the Bramble toolchain on your machine, verify it works, and take your first look at the patch REPL."
uid: bramble.tutorials.install-bramble
order: 10
sectionLabel: "Tutorials"
tags: [install, toolchain, repl, getting-started]
---

Bramble ships as a single self-contained binary that includes the compiler, runtime, and the interactive REPL (called *the patch*). You can install it through the official installer script or through your platform's package manager.

## Choose your installation method

Pick the tab for your operating system. All three paths land you on the same toolchain.

```bash tabs=true title="macOS"
# Homebrew
brew install bramble

# Or use the official installer
curl -fsSL https://get.bramble.dev | sh
```

```bash tabs=true title="Linux"
# Official installer (recommended for all distros)
curl -fsSL https://get.bramble.dev | sh

# Arch Linux (AUR)
yay -S bramble
```

```bash tabs=true title="Windows"
# Winget
winget install Bramble.Bramble

# Or PowerShell installer
irm https://get.bramble.dev/ps1 | iex
```

The installer places the `bramble` binary in `~/.bramble/bin` (or `%USERPROFILE%\.bramble\bin` on Windows) and adds it to your `PATH` automatically. Open a fresh terminal after installing.

## Verify the installation

Run the version command to confirm everything landed correctly.

```bash
bramble --version
```

```text
Bramble 1.2.4 (stable)
  compiler: bramblec 1.2.4
  vm:       brambvm 1.2.4
  thicket:  1.2.2
  trellis:  1.1.0
  sprig:    1.2.1
```

If you see output like this, you are ready to write code. The version numbers for each tool may differ slightly — that is normal.

> [!NOTE]
> Bramble 2.0 is available as a preview release. These tutorials target stable **1.2**. To install the preview alongside stable, see the toolchain management docs.

## Open the patch

The interactive REPL is called *the patch*. Start it by running `bramble` with no arguments.

```bash
bramble
```

```text
Bramble 1.2.4  |  type :help for commands, :quit to exit
patch>
```

Try a quick expression to confirm it responds.

```text
patch> 1 + 1
2
patch> "hello " + "world"
"hello world"
patch> :quit
```

The patch evaluates expressions immediately and prints their values. You will use it throughout these tutorials to experiment with snippets before putting them in files.

## What got installed

Beyond the `bramble` binary itself, the installer also placed:

| Command | Role |
|---------|------|
| `bramble` | Compiler, runtime, REPL, and test runner |
| `thicket` | Package manager — installs dependencies from [thicket.dev](https://thicket.dev) |
| `trellis` | Build and task runner — reads a `Trellisfile` |
| `sprig` | Formatter and linter |

All four tools live in the same directory and share the same version cadence. You can also invoke Thicket and Trellis through `bramble thicket` and `bramble trellis` if you prefer a single entry point.

With Bramble installed and the patch confirmed, head to [Your first program](xref:bramble.tutorials.your-first-program) to write and run your first source file.

---
title: "Your first program"
description: "Write a Bramble source file, run it with bramble run, and use the patch REPL for quick experiments."
uid: bramble.tutorials.your-first-program
order: 20
sectionLabel: "Tutorials"
tags: [hello-world, bramble-run, main, io]
---

A Bramble program lives in one or more `.bramble` files. The entry point for a runnable program is a function named `main`. Let's write the smallest meaningful program and get it running.

## Create the source file

Make a new directory for this tutorial and create a file called `hello.bramble`.

```bash
mkdir hello-bramble
cd hello-bramble
```

Open `hello.bramble` in your editor and type the following.

```bramble
import std/io

fn main() {
    io.println("Hello, Bramble!")
}
```

Two things to notice: `import` brings a standard library module into scope, and `io.println` writes a line to stdout. There are no semicolons; Bramble uses newlines as statement terminators.

## Run the program

From the directory containing `hello.bramble`, run:

```bash
bramble run hello.bramble
```

```text
Hello, Bramble!
```

`bramble run` compiles the file to bytecode and executes it in a single step. You do not need to create a project manifest for a single-file script — it works as-is.

> [!TIP]
> Pass `--release` to enable optimizations: `bramble run --release hello.bramble`. For tutorial scripts the difference is imperceptible, but it is good to know the flag exists.

## Experiment in the patch

Before writing more files, fire up the patch REPL (`bramble` with no arguments) and try the same call interactively.

```text
patch> import std/io
patch> io.println("Hello from the patch!")
Hello from the patch!
patch> io.println("The answer is ${6 * 7}")
The answer is 42
```

The last line previews string interpolation: `${ }` evaluates any expression inside the braces and splices the result into the string. You will explore that properly in [Variables and values](xref:bramble.tutorials.variables-and-values).

## What `main` returns

A `main` function with no explicit return type implicitly returns `()` — the unit type, Bramble's equivalent of void. If your program needs to signal a non-zero exit code you can return `Result`.

```bramble
import std/io

fn main() -> Result<(), str> {
    io.println("Returning a Result from main is fine too")
    Ok(())
}
```

When `main` returns `Err(msg)`, Bramble prints the message to stderr and exits with code 1. You do not need this yet, but it sets up a pattern you will see often once you start reading files and making network requests.

## Check your understanding

Before moving on, try modifying `hello.bramble` to print your own name using interpolation, then re-run with `bramble run hello.bramble`. If you see your name in the output, you are in great shape for the next lesson.

Head to [Variables and values](xref:bramble.tutorials.variables-and-values) to learn how Bramble handles data.

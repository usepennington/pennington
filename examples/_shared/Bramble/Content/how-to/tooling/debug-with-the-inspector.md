---
title: "Debug with the inspector"
description: "Use bramble inspect to set breakpoints, step through execution, and examine local variables at runtime."
uid: bramble.how-to.tooling.debug-with-the-inspector
order: 520
sectionLabel: "Tooling"
tags: [debugging, inspector, breakpoints, runtime, tooling]
---

`bramble inspect` launches the Bramble VM in debug mode and opens an interactive session where you can pause execution, step through code, and query the state of any in-scope variable. It works with both scripts run directly and scripts loaded by a host application.

## Start an inspection session

Pass the entry script to `bramble inspect` the same way you would `bramble run`:

```bash
bramble inspect src/main.brm
```

The VM pauses at the first statement and prints the current source location. You'll see a prompt:

```text
Bramble Inspector 1.2.0
Paused at src/main.brm:1:1
(inspect) _
```

## Set breakpoints

Set a breakpoint by file and line number:

```bash
(inspect) break src/lib/parser.brm:42
Breakpoint 1 set at src/lib/parser.brm:42
```

List all active breakpoints:

```bash
(inspect) breaks
  1  src/lib/parser.brm:42  (active)
```

Remove a breakpoint by its number:

```bash
(inspect) clear 1
```

## Step through execution

| Command | Shorthand | Behaviour |
|---------|-----------|-----------|
| `continue` | `c` | Run until the next breakpoint or program end |
| `step` | `s` | Execute the current statement and pause at the next one |
| `next` | `n` | Like `step`, but steps over function calls |
| `finish` | `f` | Run until the current function returns |

```text
(inspect) n
Paused at src/main.brm:7:5
(inspect) s
Paused at src/lib/parser.brm:12:9
```

## Inspect local variables

At any pause point, print all locals in the current scope:

```bash
(inspect) locals
  input   : String       = "fn add(a, b) { a + b }"
  tokens  : Token[]      = [...]
  pos     : Int          = 0
```

Print a single variable by name:

```bash
(inspect) print tokens
Token[] [
  Token { kind: Fn,     span: 0..2  },
  Token { kind: Ident,  span: 3..6  },
  ...
]
```

Evaluate an arbitrary Bramble expression in the current scope:

```bash
(inspect) eval tokens.len()
14
```

> [!NOTE]
> `eval` runs in a sandboxed sub-context. It can read locals and call pure functions but cannot mutate state or perform I/O. This keeps inspection side-effect-free.

## Attach to a running host

If you are embedding the Bramble VM in a host application, start the host with the `--inspect` flag to bind the debug port:

```bash
my-host-app --bramble-inspect=5050
```

Then attach from a second terminal:

```bash
bramble inspect --attach=5050
```

For details on embedding, see [Embed the runtime](xref:bramble.how-to.tooling.embed-the-runtime). To configure editor-integrated debugging via the LSP, see [Set up editor support](xref:bramble.how-to.tooling.set-up-editor-support).

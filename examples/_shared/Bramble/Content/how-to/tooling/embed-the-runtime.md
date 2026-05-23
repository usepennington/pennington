---
title: "Embed the runtime"
description: "Instantiate the Bramble VM inside a host program, register host-side functions the script can call, and execute a script."
uid: bramble.how-to.tooling.embed-the-runtime
order: 540
sectionLabel: "Tooling"
tags: [embedding, runtime, ffi, host-functions, vm]
---

Bramble is designed to run inside larger applications — game engines, data pipelines, CLI tools — not only as standalone scripts. The embedding API lets a host program create a VM instance, expose native functions to scripts, and execute Bramble code with full sandboxing intact.

## Add the runtime library

The embedding API ships in the `bramble-rt` package. Add it to your host project's `bramble.toml` (if the host is itself a Bramble project) or link against the C ABI shared library if the host is written in another language.

For a Bramble host:

```toml
[dependencies]
bramble-rt = "1.2"
```

For a C or C++ host, link against `libbramble.so` / `bramble.dll`. The header is `bramble_rt.h`, distributed alongside the toolchain.

## Create a VM instance

```bramble
import bramble_rt { Vm, VmOptions, HostFn, Value }

let vm = Vm.new(VmOptions {
    memory_limit: 64 * 1024 * 1024,  -- 64 MiB
    instruction_limit: Option.None,   -- no CPU cap
    sandbox: true,                    -- deny all I/O by default
})
```

`sandbox: true` is the default. In sandbox mode, scripts cannot access the filesystem, network, or environment variables unless the host explicitly grants those capabilities.

## Register host functions

Host functions are Bramble closures that bridge into native code. Declare them before loading any script that references them:

```bramble
vm.register_fn("log", HostFn.new(|args: Value[]| -> Result<Value, String> {
    let message = args[0].as_string() ?? return Result.Err("expected string")
    std_print(message)          -- native host print
    Result.Ok(Value.Unit)
}))

vm.register_fn("read_config", HostFn.new(|args: Value[]| -> Result<Value, String> {
    let key = args[0].as_string() ?? return Result.Err("expected string")
    let val = host_config_lookup(key) ?? return Result.Ok(Value.option_none())
    Result.Ok(Value.from_string(val))
}))
```

Scripts import host functions via `import host/<name>`:

```bramble
import host/log { log }
import host/read_config { read_config }

let db_url = read_config("database.url") ?? "sqlite::memory:"
log("Connecting to ${db_url}")
```

## Execute a script

Load source from a file or a string and call `run`:

```bramble
let source = std/fs.read_to_string("plugin.brm")?
let result = vm.run(source)?

match result {
    Value.Int(n)    => std/io.println("Script returned int: ${n}"),
    Value.String(s) => std/io.println("Script returned: ${s}"),
    Value.Unit      => {},
    _               => std/io.println("Unexpected return type"),
}
```

`vm.run` returns `Result<Value, VmError>`. Use `?` to propagate errors to the host's own error handling, or `match` on `VmError` for structured recovery.

> [!NOTE]
> A `Vm` instance is single-threaded. To run scripts concurrently, create one `Vm` per thread or fibre. VMs do not share state unless you explicitly pass values through `register_fn` closures.

## Reset or reuse the VM

Call `vm.reset()` to clear all script-defined globals while keeping registered host functions. This is cheaper than creating a new `Vm` when you need to run many short-lived scripts in sequence.

For the full API surface — including capability grants, bytecode precompilation, and module resolution hooks — see the [runtime embedding reference](xref:bramble.reference.api.runtime-embedding). To test scripts that call host functions, see the [testing tutorials](xref:bramble.tutorials.testing-your-code).

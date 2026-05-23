---
title: "Runtime embedding API"
description: "Reference for the Bramble C embedding API, which lets host applications create a runtime, register native functions, and exchange values with Bramble scripts."
uid: bramble.reference.api.runtime-embedding
order: 610
sectionLabel: "Embedding API"
tags: [api, embedding, runtime, ffi, host-functions]
---

The embedding API exposes a C-compatible interface for hosting the Bramble VM inside another application. Link against `libbramble` and include `<bramble/bramble.h>` to access it.

## Key types and functions

| Name | Kind | Description |
|---|---|---|
| `BrambleRuntime` | opaque type | Owns the VM state, GC, and loaded modules |
| `BrambleContext` | opaque type | A single execution context (stack + locals) within a runtime |
| `BrambleValue` | tagged union | Represents any Bramble value: bool, int, float, string, list, map, `Option`, `Result`, or a host object |
| `bramble_runtime_new` | function | Allocate and initialise a new `BrambleRuntime` |
| `bramble_runtime_free` | function | Tear down a runtime and release all associated memory |
| `bramble_runtime_register_fn` | function | Bind a native C function as a callable Bramble function |
| `bramble_context_new` | function | Create a fresh execution context within a runtime |
| `bramble_context_free` | function | Release a context; the runtime is unaffected |
| `bramble_eval_string` | function | Parse and execute a Bramble source string in a context |
| `bramble_eval_file` | function | Parse and execute a Bramble source file in a context |
| `bramble_context_get` | function | Retrieve a named binding from a context's module scope |
| `bramble_context_set` | function | Inject a named binding into a context's module scope |
| `bramble_value_type` | function | Return the `BrambleType` tag of a value |
| `bramble_value_as_int` | function | Extract a 64-bit integer from a value (asserts on wrong type) |
| `bramble_value_as_str` | function | Extract a UTF-8 string pointer and length from a value |
| `bramble_value_from_int` | function | Construct a `BrambleValue` from a C `int64_t` |
| `bramble_value_from_str` | function | Construct a `BrambleValue` from a UTF-8 buffer |
| `bramble_call` | function | Call a Bramble callable value with an argument array |
| `bramble_last_error` | function | Return the last error message as a null-terminated string |

## Lifecycle

A runtime is thread-safe for reads but requires external synchronisation for concurrent mutations. Each `BrambleContext` is not thread-safe and must be used from one thread at a time.

```c
#include <bramble/bramble.h>
#include <stdio.h>

// 1. Create runtime and context
BrambleRuntime *rt  = bramble_runtime_new(NULL);  // NULL = default options
BrambleContext *ctx = bramble_context_new(rt);

// 2. Register a host function
BrambleValue host_log(BrambleContext *ctx, BrambleValue *args, int argc) {
    size_t len;
    const char *msg = bramble_value_as_str(args[0], &len);
    printf("[host] %.*s\n", (int)len, msg);
    return bramble_value_none();  // returns Option::None
}

bramble_runtime_register_fn(rt, "host_log", host_log, /*arity=*/1);

// 3. Evaluate a script
const char *src =
    "fn greet(name: str) -> str {\n"
    "    let msg = \"hello, \" + name\n"
    "    host_log(msg)\n"
    "    msg\n"
    "}\n";

if (bramble_eval_string(ctx, src, strlen(src)) != BRAMBLE_OK) {
    fprintf(stderr, "eval error: %s\n", bramble_last_error(rt));
    goto cleanup;
}

// 4. Call a Bramble function from C
BrambleValue fn = bramble_context_get(ctx, "greet");
BrambleValue arg = bramble_value_from_str("world", 5);
BrambleValue result = bramble_call(ctx, fn, &arg, 1);

size_t rlen;
printf("result: %s\n", bramble_value_as_str(result, &rlen));

cleanup:
bramble_context_free(ctx);
bramble_runtime_free(rt);
```

## Error handling

All functions that can fail return `BRAMBLE_OK` (0) on success or a non-zero `BrambleStatus` code on failure. After a non-OK return, call `bramble_last_error(rt)` for a human-readable message. Errors do not longjmp — the host retains full control of the call stack.

## Sandbox configuration

By default a hosted runtime grants no capabilities to scripts. Pass a `BrambleRuntimeOptions` struct to `bramble_runtime_new` to enable specific capabilities:

```c
BrambleRuntimeOptions opts = {
    .allow_fs  = false,
    .allow_net = false,
    .allow_env = false,
};
BrambleRuntime *rt = bramble_runtime_new(&opts);
```

See [embed the runtime how-to](xref:bramble.how-to.tooling.embed-the-runtime) for a complete worked example, and [the sandbox and security](xref:bramble.explanation.the-sandbox-and-security) for the capability model that governs what embedded scripts can access.

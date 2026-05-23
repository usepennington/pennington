---
title: "The sandbox and security model"
description: "Bramble programs run in a default-deny sandbox and must be explicitly granted capabilities to access the filesystem, network, or other sensitive resources."
uid: bramble.explanation.the-sandbox-and-security
order: 140
sectionLabel: "Explanation"
tags: [security, sandbox, capabilities, permissions]
---

A Bramble program that you run for the first time cannot read your files, open network connections, spawn processes, or access environment variables. This is the default, not an opt-in. The sandbox is enforced at the VM level, not by process isolation — there is no separate container or OS sandbox — which means the capability system is fine-grained and lightweight, but also means you are trusting the VM's implementation of the boundary.

## Capabilities, not permissions

Bramble uses a capability model rather than a permission model. The distinction matters: permissions are typically granted to a program as a whole at startup ("this program may read files"), whereas capabilities are values that can be passed, attenuated, and scoped. A function that needs to read a configuration file can receive a `ReadCap` scoped to a specific directory, rather than receiving ambient access to the whole filesystem.

```bramble
fn load_config(cap: ReadCap<"/etc/myapp">, path: string): Result<Config, IoError> {
    let text = cap.read_file(path)?
    Config::parse(text)
}
```

Calling `cap.read_file` on a path outside the cap's scope produces a runtime error. The capability type is checked at compile time; whether the capability was actually granted is checked at runtime.

## Granting capabilities at startup

The entry point receives a set of capabilities from the host environment — typically the shell or the embedding application. In a standard CLI invocation, the host grants capabilities based on flags passed to the runtime:

```text
bramble run --cap fs:read=/home/user/data --cap net:outbound=example.com main.bram
```

Inside the program, granted capabilities are available as values in `std/caps`. A program that tries to use a capability it was not granted receives a compile-time error if the type is absent, or a runtime error if the grant was narrower than expected.

## The threat model

The sandbox is designed for two scenarios. The first is running untrusted third-party scripts — plugins, automation, downloaded utilities — without giving them access to the full host environment. The second is enforcing least-privilege in first-party code: if a module only needs to read from one directory, restricting it to that directory means a bug in that module cannot exfiltrate data from elsewhere.

What the sandbox does not protect against: bugs in the VM itself, denial-of-service through CPU or memory exhaustion (there are separate resource limits for that), and side-channel attacks. Bramble is not a security boundary against a determined adversary with access to the host; it is a practical boundary against common classes of script misbehaviour.

## Capabilities in libraries

A library function that requires a capability declares it in its signature. This propagates capability requirements upward through the call graph visibly — if you add a library that needs network access, you will see the `NetCap` appear in the types of the functions you call, and you will need to thread a capability value down to it. This is deliberately explicit. It makes auditing what a piece of code can do a matter of reading types, not reading documentation.

> [!NOTE]
> Capability attenuation — narrowing a granted capability before passing it to a sub-component — is covered in the standard library reference under [std/os](xref:bramble.reference.stdlib.os).

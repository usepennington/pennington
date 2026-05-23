---
title: "std/json"
description: "Reference for the std/json module, covering JSON parsing, serialization, and typed deserialization."
uid: bramble.reference.stdlib.json
order: 260
sectionLabel: "Standard library"
tags: [stdlib, json, serialization]
---

The `std/json` module converts between JSON text and Bramble values. It supports dynamic traversal through the `Value` type and typed round-trips via `parse_into<T>` and `stringify`.

## Importing

```bramble
import std/json
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `parse` | `(src: str) -> Result<Value, JsonError>` | Parses `src` as JSON and returns a dynamic `Value`. |
| `parse_into<T>` | `(src: str) -> Result<T, JsonError>` | Parses `src` directly into a record type `T`; field names must match JSON keys. |
| `stringify` | `(val: T) -> Result<str, JsonError>` | Serializes any record, list, map, or primitive to a JSON string. |
| `stringify_pretty` | `(val: T, indent: i64) -> Result<str, JsonError>` | Serializes with newlines and `indent`-space indentation. |
| `value_to<T>` | `(v: Value) -> Result<T, JsonError>` | Converts a `Value` into a concrete record type `T`. |

## The `Value` type

`Value` is a union type representing any JSON node:

```bramble
type Value =
    | Null
    | Bool(bool)
    | Number(f64)
    | Str(str)
    | Array([Value])
    | Object({str: Value})
```

Access nested values by matching or using the helper methods:

| Method | Signature | Description |
|---|---|---|
| `Value.as_str` | `() -> Option<str>` | Returns the inner string if this is a `Str` variant. |
| `Value.as_number` | `() -> Option<f64>` | Returns the inner number if this is a `Number` variant. |
| `Value.as_bool` | `() -> Option<bool>` | Returns the inner bool if this is a `Bool` variant. |
| `Value.get` | `(key: str) -> Option<Value>` | Looks up `key` in an `Object` variant; returns `None` otherwise. |
| `Value.index` | `(i: i64) -> Option<Value>` | Returns element `i` of an `Array` variant; returns `None` otherwise. |

## Example: round-trip with a record

Define a record, serialize it to JSON, then deserialize it back.

```bramble
import std/json

record Config {
    host: str
    port: i64
    debug: bool
}

fn main() {
    let cfg = Config {
        host: "localhost"
        port: 8080
        debug: true
    }

    let text = json.stringify(cfg)?
    std/io.println(text)

    let loaded: Config = json.parse_into<Config>(text)?
    std/io.println("host=${loaded.host} port=${loaded.port}")
}
```

```text
{"host":"localhost","port":8080,"debug":true}
host=localhost port=8080
```

## Example: dynamic traversal

When the schema is unknown at compile time, parse to `Value` and navigate manually.

```bramble
import std/json

let src = "{\"user\": {\"name\": \"ada\", \"score\": 42}}"
let root = json.parse(src)?
let name = root.get("user")?.get("name")?.as_str()
match name {
    Some(n) -> std/io.println("name: ${n}")
    None    -> std/io.eprintln("name not found")
}
```

```text
name: ada
```

## `parse_into<T>` field mapping

`parse_into<T>` maps JSON object keys to record fields by exact name. Fields present in the record but absent from the JSON produce a `JsonError` unless the field type is `Option<_>`, in which case it defaults to `None`. Extra JSON keys are ignored.

> [!WARNING]
> `Number` values in `Value` are always stored as `f64`. When converting to an `i64` field via `value_to<T>` or `parse_into<T>`, Bramble truncates the fractional part. Numbers outside the safe integer range for `f64` may lose precision silently.

## Related

- [Fetching data tutorial](xref:bramble.tutorials.fetching-data)
- [std/http](xref:bramble.reference.stdlib.http)
- [Errors as values](xref:bramble.explanation.errors-as-values)

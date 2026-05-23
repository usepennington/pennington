---
title: "Sprig: opinionated formatting"
description: "Juniper Sato introduces Sprig, Bramble's formatter and linter, and makes the case for one canonical code style across every Bramble project."
date: 2024-05-18
author: Juniper Sato
tags: [sprig, tooling, formatting, linting]
uid: bramble.blog.sprig-formatter
---

Code style debates are one of the least useful ways to spend engineering time. Sprig exists so that Bramble projects don't have one. It ships with Bramble 1.0's toolchain and it has opinions.

## What Sprig does

Sprig is two tools in one binary: a formatter and a linter. The formatter rewrites your source to a canonical style. The linter catches patterns that are legal but wrong — unreachable match arms, unused bindings, `Result` values that are silently discarded.

Running both is one command:

```bash
sprig check src/
```

If you want to apply formatting fixes in place:

```bash
sprig fmt src/
```

The formatter is idempotent. Running it twice produces the same result as running it once. This sounds obvious but getting there required careful attention to how comment placement interacts with expression rewriting.

## The style

Sprig's formatting style is not configurable in the ways that matter. Indentation is four spaces. Brace placement is consistent. Alignment of match arms is automatic. Long function signatures wrap at 100 characters. These decisions were made once, deliberately, and now they're done.

```bramble
// Before sprig fmt
fn process(items:[Item],threshold:Int,label:String)->Result<Summary,String> {
  let filtered = items.filter(|i| i.score > threshold )
  if filtered.len()==0 { Err("nothing passed threshold") }
  else { Ok(Summary{ label: label, count: filtered.len() }) }
}

// After sprig fmt
fn process(items: [Item], threshold: Int, label: String) -> Result<Summary, String> {
    let filtered = items.filter(|i| i.score > threshold)
    if filtered.len() == 0 {
        Err("nothing passed threshold")
    } else {
        Ok(Summary { label: label, count: filtered.len() })
    }
}
```

The logic is identical. The second version is readable by any Bramble developer who has spent an hour with the language, without having learned this project's particular preferences.

## The linter

The linter is where I put more of my own opinions. Some highlights:

**Unused `Result`** is a warning by default and an error in strict mode. Silently dropping an error value is one of the most common sources of "it worked in testing" bugs. If you genuinely mean to ignore a failure, write `let _ = might_fail()` and the linter understands that's intentional.

**Unreachable match arms** — if you add an arm after a wildcard, Sprig will tell you.

**Shadow without `let`** — Bramble allows rebinding a name in the same scope. Sprig flags cases where this is likely accidental rather than intentional style.

> [!NOTE]
> The linter is extensible. Custom lint rules can be registered through the `[sprig]` section of your `bramble.toml`. The [Sprig CLI reference](xref:bramble.reference.cli.sprig) documents the rule API.

## Configuring what's configurable

Sprig is opinionated about style but not about which rules you enforce in your project. You can enable or disable specific lint rules, set rules to warning vs. error, and configure a small set of formatting options (line length limit, trailing comma behavior in multi-line structures). See [how to configure Sprig formatting](xref:bramble.how-to.sprig.configure-formatting) for the full picture.

What you can't configure is "two spaces instead of four" or "opening braces on their own line". Those debates don't happen in Bramble projects. That's the point.

## CI integration

Sprig exits non-zero if any errors are found. Drop `sprig check src/` into your CI pipeline and formatting drift becomes a build failure. Formatting disputes in code review become "run sprig fmt" and move on.

Install Sprig via `thicket install --global sprig`, or it comes bundled with the full Bramble toolchain install.

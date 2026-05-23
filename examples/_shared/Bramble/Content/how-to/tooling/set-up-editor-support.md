---
title: "Set up editor support"
description: "Enable completions, diagnostics, and go-to-definition in VS Code and Neovim by connecting them to the Bramble language server."
uid: bramble.how-to.tooling.set-up-editor-support
order: 510
sectionLabel: "Tooling"
tags: [lsp, editor, vscode, neovim, tooling]
---

The Bramble language server ships as part of the standard toolchain — no separate install needed. Once `bramble lsp` is on your PATH, any LSP-capable editor can connect to it for completions, hover docs, diagnostics, rename, and go-to-definition.

## Verify the language server is available

```bash
bramble lsp --version
```

This should print the same version as `bramble --version`. If the command is not found, confirm that the toolchain's `bin` directory is in your PATH (see [Install Bramble](xref:bramble.tutorials.install-bramble)).

## Configure VS Code

Install the **Bramble** extension from the extension marketplace (extension ID `bramble-lang.bramble`). The extension starts `bramble lsp` automatically on first activation.

You can override the server path or pass extra flags in your workspace `settings.json`:

```json
{
    "bramble.lsp.serverPath": "/usr/local/bin/bramble",
    "bramble.lsp.extraArgs": ["--log-level", "info"],
    "bramble.formatting.enable": true,
    "bramble.formatting.onSave": true
}
```

Setting `bramble.formatting.onSave` to `true` runs `sprig fmt` through the LSP's formatting provider whenever you save a `.brm` file.

## Configure Neovim

With [nvim-lspconfig](https://github.com/neovim/nvim-lspconfig) installed, add the following to your config:

```lua
require('lspconfig').bramble.setup({
    cmd = { 'bramble', 'lsp' },
    filetypes = { 'bramble' },
    root_dir = require('lspconfig.util').root_pattern('bramble.toml', '.git'),
    settings = {
        bramble = {
            diagnostics = { enable = true },
            formatting = { onSave = true },
        },
    },
})
```

If you use mason.nvim, the server is registered as `bramble-lsp` and can be installed with `:MasonInstall bramble-lsp`.

## Understand what the server provides

| Feature | Notes |
|---------|-------|
| Completions | Context-aware, includes stdlib and imported symbols |
| Hover documentation | Renders doc-comments as Markdown |
| Diagnostics | Type errors, unresolved imports, Sprig lint (configurable) |
| Go-to-definition | Works across package boundaries if source is available |
| Rename | Renames all references within the workspace |
| Format on save | Delegates to `sprig fmt` |

> [!NOTE]
> Sprig lint diagnostics in the editor respect the same `.sprig.toml` configuration as the CLI. Suppressions you add inline are reflected immediately.

For inspecting runtime state during execution, see [Debug with the inspector](xref:bramble.how-to.tooling.debug-with-the-inspector).

---
title: "API Keys"
description: "Managing API keys for Forge integrations"
order: 20
uid: "forge.api-keys"
section: "Documentation"
---

## Creating an API Key

Navigate to Settings > API Keys and click "Generate New Key."

## Using Your Key

```csharp
var client = new ForgeClient(apiKey: Environment.GetEnvironmentVariable("FORGE_API_KEY"));
var docs = await client.SearchDocsAsync("authentication");
```

## Key Rotation

Rotate your keys every 90 days. The old key remains valid for 24 hours after rotation.

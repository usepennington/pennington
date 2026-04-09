---
title: "Pipeline Configuration"
description: "Configure the data processing pipeline"
order: 10
section: "Documentation"
---

## Pipeline DSL

Forge uses a custom pipeline DSL for data processing configuration:

```pipeline
source "events-db"
  transform filter when status == "active"
  transform map select name, timestamp
sink "analytics-lake"
  # Write to the analytics data lake
  output format "parquet"
```

The pipeline DSL supports `source`, `transform`, `sink`, `when`, and `output` keywords.

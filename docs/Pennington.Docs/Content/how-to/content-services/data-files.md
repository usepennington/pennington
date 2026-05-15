---
title: "Use a YAML or JSON data file in pages"
description: "Register a YAML or JSON file as a typed value any Razor page or component can read through IDataFiles. The file hot-reloads when you edit it."
uid: how-to.content-services.data-files
order: 208040
sectionLabel: "Content Services"
tags: [data, yaml, json, hot-reload, content-service]
---

When a piece of site content is structured data — sponsors, navigation, schedule, feature flags, footer links — authoring it as Markdown front-matter or hand-rolling an `IContentService` is overkill. Register the file with `AddDataFile<T>` and read it through `IDataFiles`; the file hot-reloads on edit.

## Register the file

`AddDataFile<T>(name, path)` deserializes `path` into `T` on first access and tracks the file for changes. Format is chosen from the extension: `.yml` and `.yaml` go through YamlDotNet, `.json` through `System.Text.Json`. Both deserializers use camelCase property naming with case-insensitive matching, mirroring how front matter is parsed.

```csharp
builder.Services.AddDataFile<List<Sponsor>>("sponsors", "data/sponsors.yml");
builder.Services.AddDataFile<Schedule>     ("schedule", "data/schedule.yml");
builder.Services.AddDataFile<List<NavLink>>("nav",      "data/nav.json");
```

The lookup key (`"sponsors"`) is case-insensitive and must be unique across registrations. Paths are resolved against the current working directory if relative.

The target type needs a parameterless constructor — use a record with init-only properties so YamlDotNet can populate it:

```csharp
public record Sponsor
{
    public string Name { get; init; } = "";
    public string Tier { get; init; } = "";
    public string Url { get; init; } = "";
}
```

## Read the data from a Razor page

Inject `IDataFiles` and call `Get<T>` with the registered name. The lookup is keyed by name *and* type — request the same `T` you registered with.

```razor
@inject IDataFiles Data

<section>
    <h2>Sponsors</h2>
    <ul>
        @foreach (var sponsor in Data.Get<List<Sponsor>>("sponsors").OrderBy(s => s.Tier))
        {
            <li><a href="@sponsor.Url">@sponsor.Name</a> &mdash; @sponsor.Tier</li>
        }
    </ul>
</section>
```

For lookups that may not exist (optional configuration, feature-flag style toggles), `TryGet<T>` returns `false` when the name is missing or the type does not match:

```csharp
if (Data.TryGet<FooterConfig>("footer", out var footer))
{
    // render footer
}
```

## Hot reload

Each `AddDataFile<T>` call registers the file with `IFileWatcher`. When the file changes on disk, the cached value is invalidated and the next `Get<T>` call reloads and re-deserializes it. Pages that read the data through `IDataFiles` see the fresh value on the next request — no app restart needed.

This is the same lifetime model that `MarkdownContentService<T>` uses for content files. Edits during `dotnet run` propagate immediately.

## Errors

`AddDataFile` itself never reads the file; the read happens on the first `Get<T>`. The exceptions surface there:

- **`FileNotFoundException`** — the path does not exist.
- **`NotSupportedException`** — the extension is not `.yml`, `.yaml`, or `.json`.
- **`InvalidDataException`** — the file content failed to deserialize. The message includes the absolute path and the underlying serializer error.
- **`KeyNotFoundException`** — `Get<T>` was called with a name that was never registered. The message lists every registered name.
- **`InvalidCastException`** — the registered `T` does not match the requested `T`.

## What this is not

`AddDataFile` is for data that *decorates* pages (sponsors strip on the homepage, footer links, a feature flag). It does not produce routes — a `data/speakers.yml` registered this way will not give you `/speakers/jane-doe/`. For one-page-per-record needs, write a custom <xref:how-to.content-services.custom-content-service>.

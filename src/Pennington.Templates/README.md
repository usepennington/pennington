# Pennington.Templates

`dotnet new` templates for Pennington sites.

## Install

```shell
dotnet new install Pennington.Templates
```

## Templates

| Short name | What you get |
| --- | --- |
| `pennington` | Minimal Pennington host — `AddPennington` + a catch-all endpoint that renders Markdown from `Content/`. |
| `pennington-docs` | Full DocSite scaffold — `AddDocSite` with two content areas (`guides`, `reference`) and sample pages. |
| `pennington-blog` | BlogSite scaffold — `AddBlogSite` with a sample post, home/archive/tags pages, and RSS. |

## Scaffold a site

```shell
dotnet new pennington-docs -o my-docs
cd my-docs
dotnet run
```

`dotnet run` serves the site with hot reload. `dotnet run -- build <baseUrl> <outputDir>` writes a static build.

Targets .NET 11.

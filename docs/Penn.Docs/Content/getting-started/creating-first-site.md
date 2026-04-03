---
title: "Creating Your First Site"
description: "Build a complete content site from scratch using MyLittleContentEngine"
uid: "docs.getting-started.creating-first-site"
order: 1001
---

In this tutorial, you'll create your first content site using MyLittleContentEngine. By the end, you'll have a working
Blazor application that can serve markdown content and generate static HTML files.

## What You'll Build

You'll create a simple blog-style website with:

- A home page displaying recent posts
- Individual blog post pages

## Prerequisites

Before starting, ensure you have:

- .NET 9 SDK or later installed
- A code editor (Visual Studio, VS Code, or JetBrains Rider)
- Familiarity with command-line tools

<Steps>
<Step stepNumber="1">
## Create a New Blazor Project

Start by creating a new empty ASP.NET project:

```bash
dotnet new web -n MyFirstContentSite
cd MyFirstContentSite
```
</Step>

<Step stepNumber="2">

## Add MyLittleContentEngine
Add the NuGet package references to your project, ensuring you use the `--prerelease ` option:

```bash
dotnet add package MyLittleContentEngine --prerelease 
dotnet add package MyLittleContentEngine.MonorailCss --prerelease 
```

`MyLittleContentEngine` contains the core functionality for content management, while
`MyLittleContentEngine.MonorailCss` provides a simple CSS framework for styling.

The `MyLittleContentEngine.MonorailCss` package is optional, but `MyLittleContentEngine` makes a lot of assumptions
regarding styling that you'd otherwise have to unravel without it.
We'll use it in this example to keep things simple.
</Step>
<Step stepNumber="3">

## Build Your Metadata Model

Create a model to define the structure of your blog post metadata. Add a new file `BlogFrontMatter.cs`:

```csharp:xmldocid
T:MinimalExample.BlogFrontMatter
```
</Step>
<Step stepNumber="4">
## Configure the Content Engine

Open `Program.cs` and configure MyLittleContentEngine. Replace the existing content with:

```csharp:path
examples/MinimalExample/Program.cs
```
</Step>
<Step stepNumber="5">
## Create the Content Structure

Create the content directory structure to match what we defined in `WithMarkdownContentService`:

```bash
mkdir -p Content
```
</Step>
<Step stepNumber="6">
## Write Your First Blog Post

Create your first blog post at `Content/index.md`. Make sure to include the front matter at the top of the file that 
matches the `BlogFrontMatter` model you created earlier. Here's an example:

```markdown:path
examples/MinimalExample/Content/index.md
```
</Step>
<Step stepNumber="7">

## Create Your Layout

Create `Components/Layout/MainLayout.razor` to include basic styling. This uses Tailwind CSS like syntax for styling.
Here we are defining a simple layout for our blog posts, centered in the middle of the page:

```razor:path
examples/MinimalExample/Components/Layout/MainLayout.razor
```
</Step>
<Step stepNumber="8">
## Create the Home Page

Create `Components/Layout/Home.razor` to display your blog posts. Here we are calling `GetAllContentPagesAsync` to retrieve all
the blog posts and display them in a list:

```razor:path
examples/MinimalExample/Components/Layout/Home.razor
```
</Step>
<Step stepNumber="9">
## Create a Page Displaying Component

Create `Components/Layout/Pages.razor` to display individual blog posts. Here we are calling `GetRenderedContentPageByUrlOrDefault`
to retrieve the blog post by its URL:

```razor:path
examples/MinimalExample/Components/Layout/Pages.razor
```
</Step>
<Step stepNumber="10">
## Configure `dotnet watch` Support

The last step we need to do is to ensure that the content files are watched for changes during development.
Add the following to your `.csproj` file:

```xml
<ItemGroup>
    <Watch Include="Content\**\*.*" />
</ItemGroup>
```
</Step>
<Step stepNumber="11">
## Test Your Site

Run your site in development mode:

```bash
dotnet watch
```

Navigate to `https://localhost:5001` (or the URL shown in your terminal) to see your site in action!


</Step>
</Steps>

While the page is open, try editing the `Content/index.md` file. You should see the changes reflected
immediately without needing to restart the server. Not just editing, but adding, renaming and deleting files
should also work seamlessly.

## What Success Looks Like

When `dotnet watch` is running, navigate to the URL shown in your terminal (typically `http://localhost:5131`).
You'll see:

- A home page listing your blog post(s) with titles and links
- Clicking a post title takes you to the full post rendered from your Markdown file
- The page uses basic styling from MonorailCSS

Try editing `Content/index.md` and saving — the browser refreshes automatically within a second or two. Try
adding a second `.md` file in the `Content/` directory and watch it appear in the post list without a restart.

## Next Steps

- [Connecting to Roslyn](xref:docs.getting-started.connecting-to-roslyn) — embed live, verified code examples from your solution
- [Using UI Elements](xref:docs.getting-started.using-ui-elements) — add sidebar navigation and page outlines
- [Deploying to GitHub Pages](xref:docs.getting-started.deploying-to-github-pages) — publish your site automatically
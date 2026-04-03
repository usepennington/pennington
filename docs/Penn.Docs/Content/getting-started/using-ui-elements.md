---
title: "Using UI Elements"
description: "Learn how to enhance your site with pre-built UI components from MyLittleContentEngine.UI"
uid: "docs.getting-started.using-ui-elements"
order: 1030
---

MyLittleContentEngine includes a set of pre-built UI components that make it easy to create responsive layouts for your
content sites. These components handle common patterns like navigation, page outlines, and user components.

In this tutorial, you'll learn how to integrate and use the UI components to create a documentation or blog site with
sidebar navigation, page outlines, and responsive design.

## What You'll Learn

By the end of this tutorial, you'll be able to:

- Set up and configure `MyLittleContentEngine.UI` components
- Create a sidebar navigation with `TableOfContentsNavigation`
- Add page outline navigation with `OutlineNavigation`

## Prerequisites

Before starting, ensure you have:

- Completed the [Creating Your First Site](xref:docs.getting-started.creating-first-site) tutorial
- A working MyLittleContentEngine site with multiple content pages
- Basic familiarity with Blazor components and CSS

<Steps>
<Step stepNumber="1">
## Add the UI Package

First, add a reference to the MyLittleContentEngine.UI package:

```bash
dotnet add package MyLittleContentEngine.UI
```

</Step>

<Step stepNumber="2">
## Import Components and Scripts

Add the UI components to your `Components/_Imports.razor` file:

```razor
@using MyLittleContentEngine.UI.Components
```



Inject the `LinkService` at the top of your `Components/App.razor`:

```razor
@inject LinkService LinkService

```

Add the required scripts to your `Components/App.razor` file in the closing `<head>` tag, ensuring to use the
<xref:docs.guides.linking-documents-and-media> to generate the correct paths:
```razor
<script src="@LinkService.GetLink("/_content/MyLittleContentEngine.UI/scripts.js")" defer></script>
```

These scripts aren't necessary for the components to function, but they provide additional features like highlighting
the current page in the navigation.
</Step>

<Step stepNumber="3">
## Set Up TableOfContentsNavigation

The `TableOfContentsNavigation` component automatically generates navigation based on your content structure and front
matter.

Create or update your `Components/Layout/MainLayout.razor` with a sidebar navigation:

```razor:path
examples/UserInterfaceExample/Components/Layout/MainLayout.razor
```

This creates a responsive sidebar layout with sticky navigation that scrolls independently from the main content.
</Step>

<Step stepNumber="4">
## Add OutlineNavigation for Page Structure

The `OutlineNavigation` component shows the outline of the current page based on its headings (h2, h3, etc.).

Update your `Components/Layout/Pages.razor` to include page outline navigation:

```razor:path
examples/UserInterfaceExample/Components/Layout/Pages.razor
```

This creates a three-column layout: sidebar navigation, main content, and page outline.
</Step>

<Step stepNumber="5">
## Test Your UI Components

Run your site in development mode:

```bash
dotnet watch
```

Navigate to your site and you should see:

1. **Sidebar Navigation** - Auto-generated from your content structure
2. **Page Outline** - Shows headings from the current page with anchor links

Test the functionality by:

- Clicking navigation links to move between pages
- Using the page outline to jump to different sections
  </Step>

</Steps>

## What Success Looks Like

After running `dotnet watch`, your site will have a three-column layout:

- **Left column**: Sidebar navigation generated automatically from your content files and their front matter `title` values
- **Center column**: Your page content
- **Right column**: A page outline listing the headings on the current page as anchor links

Navigate between pages and notice the sidebar highlights the current page. Add more content pages and watch the
sidebar populate without any manual configuration — it reads your content structure automatically.

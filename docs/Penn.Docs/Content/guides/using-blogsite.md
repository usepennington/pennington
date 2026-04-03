---
title: "Using the BlogSite Package"
description: "Learn how to create a blog site using the BlogSite package with customizable themes, RSS feeds, and social features"
uid: "docs.getting-started.using-blogsite"
order: 2500
---

The `MyLittleContentEngine.BlogSite` package provides a complete blog site solution with minimal setup. It includes all the components, layouts, and styling needed to create a professional blog with customizable branding, RSS feeds, and social media integration.

> [!IMPORTANT]  
> While functional, the `BlogSite` package drives the documentation for my personal blog. It can and will
> change as this site changes. It is better suited as inspiration or proof-of-concepts than a blog you want total control over.

## What You'll Build

You'll create a blog site with:

- A nice little blog layout with posts and archives
- Individual blog post pages with metadata
- Tag-based organization and filtering
- RSS feed generation
- Responsive design with dark/light mode
- Social media integration
- A bit of custom branding and styling

<Steps>
<Step stepNumber="1">
## Create a New Blazor Project

Start by creating a new minimal web project:

```bash
dotnet new web -n MyBlogSite
cd MyBlogSite
```
</Step>

<Step stepNumber="2">

## Add the BlogSite Package

Add the BlogSite package reference to your project:

```bash
dotnet add package MyLittleContentEngine.BlogSite
```

This package includes all the dependencies you need:
- `MyLittleContentEngine` - Core content management functionality
- `MyLittleContentEngine.UI` - UI components for blogs
- `MyLittleContentEngine.MonorailCss` - CSS framework for styling
- `Mdazor` - Markdown rendering for Blazor
</Step>

<Step stepNumber="3">

## Configure File Watching for Development

Add the following to your `.csproj` file so content changes trigger live reload during development:

```xml
<ItemGroup>
    <Watch Include="Content/**/*.*"/>
</ItemGroup>
```
</Step>

<Step stepNumber="4">

## Configure the BlogSite

Replace the content of `Program.cs` with the following minimal configuration:

```csharp
using MyLittleContentEngine.BlogSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(_ => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "A blog about my adventures in coding",
});

var app = builder.Build();

app.UseBlogSite();

await app.RunBlogSiteAsync(args);
```

This minimal setup provides a complete blog site with default styling and layout.
</Step>

<Step stepNumber="5">

## Create the Content Structure

Create the blog content directory structure:

```bash
mkdir -p Content/Blog
```

The BlogSite package expects your blog posts to be in the `Content/Blog` directory by default.
</Step>

<Step stepNumber="6">

## Write Your First Blog Post

Create your first blog post at `Content/Blog/2024/01/welcome-to-my-blog.md`:

```bash
mkdir -p Content/Blog/2024/01
```

I like to put my blog posts in a year/month folder structure, but you can use any structure you want.

```markdown
---
title: "Welcome to My Blog"
description: "My first blog post using MyLittleContentEngine"
date: 2024-01-15
tags: ["blogging", "getting-started"]
---

# Welcome to My Blog

This is my first blog post using MyLittleContentEngine! I'm excited to share my thoughts and experiences.

## What You Can Expect

- Regular updates about my coding journey
- Tips and tricks I've learned
- Project showcases
- Technical deep-dives

Stay tuned for more content!
```
</Step>

<Step stepNumber="7">

## Customize Your Blog

You can customize various aspects of your blog by modifying the options in `Program.cs`:

```csharp
using MonorailCss;
using MyLittleContentEngine.BlogSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(_ => new BlogSiteOptions
{
    // Basic site information
    SiteTitle = "My Coding Blog",
    Description = "Adventures in software development",
    CanonicalBaseUrl = "https://myblog.example.com",
    
    // Styling and branding
    PrimaryHue = 250, // Purple theme (0-360)
    BaseColorName = ColorNames.Slate,
    DisplayFontFamily = "Inter",
    BodyFontFamily = "Inter",
    
    // Blog configuration
    AuthorName = "Your Name",
    AuthorBio = "Software developer passionate about clean code",
    EnableRss = true,
    EnableSitemap = true,
    
    // Navigation links
    MainSiteLinks = [
        new HeaderLink("About", "/about"),
        new HeaderLink("Contact", "/contact")
    ],
    
    // Advanced customization
    ExtraStyles = """
        .blog-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        """
});

var app = builder.Build();
app.UseBlogSite();
await app.RunBlogSiteAsync(args);
```
</Step>

<Step stepNumber="8">

## Add Social Media Integration

For social media features, you can add social links and project showcases:

```csharp
builder.Services.AddBlogSite(_ => new BlogSiteOptions
{
    SiteTitle = "My Coding Blog",
    Description = "Adventures in software development",

    // Social media links
    Socials = [
        new SocialLink(
            Icon: SocialIcons.BlueskyIcon,
            Url: "https://bsky.app/yourusername"
        ),
        new SocialLink(
            Icon: SocialIcons.GithubIcon,
            Url: "https://github.com/yourusername"
        )
    ],
    
    // Project showcase
    MyWork = [
        new Project(
            Title: "Awesome Library",
            Description: "A useful library for developers",
            Url: "https://github.com/yourusername/awesome-library"
        ),
        new Project(
            Title: "Cool App",
            Description: "An innovative web application",
            Url: "https://coolapp.example.com"
        )
    ],
    
    // Custom hero content for home page
    HeroContent = new HeroContent(
        Title: "Welcome to My Blog",
        Description: "Sharing my journey in software development"
    )
});
```
</Step>

<Step stepNumber="9">

## Add Custom HTML and Fonts

For advanced customization, you can add custom HTML to the head section:

```csharp
builder.Services.AddBlogSite(_ => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "A blog about coding",

    // Custom HTML for head section
    AdditionalHtmlHeadContent = """
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
        <meta name="author" content="Your Name">
        """
});
```
</Step>

<Step stepNumber="10">

## Test Your Blog Site

Run your site in development mode:

```bash
dotnet watch
```

Navigate to `https://localhost:5001` to see your blog in action!

While the page is open, try editing your blog post. You should see the changes reflected immediately without needing to restart the server.
</Step>
</Steps>

## What Success Looks Like

After running `dotnet watch`, navigate to the URL shown in your terminal (typically `http://localhost:5131`).
You'll see a blog homepage with:

- Your post listed with title, date, and description
- A sidebar with your site name, author info, and tag list
- A header with your site title and any navigation links you configured

Click through to the post and you'll see the full Markdown content rendered with your blog layout. Edit your
post file and save — the browser refreshes automatically without restarting the server.

## Blog Post Front Matter

Your blog posts support rich metadata in the front matter. The BlogSite package uses `BlogSiteFrontMatter` which includes:

```markdown
---
title: "My Post Title"
description: "A brief description of the post"
author: "Author Name"
date: 2024-01-15
tags: ["tag1", "tag2", "tag3"]
series: "optional name of a series that this post is part of"
repository: "optional repository link"
uid: "unique-identifier-for-xref-links"
is_draft: false
redirect_url: "optional-redirect-target"
---
```

### Front Matter Properties

- **title**: The title of the blog post (required)
- **description**: A brief description for SEO and summaries (required)
- **author**: Author name for the post
- **date**: Publication date (defaults to current time)
- **tags**: Array of tags for categorization
- **series**: Optional series name for grouping related posts
- **repository**: Optional repository or project link
- **uid**: Unique identifier for cross-referencing
- **is_draft**: Set to true to exclude from generation
- **redirect_url**: Optional URL to redirect this page to


## Available Configuration Options

The `BlogSiteOptions` class provides many customization options:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `SiteTitle` | string | required | The title displayed in the header |
| `Description` | string | required | Site description for SEO |
| `PrimaryHue` | int | 250 | Primary color hue (0-360) |
| `BaseColorName` | string | "Slate" | Base color palette name |
| `CanonicalBaseUrl` | string? | null | Canonical URL for SEO and RSS |
| `MainSiteLinks` | HeaderLink[] | [] | Navigation links in header/footer |
| `ContentRootPath` | string | "Content" | Path to content directory |
| `BlogContentPath` | string | "Blog" | Path to blog content (relative to ContentRootPath) |
| `BlogBaseUrl` | string | "/blog" | Base URL for blog posts |
| `TagsPageUrl` | string | "/tags" | URL for the tags page |
| `ExtraStyles` | string? | null | Additional CSS styles |
| `HeroContent` | HeroContent? | null | Custom hero content for home page |
| `DisplayFontFamily` | string? | null | Custom font family for display elements |
| `BodyFontFamily` | string? | null | Custom font family for body text |
| `AdditionalHtmlHeadContent` | string? | null | Custom HTML for head section |
| `AdditionalRoutingAssemblies` | Assembly[] | [] | List of additional assemblies to scan for routing |
| `AuthorName` | string? | null | Author name for the blog |
| `AuthorBio` | string? | null | Author bio for the blog |
| `EnableRss` | bool | true | Enable RSS feed generation |
| `EnableSitemap` | bool | true | Enable sitemap generation |
| `MyWork` | Project[] | [] | Projects to include in sidebar |
| `Socials` | SocialLink[] | [] | Social media links |
| `SolutionPath` | string? | null | Path to solution file for API docs |
| `SocialMediaImageUrlFactory` | Func<MarkdownContentPage<BlogSiteFrontMatter>, string>? | null | Function to generate social media image URLs |


## Next Steps

The BlogSite package allows you to get up and running quickly, but there are no promises made
that the design or functionality of the site will remain consistent. It's what drives my personal
blog, so as my whims change so will the package. Use it for quick proof-of-concepts, demos, or inspiration
for your own blog using the `MyLittleContentEngine` services directly.
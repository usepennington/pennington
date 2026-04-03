---
title: "Optimizing CSS and JavaScript for Production"
description: "Optimize your MyLittleContentEngine site's CSS and JavaScript assets for better performance in production deployments"
uid: "docs.guides.optimizing-css-and-javascript"
order: 2300
---

MyLittleContentEngine generates CSS dynamically at runtime using MonorailCSS and includes JavaScript from the UI
package. While this provides an excellent development experience, you might want to optimize these assets for production
deployments to improve load times and user experience.

> [!IMPORTANT]
> The optimization techniques shown here are **optional** and represent one possible approach. There are many other
> tools and strategies for asset optimization. Choose the approach that best fits your deployment pipeline and
> performance requirements.

## Understanding MyLittleContentEngine Asset Architecture

Before optimizing, it's helpful to understand how CSS and JavaScript work in MyLittleContentEngine:

### CSS Generation with MonorailCSS

MyLittleContentEngine uses a **runtime CSS generation** approach:

1. **Class Collection**: `CssClassCollectorMiddleware` scans HTML responses for CSS classes
2. **Dynamic Stylesheet**: CSS is generated on-demand at `/styles.css` containing only used classes
3. **Color Palettes**: Colors are dynamically generated from configurable hue values
4. **Purged Output**: Only CSS classes actually used in your content are included

### JavaScript from UI Package

The UI package provides JavaScript functionality in a single bundle:

- **Single File**: All JavaScript is in `scripts.js` (~48KB unminified)
- **Class-Based**: Uses ES6 classes for different features (theme switching, search, etc.)
- **Dynamic Imports**: External libraries (Mermaid, Highlight.js) loaded from CDN as needed
- **No Build Process**: Served directly without bundling or minification

## Production Optimization Example

Here's an example approach using the [tdewolff/minify](https://github.com/tdewolff/minify) tool in a GitHub Actions
workflow:

### GitHub Actions Workflow

This example shows how to minify CSS and JavaScript files after static site generation:

```yaml
name: Build and Deploy with Minification

on:
  push:
    branches: [ main ]

env:
  WEBAPP_PATH: ./docs/MyLittleContentEngine.Docs/
  WEBAPP_CSPROJ: MyLittleContentEngine.Docs.csproj

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Install .NET
        uses: actions/setup-dotnet@v4
        with:
          global-json-file: global.json

      # Generate static site with all assets
      - name: Generate static files
        env:
          ASPNETCORE_ENVIRONMENT: Production
        run: |
          dotnet build
          dotnet run --project ${{ env.WEBAPP_PATH }}${{ env.WEBAPP_CSPROJ }} \
            --configuration Release -- build "/MyLittleContentEngine/"

      # Install minification tool
      - name: Install minify
        run: |
          curl -sfL https://github.com/tdewolff/minify/releases/latest/download/minify_linux_amd64.tar.gz | tar -xzf - -C /tmp
          sudo mv /tmp/minify /usr/local/bin/

      # Minify CSS and JavaScript files in the output directory
      - name: Minify assets
        run: |
          # Find and minify all CSS files
          find "${{ env.WEBAPP_PATH }}output" -type f -name "*.css" | while read cssfile; do
            /usr/local/bin/minify -o "$cssfile" "$cssfile"
            echo "Minified $cssfile"
          done

          # Find and minify all JavaScript files  
          find "${{ env.WEBAPP_PATH }}output" -type f -name "*.js" | while read jsfile; do
            /usr/local/bin/minify -o "$jsfile" "$jsfile"
            echo "Minified $jsfile"
          done

      # Deploy optimized assets
      - name: Deploy to hosting
        run: |
          # Your deployment step here
          echo "Deploy from ${{ env.WEBAPP_PATH }}output"
```

### What This Optimization Does

The minification process provides several benefits:

#### CSS Optimization

- **Whitespace Removal**: Removes unnecessary spaces, tabs, and newlines
- **Comment Stripping**: Removes CSS comments
- **Property Optimization**: Shortens color codes and combines properties where possible
- **Size Reduction**: Typically reduces CSS size by 20-30%

#### JavaScript Optimization

- **Whitespace Removal**: Removes unnecessary formatting
- **Comment Removal**: Strips JavaScript comments
- **Variable Minification**: Shortens local variable names
- **Size Reduction**: Typically reduces JavaScript size by 15-25%

### Expected Results

For a typical MyLittleContentEngine site, you might see optimizations like:

```bash
# Before minification
styles.css: 45.2 KB
scripts.js: 48.8 KB
Total: 94.0 KB

# After minification  
styles.css: 31.7 KB (-30%)
scripts.js: 38.1 KB (-22%)
Total: 69.8 KB (-26%)
```

## Summary

Asset optimization for MyLittleContentEngine is an optional but valuable step for production deployments. The example
shown using the `minify` tool in GitHub Actions represents one straightforward approach, but many alternatives exist
depending on your specific requirements and deployment pipeline.

Key takeaways:

- **Optional Process**: Optimization is not required but can improve performance
- **Post-Build Step**: Apply optimizations after static site generation
- **Measure Impact**: Always measure the actual performance impact

The most important aspect is measuring the real-world impact of your optimizations on your users' experience rather than
just focusing on file size reductions.
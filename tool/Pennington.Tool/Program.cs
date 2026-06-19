using Pennington.BlogSite;
using Pennington.DocSite;
using Pennington.Tool;

// Tool-owned options (--root) are split out; everything else is forwarded to the Pennington
// engine, which classifies the verb (serve / build / diag) from the process command line.
var (root, forwardArgs) = ToolArgs.Resolve(args);

var configPath = Path.Combine(root, "pennington.toml");
if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"error: no 'pennington.toml' found in '{root}'.");
    Console.Error.WriteLine("Run pennington from a site folder, or pass --root=<folder>.");
    return 1;
}

// Run as if launched from the site folder so the templates' relative ContentRootPath, static
// files, and build output all resolve against it. Must precede CreateBuilder, which captures the
// content root from the current directory.
Directory.SetCurrentDirectory(root);

PenningtonToolConfig config;
try
{
    config = PenningtonToolConfig.Load(configPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

var builder = WebApplication.CreateBuilder(forwardArgs);

if (config.Template == "blog")
{
    builder.Services.AddBlogSite(config.ToBlogSiteOptions);
    var app = builder.Build();
    app.UseBlogSite();
    await app.RunBlogSiteAsync(forwardArgs);
}
else
{
    builder.Services.AddDocSite(config.ToDocSiteOptions);
    var app = builder.Build();
    app.UseDocSite();
    await app.RunDocSiteAsync(forwardArgs);
}

return Environment.ExitCode;

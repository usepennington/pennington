using Penn.DocSite;
using Penn.Roslyn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Prism",
    Description = "Source Generators for .NET",
    ContentRootPath = "Content",
});

// Note: Roslyn integration requires a valid solution.
// In testing, code blocks will show raw source if Roslyn can't load.
builder.Services.AddPennRoslyn(options =>
{
    options.SolutionPath = Path.Combine(builder.Environment.ContentRootPath, "src", "Prism.slnx");
});

var app = builder.Build();
app.UseDocSite();

await app.RunDocSiteAsync(args);

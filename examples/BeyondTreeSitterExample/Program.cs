using Pennington.DocSite;
using Pennington.TreeSitter;

var builder = WebApplication.CreateBuilder(args);

// Same DocSite host shape as the other tutorials. The extra line is
// `AddPenningtonTreeSitter` below, which lights up the `:symbol` code-fence
// modifier for *any* tree-sitter-supported language (Python, Rust, Go,
// TypeScript, …). Where `Pennington.Roslyn` resolves C#/VB by XmlDocId,
// tree-sitter resolves a member by name path across many languages.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beyond Tree-sitter",
    Description = "Pulling live multi-language snippets into docs with :symbol fences.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Beyond Tree-sitter</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
});

// Point the integration at the Samples/ folder next to this Program.cs. The
// path is resolved relative to the host's working directory (the folder that
// contains this csproj when `dotnet run` is invoked). Each `:symbol` fence body
// is `<file> > <Member.Path>`, with the file resolved under this ContentRoot.
builder.Services.AddPenningtonTreeSitter(treeSitter =>
{
    treeSitter.ContentRoot = "Samples";
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

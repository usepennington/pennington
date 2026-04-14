using Pennington.DocSite;
using Pennington.Roslyn;

var builder = WebApplication.CreateBuilder(args);

// Same DocSite host shape as the previous tutorials — the difference here is
// the extra `AddPenningtonRoslyn` line below, which lights up `:xmldocid`,
// `:xmldocid,bodyonly`, `:xmldocid-diff`, and `:path` code-fence modifiers.
// With those wired, doc markdown can reference real types and methods by
// their XmlDocId and Pennington will pull the current source straight into
// the rendered page.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beyond Roslyn",
    Description = "Pulling live code snippets into docs with xmldocid fences.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Beyond Roslyn</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
});

// Point Pennington.Roslyn at the *inner* slnx next to this Program.cs. The
// path is resolved relative to the host's working directory (the folder that
// contains this csproj when `dotnet run` is invoked), so "BeyondRoslynExample.slnx"
// is enough — no need to walk up to the repo root. The inner slnx registers
// only the `Sample` library whose types the markdown fences reference.
//
// `AddPenningtonRoslyn` takes a `RoslynOptions` action. When `SolutionPath` is
// set it registers `RoslynCodeBlockPreprocessor`, the MSBuild workspace, the
// symbol-extraction service, and the xmldoc-HTML renderer — everything needed
// for `csharp:xmldocid` fences to resolve.
builder.Services.AddPenningtonRoslyn(roslyn =>
{
    roslyn.SolutionPath = "BeyondRoslynExample.slnx";
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);

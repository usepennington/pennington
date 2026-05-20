namespace Pennington.IntegrationTests.DocsSite;

using System.Text;
using Infrastructure;
using LlmsTxt;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Eyeball smoke test — emits a sample of actual converted markdown so the
/// output quality is visible in test logs. Not an assertion test; skip in CI
/// if too noisy.
/// </summary>
[Collection(DocsTestServerCollection.Name)]
public class LlmsTxtSmokeTest
{
    private readonly DocsWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public LlmsTxtSmokeTest(DocsWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task DumpFirstMarkdownFile()
    {
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
        var files = await llmsService.GetMarkdownFilesAsync();

        files.Count.ShouldBeGreaterThan(0);

        var first = files[0];
        var text = Encoding.UTF8.GetString(first.Content);

        _output.WriteLine($"=== {first.OutputPath.Value} ({text.Length} chars) ===");
        _output.WriteLine(text.Length > 2000 ? text[..2000] + "\n...[truncated]" : text);
    }

    [Fact]
    public async Task DumpWritingMarkdownFile()
    {
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
        var files = await llmsService.GetMarkdownFilesAsync();

        var target = files.FirstOrDefault(f =>
            f.OutputPath.Value.Contains("writing-markdown-with-penn-extensions", StringComparison.OrdinalIgnoreCase));

        if (target is null)
        {
            _output.WriteLine("No writing-markdown file found. Available files:");
            foreach (var f in files.Take(20))
            {
                _output.WriteLine($"  {f.OutputPath.Value}");
            }

            return;
        }

        var text = Encoding.UTF8.GetString(target.Content);
        _output.WriteLine($"=== {target.OutputPath.Value} ({text.Length} chars) ===");
        _output.WriteLine(text);
    }

    [Fact]
    public async Task DumpLlmsTxtIndex()
    {
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
        var index = await llmsService.GetLlmsTxtAsync();

        _output.WriteLine($"=== /llms.txt ({index.Length} chars) ===");
        _output.WriteLine(index.Length > 3000 ? index[..3000] + "\n...[truncated]" : index);
    }
}
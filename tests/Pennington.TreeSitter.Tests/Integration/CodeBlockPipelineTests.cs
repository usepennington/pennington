namespace Pennington.TreeSitter.Tests.Integration;

using Microsoft.AspNetCore.Http;
using Pennington.Highlighting;
using Pennington.Markdown.Extensions;
using Pennington.TreeSitter;
using Pennington.TreeSitter.Fragments;
using Pennington.TreeSitter.Parsing;
using Pennington.TreeSitter.Preprocessing;
using Pennington.TreeSitter.Resolution;

/// <summary>
/// Drives the real <see cref="CodeBlockRenderingService"/> so the test exercises the same pipeline the markdown
/// renderer uses: preprocessor selection by priority, fragment extraction, highlighting, and HTML wrapping.
/// </summary>
public sealed class CodeBlockPipelineTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "pennington-ts-pipeline-" + Guid.NewGuid().ToString("N"));

    private CodeBlockRenderingService CreatePipeline()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(
            Path.Combine(_root, "calc.py"),
            "class Calculator:\n    def add(self, a, b):\n        return a + b\n\n    def subtract(self, a, b):\n        return a - b\n");

        var options = new TreeSitterOptions { ContentRoot = _root };
        var fragmentService = new SourceFragmentService(options, new TreeSitterParserPool(), new NamePathResolver());
        var highlighting = new HighlightingService([]);
        var preprocessor = new TreeSitterCodeBlockPreprocessor(fragmentService, highlighting, new HttpContextAccessor());

        return new CodeBlockRenderingService(highlighting, [preprocessor]);
    }

    [Fact]
    public void Symbol_fence_renders_extracted_source()
    {
        var pipeline = CreatePipeline();

        var html = pipeline.Render("calc.py > Calculator.add", "python:symbol");

        html.ShouldContain("def add(self, a, b):");
        html.ShouldContain("return a + b");
        html.ShouldContain("language-python");
    }

    [Fact]
    public void Unresolved_member_renders_error_comment()
    {
        var pipeline = CreatePipeline();

        var html = pipeline.Render("calc.py > Calculator.missing", "python:symbol");

        html.ShouldContain("// Error:");
        html.ShouldContain("not found");
    }

    [Fact]
    public void Symbol_diff_fence_renders_unified_diff()
    {
        var pipeline = CreatePipeline();

        var html = pipeline.Render(
            "calc.py > Calculator.add\ncalc.py > Calculator.subtract",
            "python:symbol-diff,bodyonly");

        html.ShouldContain("has-diff");
        html.ShouldContain("diff-remove");
        html.ShouldContain("diff-add");
    }

    [Fact]
    public void Symbol_diff_fence_requires_exactly_two_references()
    {
        var pipeline = CreatePipeline();

        var html = pipeline.Render("calc.py > Calculator.add", "python:symbol-diff");

        html.ShouldContain("requires exactly 2 references");
    }

    [Fact]
    public void Symbol_diff_fence_renders_error_for_unresolved_member()
    {
        var pipeline = CreatePipeline();

        var html = pipeline.Render(
            "calc.py > Calculator.add\ncalc.py > Calculator.missing",
            "python:symbol-diff");

        html.ShouldContain("// ");
        html.ShouldContain("not found");
    }

    [Fact]
    public void Symbol_fence_with_imports_prepends_file_imports()
    {
        var pipeline = CreatePipeline();
        File.WriteAllText(
            Path.Combine(_root, "mathy.py"),
            "import math\n\nclass Mathy:\n    def root(self, x):\n        return math.sqrt(x)\n");

        var html = pipeline.Render("mathy.py > Mathy.root", "python:symbol,imports");

        html.ShouldContain("import math");
        html.ShouldContain("def root(self, x):");
    }

    [Fact]
    public void Symbol_fence_with_signatures_elides_member_bodies()
    {
        var pipeline = CreatePipeline();
        File.WriteAllText(
            Path.Combine(_root, "Calc.cs"),
            "public class Calc\n{\n    public int Add(int a, int b)\n    {\n        return a + b;\n    }\n}\n");

        var html = pipeline.Render("Calc.cs > Calc", "csharp:symbol,signatures");

        html.ShouldContain("public int Add(int a, int b)");
        html.ShouldNotContain("return a + b");
    }

    [Fact]
    public void Plain_fence_without_symbol_modifier_is_left_to_normal_highlighting()
    {
        var pipeline = CreatePipeline();

        var html = pipeline.Render("print('hi')", "python");

        // No file lookup happened — the literal code is rendered, not an extraction error.
        html.ShouldContain("print(&#39;hi&#39;)");
        html.ShouldNotContain("// Error:");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}

namespace Pennington.TreeSitter.Tests.Fragments;

using Pennington.TreeSitter;
using Pennington.TreeSitter.Fragments;
using Pennington.TreeSitter.Parsing;
using Pennington.TreeSitter.Resolution;

public sealed class SourceFragmentServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "pennington-ts-tests-" + Guid.NewGuid().ToString("N"));

    private ISourceFragmentService CreateService()
    {
        Directory.CreateDirectory(_root);
        var options = new TreeSitterOptions { ContentRoot = _root };
        return new SourceFragmentService(options, new TreeSitterParserPool(), new NamePathResolver());
    }

    private void WriteFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    [Fact]
    public void Extracts_named_member_from_file()
    {
        var service = CreateService();
        WriteFile("calc.py", "class Calculator:\n    def add(self, a, b):\n        return a + b\n");

        var result = service.GetFragment("python", "calc.py", "Calculator.add", bodyOnly: false);

        result.Succeeded.ShouldBeTrue();
        result.Text!.ShouldContain("def add(self, a, b):");
    }

    [Fact]
    public void Empty_name_path_returns_whole_file()
    {
        var service = CreateService();
        WriteFile("data.json", """{ "a": 1 }""");

        var result = service.GetFragment("json", "data.json", string.Empty, bodyOnly: false);

        result.Succeeded.ShouldBeTrue();
        result.Text.ShouldBe("""{ "a": 1 }""");
    }

    [Fact]
    public void Missing_file_fails()
    {
        var service = CreateService();

        var result = service.GetFragment("python", "nope.py", "X", bodyOnly: false);

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("File not found");
    }

    [Fact]
    public void Path_traversal_is_rejected()
    {
        var service = CreateService();

        var result = service.GetFragment("python", "../escape.py", "X", bodyOnly: false);

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid file path");
    }

    [Fact]
    public void Unresolved_member_fails()
    {
        var service = CreateService();
        WriteFile("calc.py", "class Calculator:\n    pass\n");

        var result = service.GetFragment("python", "calc.py", "Calculator.missing", bodyOnly: false);

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("not found");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}

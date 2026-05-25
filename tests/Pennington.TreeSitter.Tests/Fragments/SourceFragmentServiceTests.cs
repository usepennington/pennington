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

        var result = service.GetFragment("python", "calc.py", "Calculator.add", FragmentOptions.Default);

        result.Succeeded.ShouldBeTrue();
        result.Text!.ShouldContain("def add(self, a, b):");
    }

    [Fact]
    public void Empty_name_path_returns_whole_file()
    {
        var service = CreateService();
        WriteFile("data.json", """{ "a": 1 }""");

        var result = service.GetFragment("json", "data.json", string.Empty, FragmentOptions.Default);

        result.Succeeded.ShouldBeTrue();
        result.Text.ShouldBe("""{ "a": 1 }""");
    }

    [Fact]
    public void Missing_file_fails()
    {
        var service = CreateService();

        var result = service.GetFragment("python", "nope.py", "X", FragmentOptions.Default);

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("File not found");
    }

    [Fact]
    public void Path_traversal_is_rejected()
    {
        var service = CreateService();

        var result = service.GetFragment("python", "../escape.py", "X", FragmentOptions.Default);

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid file path");
    }

    [Fact]
    public void Unresolved_member_fails()
    {
        var service = CreateService();
        WriteFile("calc.py", "class Calculator:\n    pass\n");

        var result = service.GetFragment("python", "calc.py", "Calculator.missing", FragmentOptions.Default);

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("not found");
    }

    [Fact]
    public void IncludeImports_prepends_csharp_usings_above_the_member()
    {
        var service = CreateService();
        WriteFile("Calc.cs", """
            using System;
            using System.Text;

            namespace Sample;

            public class Calc
            {
                public int Add(int a, int b) => a + b;
            }
            """);

        var result = service.GetFragment("csharp", "Calc.cs", "Calc.Add", new FragmentOptions { IncludeImports = true });

        result.Succeeded.ShouldBeTrue();
        result.Text!.ShouldBe("""
            using System;
            using System.Text;

            public int Add(int a, int b) => a + b;
            """);
    }

    [Fact]
    public void IncludeImports_prepends_python_imports()
    {
        var service = CreateService();
        WriteFile("calc.py", "import os\nfrom math import sqrt\n\nclass Calculator:\n    def root(self):\n        return sqrt(2)\n");

        var result = service.GetFragment("python", "calc.py", "Calculator.root", new FragmentOptions { IncludeImports = true });

        result.Succeeded.ShouldBeTrue();
        result.Text!.ShouldStartWith("import os\nfrom math import sqrt\n\n");
        result.Text!.ShouldContain("def root(self):");
    }

    [Fact]
    public void IncludeImports_is_a_no_op_when_the_file_has_no_imports()
    {
        var service = CreateService();
        WriteFile("calc.py", "class Calculator:\n    def add(self, a, b):\n        return a + b\n");

        var result = service.GetFragment("python", "calc.py", "Calculator.add", new FragmentOptions { IncludeImports = true });

        result.Succeeded.ShouldBeTrue();
        result.Text!.ShouldStartWith("def add(self, a, b):");
    }

    [Fact]
    public void IncludeImports_composes_with_bodyonly()
    {
        var service = CreateService();
        WriteFile("Calc.cs", """
            using System;

            public class Calc
            {
                public int Add(int a, int b)
                {
                    return a + b;
                }
            }
            """);

        var result = service.GetFragment("csharp", "Calc.cs", "Calc.Add", new FragmentOptions { BodyOnly = true, IncludeImports = true });

        result.Succeeded.ShouldBeTrue();
        result.Text!.ShouldBe("using System;\n\nreturn a + b;");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}

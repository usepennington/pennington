namespace Pennington.Roslyn.Tests.Symbols;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pennington.Roslyn.Symbols;
using Pennington.Roslyn.Utilities;

public sealed class CodeFragmentExtractorTests
{
    [Fact]
    public async Task Extracts_Full_Method_Declaration()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = "public class Foo { public void Bar() { Console.WriteLine(); } }";
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: false);

        result.ShouldContain("public void Bar()");
        result.ShouldContain("Console.WriteLine()");
    }

    [Fact]
    public async Task Extracts_Method_Body_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = "public class Foo { public void Bar() { Console.WriteLine(); } }";
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: true);

        result.ShouldContain("Console.WriteLine()");
        result.ShouldNotContain("public void Bar()");
    }

    [Fact]
    public async Task Extracts_Expression_Body()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = "public class Foo { public int Bar() => 42; }";
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: true);

        result.ShouldBe("42");
    }

    [Fact]
    public async Task Extracts_Class_Body_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = "public class Foo { public int X { get; } }";
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(classDecl, code, bodyOnly: true);

        result.ShouldContain("public int X { get; }");
        result.ShouldNotContain("public class Foo");
    }

    [Fact]
    public async Task Full_Declaration_Returns_Complete_Text()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = "public class Foo { public int X { get; } }";
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(classDecl, code, bodyOnly: false);

        result.ShouldContain("public class Foo");
        result.ShouldContain("public int X { get; }");
    }

    [Fact]
    public async Task Declaration_Strips_Leading_Xmldoc_When_IncludeLeadingTrivia_False()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = """
            public class Foo
            {
                /// <summary>Do the thing.</summary>
                public void Bar() { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: false, includeLeadingTrivia: false);

        result.ShouldNotContain("<summary>");
        result.ShouldContain("public void Bar()");
    }

    [Fact]
    public async Task Declaration_Keeps_Leading_Xmldoc_By_Default()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = """
            public class Foo
            {
                /// <summary>Do the thing.</summary>
                public void Bar() { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: false);

        result.ShouldContain("<summary>Do the thing.</summary>");
        result.ShouldContain("public void Bar()");
    }

    [Fact]
    public async Task BodyOnly_Method_DedentsCleanly()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = """
            public class Foo
            {
                public void Bar()
                {
                    Console.WriteLine();
                    DoMore();
                }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: true);
        var normalized = TextFormatter.NormalizeIndents(result);

        normalized.ShouldBe("Console.WriteLine();\nDoMore();");
    }

    [Fact]
    public async Task Declaration_WithoutLeadingTrivia_DedentsCleanly()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = """
            public class Foo
            {
                /// <summary>Do.</summary>
                public void Bar()
                {
                    Console.WriteLine();
                }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: false, includeLeadingTrivia: false);
        var normalized = TextFormatter.NormalizeIndents(result);

        normalized.ShouldBe("public void Bar()\n{\n    Console.WriteLine();\n}");
    }

    [Fact]
    public async Task BodyOnly_TypeBraceContent_DedentsCleanly()
    {
        var ct = TestContext.Current.CancellationToken;
        var code = """
            namespace Foo
            {
                public class Bar
                {
                    public int X { get; }
                    public int Y { get; }
                }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
        var root = await tree.GetRootAsync(ct);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(classDecl, code, bodyOnly: true);
        var normalized = TextFormatter.NormalizeIndents(result);

        normalized.ShouldBe("public int X { get; }\npublic int Y { get; }");
    }
}
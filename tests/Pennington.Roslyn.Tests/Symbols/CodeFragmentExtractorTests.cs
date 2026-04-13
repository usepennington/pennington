namespace Pennington.Roslyn.Tests.Symbols;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pennington.Roslyn.Symbols;

public sealed class CodeFragmentExtractorTests
{
    [Fact]
    public async Task Extracts_Full_Method_Declaration()
    {
        var code = "public class Foo { public void Bar() { Console.WriteLine(); } }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: false);

        result.ShouldContain("public void Bar()");
        result.ShouldContain("Console.WriteLine()");
    }

    [Fact]
    public async Task Extracts_Method_Body_Only()
    {
        var code = "public class Foo { public void Bar() { Console.WriteLine(); } }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: true);

        result.ShouldContain("Console.WriteLine()");
        result.ShouldNotContain("public void Bar()");
    }

    [Fact]
    public async Task Extracts_Expression_Body()
    {
        var code = "public class Foo { public int Bar() => 42; }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(method, code, bodyOnly: true);

        result.ShouldBe("42");
    }

    [Fact]
    public async Task Extracts_Class_Body_Only()
    {
        var code = "public class Foo { public int X { get; } }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(classDecl, code, bodyOnly: true);

        result.ShouldContain("public int X { get; }");
        result.ShouldNotContain("public class Foo");
    }

    [Fact]
    public async Task Full_Declaration_Returns_Complete_Text()
    {
        var code = "public class Foo { public int X { get; } }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var result = await CodeFragmentExtractor.ExtractCodeFragmentAsync(classDecl, code, bodyOnly: false);

        result.ShouldContain("public class Foo");
        result.ShouldContain("public int X { get; }");
    }
}
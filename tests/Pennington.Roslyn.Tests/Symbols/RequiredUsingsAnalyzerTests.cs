namespace Pennington.Roslyn.Tests.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pennington.Roslyn.Symbols;

public sealed class RequiredUsingsAnalyzerTests
{
    [Fact]
    public async Task Includes_Using_For_Type_Referenced_In_Body()
    {
        var (root, model) = await CompileAsync("""
            using System.Text;
            using System.IO;

            namespace Sample;

            public static class Demo
            {
                public static void Build()
                {
                    var sb = new StringBuilder();
                    sb.Append("hi");
                }
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldContain("using System.Text;");
        result.ShouldNotContain("using System.IO;");
    }

    [Fact]
    public async Task Includes_Using_For_Extension_Method()
    {
        var (root, model) = await CompileAsync("""
            using System.Collections.Generic;
            using System.Linq;

            namespace Sample;

            public static class Demo
            {
                public static int Count(List<int> xs) => xs.Where(x => x > 0).Count();
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldContain("using System.Linq;");
    }

    [Fact]
    public async Task Includes_Using_Static_Only_When_Member_Is_Unqualified()
    {
        var (root, model) = await CompileAsync("""
            using static System.Math;

            namespace Sample;

            public static class Demo
            {
                public static double Hypot(double x, double y) => Sqrt(x * x + y * y);
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldContain("using static System.Math;");
    }

    [Fact]
    public async Task Excludes_Using_Static_When_Member_Is_Qualified()
    {
        var (root, model) = await CompileAsync("""
            using static System.Math;

            namespace Sample;

            public static class Demo
            {
                public static double Hypot(double x, double y) => System.Math.Sqrt(x * x + y * y);
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldNotContain("using static System.Math;");
    }

    [Fact]
    public async Task Includes_Alias_Only_When_Referenced()
    {
        var (root, model) = await CompileAsync("""
            using Out = System.Console;

            namespace Sample;

            public static class Demo
            {
                public static void Greet() => Out.WriteLine("hi");
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldContain("using Out = System.Console;");
    }

    [Fact]
    public async Task Excludes_Alias_When_Not_Referenced()
    {
        var (root, model) = await CompileAsync("""
            using Out = System.Console;
            using System.Text;

            namespace Sample;

            public static class Demo
            {
                public static string Build() => new StringBuilder().ToString();
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldNotContain("using Out = System.Console;");
        result.ShouldContain("using System.Text;");
    }

    [Fact]
    public async Task Excludes_Using_For_Same_Namespace_As_Declaration()
    {
        var (root, model) = await CompileAsync("""
            using Sample;

            namespace Sample;

            public class Helper { public static int Value() => 1; }

            public static class Demo
            {
                public static int Use() => Helper.Value();
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == "Use");

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldNotContain("using Sample;");
    }

    [Fact]
    public async Task Returns_Empty_For_Body_With_No_External_Dependencies()
    {
        var (root, model) = await CompileAsync("""
            using System.Text;

            namespace Sample;

            public static class Demo
            {
                public static int Add(int a, int b) => a + b;
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Plain_Using_Does_Not_Cover_SubNamespace()
    {
        // `using System;` does NOT import System.Text.StringBuilder — match must be exact.
        var (root, model) = await CompileAsync("""
            using System;
            using System.Text;

            namespace Sample;

            public static class Demo
            {
                public static string Build() => new StringBuilder().ToString();
            }
            """);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var result = AnalyzeAsStrings(method, model, root);

        result.ShouldContain("using System.Text;");
        result.ShouldNotContain("using System;");
    }

    private static IReadOnlyList<string> AnalyzeAsStrings(
        MethodDeclarationSyntax method,
        SemanticModel model,
        CompilationUnitSyntax root)
        => RequiredUsingsAnalyzer.Analyze(method, model, root)
            .Select(d => d.ToString().Trim())
            .ToList();

    private static readonly Lazy<MetadataReference[]> _references = new(() =>
    {
        var trusted = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;
        return trusted
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToArray();
    });

    private static async Task<(CompilationUnitSyntax Root, SemanticModel Model)> CompileAsync(string source)
    {
        var ct = TestContext.Current.CancellationToken;
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), cancellationToken: ct);
        var compilation = CSharpCompilation.Create(
            "Fixture",
            [tree],
            _references.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var root = (CompilationUnitSyntax)await tree.GetRootAsync(ct);
        var model = compilation.GetSemanticModel(tree);
        return (root, model);
    }
}
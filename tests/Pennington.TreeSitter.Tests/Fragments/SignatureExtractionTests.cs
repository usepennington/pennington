namespace Pennington.TreeSitter.Tests.Fragments;

using Pennington.TreeSitter.Fragments;
using static TreeSitterTestHelper;

/// <summary>Outline extraction: <see cref="FragmentOptions.SignaturesOnly"/> elides member bodies to a marker.</summary>
public sealed class SignatureExtractionTests
{
    private static readonly FragmentOptions Signatures = new() { SignaturesOnly = true };

    [Fact]
    public void CSharp_type_renders_member_signatures_with_elided_bodies()
    {
        const string source = """
            public class Calc
            {
                public int Add(int a, int b)
                {
                    return a + b;
                }

                public int Sub(int a, int b)
                {
                    return a - b;
                }
            }
            """;

        var outline = Extract("csharp", source, "Calc", Signatures);

        outline.ShouldNotBeNull();
        outline.ShouldContain("public class Calc");
        outline.ShouldContain("public int Add(int a, int b)");
        outline.ShouldContain("public int Sub(int a, int b)");
        outline.ShouldContain("{ … }");
        outline.ShouldNotContain("return a + b;");
        outline.ShouldNotContain("return a - b;");
    }

    [Fact]
    public void CSharp_single_member_renders_just_its_signature()
    {
        const string source = """
            public class Calc
            {
                public int Add(int a, int b)
                {
                    return a + b;
                }
            }
            """;

        var outline = Extract("csharp", source, "Calc.Add", Signatures);

        outline.ShouldNotBeNull();
        outline.ShouldContain("public int Add(int a, int b)");
        outline.ShouldContain("{ … }");
        outline.ShouldNotContain("return a + b;");
    }

    [Fact]
    public void Python_type_elides_suites_best_effort()
    {
        const string source = """
            class Calculator:
                def add(self, a, b):
                    return a + b
            """;

        var outline = Extract("python", source, "Calculator", Signatures);

        outline.ShouldNotBeNull();
        outline.ShouldContain("def add(self, a, b):");
        outline.ShouldContain("…");
        outline.ShouldNotContain("return a + b");
    }
}

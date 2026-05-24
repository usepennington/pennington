namespace Pennington.TreeSitter.Tests.Resolution;

using static TreeSitterTestHelper;

/// <summary>
/// Resolver tests that double as verification that the built-in per-language configs match the node-type and
/// field names of the grammars bundled in the TreeSitter.DotNet package.
/// </summary>
public sealed class NamePathResolverTests
{
    [Fact]
    public void CSharp_resolves_method_through_transparent_namespace()
    {
        const string source = """
            namespace Sample;

            public class Calculator
            {
                public int Add(int a, int b)
                {
                    return a + b;
                }
            }
            """;

        var fragment = Extract("csharp", source, "Calculator.Add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("public int Add(int a, int b)");
        fragment.ShouldContain("return a + b;");
    }

    [Fact]
    public void CSharp_bodyonly_returns_statements_without_signature_or_braces()
    {
        const string source = """
            public class Calculator
            {
                public int Add(int a, int b)
                {
                    return a + b;
                }
            }
            """;

        var fragment = Extract("csharp", source, "Calculator.Add", bodyOnly: true);

        fragment.ShouldNotBeNull();
        fragment.ShouldBe("return a + b;");
    }

    [Fact]
    public void Python_resolves_method_and_dedents()
    {
        const string source = """
            class Calculator:
                def add(self, a, b):
                    return a + b
            """;

        var fragment = Extract("python", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldBe("def add(self, a, b):\n    return a + b");
    }

    [Fact]
    public void Rust_resolves_method_in_impl_block_via_type_name()
    {
        const string source = """
            struct Calculator;

            impl Calculator {
                fn add(&self, a: i32, b: i32) -> i32 {
                    a + b
                }
            }
            """;

        var fragment = Extract("rust", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("fn add(&self, a: i32, b: i32) -> i32");
        fragment.ShouldContain("a + b");
    }

    [Fact]
    public void TypeScript_resolves_exported_class_method()
    {
        const string source = """
            export class Calculator {
                add(a: number, b: number): number {
                    return a + b;
                }
            }
            """;

        var fragment = Extract("typescript", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("add(a: number, b: number): number");
        fragment.ShouldContain("return a + b;");
    }

    [Fact]
    public void Go_resolves_free_function()
    {
        const string source = """
            package main

            func Add(a int, b int) int {
                return a + b
            }
            """;

        var fragment = Extract("go", source, "Add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("func Add(a int, b int) int");
        fragment.ShouldContain("return a + b");
    }

    [Fact]
    public void JavaScript_resolves_exported_class_method()
    {
        const string source = """
            export class Calculator {
                add(a, b) {
                    return a + b;
                }
            }
            """;

        var fragment = Extract("javascript", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("add(a, b)");
        fragment.ShouldContain("return a + b;");
    }

    [Fact]
    public void Java_resolves_class_method()
    {
        const string source = """
            class Calculator {
                int add(int a, int b) {
                    return a + b;
                }
            }
            """;

        var fragment = Extract("java", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("int add(int a, int b)");
        fragment.ShouldContain("return a + b;");
    }

    [Fact]
    public void Ruby_resolves_class_method()
    {
        const string source = """
            class Calculator
              def add(a, b)
                a + b
              end
            end
            """;

        var fragment = Extract("ruby", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("def add(a, b)");
        fragment.ShouldContain("a + b");
    }

    [Fact]
    public void Php_resolves_class_method()
    {
        const string source = """
            <?php
            class Calculator {
                function add($a, $b) {
                    return $a + $b;
                }
            }
            """;

        var fragment = Extract("php", source, "Calculator.add");

        fragment.ShouldNotBeNull();
        fragment.ShouldContain("function add($a, $b)");
        fragment.ShouldContain("return $a + $b;");
    }

    [Fact]
    public void Unresolved_member_returns_null()
    {
        const string source = "public class Calculator { }";

        Extract("csharp", source, "Calculator.Missing").ShouldBeNull();
    }
}

namespace Pennington.Roslyn.Tests.Documentation;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pennington.Roslyn.Documentation;
using Xunit;

public sealed class InheritDocResolverTests
{
    private static (Compilation Compilation, INamedTypeSymbol Type) Compile(string source, string typeName)
    {
        var tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Preview, documentationMode: DocumentationMode.Parse));

        var compilation = CSharpCompilation.Create(
            "Fixture",
            [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var type = compilation.GetTypeByMetadataName(typeName)
            ?? throw new InvalidOperationException($"Type {typeName} not found");

        return (compilation, type);
    }

    [Fact]
    public void Resolves_Summary_From_Implemented_Interface_Member()
    {
        const string source = """
            namespace Fixtures;

            public interface IThing
            {
                /// <summary>The thing's name.</summary>
                string Name { get; }
            }

            public sealed record Thing : IThing
            {
                /// <inheritdoc/>
                public string Name { get; init; } = "";
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();

        var resolved = InheritDocResolver.Resolve(
            property.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken),
            property);

        resolved.ShouldNotBeNull();
        resolved.ShouldContain("The thing's name.");
        resolved.ShouldNotContain("<inheritdoc");
    }

    [Fact]
    public void Child_Summary_Wins_Over_Inherited()
    {
        const string source = """
            namespace Fixtures;

            public interface IThing
            {
                /// <summary>Interface summary.</summary>
                string Name { get; }
            }

            public sealed record Thing : IThing
            {
                /// <summary>Record summary.</summary>
                /// <inheritdoc/>
                public string Name { get; init; } = "";
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();

        var resolved = InheritDocResolver.Resolve(
            property.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken),
            property);

        resolved.ShouldNotBeNull();
        resolved.ShouldContain("Record summary.");
        resolved.ShouldNotContain("Interface summary.");
    }

    [Fact]
    public void Returns_Input_Unchanged_When_No_Inheritdoc()
    {
        const string source = """
            namespace Fixtures;

            public sealed record Thing
            {
                /// <summary>Just a name.</summary>
                public string Name { get; init; } = "";
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();
        var raw = property.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken);

        var resolved = InheritDocResolver.Resolve(raw, property);

        resolved.ShouldBe(raw);
    }

    [Fact]
    public void Leaves_Inheritdoc_With_Cref_Alone()
    {
        const string source = """
            namespace Fixtures;

            public interface IThing
            {
                /// <summary>Interface summary.</summary>
                string Name { get; }
            }

            public sealed record Thing : IThing
            {
                /// <inheritdoc cref="IThing.Name"/>
                public string Name { get; init; } = "";
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();
        var raw = property.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken);

        var resolved = InheritDocResolver.Resolve(raw, property);

        // cref variant out of scope — returned unchanged.
        resolved.ShouldBe(raw);
    }

    [Fact]
    public void Returns_Input_When_Base_Has_No_Docs()
    {
        const string source = """
            namespace Fixtures;

            public interface IThing
            {
                string Name { get; }
            }

            public sealed record Thing : IThing
            {
                /// <inheritdoc/>
                public string Name { get; init; } = "";
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();
        var raw = property.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken);

        var resolved = InheritDocResolver.Resolve(raw, property);

        // No base xmldoc found → falls through unchanged, preserving existing
        // no-summary signal for callers.
        resolved.ShouldBe(raw);
    }

    [Fact]
    public void Resolves_Through_Two_Levels_Of_Inheritdoc()
    {
        const string source = """
            namespace Fixtures;

            public interface IBase
            {
                /// <summary>Root summary.</summary>
                string Name { get; }
            }

            public abstract class Middle : IBase
            {
                /// <inheritdoc/>
                public abstract string Name { get; }
            }

            public sealed class Thing : Middle
            {
                /// <inheritdoc/>
                public override string Name => "";
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();

        var resolved = InheritDocResolver.Resolve(
            property.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken),
            property);

        resolved.ShouldNotBeNull();
        resolved.ShouldContain("Root summary.");
        resolved.ShouldNotContain("<inheritdoc");
    }

    [Fact]
    public void Inherits_Param_Tags_From_Interface_Method()
    {
        const string source = """
            namespace Fixtures;

            public interface IThing
            {
                /// <summary>Does a thing.</summary>
                /// <param name="value">The input value.</param>
                /// <returns>The result.</returns>
                int Do(int value);
            }

            public sealed class Thing : IThing
            {
                /// <inheritdoc/>
                public int Do(int value) => value;
            }
            """;

        var (_, type) = Compile(source, "Fixtures.Thing");
        var method = type.GetMembers("Do").OfType<IMethodSymbol>().Single();

        var resolved = InheritDocResolver.Resolve(
            method.GetDocumentationCommentXml(cancellationToken: TestContext.Current.CancellationToken),
            method);

        resolved.ShouldNotBeNull();
        resolved.ShouldContain("Does a thing.");
        resolved.ShouldContain("The input value.");
        resolved.ShouldContain("The result.");
    }
}
namespace Pennington.Roslyn.Tests.Documentation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Roslyn.Documentation;
using Pennington.Roslyn.Symbols;
using Pennington.Roslyn.Workspace;

public sealed class MemberEnumeratorTests
{
    private const string Source = """
        namespace Fixtures;

        public sealed class Options
        {
            /// <summary>The site's title.</summary>
            public string Title { get; set; } = "Untitled";

            /// <summary>Canonical base URL, or <c>null</c> to disable feeds.</summary>
            public string? BaseUrl { get; set; }

            /// <summary>The locale code used when none is specified.</summary>
            public string DefaultLocale { get; init; } = "en";

            /// <summary>Registers the widget.</summary>
            /// <param name="name">Widget name.</param>
            /// <returns>True if the widget was new.</returns>
            public bool Register(string name) => true;
        }

        public interface IExample
        {
            /// <summary>The count.</summary>
            int Count { get; }

            /// <summary>Does something.</summary>
            void Do();
        }

        public sealed record RequiredOptions
        {
            public required string Name { get; init; }
            public required string Description { get; init; }
            public bool EnableFeature { get; init; }
            public int RetryCount { get; init; }
            public string? Nickname { get; init; }
            public string Description2 => "computed";
        }

        public interface IDefaults
        {
            bool IsDraft => false;
            int Priority { get; }
        }

        /// <summary>One auto-discovered public type, slugged for the route segment.</summary>
        /// <param name="Slug">Slug used as the {key} route segment.</param>
        /// <param name="XmlDocId">XmlDocId of the type.</param>
        /// <param name="TypeName">Short type name without namespace.</param>
        public sealed record PositionalEntry(string Slug, string XmlDocId, string TypeName);

        /// <summary>Mixed positional + body — only the param-backed ones get the fallback.</summary>
        /// <param name="Name">The receiver name.</param>
        public sealed record MixedEntry(string Name)
        {
            /// <summary>Has its own summary already.</summary>
            public string Description { get; init; } = string.Empty;

            public string Untouched { get; init; } = string.Empty;
        }
        """;

    private static async Task<IMemberEnumerator> BuildEnumeratorAsync()
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var project = workspace.AddProject(ProjectInfo.Create(
            projectId,
            VersionStamp.Create(),
            "Fixture",
            "Fixture",
            LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview, documentationMode: DocumentationMode.Parse),
            metadataReferences:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            ]));

        var documentId = DocumentId.CreateNewId(projectId);
        workspace.AddDocument(DocumentInfo.Create(
            documentId,
            "Fixture.cs",
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(Source), VersionStamp.Create()))));

        var workspaceService = new StubWorkspaceService(workspace.CurrentSolution);
        var symbolService = new SymbolExtractionService(workspaceService, NullLogger<SymbolExtractionService>.Instance);
        return new MemberEnumerator(symbolService, new XmlDocParser());
    }

    [Fact]
    public async Task Enumerates_Public_Properties_Alphabetically()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.Options",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        members.Select(m => m.Name).ShouldBe(["BaseUrl", "DefaultLocale", "Title"]);
    }

    [Fact]
    public async Task Extracts_Property_Defaults_From_Initializer()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.Options",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        var title = members.Single(m => m.Name == "Title");
        title.DefaultValue.ShouldBe("\"Untitled\"");

        // Nullable reference without an initializer falls back to the literal "null"
        // default so the Default column shows something useful instead of "—".
        var baseUrl = members.Single(m => m.Name == "BaseUrl");
        baseUrl.DefaultValue.ShouldBe("null");
    }

    [Fact]
    public async Task Formats_Nullable_Type_Display()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.Options",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        members.Single(m => m.Name == "BaseUrl").TypeDisplay.ShouldBe("string?");
        members.Single(m => m.Name == "Title").TypeDisplay.ShouldBe("string");
    }

    [Fact]
    public async Task Parses_Summary_Xmldoc_Into_Descriptor()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.Options",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        var title = members.Single(m => m.Name == "Title");
        title.Xmldoc.HasSummary.ShouldBeTrue();
        title.Xmldoc.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("The site's title.");
    }

    [Fact]
    public async Task Enumerates_Methods_Excluding_Property_Accessors()
    {
        var enumerator = await BuildEnumeratorAsync();

        var methods = await enumerator.EnumerateAsync(
            "T:Fixtures.Options",
            MemberKind.Methods,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        methods.Select(m => m.Name).ShouldBe(["Register"]);
        methods[0].TypeDisplay.ShouldBe("bool Register(string name)");
    }

    [Fact]
    public async Task Returns_Empty_For_Unresolved_Type()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Does.Not.Exist",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        members.ShouldBeEmpty();
    }

    [Fact]
    public async Task Interface_Members_Are_Enumerable()
    {
        var enumerator = await BuildEnumeratorAsync();

        var props = await enumerator.EnumerateAsync(
            "T:Fixtures.IExample",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        props.Single().Name.ShouldBe("Count");

        var methods = await enumerator.EnumerateAsync(
            "T:Fixtures.IExample",
            MemberKind.Methods,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        methods.Single().Name.ShouldBe("Do");
    }

    [Fact]
    public async Task Required_Property_Is_Marked_Required_With_Null_Default()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.RequiredOptions",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        var name = members.Single(m => m.Name == "Name");
        name.IsRequired.ShouldBeTrue();
        name.DefaultValue.ShouldBeNull();
    }

    [Fact]
    public async Task Auto_Property_Without_Initializer_Falls_Back_To_Clr_Default()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.RequiredOptions",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        members.Single(m => m.Name == "EnableFeature").DefaultValue.ShouldBe("false");
        members.Single(m => m.Name == "RetryCount").DefaultValue.ShouldBe("0");
        members.Single(m => m.Name == "Nickname").DefaultValue.ShouldBe("null");
    }

    [Fact]
    public async Task Concrete_Expression_Bodied_Property_Reports_No_Default()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.RequiredOptions",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        members.Single(m => m.Name == "Description2").DefaultValue.ShouldBeNull();
    }

    [Fact]
    public async Task Interface_Expression_Bodied_Literal_Is_Reported_As_Default()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.IDefaults",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        members.Single(m => m.Name == "IsDraft").DefaultValue.ShouldBe("false");
        members.Single(m => m.Name == "Priority").DefaultValue.ShouldBeNull();
    }

    [Fact]
    public async Task Positional_Record_Property_Inherits_Summary_From_Param()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.PositionalEntry",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        var slug = members.Single(m => m.Name == "Slug");
        slug.Xmldoc.HasSummary.ShouldBeTrue();
        slug.Xmldoc.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("Slug used as the {key} route segment.");

        var typeName = members.Single(m => m.Name == "TypeName");
        typeName.Xmldoc.HasSummary.ShouldBeTrue();
        typeName.Xmldoc.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("Short type name without namespace.");
    }

    [Fact]
    public async Task Param_Fallback_Does_Not_Override_Existing_Summary()
    {
        var enumerator = await BuildEnumeratorAsync();

        var members = await enumerator.EnumerateAsync(
            "T:Fixtures.MixedEntry",
            MemberKind.Properties,
            AccessFilter.Public,
            MemberOrder.Alphabetical);

        // Positional `Name` gets its summary from <param name="Name">.
        var name = members.Single(m => m.Name == "Name");
        name.Xmldoc.HasSummary.ShouldBeTrue();
        name.Xmldoc.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("The receiver name.");

        // Body-defined `Description` keeps its own summary.
        var description = members.Single(m => m.Name == "Description");
        description.Xmldoc.HasSummary.ShouldBeTrue();
        description.Xmldoc.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("Has its own summary already.");

        // Body-defined `Untouched` has no doc and stays without one — the fallback
        // must only fire for positional record parameters, not arbitrary undocumented members.
        var untouched = members.Single(m => m.Name == "Untouched");
        untouched.Xmldoc.HasSummary.ShouldBeFalse();
    }

    private sealed class StubWorkspaceService(Solution solution) : ISolutionWorkspaceService
    {
        public Task<Solution> LoadSolutionAsync(string solutionPath) => Task.FromResult(solution);

        public Task<IEnumerable<Project>> GetProjectsAsync(Func<Project, bool>? filter = null)
            => Task.FromResult<IEnumerable<Project>>(filter is null ? solution.Projects : solution.Projects.Where(filter));

        public Task<Compilation?> GetCompilationAsync(Project project) => project.GetCompilationAsync();

        public void InvalidateSolution() { }

        public void UpdateDocument(string filePath) { }

        public void Dispose() { }
    }
}
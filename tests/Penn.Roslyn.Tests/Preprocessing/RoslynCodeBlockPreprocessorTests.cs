namespace Penn.Roslyn.Tests.Preprocessing;

using Microsoft.Extensions.DependencyInjection;
using Penn.Highlighting;
using Penn.Markdown.Extensions;
using Penn.Roslyn.Highlighting;
using Penn.Roslyn.Preprocessing;

public sealed class RoslynCodeBlockPreprocessorTests
{
    [Fact]
    public void ParseLanguageId_Extracts_XmlDocId()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp:xmldocid");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("xmldocid");
    }

    [Fact]
    public void ParseLanguageId_Extracts_Path()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp:path");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("path");
    }

    [Fact]
    public void ParseLanguageId_Extracts_XmlDocIdDiff()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp:xmldocid-diff");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("xmldocid-diff");
    }

    [Fact]
    public void ParseLanguageId_Extracts_Bodyonly()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp:xmldocid,bodyonly");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("xmldocid,bodyonly");
    }

    [Fact]
    public void ParseLanguageId_Extracts_XmlDocIdDiff_Bodyonly()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp:xmldocid-diff,bodyonly");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("xmldocid-diff,bodyonly");
    }

    [Fact]
    public void ParseLanguageId_Returns_Null_Modifier_For_No_Modifier()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBeNull();
    }

    [Fact]
    public void ParseLanguageId_Returns_Null_Modifier_For_Plain_Language()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("python");

        baseLanguage.ShouldBe("python");
        modifier.ShouldBeNull();
    }

    [Fact]
    public void ParseLanguageId_Is_Case_Insensitive()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("csharp:XmlDocId");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("xmldocid");
    }

    [Fact]
    public void ParseLanguageId_Trims_Whitespace()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("  csharp:xmldocid  ");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("xmldocid");
    }

    [Fact]
    public void ParseLanguageId_Vb_Path()
    {
        var (baseLanguage, modifier) = RoslynCodeBlockPreprocessor.ParseLanguageId("vb:path");

        baseLanguage.ShouldBe("vb");
        modifier.ShouldBe("path");
    }

    [Fact]
    public void TryProcess_Returns_Null_For_Unrecognized_Language()
    {
        var preprocessor = CreatePreprocessor();

        var result = preprocessor.TryProcess("some code", "python");

        result.ShouldBeNull();
    }

    [Fact]
    public void TryProcess_Returns_Null_For_Plain_CSharp()
    {
        var preprocessor = CreatePreprocessor();

        var result = preprocessor.TryProcess("var x = 42;", "csharp");

        result.ShouldBeNull();
    }

    [Fact]
    public void Priority_Is_100()
    {
        var preprocessor = CreatePreprocessor();

        preprocessor.Priority.ShouldBe(100);
    }

    [Fact]
    public void AddPennRoslyn_Registers_ICodeHighlighter()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPennRoslyn();

        var provider = services.BuildServiceProvider();
        var highlighter = provider.GetService<ICodeHighlighter>();

        highlighter.ShouldNotBeNull();
        highlighter.ShouldBeOfType<RoslynHighlighter>();
    }

    [Fact]
    public void AddPennRoslyn_Registers_SyntaxHighlighter_As_Singleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPennRoslyn();

        var provider = services.BuildServiceProvider();
        var h1 = provider.GetService<SyntaxHighlighter>();
        var h2 = provider.GetService<SyntaxHighlighter>();

        h1.ShouldNotBeNull();
        h1.ShouldBeSameAs(h2);
    }

    [Fact]
    public void AddPennRoslyn_Without_SolutionPath_Does_Not_Register_Preprocessor()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPennRoslyn();

        var provider = services.BuildServiceProvider();
        var preprocessor = provider.GetService<ICodeBlockPreprocessor>();

        preprocessor.ShouldBeNull();
    }

    [Fact]
    public void AddPennRoslyn_With_SolutionPath_Registers_ICodeBlockPreprocessor()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Register IFileWatcher which SolutionWorkspaceService requires
        services.AddSingleton<Penn.Infrastructure.IFileWatcher, StubFileWatcher>();

        services.AddPennRoslyn(opts => opts.SolutionPath = @"C:\fake\solution.sln");

        var descriptors = services.Where(d => d.ServiceType == typeof(ICodeBlockPreprocessor)).ToList();
        descriptors.Count.ShouldBe(1);
    }

    private static RoslynCodeBlockPreprocessor CreatePreprocessor()
    {
        return new RoslynCodeBlockPreprocessor(
            new StubSymbolExtractionService(),
            new SyntaxHighlighter(),
            new RoslynOptions(),
            new Penn.Generation.BuildDiagnosticsCollector());
    }

    /// <summary>Stub that returns empty for any extraction call.</summary>
    private sealed class StubSymbolExtractionService : Penn.Roslyn.Symbols.ISymbolExtractionService
    {
        public Task<IReadOnlyDictionary<string, Penn.Roslyn.Symbols.SymbolInfo>> ExtractSymbolsAsync(
            Microsoft.CodeAnalysis.Solution solution)
            => Task.FromResult<IReadOnlyDictionary<string, Penn.Roslyn.Symbols.SymbolInfo>>(
                new Dictionary<string, Penn.Roslyn.Symbols.SymbolInfo>());

        public Task<Penn.Roslyn.Symbols.SymbolInfo?> FindSymbolAsync(string xmlDocId)
            => Task.FromResult<Penn.Roslyn.Symbols.SymbolInfo?>(null);

        public Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false)
            => Task.FromResult(string.Empty);

        public void ClearCache() { }
    }

    /// <summary>Stub file watcher for DI registration tests.</summary>
    private sealed class StubFileWatcher : Penn.Infrastructure.IFileWatcher
    {
        public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true) { }
        public void SubscribeToChanges(Action onUpdate) { }
        public void Dispose() { }
    }
}

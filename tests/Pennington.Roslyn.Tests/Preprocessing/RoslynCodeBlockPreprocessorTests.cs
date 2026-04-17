namespace Pennington.Roslyn.Tests.Preprocessing;

using Markdown.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Highlighting;
using Pennington.Roslyn.Highlighting;
using Pennington.Roslyn.Preprocessing;

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
    public void ProcessPath_Accepts_Bare_Filename_SolutionPath()
    {
        // Regression: ProcessPath used to call Path.GetDirectoryName directly on
        // SolutionPath. A bare filename (no directory component) returns empty,
        // which triggered a spurious "Solution directory not found" error — even
        // though the rest of Pennington resolves SolutionPath against the process
        // CWD. See postmortem-BeyondRoslynExample.md.
        var preprocessor = new RoslynCodeBlockPreprocessor(
            new StubSymbolExtractionService(),
            new SyntaxHighlighter(),
            new RoslynOptions { SolutionPath = "bare-filename.slnx" },
            new NullHttpContextAccessor());

        var result = preprocessor.TryProcess("nonexistent-file-for-test.cs", "csharp:path");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldNotContain("Solution directory not found");
        // File doesn't exist — we expect a clean "File not found" error, proving
        // the preprocessor got past the directory-resolution step.
        result.HighlightedHtml.ShouldContain("File not found");
    }

    [Fact]
    public void ProcessPath_Resolves_File_Relative_To_SolutionPath_Directory()
    {
        // End-to-end: a :path fence body resolves relative to the SolutionPath's
        // directory. This covers the happy path for the bare-filename fix.
        var tempDir = Path.Combine(Path.GetTempPath(), $"penn-roslyn-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var solutionFile = Path.Combine(tempDir, "fake.slnx");
            File.WriteAllText(solutionFile, "<Solution />");
            var contentFile = Path.Combine(tempDir, "sample.cs");
            File.WriteAllText(contentFile, "var sentinel = 42;");

            var preprocessor = new RoslynCodeBlockPreprocessor(
                new StubSymbolExtractionService(),
                new SyntaxHighlighter(),
                new RoslynOptions { SolutionPath = solutionFile },
                new NullHttpContextAccessor());

            var result = preprocessor.TryProcess("sample.cs", "csharp:path");

            result.ShouldNotBeNull();
            result.HighlightedHtml.ShouldNotContain("File not found");
            result.HighlightedHtml.ShouldNotContain("Solution directory not found");
            result.HighlightedHtml.ShouldContain("sentinel");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void AddPenningtonRoslyn_Registers_ICodeHighlighter()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPenningtonRoslyn();

        var provider = services.BuildServiceProvider();
        var highlighter = provider.GetService<ICodeHighlighter>();

        highlighter.ShouldNotBeNull();
        highlighter.ShouldBeOfType<RoslynHighlighter>();
    }

    [Fact]
    public void AddPenningtonRoslyn_Registers_SyntaxHighlighter_As_Singleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPenningtonRoslyn();

        var provider = services.BuildServiceProvider();
        var h1 = provider.GetService<SyntaxHighlighter>();
        var h2 = provider.GetService<SyntaxHighlighter>();

        h1.ShouldNotBeNull();
        h1.ShouldBeSameAs(h2);
    }

    [Fact]
    public void AddPenningtonRoslyn_Without_SolutionPath_Does_Not_Register_Preprocessor()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPenningtonRoslyn();

        var provider = services.BuildServiceProvider();
        var preprocessor = provider.GetService<ICodeBlockPreprocessor>();

        preprocessor.ShouldBeNull();
    }

    [Fact]
    public void AddPenningtonRoslyn_With_SolutionPath_Registers_ICodeBlockPreprocessor()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Register IFileWatcher which SolutionWorkspaceService requires
        services.AddSingleton<Infrastructure.IFileWatcher, StubFileWatcher>();

        services.AddPenningtonRoslyn(opts => opts.SolutionPath = @"C:\fake\solution.sln");

        var descriptors = services.Where(d => d.ServiceType == typeof(ICodeBlockPreprocessor)).ToList();
        descriptors.Count.ShouldBe(1);
    }

    private static RoslynCodeBlockPreprocessor CreatePreprocessor()
    {
        return new RoslynCodeBlockPreprocessor(
            new StubSymbolExtractionService(),
            new SyntaxHighlighter(),
            new RoslynOptions(),
            new NullHttpContextAccessor());
    }

    private sealed class NullHttpContextAccessor : Microsoft.AspNetCore.Http.IHttpContextAccessor
    {
        public Microsoft.AspNetCore.Http.HttpContext? HttpContext { get; set; }
    }

    /// <summary>Stub that returns empty for any extraction call.</summary>
    private sealed class StubSymbolExtractionService : Pennington.Roslyn.Symbols.ISymbolExtractionService
    {
        public Task<IReadOnlyDictionary<string, Pennington.Roslyn.Symbols.SymbolInfo>> ExtractSymbolsAsync(
            Microsoft.CodeAnalysis.Solution solution)
            => Task.FromResult<IReadOnlyDictionary<string, Pennington.Roslyn.Symbols.SymbolInfo>>(
                new Dictionary<string, Pennington.Roslyn.Symbols.SymbolInfo>());

        public Task<Pennington.Roslyn.Symbols.SymbolInfo?> FindSymbolAsync(string xmlDocId)
            => Task.FromResult<Pennington.Roslyn.Symbols.SymbolInfo?>(null);

        public Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false, bool includeLeadingTrivia = true)
            => Task.FromResult(string.Empty);

        public void ClearCache() { }
    }

    /// <summary>Stub file watcher for DI registration tests.</summary>
    private sealed class StubFileWatcher : Infrastructure.IFileWatcher
    {
        public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true) { }
        public void SubscribeToChanges(Action onUpdate) { }
        public void SubscribeToChanges(Action<Infrastructure.FileChangeNotification> onUpdate) { }
        public void Dispose() { }
    }
}
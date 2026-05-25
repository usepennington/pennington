namespace Pennington.TreeSitter.Tests.Preprocessing;

using Pennington.TreeSitter.Preprocessing;

public sealed class TreeSitterCodeBlockPreprocessorTests
{
    [Fact]
    public void ParseLanguageId_without_marker_passes_through()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("python");

        baseLanguage.ShouldBe("python");
        modifier.ShouldBeNull();
        options.BodyOnly.ShouldBeFalse();
    }

    [Fact]
    public void ParseLanguageId_extracts_symbol_modifier()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("python:symbol");

        baseLanguage.ShouldBe("python");
        modifier.ShouldBe("symbol");
        options.BodyOnly.ShouldBeFalse();
    }

    [Fact]
    public void ParseLanguageId_detects_bodyonly_flag()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("rust:symbol,bodyonly");

        baseLanguage.ShouldBe("rust");
        modifier.ShouldBe("symbol");
        options.BodyOnly.ShouldBeTrue();
    }

    [Fact]
    public void ParseLanguageId_extracts_symbol_diff_modifier()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("python:symbol-diff");

        baseLanguage.ShouldBe("python");
        modifier.ShouldBe("symbol-diff");
        options.BodyOnly.ShouldBeFalse();
    }

    [Fact]
    public void ParseLanguageId_extracts_symbol_diff_bodyonly()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("rust:symbol-diff,bodyonly");

        baseLanguage.ShouldBe("rust");
        modifier.ShouldBe("symbol-diff");
        options.BodyOnly.ShouldBeTrue();
    }

    [Fact]
    public void ParseLanguageId_parses_imports_and_bodyonly_flags_together()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("csharp:symbol,bodyonly,imports");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("symbol");
        options.BodyOnly.ShouldBeTrue();
        options.IncludeImports.ShouldBeTrue();
        options.SignaturesOnly.ShouldBeFalse();
    }

    [Fact]
    public void ParseLanguageId_parses_signatures_flag()
    {
        var (baseLanguage, modifier, options) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("csharp:symbol,signatures");

        baseLanguage.ShouldBe("csharp");
        modifier.ShouldBe("symbol");
        options.SignaturesOnly.ShouldBeTrue();
        options.BodyOnly.ShouldBeFalse();
        options.IncludeImports.ShouldBeFalse();
    }

    [Fact]
    public void ParseReference_splits_path_and_name_path()
    {
        var (filePath, namePath) = TreeSitterCodeBlockPreprocessor.ParseReference("src/calc.py > Calculator.add");

        filePath.ShouldBe("src/calc.py");
        namePath.ShouldBe("Calculator.add");
    }

    [Fact]
    public void ParseReference_bare_path_yields_empty_name_path()
    {
        var (filePath, namePath) = TreeSitterCodeBlockPreprocessor.ParseReference("src/calc.py");

        filePath.ShouldBe("src/calc.py");
        namePath.ShouldBe(string.Empty);
    }
}

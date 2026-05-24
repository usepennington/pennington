namespace Pennington.TreeSitter.Tests.Preprocessing;

using Pennington.TreeSitter.Preprocessing;

public sealed class TreeSitterCodeBlockPreprocessorTests
{
    [Fact]
    public void ParseLanguageId_without_marker_passes_through()
    {
        var (baseLanguage, modifier, bodyOnly) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("python");

        baseLanguage.ShouldBe("python");
        modifier.ShouldBeNull();
        bodyOnly.ShouldBeFalse();
    }

    [Fact]
    public void ParseLanguageId_extracts_symbol_modifier()
    {
        var (baseLanguage, modifier, bodyOnly) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("python:symbol");

        baseLanguage.ShouldBe("python");
        modifier.ShouldBe("symbol");
        bodyOnly.ShouldBeFalse();
    }

    [Fact]
    public void ParseLanguageId_detects_bodyonly_flag()
    {
        var (baseLanguage, modifier, bodyOnly) = TreeSitterCodeBlockPreprocessor.ParseLanguageId("rust:symbol,bodyonly");

        baseLanguage.ShouldBe("rust");
        modifier.ShouldBe("symbol");
        bodyOnly.ShouldBeTrue();
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

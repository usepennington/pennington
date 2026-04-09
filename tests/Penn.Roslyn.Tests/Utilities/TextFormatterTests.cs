namespace Pennington.Roslyn.Tests.Utilities;

using Pennington.Roslyn.Utilities;

public sealed class TextFormatterTests
{
    [Fact]
    public void Normalizes_Indentation()
    {
        var code = "    var x = 1;\n    var y = 2;";

        var result = TextFormatter.NormalizeIndents(code);

        result.ShouldBe("var x = 1;\nvar y = 2;");
    }

    [Fact]
    public void Preserves_Relative_Indentation()
    {
        var code = "    if (true)\n        return;\n    end";

        var result = TextFormatter.NormalizeIndents(code);

        result.ShouldBe("if (true)\n    return;\nend");
    }

    [Fact]
    public void Handles_Empty_Lines()
    {
        var code = "    var x = 1;\n\n    var y = 2;";

        var result = TextFormatter.NormalizeIndents(code);

        result.ShouldBe("var x = 1;\n\nvar y = 2;");
    }

    [Fact]
    public void Returns_Empty_For_Empty_Input()
    {
        var result = TextFormatter.NormalizeIndents("");

        result.ShouldBe("");
    }

    [Fact]
    public void Returns_Unchanged_When_No_Common_Indent()
    {
        var code = "var x = 1;\n    var y = 2;";

        var result = TextFormatter.NormalizeIndents(code);

        result.ShouldBe("var x = 1;\n    var y = 2;");
    }

    [Fact]
    public void Handles_Null_Input()
    {
        var result = TextFormatter.NormalizeIndents(null!);

        result.ShouldBeNull();
    }
}

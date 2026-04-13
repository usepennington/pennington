namespace Pennington.Roslyn.Tests.Symbols;

using Pennington.Roslyn.Symbols;

public sealed class XmlDocIdNormalizerTests
{
    [Fact]
    public void Strips_Namespace_From_Simple_Parameter()
    {
        var result = XmlDocIdNormalizer.Normalize("M:Type.Method(System.String)");

        result.ShouldBe("M:Type.Method(String)");
    }

    [Fact]
    public void Strips_Multiple_Parameters()
    {
        var result = XmlDocIdNormalizer.Normalize("M:Type.Method(System.String,System.Int32)");

        result.ShouldBe("M:Type.Method(String,Int32)");
    }

    [Fact]
    public void Preserves_Generic_Params()
    {
        var result = XmlDocIdNormalizer.Normalize("M:Type.Method(`0)");

        result.ShouldBe("M:Type.Method(`0)");
    }

    [Fact]
    public void Preserves_Non_Method_IDs()
    {
        var result = XmlDocIdNormalizer.Normalize("T:MyNamespace.MyClass");

        result.ShouldBe("T:MyNamespace.MyClass");
    }

    [Fact]
    public void Handles_Nested_Generics()
    {
        var result = XmlDocIdNormalizer.Normalize(
            "M:Type.Method(System.Collections.Generic.List{System.String})");

        result.ShouldBe("M:Type.Method(List{String})");
    }

    [Fact]
    public void Handles_Empty_String()
    {
        var result = XmlDocIdNormalizer.Normalize("");

        result.ShouldBe("");
    }

    [Fact]
    public void Handles_Null_String()
    {
        var result = XmlDocIdNormalizer.Normalize(null!);

        result.ShouldBeNull();
    }

    [Fact]
    public void Handles_Array_Parameters()
    {
        var result = XmlDocIdNormalizer.Normalize("M:Type.Method(System.String[])");

        result.ShouldBe("M:Type.Method(String[])");
    }

    [Fact]
    public void Handles_Double_Backtick_Generic_Params()
    {
        var result = XmlDocIdNormalizer.Normalize("M:Type.Method(``0)");

        result.ShouldBe("M:Type.Method(``0)");
    }

    [Fact]
    public void Handles_Mixed_Generic_And_Qualified_Params()
    {
        var result = XmlDocIdNormalizer.Normalize("M:Type.Method(`0,System.String)");

        result.ShouldBe("M:Type.Method(`0,String)");
    }
}
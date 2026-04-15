namespace Pennington.Roslyn.Tests.Documentation;

using Pennington.Roslyn.Documentation;

public sealed class XmlDocParserTests
{
    private readonly XmlDocParser _parser = new();

    [Fact]
    public void Null_Or_Empty_Returns_Empty()
    {
        _parser.Parse(null).ShouldBe(ParsedXmlDoc.Empty);
        _parser.Parse("").ShouldBe(ParsedXmlDoc.Empty);
        _parser.Parse("   ").ShouldBe(ParsedXmlDoc.Empty);
    }

    [Fact]
    public void Parses_Summary_Text()
    {
        var xml = """
            <member name="T:Foo">
                <summary>Hello world.</summary>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.Summary.Length.ShouldBe(1);
        parsed.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("Hello world.");
    }

    [Fact]
    public void Parses_Inline_Code_And_Text_In_Summary()
    {
        var xml = """
            <member name="T:Foo">
                <summary>Use <c>AddPennington</c> to register.</summary>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.Summary.Length.ShouldBe(3);
        parsed.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("Use ");
        parsed.Summary[1].ShouldBeCase<InlineCodeNode>().Text.ShouldBe("AddPennington");
        parsed.Summary[2].ShouldBeCase<TextNode>().Text.ShouldBe(" to register.");
    }

    [Fact]
    public void Preserves_Whitespace_Around_Cref_In_Mixed_Content()
    {
        var xml = """
            <member name="T:Foo">
                <summary>See <see cref="T:System.String"/> for details.</summary>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.Summary.Length.ShouldBe(3);
        parsed.Summary[0].ShouldBeCase<TextNode>().Text.ShouldBe("See ");
        parsed.Summary[1].ShouldBeCase<CrefNode>().CrefId.ShouldBe("T:System.String");
        parsed.Summary[2].ShouldBeCase<TextNode>().Text.ShouldBe(" for details.");
    }

    [Fact]
    public void Parses_Cref_As_CrefNode()
    {
        var xml = """
            <member name="T:Foo">
                <summary>See <see cref="T:System.String"/> for details.</summary>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        var cref = parsed.Summary.Cases<CrefNode>().ShouldHaveSingleItem();
        cref.CrefId.ShouldBe("T:System.String");
    }

    [Fact]
    public void Parses_See_Langword_As_InlineCode()
    {
        var xml = """
            <member name="T:Foo">
                <summary><see langword="null"/> by default.</summary>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.Summary[0].ShouldBeCase<InlineCodeNode>().Text.ShouldBe("null");
    }

    [Fact]
    public void Parses_Params_By_Name()
    {
        var xml = """
            <member name="M:Foo.Do(System.String,System.Int32)">
                <param name="name">The thing's name.</param>
                <param name="count">How many.</param>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.Params.Count.ShouldBe(2);
        parsed.Params["name"][0].ShouldBeCase<TextNode>().Text.ShouldBe("The thing's name.");
        parsed.Params["count"][0].ShouldBeCase<TextNode>().Text.ShouldBe("How many.");
    }

    [Fact]
    public void Parses_Returns_And_Remarks()
    {
        var xml = """
            <member name="M:Foo.Do">
                <returns>The widget.</returns>
                <remarks>Called from the widget factory.</remarks>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.Returns[0].ShouldBeCase<TextNode>().Text.ShouldBe("The widget.");
        parsed.Remarks[0].ShouldBeCase<TextNode>().Text.ShouldBe("Called from the widget factory.");
    }

    [Fact]
    public void Parses_Para_Children()
    {
        var xml = """
            <member name="T:Foo">
                <remarks>
                    <para>First paragraph.</para>
                    <para>Second paragraph.</para>
                </remarks>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        var paras = parsed.Remarks.Cases<ParaNode>().ToList();
        paras.Count.ShouldBe(2);
        paras[0].Children[0].ShouldBeCase<TextNode>().Text.ShouldBe("First paragraph.");
        paras[1].Children[0].ShouldBeCase<TextNode>().Text.ShouldBe("Second paragraph.");
    }

    [Fact]
    public void Parses_SeeAlso_Crefs()
    {
        var xml = """
            <member name="T:Foo">
                <seealso cref="T:Bar"/>
                <seealso cref="T:Baz"/>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        parsed.SeeAlso.ShouldBe(["T:Bar", "T:Baz"]);
    }

    [Fact]
    public void Parses_Bullet_List()
    {
        var xml = """
            <member name="T:Foo">
                <summary>
                    <list type="bullet">
                        <item><description>One</description></item>
                        <item><description>Two</description></item>
                    </list>
                </summary>
            </member>
            """;

        var parsed = _parser.Parse(xml);

        var list = parsed.Summary.Cases<ListNode>().ShouldHaveSingleItem();
        list.Kind.ShouldBe("bullet");
        list.Items.Length.ShouldBe(2);
        list.Items[0].Description[0].ShouldBeCase<TextNode>().Text.ShouldBe("One");
    }

    [Fact]
    public void Malformed_Xml_Returns_Empty()
    {
        _parser.Parse("<member><summary>oops</summary").ShouldBe(ParsedXmlDoc.Empty);
    }
}
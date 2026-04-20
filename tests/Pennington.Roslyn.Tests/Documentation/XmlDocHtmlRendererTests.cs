namespace Pennington.Roslyn.Tests.Documentation;

using System.Collections.Immutable;
using Pennington.ApiMetadata;

public sealed class XmlDocHtmlRendererTests
{
    private readonly XmlDocHtmlRenderer _renderer = new();
    private readonly XmlDocParser _parser = new();

    [Fact]
    public void Inline_Renders_Plain_Text_Html_Encoded()
    {
        var nodes = ImmutableArray.Create(new XmlDocNode(new TextNode("3 < 5 & true")));

        _renderer.RenderInlineHtml(nodes).ShouldBe("3 &lt; 5 &amp; true");
    }

    [Fact]
    public void Inline_Renders_InlineCode()
    {
        var nodes = ImmutableArray.Create(new XmlDocNode(new InlineCodeNode("AddPennington")));

        _renderer.RenderInlineHtml(nodes).ShouldBe("<code>AddPennington</code>");
    }

    [Fact]
    public void Inline_Renders_Cref_As_Code_Span()
    {
        var nodes = ImmutableArray.Create(new XmlDocNode(new CrefNode("T:Pennington.Infrastructure.PenningtonOptions", null)));

        _renderer.RenderInlineHtml(nodes).ShouldBe("<code>PenningtonOptions</code>");
    }

    [Fact]
    public void Inline_Uses_Cref_DisplayText_When_Present()
    {
        var nodes = ImmutableArray.Create(new XmlDocNode(new CrefNode("T:System.String", "a string")));

        _renderer.RenderInlineHtml(nodes).ShouldBe("<code>a string</code>");
    }

    [Fact]
    public void Inline_Strips_Method_Generic_Arity_From_Cref()
    {
        var nodes = ImmutableArray.Create(new XmlDocNode(new CrefNode("M:Ns.Type.AddMarkdownContent``1", null)));

        _renderer.RenderInlineHtml(nodes).ShouldBe("<code>AddMarkdownContent</code>");
    }

    [Fact]
    public void Inline_Strips_Type_Generic_Arity_From_Cref()
    {
        var nodes = ImmutableArray.Create(new XmlDocNode(new CrefNode("T:System.Collections.Generic.List`1", null)));

        _renderer.RenderInlineHtml(nodes).ShouldBe("<code>List</code>");
    }

    [Fact]
    public void Inline_Preserves_Whitespace_Around_Cref()
    {
        var parsed = _parser.Parse("""<member><summary>See <see cref="T:System.String"/> for details.</summary></member>""");

        _renderer.RenderInlineHtml(parsed.Summary).ShouldBe("See <code>String</code> for details.");
    }

    [Fact]
    public void Block_Wraps_Text_In_Paragraph()
    {
        var parsed = _parser.Parse("""<member><summary>Hello world.</summary></member>""");

        _renderer.RenderHtml(parsed.Summary).ShouldBe("<p>Hello world.</p>");
    }

    [Fact]
    public void Block_Separates_Paragraphs()
    {
        var parsed = _parser.Parse("""
            <member>
                <remarks>
                    <para>First.</para>
                    <para>Second.</para>
                </remarks>
            </member>
            """);

        _renderer.RenderHtml(parsed.Remarks).ShouldBe("<p>First.</p><p>Second.</p>");
    }

    [Fact]
    public void Block_Renders_Bullet_List()
    {
        var parsed = _parser.Parse("""
            <member>
                <summary>
                    <list type="bullet">
                        <item><description>alpha</description></item>
                        <item><description>beta</description></item>
                    </list>
                </summary>
            </member>
            """);

        _renderer.RenderHtml(parsed.Summary).ShouldBe("<ul><li>alpha</li><li>beta</li></ul>");
    }

    [Fact]
    public void Block_Renders_Number_List_As_Ol()
    {
        var parsed = _parser.Parse("""
            <member>
                <summary>
                    <list type="number">
                        <item><description>one</description></item>
                    </list>
                </summary>
            </member>
            """);

        _renderer.RenderHtml(parsed.Summary).ShouldBe("<ol><li>one</li></ol>");
    }

    [Fact]
    public void Block_Renders_Code_Block_Outside_Paragraph()
    {
        var parsed = _parser.Parse("""
            <member>
                <remarks>
                    <code language="csharp">var x = 1;</code>
                </remarks>
            </member>
            """);

        _renderer.RenderHtml(parsed.Remarks).ShouldBe("<pre><code class=\"language-csharp\">var x = 1;</code></pre>");
    }

    [Fact]
    public void Inline_Does_Not_Wrap_In_Paragraph()
    {
        var parsed = _parser.Parse("""<member><summary>Short sentence.</summary></member>""");

        _renderer.RenderInlineHtml(parsed.Summary).ShouldBe("Short sentence.");
    }
}
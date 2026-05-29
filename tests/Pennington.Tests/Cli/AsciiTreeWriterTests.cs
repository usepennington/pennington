namespace Pennington.Tests.Cli;

using Pennington.Cli;
using Shouldly;
using Xunit;

public class AsciiTreeWriterTests
{
    private sealed record Node(string Name, IReadOnlyList<Node> Children);

    [Fact]
    public void Renders_box_drawing_connectors()
    {
        var nodes = new List<Node>
        {
            new("a", [new Node("a1", []), new Node("a2", [])]),
            new("b", []),
        };

        var writer = new StringWriter();
        AsciiTreeWriter.Write(writer, nodes, n => n.Name, n => n.Children);

        writer.ToString().ReplaceLineEndings("\n")
            .ShouldBe("├─ a\n│  ├─ a1\n│  └─ a2\n└─ b\n");
    }

    [Fact]
    public void Respects_max_depth()
    {
        var nodes = new List<Node> { new("a", [new Node("a1", [])]) };

        var writer = new StringWriter();
        AsciiTreeWriter.Write(writer, nodes, n => n.Name, n => n.Children, maxDepth: 1);

        writer.ToString().ReplaceLineEndings("\n").ShouldBe("└─ a\n");
    }
}

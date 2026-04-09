namespace Pennington.FrontMatter;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

/// <summary>
/// Wraps an <see cref="IParser"/> to reject YAML anchors, aliases, and non-standard type tags.
/// Prevents billion-laughs expansion attacks and arbitrary type instantiation.
/// </summary>
internal sealed class SafeYamlParser(IParser inner) : IParser
{
    private static readonly HashSet<string> AllowedTags =
    [
        "tag:yaml.org,2002:str",
        "tag:yaml.org,2002:int",
        "tag:yaml.org,2002:float",
        "tag:yaml.org,2002:bool",
        "tag:yaml.org,2002:null",
        "tag:yaml.org,2002:seq",
        "tag:yaml.org,2002:map",
        "tag:yaml.org,2002:timestamp",
    ];

    public ParsingEvent? Current => inner.Current;

    public bool MoveNext()
    {
        var result = inner.MoveNext();

        switch (inner.Current)
        {
            case AnchorAlias alias:
                throw new YamlException(alias.Start, alias.End,
                    "YAML aliases are not permitted in front matter.");

            case NodeEvent { Anchor.IsEmpty: false } node:
                throw new YamlException(node.Start, node.End,
                    "YAML anchors are not permitted in front matter.");

            case NodeEvent { Tag: { IsNonSpecific: false, IsEmpty: false } tag } node
                when !AllowedTags.Contains(tag.Value):
                throw new YamlException(node.Start, node.End,
                    $"YAML type tags are not permitted in front matter. Tag: {tag.Value}");
        }

        return result;
    }
}

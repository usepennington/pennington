namespace Pennington.FrontMatter;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

/// <summary>
/// Replays a buffered list of <see cref="ParsingEvent"/>s as an <see cref="IParser"/>,
/// so a single character scan of the YAML can drive both the unknown-key diagnostic
/// pass and the deserialization pass without tokenizing the source twice.
/// </summary>
internal sealed class BufferedYamlParser(IReadOnlyList<ParsingEvent> events) : IParser
{
    private int _index = -1;

    /// <inheritdoc/>
    public ParsingEvent? Current => _index >= 0 && _index < events.Count ? events[_index] : null;

    /// <inheritdoc/>
    public bool MoveNext()
    {
        _index++;
        return _index < events.Count;
    }
}

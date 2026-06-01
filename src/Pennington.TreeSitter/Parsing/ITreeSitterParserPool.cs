namespace Pennington.TreeSitter.Parsing;

/// <summary>
/// Provides reusable tree-sitter parsers per language. Tree-sitter parsers are not thread-safe, so the pool
/// hands out one parser at a time via a disposable lease; languages (which are immutable) are shared.
/// </summary>
internal interface ITreeSitterParserPool : IDisposable
{
    /// <summary>Rents a parser bound to the named language. Dispose the returned lease to return it to the pool.</summary>
    ParserLease Rent(string treeSitterLanguageName);
}

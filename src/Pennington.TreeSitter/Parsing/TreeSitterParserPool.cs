namespace Pennington.TreeSitter.Parsing;

using System.Collections.Concurrent;
using TsLanguage = global::TreeSitter.Language;
using TsParser = global::TreeSitter.Parser;

/// <summary>A disposable rental of a tree-sitter parser; disposing returns the parser to its pool.</summary>
internal readonly struct ParserLease : IDisposable
{
    private readonly TreeSitterParserPool _pool;
    private readonly string _languageName;

    internal ParserLease(TreeSitterParserPool pool, string languageName, TsParser parser)
    {
        _pool = pool;
        _languageName = languageName;
        Parser = parser;
    }

    /// <summary>The rented parser, already bound to the requested language.</summary>
    public TsParser Parser { get; }

    /// <summary>Returns the parser to the pool for reuse.</summary>
    public void Dispose() => _pool.Return(_languageName, Parser);
}

/// <summary>
/// Caches one tree-sitter <see cref="TsLanguage"/> per language name (immutable, shared) and pools idle
/// <see cref="TsParser"/> instances for reuse. Registered as a process-lifetime singleton.
/// </summary>
internal sealed class TreeSitterParserPool : ITreeSitterParserPool
{
    private readonly ConcurrentDictionary<string, TsLanguage> _languages = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentBag<TsParser>> _idle = new(StringComparer.Ordinal);
    private bool _disposed;

    public ParserLease Rent(string treeSitterLanguageName)
    {
        var bag = _idle.GetOrAdd(treeSitterLanguageName, static _ => new ConcurrentBag<TsParser>());
        if (!bag.TryTake(out var parser))
        {
            var language = _languages.GetOrAdd(treeSitterLanguageName, static name => new TsLanguage(name));
            parser = new TsParser(language);
        }

        return new ParserLease(this, treeSitterLanguageName, parser);
    }

    internal void Return(string languageName, TsParser parser)
    {
        if (_disposed)
        {
            parser.Dispose();
            return;
        }

        _idle.GetOrAdd(languageName, static _ => new ConcurrentBag<TsParser>()).Add(parser);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var bag in _idle.Values)
        {
            while (bag.TryTake(out var parser))
            {
                parser.Dispose();
            }
        }

        foreach (var language in _languages.Values)
        {
            language.Dispose();
        }
    }
}

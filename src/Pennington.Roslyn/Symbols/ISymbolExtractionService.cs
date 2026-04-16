namespace Pennington.Roslyn.Symbols;

using Microsoft.CodeAnalysis;

/// <summary>Extracts symbol metadata and source fragments from the configured Roslyn <see cref="Solution"/>, keyed by xmldocid.</summary>
public interface ISymbolExtractionService
{
    /// <summary>Walks the solution and returns a map of xmldocid to <see cref="SymbolInfo"/> for every documented symbol.</summary>
    Task<IReadOnlyDictionary<string, SymbolInfo>> ExtractSymbolsAsync(Solution solution);

    /// <summary>Returns the <see cref="SymbolInfo"/> for the given xmldocid, or <see langword="null"/> if no symbol matches.</summary>
    Task<SymbolInfo?> FindSymbolAsync(string xmlDocId);

    /// <summary>Returns the source-code fragment for the symbol identified by <paramref name="xmlDocId"/>, optionally limited to the member body and optionally including leading trivia (comments/attributes).</summary>
    Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false, bool includeLeadingTrivia = true);

    /// <summary>Clears any cached symbol lookups so the next query re-walks the solution.</summary>
    void ClearCache();
}

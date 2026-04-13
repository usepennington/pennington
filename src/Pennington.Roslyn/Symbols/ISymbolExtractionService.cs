namespace Pennington.Roslyn.Symbols;

using Microsoft.CodeAnalysis;

public interface ISymbolExtractionService
{
    Task<IReadOnlyDictionary<string, SymbolInfo>> ExtractSymbolsAsync(Solution solution);
    Task<SymbolInfo?> FindSymbolAsync(string xmlDocId);
    Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false);
    void ClearCache();
}
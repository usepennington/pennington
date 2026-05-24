namespace Pennington.Roslyn.Symbols;

using Microsoft.CodeAnalysis;

/// <summary>Extracts symbol metadata and source fragments from the configured Roslyn <see cref="Solution"/>, keyed by xmldocid.</summary>
public interface ISymbolExtractionService
{
    /// <summary>Walks the given projects and returns a map of xmldocid to <see cref="SymbolInfo"/> for every documented symbol. Callers pass the workspace's filtered project set (one per multi-targeted csproj) so symbols aren't extracted once per target framework.</summary>
    Task<IReadOnlyDictionary<string, SymbolInfo>> ExtractSymbolsAsync(IEnumerable<Project> projects);

    /// <summary>Returns the <see cref="SymbolInfo"/> for the given xmldocid, or <see langword="null"/> if no symbol matches.</summary>
    Task<SymbolInfo?> FindSymbolAsync(string xmlDocId);

    /// <summary>Returns the source-code fragment for the symbol identified by <paramref name="xmlDocId"/>, optionally limited to the member body and optionally including leading trivia (comments/attributes).</summary>
    Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false, bool includeLeadingTrivia = true);

    /// <summary>Returns the source-code fragment for the symbol identified by <paramref name="xmlDocId"/> together with the file-local <c>using</c> directives the fragment depends on. <see cref="CodeFragmentResult.Usings"/> is empty when the symbol cannot be resolved.</summary>
    Task<CodeFragmentResult> ExtractCodeFragmentWithUsingsAsync(string xmlDocId, bool bodyOnly = false, bool includeLeadingTrivia = true);

    /// <summary>Returns the declaration signature (no body, no accessor list) for the member identified by <paramref name="xmlDocId"/>. Falls back to the full declaration for symbols that have no separable body.</summary>
    Task<string> ExtractDeclarationSignatureAsync(string xmlDocId);

    /// <summary>Clears any cached symbol lookups so the next query re-walks the solution.</summary>
    void ClearCache();

    /// <summary>Triggers the symbol table walk if it has not already run. Safe to call concurrently with real queries — both share one <see cref="Infrastructure.AsyncLazy{T}"/> task.</summary>
    Task WarmupAsync(CancellationToken cancellationToken = default);
}
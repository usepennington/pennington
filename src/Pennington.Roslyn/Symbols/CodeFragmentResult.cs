namespace Pennington.Roslyn.Symbols;

using System.Collections.Immutable;

/// <summary>The dedented source-text fragment for a symbol plus the file-local <c>using</c> directives the fragment depends on.</summary>
/// <param name="Fragment">Dedented source text for the symbol's declaration or body.</param>
/// <param name="Usings">Required file-local using directives in their original source order, each as the verbatim directive text (e.g. <c>using System.Text;</c>).</param>
public sealed record CodeFragmentResult(string Fragment, ImmutableList<string> Usings);

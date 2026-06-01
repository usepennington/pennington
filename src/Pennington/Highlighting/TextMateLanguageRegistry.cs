namespace Pennington.Highlighting;

using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

/// <summary>
/// Registry over the built-in TextMate grammar set. Exposes the underlying
/// <see cref="Registry"/> and resolves a language identifier to its TextMate scope.
/// </summary>
public sealed class TextMateLanguageRegistry
{
    private readonly Registry _registry;
    private readonly RegistryOptions _registryOptions;

    /// <summary>Initializes the registry over the built-in grammar set.</summary>
    public TextMateLanguageRegistry()
    {
        _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        _registry = new Registry(_registryOptions);
    }

    /// <summary>Gets the internal TextMate registry instance.</summary>
    internal Registry Registry => _registry;

    /// <summary>Resolves the TextMate scope name for a built-in language identifier, or <see langword="null"/> when unknown.</summary>
    internal string? GetScopeNameForLanguage(string languageId) =>
        _registryOptions.GetScopeByLanguageId(languageId.ToLowerInvariant());
}

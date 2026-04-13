namespace Pennington.Highlighting;

using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

/// <summary>
/// Registry for managing TextMate language grammars and scope mappings.
/// Allows registration of custom languages in addition to built-in ones.
/// </summary>
public sealed class TextMateLanguageRegistry
{
    private readonly Registry _registry;
    private readonly CustomGrammarRegistryOptions _registryOptions;
    private readonly Dictionary<string, string> _customScopeMappings = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TextMateLanguageRegistry"/> class.
    /// </summary>
    /// <param name="configure">Optional callback to configure custom languages.</param>
    public TextMateLanguageRegistry(Action<TextMateLanguageRegistry>? configure = null)
    {
        _registryOptions = new CustomGrammarRegistryOptions(ThemeName.DarkPlus);
        _registry = new Registry(_registryOptions);

        configure?.Invoke(this);
    }

    /// <summary>
    /// Gets the internal TextMate registry instance.
    /// </summary>
    internal Registry Registry => _registry;

    /// <summary>
    /// Adds a custom language-to-scope mapping.
    /// </summary>
    /// <param name="languageId">The language identifier (e.g., "mylang").</param>
    /// <param name="scopeName">The TextMate scope name (e.g., "source.mylang").</param>
    /// <returns>This instance for method chaining.</returns>
    public TextMateLanguageRegistry AddGrammar(string languageId, string scopeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeName);

        _customScopeMappings[languageId.ToLowerInvariant()] = scopeName;
        return this;
    }

    /// <summary>
    /// Loads a grammar from a JSON string and associates it with a language identifier.
    /// </summary>
    /// <param name="languageId">The language identifier (e.g., "mylang").</param>
    /// <param name="grammarJson">The TextMate grammar in JSON format.</param>
    /// <returns>This instance for method chaining.</returns>
    public TextMateLanguageRegistry AddGrammarFromJson(string languageId, string grammarJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(grammarJson);

        var scopeName = $"source.{languageId.ToLowerInvariant()}";

        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(grammarJson);
            if (jsonDoc.RootElement.TryGetProperty("scopeName", out var scopeNameElement))
            {
                var extractedScope = scopeNameElement.GetString();
                if (!string.IsNullOrEmpty(extractedScope))
                {
                    scopeName = extractedScope;
                }
            }
        }
        catch
        {
            // If parsing fails, use the default scope name
        }

        _registryOptions.AddCustomGrammar(scopeName, grammarJson);
        _customScopeMappings[languageId.ToLowerInvariant()] = scopeName;

        return this;
    }

    /// <summary>
    /// Attempts to get the scope name for a given language identifier.
    /// Checks custom mappings first, then falls back to built-in language IDs.
    /// </summary>
    internal string? GetScopeNameForLanguage(string languageId)
    {
        var normalizedId = languageId.ToLowerInvariant();

        if (_customScopeMappings.TryGetValue(normalizedId, out var scopeName))
        {
            return scopeName;
        }

        return _registryOptions.BaseOptions.GetScopeByLanguageId(normalizedId);
    }

    /// <summary>
    /// Custom RegistryOptions that supports loading grammars from JSON strings at runtime.
    /// </summary>
    private sealed class CustomGrammarRegistryOptions : IRegistryOptions
    {
        private readonly RegistryOptions _baseOptions;
        private readonly Dictionary<string, string> _customGrammars = new();

        public CustomGrammarRegistryOptions(ThemeName defaultTheme)
        {
            _baseOptions = new RegistryOptions(defaultTheme);
        }

        internal RegistryOptions BaseOptions => _baseOptions;

        public void AddCustomGrammar(string scopeName, string grammarJson)
        {
            _customGrammars[scopeName] = grammarJson;
        }

        public IRawGrammar? GetGrammar(string scopeName)
        {
            if (_customGrammars.TryGetValue(scopeName, out var grammarJson))
            {
                try
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(grammarJson);
                    using var stream = new MemoryStream(bytes);
                    using var reader = new StreamReader(stream);
                    return GrammarReader.ReadGrammarSync(reader);
                }
                catch
                {
                    // Fall through to base implementation
                }
            }

            return _baseOptions.GetGrammar(scopeName);
        }

        public IRawTheme GetTheme(string scopeName) => _baseOptions.GetTheme(scopeName);

        public ICollection<string> GetInjections(string scopeName) => _baseOptions.GetInjections(scopeName);

        public IRawTheme GetDefaultTheme() => _baseOptions.GetDefaultTheme();
    }
}
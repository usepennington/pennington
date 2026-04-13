namespace Prism.V2.Generators;

/// <summary>
/// Generates strongly-typed enum helpers from annotated enums.
/// </summary>
public class EnumGenerator
{
    private readonly List<string> _diagnostics = [];

    /// <summary>
    /// Initializes the generator with the compilation context.
    /// </summary>
    public void Initialize(GeneratorContext context)
    {
        context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
        _diagnostics.Clear();
    }

    /// <summary>
    /// Executes the source generation for all discovered enums.
    /// </summary>
    public void Execute(GeneratorContext context)
    {
        if (context.SyntaxReceiver is not EnumSyntaxReceiver receiver)
            return;

        foreach (var enumDecl in receiver.Candidates)
        {
            var source = GenerateEnumHelper(enumDecl, context);
            context.AddSource($"{enumDecl.Name}_Extensions.g.cs", source);
        }
    }

    private string GenerateEnumHelper(EnumDeclaration enumDecl, GeneratorContext context)
    {
        return $$"""
            namespace {{enumDecl.Namespace}};

            public static class {{enumDecl.Name}}Extensions
            {
                public static string ToDisplayString(this {{enumDecl.Name}} value)
                    => value.ToString();

                public static bool TryParse(string value, out {{enumDecl.Name}} result)
                    => Enum.TryParse(value, ignoreCase: true, out result);
            }
            """;
    }
}

// Supporting types for the generator
public record GeneratorContext
{
    public object? SyntaxReceiver { get; init; }
    public void RegisterForSyntaxNotifications(Func<object> factory) { }
    public void AddSource(string hintName, string source) { }
}

public record EnumDeclaration(string Name, string Namespace);

public class EnumSyntaxReceiver
{
    public List<EnumDeclaration> Candidates { get; } = [];
}
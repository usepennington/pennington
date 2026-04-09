namespace Prism.V1.Generators;

/// <summary>
/// Legacy enum generator — v1 implementation preserved for documentation diffs.
/// </summary>
public class EnumGenerator
{
    /// <summary>
    /// Initializes the generator.
    /// </summary>
    public void Initialize(object context)
    {
        // V1 had no syntax receiver registration
    }

    /// <summary>
    /// Executes the source generation.
    /// </summary>
    public void Execute(object context)
    {
        // V1 used reflection to find enums at runtime
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var enumTypes = assembly.GetTypes().Where(t => t.IsEnum);
            foreach (var enumType in enumTypes)
            {
                GenerateHelper(enumType);
            }
        }
    }

    private void GenerateHelper(Type enumType)
    {
        // V1 generated to disk, not to compilation
        var source = $"// Generated helper for {enumType.Name}";
        File.WriteAllText($"{enumType.Name}_Helper.cs", source);
    }
}

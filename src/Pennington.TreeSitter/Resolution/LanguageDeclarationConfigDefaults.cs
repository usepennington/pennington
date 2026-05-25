namespace Pennington.TreeSitter.Resolution;

/// <summary>Built-in <see cref="LanguageDeclarationConfig"/> definitions seeded into <see cref="TreeSitterOptions"/>.</summary>
public static class LanguageDeclarationConfigDefaults
{
    /// <summary>
    /// Creates the default fence-language to declaration-config map, keyed case-insensitively and including
    /// common aliases (e.g. <c>cs</c>, <c>py</c>, <c>ts</c>, <c>rs</c>).
    /// </summary>
    public static Dictionary<string, LanguageDeclarationConfig> CreateDefaults()
    {
        var map = new Dictionary<string, LanguageDeclarationConfig>(StringComparer.OrdinalIgnoreCase);

        Add(map, CSharp(), "csharp", "cs", "c#");
        Add(map, Python(), "python", "py");
        Add(map, TypeScript(), "typescript", "ts");
        Add(map, JavaScript(), "javascript", "js");
        Add(map, Java(), "java");
        Add(map, Rust(), "rust", "rs");
        Add(map, Go(), "go", "golang");
        Add(map, Ruby(), "ruby", "rb");
        Add(map, Php(), "php");

        return map;
    }

    private static void Add(Dictionary<string, LanguageDeclarationConfig> map, LanguageDeclarationConfig config, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            map[alias] = config;
        }
    }

    private static LanguageDeclarationConfig CSharp() => new()
    {
        TreeSitterLanguageName = "C#",
        DeclarationNodeTypes = new HashSet<string>
        {
            "class_declaration", "struct_declaration", "record_declaration", "record_struct_declaration",
            "interface_declaration", "enum_declaration", "method_declaration", "constructor_declaration",
            "property_declaration", "delegate_declaration",
        },
        // Namespaces are structural: address types as `Type.Member`, not `Namespace.Type.Member`.
        TransparentNodeTypes = new HashSet<string>
        {
            "declaration_list", "namespace_declaration", "file_scoped_namespace_declaration",
        },
        ImportNodeTypes = new HashSet<string> { "using_directive" },
    };

    private static LanguageDeclarationConfig Python() => new()
    {
        TreeSitterLanguageName = "Python",
        DeclarationNodeTypes = new HashSet<string> { "class_definition", "function_definition" },
        TransparentNodeTypes = new HashSet<string> { "block", "decorated_definition" },
        ImportNodeTypes = new HashSet<string>
        {
            "import_statement", "import_from_statement", "future_import_statement",
        },
    };

    private static LanguageDeclarationConfig TypeScript() => new()
    {
        TreeSitterLanguageName = "TypeScript",
        DeclarationNodeTypes = new HashSet<string>
        {
            "class_declaration", "abstract_class_declaration", "function_declaration",
            "method_definition", "interface_declaration", "enum_declaration",
        },
        TransparentNodeTypes = new HashSet<string> { "class_body", "export_statement" },
        ImportNodeTypes = new HashSet<string> { "import_statement" },
    };

    private static LanguageDeclarationConfig JavaScript() => new()
    {
        TreeSitterLanguageName = "JavaScript",
        DeclarationNodeTypes = new HashSet<string>
        {
            "class_declaration", "function_declaration", "generator_function_declaration", "method_definition",
        },
        TransparentNodeTypes = new HashSet<string> { "class_body", "export_statement", "statement_block" },
        ImportNodeTypes = new HashSet<string> { "import_statement" },
    };

    private static LanguageDeclarationConfig Java() => new()
    {
        TreeSitterLanguageName = "Java",
        DeclarationNodeTypes = new HashSet<string>
        {
            "class_declaration", "interface_declaration", "enum_declaration", "record_declaration",
            "annotation_type_declaration", "method_declaration", "constructor_declaration",
        },
        TransparentNodeTypes = new HashSet<string> { "class_body", "interface_body", "enum_body", "annotation_type_body" },
        ImportNodeTypes = new HashSet<string> { "import_declaration" },
    };

    private static LanguageDeclarationConfig Ruby() => new()
    {
        TreeSitterLanguageName = "Ruby",
        DeclarationNodeTypes = new HashSet<string> { "class", "module", "method", "singleton_method" },
        TransparentNodeTypes = new HashSet<string> { "body_statement" },
    };

    private static LanguageDeclarationConfig Php() => new()
    {
        TreeSitterLanguageName = "PHP",
        DeclarationNodeTypes = new HashSet<string>
        {
            "class_declaration", "interface_declaration", "trait_declaration", "enum_declaration",
            "method_declaration", "function_definition",
        },
        TransparentNodeTypes = new HashSet<string> { "declaration_list" },
        ImportNodeTypes = new HashSet<string> { "namespace_use_declaration" },
    };

    private static LanguageDeclarationConfig Rust() => new()
    {
        TreeSitterLanguageName = "Rust",
        DeclarationNodeTypes = new HashSet<string>
        {
            "struct_item", "enum_item", "trait_item", "impl_item", "function_item",
            "mod_item", "const_item", "static_item", "type_item", "union_item",
        },
        // An `impl Calculator` block is identified by its implemented type, so `Calculator.method`
        // descends into the impl as well as the struct (both match the `Calculator` segment).
        NameFieldOverrides = new Dictionary<string, string> { ["impl_item"] = "type" },
        TransparentNodeTypes = new HashSet<string> { "declaration_list" },
        ImportNodeTypes = new HashSet<string> { "use_declaration" },
    };

    private static LanguageDeclarationConfig Go() => new()
    {
        TreeSitterLanguageName = "Go",
        DeclarationNodeTypes = new HashSet<string> { "function_declaration", "method_declaration", "type_spec" },
        TransparentNodeTypes = new HashSet<string> { "type_declaration" },
        ImportNodeTypes = new HashSet<string> { "import_declaration" },
    };
}

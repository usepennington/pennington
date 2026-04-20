namespace Pennington.ApiMetadata.Reflection;

using System.Reflection;
using System.Text;

/// <summary>Builds human-readable C# signatures (return types, parameter types, declarations) for reflection types. Output is fed to <see cref="Pennington.Highlighting.ICodeHighlighter"/> and displayed in the API reference UI.</summary>
internal static class SignatureFormatter
{
    public static string Display(Type t)
    {
        if (t.IsByRef) return "ref " + Display(t.GetElementType()!);
        if (t.IsPointer) return Display(t.GetElementType()!) + "*";
        if (t.IsArray)
        {
            var elem = Display(t.GetElementType()!);
            var rank = t.GetArrayRank();
            return rank == 1 ? elem + "[]" : elem + "[" + new string(',', rank - 1) + "]";
        }
        if (t.IsGenericParameter) return t.Name;
        if (t.IsConstructedGenericType)
        {
            var def = t.GetGenericTypeDefinition();
            if (def.Namespace == "System" && def.Name == "Nullable`1")
            {
                return Display(t.GetGenericArguments()[0]) + "?";
            }
            var baseName = def.Name.Contains('`') ? def.Name[..def.Name.IndexOf('`')] : def.Name;
            var sb = new StringBuilder(baseName).Append('<');
            var args = t.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Display(args[i]));
            }
            sb.Append('>');
            return sb.ToString();
        }

        return t.FullName switch
        {
            "System.Void" => "void",
            "System.Object" => "object",
            "System.String" => "string",
            "System.Boolean" => "bool",
            "System.Byte" => "byte",
            "System.SByte" => "sbyte",
            "System.Int16" => "short",
            "System.UInt16" => "ushort",
            "System.Int32" => "int",
            "System.UInt32" => "uint",
            "System.Int64" => "long",
            "System.UInt64" => "ulong",
            "System.Single" => "float",
            "System.Double" => "double",
            "System.Decimal" => "decimal",
            "System.Char" => "char",
            _ => t.Name,
        };
    }

    public static string MemberDeclaration(MemberInfo m) => m switch
    {
        MethodInfo method => MethodDeclaration(method),
        ConstructorInfo ctor => CtorDeclaration(ctor),
        PropertyInfo prop => PropertyDeclaration(prop),
        FieldInfo field => FieldDeclaration(field),
        EventInfo evt => EventDeclaration(evt),
        _ => m.Name,
    };

    private static string MethodDeclaration(MethodInfo m)
    {
        var sb = new StringBuilder();
        sb.Append("public ");
        if (m.IsStatic) sb.Append("static ");
        sb.Append(Display(m.ReturnType)).Append(' ').Append(m.Name);
        if (m.IsGenericMethod)
        {
            sb.Append('<').Append(string.Join(", ", m.GetGenericArguments().Select(a => a.Name))).Append('>');
        }
        AppendParameters(sb, m.GetParameters());
        return sb.ToString();
    }

    private static string CtorDeclaration(ConstructorInfo c)
    {
        var sb = new StringBuilder("public ").Append(c.DeclaringType?.Name ?? "ctor");
        AppendParameters(sb, c.GetParameters());
        return sb.ToString();
    }

    private static string PropertyDeclaration(PropertyInfo p)
    {
        var sb = new StringBuilder("public ");
        var getMethod = p.GetMethod;
        if (getMethod is not null && getMethod.IsStatic) sb.Append("static ");
        sb.Append(Display(p.PropertyType)).Append(' ').Append(p.Name).Append(" { ");
        if (p.CanRead) sb.Append("get; ");
        if (p.CanWrite) sb.Append("set; ");
        sb.Append('}');
        return sb.ToString();
    }

    private static string FieldDeclaration(FieldInfo f)
    {
        var sb = new StringBuilder("public ");
        if (f.IsStatic) sb.Append("static ");
        if (f.IsInitOnly) sb.Append("readonly ");
        if (f.IsLiteral) sb.Append("const ");
        sb.Append(Display(f.FieldType)).Append(' ').Append(f.Name);
        return sb.ToString();
    }

    private static string EventDeclaration(EventInfo e)
        => "public event " + Display(e.EventHandlerType!) + " " + e.Name;

    private static void AppendParameters(StringBuilder sb, ParameterInfo[] parameters)
    {
        sb.Append('(');
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            var p = parameters[i];
            if (p.ParameterType.IsByRef)
            {
                // ParameterInfo.IsOut and IsIn disambiguate ref/out/in on by-ref types.
                sb.Append(p.IsOut ? "out " : p.IsIn ? "in " : "ref ");
                sb.Append(Display(p.ParameterType.GetElementType()!));
            }
            else
            {
                sb.Append(Display(p.ParameterType));
            }
            sb.Append(' ').Append(p.Name);
        }
        sb.Append(')');
    }
}

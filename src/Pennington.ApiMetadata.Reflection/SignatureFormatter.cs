namespace Pennington.ApiMetadata.Reflection;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>Builds human-readable C# signatures (return types, parameter types, declarations) for reflection types. Output is fed to <see cref="Highlighting.ICodeHighlighter"/> and displayed in the API reference UI.</summary>
internal static class SignatureFormatter
{
    public static string Display(Type t)
    {
        if (t.IsByRef)
        {
            return "ref " + Display(t.GetElementType()!);
        }

        if (t.IsPointer)
        {
            return Display(t.GetElementType()!) + "*";
        }

        if (t.IsArray)
        {
            var elem = Display(t.GetElementType()!);
            var rank = t.GetArrayRank();
            return rank == 1 ? elem + "[]" : elem + "[" + new string(',', rank - 1) + "]";
        }
        if (t.IsGenericParameter)
        {
            return t.Name;
        }

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
                if (i > 0)
                {
                    sb.Append(", ");
                }

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
        if (m.IsStatic)
        {
            sb.Append("static ");
        }

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
        if (getMethod is not null && getMethod.IsStatic)
        {
            sb.Append("static ");
        }

        sb.Append(Display(p.PropertyType)).Append(' ').Append(p.Name).Append(" { ");
        if (p.CanRead)
        {
            sb.Append("get; ");
        }

        if (p.CanWrite)
        {
            sb.Append("set; ");
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string FieldDeclaration(FieldInfo f)
    {
        var sb = new StringBuilder("public ");
        if (f.IsStatic)
        {
            sb.Append("static ");
        }

        if (f.IsInitOnly)
        {
            sb.Append("readonly ");
        }

        if (f.IsLiteral)
        {
            sb.Append("const ");
        }

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
            if (i > 0)
            {
                sb.Append(", ");
            }

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
            if (p.HasDefaultValue)
            {
                sb.Append(" = ").Append(FormatConstant(p.RawDefaultValue, p.ParameterType));
            }
        }
        sb.Append(')');
    }

    /// <summary>
    /// Renders the full type declaration line — modifiers, kind keyword, name with type
    /// parameters, record positional parameters, base list, and generic constraints —
    /// e.g. <c>public sealed record Foo&lt;T&gt;(int A) : Bar, IBaz where T : class</c>.
    /// </summary>
    public static string TypeDeclaration(Type t)
    {
        var sb = new StringBuilder("public ");

        if (t.IsAbstract && t.IsSealed && !t.IsEnum)
        {
            sb.Append("static ");
        }
        else if (t.IsSealed && !t.IsValueType && !t.IsEnum && !IsDelegate(t))
        {
            sb.Append("sealed ");
        }
        else if (t.IsAbstract && !t.IsInterface)
        {
            sb.Append("abstract ");
        }

        var isRecord = IsRecord(t);
        var keyword = t.IsEnum ? "enum"
            : t.IsInterface ? "interface"
            : IsDelegate(t) ? "delegate"
            : t.IsValueType ? (isRecord ? "record struct" : "struct")
            : isRecord ? "record" : "class";
        sb.Append(keyword).Append(' ').Append(GenericTypeName(t));

        AppendRecordParameters(sb, t, isRecord);
        AppendBaseList(sb, t);
        AppendConstraints(sb, t);
        return sb.ToString();
    }

    /// <summary>Renders an extension method as a stand-alone signature with <c>this</c> on the receiver parameter, used on the receiver type's reference page.</summary>
    public static string ExtensionSignature(MethodInfo m)
    {
        var sb = new StringBuilder();
        sb.Append(Display(m.ReturnType)).Append(' ').Append(m.Name);
        if (m.IsGenericMethod)
        {
            sb.Append('<').Append(string.Join(", ", m.GetGenericArguments().Select(a => a.Name))).Append('>');
        }

        var parameters = m.GetParameters();
        sb.Append('(');
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            if (i == 0)
            {
                sb.Append("this ");
            }

            sb.Append(Display(parameters[i].ParameterType)).Append(' ').Append(parameters[i].Name);
        }
        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>Formats a compile-time constant (const field value or default parameter value) as the C# literal a reader would type.</summary>
    public static string FormatConstant(object? value, Type type)
    {
        if (value is null)
        {
            return type.IsValueType && Nullable.GetUnderlyingType(type) is null ? "default" : "null";
        }

        return value switch
        {
            string s => "\"" + s + "\"",
            bool b => b ? "true" : "false",
            char c => "'" + c + "'",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? string.Empty,
        };
    }

    private static void AppendRecordParameters(StringBuilder sb, Type t, bool isRecord)
    {
        if (!isRecord)
        {
            return;
        }

        // The positional parameters live on the primary constructor — the public ctor whose
        // parameters are not the synthesized copy constructor (single same-type parameter).
        var primary = t.GetConstructors()
            .FirstOrDefault(c => c.GetParameters() is { Length: > 0 } ps
                && !(ps.Length == 1 && ps[0].ParameterType == t));
        if (primary is null)
        {
            return;
        }

        AppendParameters(sb, primary.GetParameters());
    }

    private static void AppendBaseList(StringBuilder sb, Type t)
    {
        if (t.IsEnum || t.IsInterface || IsDelegate(t))
        {
            return;
        }

        var bases = new List<string>();
        if (t.BaseType is { } bt && bt != typeof(object) && bt != typeof(ValueType))
        {
            bases.Add(Display(bt));
        }

        bases.AddRange(DirectInterfaces(t).Select(Display));
        if (bases.Count > 0)
        {
            sb.Append(" : ").Append(string.Join(", ", bases));
        }
    }

    private static void AppendConstraints(StringBuilder sb, Type t)
    {
        if (!t.IsGenericTypeDefinition)
        {
            return;
        }

        foreach (var arg in t.GetGenericArguments())
        {
            var clause = ConstraintClause(arg);
            if (clause is not null)
            {
                sb.Append(" where ").Append(arg.Name).Append(" : ").Append(clause);
            }
        }
    }

    private static string? ConstraintClause(Type arg)
    {
        var parts = new List<string>();
        var attrs = arg.GenericParameterAttributes;
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
        {
            parts.Add("class");
        }
        else if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
        {
            parts.Add("struct");
        }

        foreach (var c in arg.GetGenericParameterConstraints())
        {
            if (c == typeof(ValueType))
            {
                continue; // covered by the struct constraint above
            }

            parts.Add(Display(c));
        }

        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
            && !attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
        {
            parts.Add("new()");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    private static IEnumerable<Type> DirectInterfaces(Type t)
    {
        // GetInterfaces() is transitive; subtract those already implied by the base type and by
        // other implemented interfaces to leave only the directly-declared set.
        var all = t.GetInterfaces();
        var implied = new HashSet<Type>();
        if (t.BaseType is { } bt)
        {
            implied.UnionWith(bt.GetInterfaces());
        }

        foreach (var i in all)
        {
            implied.UnionWith(i.GetInterfaces());
        }

        return all.Where(i => !implied.Contains(i));
    }

    private static string GenericTypeName(Type t)
    {
        if (!t.IsGenericType)
        {
            return t.Name;
        }

        var name = t.Name;
        var tick = name.IndexOf('`');
        var baseName = tick < 0 ? name : name[..tick];
        return baseName + "<" + string.Join(", ", t.GetGenericArguments().Select(Display)) + ">";
    }

    private static bool IsRecord(Type t)
        => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(m => m.Name == "<Clone>$");

    private static bool IsDelegate(Type t)
        => typeof(MulticastDelegate).IsAssignableFrom(t.BaseType);
}
namespace Pennington.ApiMetadata.Reflection;

using System.Reflection;
using System.Text;

/// <summary>
/// Produces C# xmldocid strings (e.g. <c>T:Foo.Bar</c>, <c>M:Foo.Bar.Baz(System.Int32)</c>)
/// for types and members obtained through reflection. Mirrors the format emitted by the
/// C# compiler into <c>.xml</c> doc files so the string can index directly into the
/// companion xmldoc.
/// </summary>
internal static class XmlDocIdFormatter
{
    /// <summary>Returns the <c>T:</c> xmldocid for a type.</summary>
    public static string ForType(Type type) => "T:" + FullTypeName(type);

    /// <summary>Returns the appropriate <c>M:</c>/<c>P:</c>/<c>F:</c>/<c>E:</c> xmldocid for a member.</summary>
    public static string ForMember(MemberInfo member) => member.MemberType switch
    {
        MemberTypes.Method => "M:" + MethodName((MethodInfo)member) + Parameters(((MethodInfo)member).GetParameters()),
        MemberTypes.Constructor => "M:" + CtorName((ConstructorInfo)member) + Parameters(((ConstructorInfo)member).GetParameters()),
        MemberTypes.Property => "P:" + PropertyName((PropertyInfo)member),
        MemberTypes.Field => "F:" + FullTypeName(member.DeclaringType!) + "." + member.Name,
        MemberTypes.Event => "E:" + FullTypeName(member.DeclaringType!) + "." + member.Name,
        MemberTypes.NestedType => "T:" + FullTypeName((Type)member),
        _ => string.Empty,
    };

    private static string MethodName(MethodInfo m)
    {
        var sb = new StringBuilder();
        sb.Append(FullTypeName(m.DeclaringType!)).Append('.').Append(m.Name);
        if (m.IsGenericMethod)
        {
            sb.Append("``").Append(m.GetGenericArguments().Length);
        }
        return sb.ToString();
    }

    private static string CtorName(ConstructorInfo c)
    {
        var decl = FullTypeName(c.DeclaringType!);
        return decl + (c.IsStatic ? ".#cctor" : ".#ctor");
    }

    private static string PropertyName(PropertyInfo p)
    {
        var sb = new StringBuilder();
        sb.Append(FullTypeName(p.DeclaringType!)).Append('.').Append(p.Name);
        var indexParams = p.GetIndexParameters();
        if (indexParams.Length > 0)
        {
            sb.Append(Parameters(indexParams));
        }
        return sb.ToString();
    }

    private static string Parameters(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("(");
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(TypeRef(parameters[i].ParameterType));
        }
        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>
    /// Produces the xmldocid form of a type used in a method/property signature.
    /// Generic arguments use braces ({T}), generic parameters use backticks (`N for type,
    /// ``N for method), nested types use `.` (not `+`), arrays use `[]`, ref uses `@`,
    /// pointer uses `*`.
    /// </summary>
    private static string TypeRef(Type t)
    {
        if (t.IsByRef)
        {
            return TypeRef(t.GetElementType()!) + "@";
        }

        if (t.IsPointer)
        {
            return TypeRef(t.GetElementType()!) + "*";
        }

        if (t.IsArray)
        {
            var elem = TypeRef(t.GetElementType()!);
            var rank = t.GetArrayRank();
            return rank == 1 ? elem + "[]" : elem + "[" + new string(',', rank - 1) + "]";
        }
        if (t.IsGenericParameter)
        {
            var prefix = t.DeclaringMethod is not null ? "``" : "`";
            return prefix + t.GenericParameterPosition;
        }
        if (t.IsConstructedGenericType)
        {
            var def = t.GetGenericTypeDefinition();
            var baseName = StripGenericArity(FullTypeName(def));
            var sb = new StringBuilder(baseName).Append('{');
            var args = t.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(TypeRef(args[i]));
            }
            sb.Append('}');
            return sb.ToString();
        }
        return FullTypeName(t);
    }

    /// <summary>Namespace-qualified type name with nested-type separator normalized to <c>.</c>. Keeps generic-arity backticks (e.g. <c>List`1</c>).</summary>
    private static string FullTypeName(Type t)
    {
        if (t.IsNested)
        {
            return FullTypeName(t.DeclaringType!) + "." + t.Name;
        }
        return string.IsNullOrEmpty(t.Namespace) ? t.Name : t.Namespace + "." + t.Name;
    }

    private static string StripGenericArity(string name)
    {
        // Strip trailing `N from each segment so `List`1` → `List` before braces get applied.
        var sb = new StringBuilder(name.Length);
        for (var i = 0; i < name.Length; i++)
        {
            if (name[i] == '`')
            {
                while (i + 1 < name.Length && char.IsDigit(name[i + 1]))
                {
                    i++;
                }

                continue;
            }
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}
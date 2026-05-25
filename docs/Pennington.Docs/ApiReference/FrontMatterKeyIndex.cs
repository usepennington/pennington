namespace Pennington.Docs.ApiReference;

using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using Pennington.FrontMatter;
using Pennington.Infrastructure;

/// <summary>
/// One YAML front-matter key observed on one or more concrete <c>IFrontMatter</c> implementations.
/// </summary>
internal sealed record FrontMatterKeyEntry(
    string YamlKey,
    string ClrPropertyName,
    string TypeDisplay,
    string? DefaultValue,
    ImmutableArray<string> AppliesTo,
    string DeclaringSurface,
    string XmlDocId);

/// <summary>
/// Singleton that reflects over every public type implementing <see cref="IFrontMatter"/> in the
/// referenced Pennington assemblies and projects their declared properties into a per-YAML-key
/// catalog for the front-matter reference page. Pure reflection — no compilation.
/// </summary>
internal sealed class FrontMatterKeyIndex
{
    private static readonly string[] CapabilityInterfaces =
    [
        "IFrontMatter",
        "ITaggable",
        "IOrderable",
        "ISectionable",
        "IRedirectable",
    ];

    private readonly AsyncLazy<ImmutableArray<FrontMatterKeyEntry>> _entries;

    public FrontMatterKeyIndex()
    {
        _entries = new AsyncLazy<ImmutableArray<FrontMatterKeyEntry>>(() => Task.FromResult(Build()));
    }

    public Task<ImmutableArray<FrontMatterKeyEntry>> GetEntriesAsync() => _entries.Value;

    private static ImmutableArray<FrontMatterKeyEntry> Build()
    {
        var observations = new List<(string YamlKey, string Clr, string TypeDisplay, string? DefaultValue, string Record, string Surface, string XmlDocId)>();
        var nullability = new NullabilityInfoContext();

        foreach (var type in FrontMatterTypes())
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                observations.Add((
                    YamlKey: ToCamelCase(property.Name),
                    Clr: property.Name,
                    TypeDisplay: TypeDisplay(property, nullability),
                    DefaultValue: ExtractDefault(property, nullability),
                    Record: type.Name,
                    Surface: ResolveDeclaringSurface(type, property.Name),
                    XmlDocId: "P:" + FullName(type) + "." + property.Name));
            }
        }

        return observations
            .GroupBy(o => o.YamlKey, StringComparer.Ordinal)
            .Select(group =>
            {
                var primary = group.First();
                var records = group
                    .Select(o => o.Record)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(r => r, StringComparer.Ordinal)
                    .ToImmutableArray();

                var types = group
                    .Select(o => o.TypeDisplay)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var typeDisplay = types.Count == 1 ? types[0] : string.Join(" / ", types);

                var defaults = group
                    .Select(o => o.DefaultValue)
                    .Where(d => d is not null)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var defaultValue = defaults.Count switch
                {
                    0 => null,
                    1 => defaults[0],
                    _ => string.Join(" / ", defaults),
                };

                var xmlDocId = group
                    .FirstOrDefault(o => !string.IsNullOrEmpty(o.XmlDocId))
                    .XmlDocId ?? primary.XmlDocId;

                return new FrontMatterKeyEntry(
                    YamlKey: primary.YamlKey,
                    ClrPropertyName: primary.Clr,
                    TypeDisplay: typeDisplay,
                    DefaultValue: defaultValue,
                    AppliesTo: records,
                    DeclaringSurface: primary.Surface,
                    XmlDocId: xmlDocId);
            })
            .OrderBy(e => e.YamlKey, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static IEnumerable<Type> FrontMatterTypes()
    {
        var entry = Assembly.GetEntryAssembly()?.GetName().Name;
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "Pennington*.dll"))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, entry, StringComparison.Ordinal))
            {
                continue;
            }

            Assembly asm;
            try { asm = Assembly.Load(new AssemblyName(name)); }
            catch { continue; }

            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null).ToArray()!; }
            catch { continue; }

            foreach (var t in types)
            {
                if (t is { IsPublic: true, IsAbstract: false }
                    && (t.IsClass || t.IsValueType)
                    && typeof(IFrontMatter).IsAssignableFrom(t))
                {
                    yield return t;
                }
            }
        }
    }

    private static string ResolveDeclaringSurface(Type record, string propertyName)
    {
        foreach (var iface in record.GetInterfaces())
        {
            if (!CapabilityInterfaces.Contains(iface.Name, StringComparer.Ordinal))
            {
                continue;
            }

            if (iface.GetProperties().Any(p => string.Equals(p.Name, propertyName, StringComparison.Ordinal)))
            {
                return iface.Name;
            }
        }

        return "record-local";
    }

    private static string? ExtractDefault(PropertyInfo property, NullabilityInfoContext nullability)
    {
        // Property initializers (= 0, = "x") compile into the constructor and are not visible to
        // reflection, so fall back to type heuristics when no initializer was present.
        var type = property.PropertyType;
        if (Nullable.GetUnderlyingType(type) is not null
            || (!type.IsValueType && nullability.Create(property).ReadState == NullabilityState.Nullable))
        {
            return "null";
        }

        if (type == typeof(bool))
        {
            return "false";
        }

        if (type == typeof(string))
        {
            return "\"\"";
        }

        return null;
    }

    private static string TypeDisplay(PropertyInfo property, NullabilityInfoContext nullability)
    {
        var display = TypeName(property.PropertyType);
        if (!property.PropertyType.IsValueType
            && nullability.Create(property).ReadState == NullabilityState.Nullable)
        {
            display += "?";
        }

        return display;
    }

    private static string TypeName(Type t)
    {
        if (Nullable.GetUnderlyingType(t) is { } underlying)
        {
            return TypeName(underlying) + "?";
        }

        if (t.IsArray)
        {
            return TypeName(t.GetElementType()!) + "[]";
        }

        if (t.IsGenericType)
        {
            var name = t.Name;
            var tick = name.IndexOf('`');
            var baseName = tick < 0 ? name : name[..tick];
            return baseName + "<" + string.Join(", ", t.GetGenericArguments().Select(TypeName)) + ">";
        }

        return t.FullName switch
        {
            "System.String" => "string",
            "System.Boolean" => "bool",
            "System.Int32" => "int",
            "System.Int64" => "long",
            "System.Double" => "double",
            "System.Object" => "object",
            _ => t.Name,
        };
    }

    private static string FullName(Type t)
    {
        if (t.IsNested)
        {
            return FullName(t.DeclaringType!) + "." + t.Name;
        }

        return string.IsNullOrEmpty(t.Namespace) ? t.Name : t.Namespace + "." + t.Name;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

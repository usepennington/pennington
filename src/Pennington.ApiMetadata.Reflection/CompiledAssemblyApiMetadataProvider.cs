namespace Pennington.ApiMetadata.Reflection;

using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pennington.Highlighting;
using Pennington.Infrastructure;

/// <summary>
/// Reflection-backed <see cref="IApiMetadataProvider"/>. Loads configured <c>.dll</c> files
/// via <see cref="MetadataLoadContext"/>, pairs each with its companion <c>.xml</c> xmldoc
/// file, and materializes the provider DTOs. No source code and no MSBuild workspace required.
/// </summary>
public sealed class CompiledAssemblyApiMetadataProvider : IApiMetadataProvider
{
    private readonly CompiledAssemblyApiOptions _options;
    private readonly IXmlDocParser _xmlDocParser;
    private readonly ICodeHighlighter _highlighter;
    private readonly AsyncLazy<Catalog> _catalog;

    /// <summary>Initializes the provider.</summary>
    public CompiledAssemblyApiMetadataProvider(
        CompiledAssemblyApiOptions options,
        IXmlDocParser xmlDocParser,
        ICodeHighlighter highlighter)
    {
        _options = options;
        _xmlDocParser = xmlDocParser;
        _highlighter = highlighter;
        _catalog = new AsyncLazy<Catalog>(LoadAsync);
    }

    /// <inheritdoc />
    public async Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync() => (await _catalog.Value).Types;

    /// <inheritdoc />
    public async Task<ApiTypeDetail?> GetTypeAsync(string uid)
    {
        var cat = await _catalog.Value;
        return cat.TypeDetails.TryGetValue(uid, out var d) ? d : null;
    }

    /// <inheritdoc />
    public async Task<ImmutableArray<ApiMember>> GetMembersAsync(
        string typeUid, MemberKind kind, AccessFilter access, MemberOrder order)
    {
        var cat = await _catalog.Value;
        if (!cat.MembersByType.TryGetValue(typeUid, out var all))
        {
            return [];
        }

        var filtered = kind == MemberKind.All ? all : all.Where(m => m.Kind == kind);
        return order switch
        {
            MemberOrder.Alphabetical => filtered
                .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                .ToImmutableArray(),
            _ => filtered.ToImmutableArray(),
        };
    }

    /// <inheritdoc />
    public Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName)
        => Task.FromResult(ImmutableArray<ExtensionMethodEntry>.Empty);

    /// <inheritdoc />
    public async Task<ParsedXmlDoc> GetXmldocAsync(string uid)
    {
        var cat = await _catalog.Value;
        return cat.Xmldocs.TryGetValue(uid, out var d) ? d : ParsedXmlDoc.Empty;
    }

    /// <inheritdoc />
    public async Task<ApiMember?> GetMemberAsync(string uid)
    {
        var cat = await _catalog.Value;
        return cat.MembersByUid.TryGetValue(uid, out var m) ? m : null;
    }

    private async Task<Catalog> LoadAsync()
    {
        var typeBuilder = ImmutableArray.CreateBuilder<ApiTypeSummary>();
        var typeDetails = new Dictionary<string, ApiTypeDetail>(StringComparer.Ordinal);
        var membersByType = new Dictionary<string, List<ApiMember>>(StringComparer.Ordinal);
        var membersByUid = new Dictionary<string, ApiMember>(StringComparer.Ordinal);
        var xmldocs = new Dictionary<string, ParsedXmlDoc>(StringComparer.Ordinal);

        var assemblyPaths = new List<string>();
        foreach (var dir in _options.AssemblyDirectories)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            assemblyPaths.AddRange(Directory.EnumerateFiles(dir, "*.dll", SearchOption.TopDirectoryOnly));
        }
        foreach (var path in _options.AssemblyFiles)
        {
            if (File.Exists(path))
            {
                assemblyPaths.Add(path);
            }
        }
        if (assemblyPaths.Count == 0)
        {
            return Catalog.Empty;
        }

        var resolverPaths = BuildResolverPaths(assemblyPaths);
        var resolver = new PathAssemblyResolver(resolverPaths);
        using var ctx = new MetadataLoadContext(resolver);

        foreach (var dll in assemblyPaths)
        {
            Assembly asm;
            try { asm = ctx.LoadFromAssemblyPath(dll); }
            catch { continue; }

            var xmlPath = Path.ChangeExtension(dll, ".xml");
            var xmldoc = XmlDocFile.Load(xmlPath, _xmlDocParser);
            var assemblyName = asm.GetName().Name ?? Path.GetFileNameWithoutExtension(dll);

            Type[] types;
            try { types = asm.GetExportedTypes(); }
            catch { continue; }

            foreach (var t in types)
            {
                try
                {
                    ReflectType(t, assemblyName, xmldoc, typeBuilder, typeDetails, membersByType, membersByUid, xmldocs);
                }
                catch
                {
                    // Skip any type whose metadata couldn't be fully resolved (e.g. a
                    // reference assembly is missing). One bad type shouldn't blank the
                    // whole provider.
                }
            }
        }

        await Task.Yield(); // surface async contract without extra cost

        return new Catalog(
            typeBuilder.OrderBy(t => t.FullTypeName, StringComparer.OrdinalIgnoreCase).ToImmutableArray(),
            typeDetails.ToImmutableDictionary(),
            membersByType.ToImmutableDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray()),
            membersByUid.ToImmutableDictionary(),
            xmldocs.ToImmutableDictionary());
    }

    private void ReflectType(
        Type type,
        string assemblyName,
        XmlDocFile xmldoc,
        ImmutableArray<ApiTypeSummary>.Builder typeBuilder,
        Dictionary<string, ApiTypeDetail> typeDetails,
        Dictionary<string, List<ApiMember>> membersByType,
        Dictionary<string, ApiMember> membersByUid,
        Dictionary<string, ParsedXmlDoc> xmldocs)
    {
        if (ShouldSkipType(type))
        {
            return;
        }

        var uid = XmlDocIdFormatter.ForType(type);
        var parsed = xmldoc.Get(uid);
        xmldocs[uid] = parsed;

        var kind = ClassifyType(type);
        var summary = new ApiTypeSummary(
            Uid: uid,
            Name: DisplayTypeName(type),
            Namespace: type.Namespace ?? string.Empty,
            Assembly: assemblyName,
            Kind: kind,
            Summary: FirstSentence(parsed));
        typeBuilder.Add(summary);

        typeDetails[uid] = new ApiTypeDetail(
            Summary: summary,
            Xmldoc: parsed,
            SignatureHtml: _highlighter.Highlight(BuildTypeDeclaration(type), "csharp"),
            Inheritance: InheritanceChain(type),
            Implements: ImplementedInterfaces(type),
            Source: null);

        var list = new List<ApiMember>();
        foreach (var m in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            ApiMember? apiMember;
            try { apiMember = TryReflectMember(m, xmldoc); }
            catch { continue; }
            if (apiMember is null)
            {
                continue;
            }

            list.Add(apiMember);
            membersByUid[apiMember.Uid] = apiMember;
            xmldocs[apiMember.Uid] = apiMember.Xmldoc;
        }

        if (list.Count > 0)
        {
            membersByType[uid] = list;
        }

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
        {
            ReflectType(nested, assemblyName, xmldoc, typeBuilder, typeDetails, membersByType, membersByUid, xmldocs);
        }
    }

    private ApiMember? TryReflectMember(MemberInfo m, XmlDocFile xmldoc)
    {
        if (IsCompilerGenerated(m))
        {
            return null;
        }

        if (m is MethodInfo method && ShouldSkipMethod(method))
        {
            return null;
        }

        var uid = XmlDocIdFormatter.ForMember(m);
        if (string.IsNullOrEmpty(uid))
        {
            return null;
        }

        var parsed = xmldoc.Get(uid);
        var kind = ClassifyMember(m);
        if (kind is null)
        {
            return null;
        }

        var parameters = ImmutableArray<ApiParameter>.Empty;
        string? returnTypeDisplay = null;
        var typeDisplay = string.Empty;
        var isRequired = false;

        switch (m)
        {
            case MethodInfo mi:
                parameters = BuildParameters(mi.GetParameters(), parsed);
                var returnsVoid = string.Equals(mi.ReturnType.FullName, "System.Void", StringComparison.Ordinal);
                if (!returnsVoid)
                {
                    returnTypeDisplay = SignatureFormatter.Display(mi.ReturnType);
                }
                typeDisplay = SignatureFormatter.Display(mi.ReturnType);
                break;
            case ConstructorInfo ci:
                parameters = BuildParameters(ci.GetParameters(), parsed);
                break;
            case PropertyInfo pi:
                typeDisplay = SignatureFormatter.Display(pi.PropertyType);
                isRequired = IsRequired(pi);
                break;
            case FieldInfo fi:
                typeDisplay = SignatureFormatter.Display(fi.FieldType);
                break;
            case EventInfo ei:
                typeDisplay = SignatureFormatter.Display(ei.EventHandlerType!);
                break;
        }

        return new ApiMember(
            Uid: uid,
            Name: MemberDisplayName(m),
            Kind: kind.Value,
            TypeDisplay: typeDisplay,
            DefaultValue: null,
            IsRequired: isRequired,
            HasInheritDocDirective: false,
            Xmldoc: parsed,
            SignatureHtml: _highlighter.Highlight(SignatureFormatter.MemberDeclaration(m), "csharp"),
            Parameters: parameters,
            ReturnTypeDisplay: returnTypeDisplay);
    }

    private static ImmutableArray<ApiParameter> BuildParameters(ParameterInfo[] parameters, ParsedXmlDoc parsed)
    {
        if (parameters.Length == 0)
        {
            return [];
        }

        var builder = ImmutableArray.CreateBuilder<ApiParameter>(parameters.Length);
        foreach (var p in parameters)
        {
            var description = parsed.Params.TryGetValue(p.Name ?? string.Empty, out var nodes) ? nodes : [];
            builder.Add(new ApiParameter(
                Name: p.Name ?? string.Empty,
                TypeDisplay: SignatureFormatter.Display(p.ParameterType),
                Description: description));
        }
        return builder.ToImmutable();
    }

    private static bool ShouldSkipType(Type t)
    {
        if (!(t.IsPublic || t.IsNestedPublic))
        {
            return false;  // GetExportedTypes already handles this; belt-and-suspenders
        }
        // Filter out compiler-generated helper types.
        if (t.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
        {
            return true;
        }
        // Skip nested types here — the outer type enumerates them explicitly so we visit
        // them with the right reflection context. Treating them as top-level doubles them up.
        if (t.IsNested)
        {
            return true;
        }

        return false;
    }

    private static bool ShouldSkipMethod(MethodInfo m)
    {
        // Skip property/event accessors and operator backing methods — they'll be surfaced
        // via the property/event itself.
        var name = m.Name;
        if (name.StartsWith("get_", StringComparison.Ordinal))
        {
            return true;
        }

        if (name.StartsWith("set_", StringComparison.Ordinal))
        {
            return true;
        }

        if (name.StartsWith("add_", StringComparison.Ordinal))
        {
            return true;
        }

        if (name.StartsWith("remove_", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static bool IsCompilerGenerated(MemberInfo m)
        => m.GetCustomAttributesData().Any(a => a.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);

    private static bool IsRequired(PropertyInfo p)
        => p.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");

    private static MemberKind? ClassifyMember(MemberInfo m) => m switch
    {
        PropertyInfo => MemberKind.Properties,
        FieldInfo => MemberKind.Fields,
        ConstructorInfo => MemberKind.Constructors,
        EventInfo => MemberKind.Events,
        MethodInfo => MemberKind.Methods,
        _ => null,
    };

    private static ApiTypeKind ClassifyType(Type t)
    {
        if (t.IsEnum)
        {
            return ApiTypeKind.Enum;
        }

        if (t.IsInterface)
        {
            return ApiTypeKind.Interface;
        }

        if (t.IsValueType)
        {
            return ApiTypeKind.Struct;
        }

        if (t.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute" && t.IsValueType))
        {
            return ApiTypeKind.Struct;
        }

        if (IsRecord(t))
        {
            return ApiTypeKind.Record;
        }

        if (typeof(MulticastDelegate).IsAssignableFrom(t.BaseType))
        {
            return ApiTypeKind.Delegate;
        }

        return ApiTypeKind.Class;
    }

    private static bool IsRecord(Type t)
        => t.GetMethods().Any(m => m.Name == "<Clone>$" && m.DeclaringType == t);

    private static string DisplayTypeName(Type t)
    {
        if (!t.IsGenericType)
        {
            return t.Name;
        }

        var name = t.Name;
        var tick = name.IndexOf('`');
        if (tick < 0)
        {
            return name;
        }

        var baseName = name[..tick];
        var args = t.GetGenericArguments();
        return baseName + "<" + string.Join(", ", args.Select(a => a.Name)) + ">";
    }

    private static string MemberDisplayName(MemberInfo m)
    {
        if (m is ConstructorInfo c)
        {
            return c.DeclaringType?.Name.Split('`')[0] ?? ".ctor";
        }

        if (m is MethodInfo mi && mi.IsGenericMethod)
        {
            return mi.Name + "<" + string.Join(", ", mi.GetGenericArguments().Select(a => a.Name)) + ">";
        }
        return m.Name;
    }

    private static string BuildTypeDeclaration(Type t)
    {
        var kind = t.IsEnum ? "enum"
            : t.IsInterface ? "interface"
            : t.IsValueType ? "struct"
            : IsRecord(t) ? "record"
            : typeof(MulticastDelegate).IsAssignableFrom(t.BaseType) ? "delegate"
            : "class";

        var access = "public ";
        var modifiers = t.IsSealed && !t.IsValueType && !t.IsEnum ? "sealed " : t.IsAbstract && !t.IsInterface ? "abstract " : string.Empty;
        return access + modifiers + kind + " " + DisplayTypeName(t);
    }

    private static ImmutableArray<string> InheritanceChain(Type t)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        for (var cur = t.BaseType; cur is not null; cur = cur.BaseType)
        {
            builder.Add(XmlDocIdFormatter.ForType(cur));
        }
        return builder.ToImmutable();
    }

    private static ImmutableArray<string> ImplementedInterfaces(Type t)
    {
        var ifaces = t.GetInterfaces();
        if (ifaces.Length == 0)
        {
            return [];
        }

        return ifaces.Select(XmlDocIdFormatter.ForType).ToImmutableArray();
    }

    private static string? FirstSentence(ParsedXmlDoc doc)
    {
        if (!doc.HasSummary)
        {
            return null;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var node in doc.Summary)
        {
            switch (node.Value)
            {
                case TextNode t: sb.Append(t.Text); break;
                case InlineCodeNode c: sb.Append(c.Text); break;
                case ParaNode p:
                    foreach (var child in p.Children)
                    {
                        if (child.Value is TextNode pt)
                        {
                            sb.Append(pt.Text);
                        }
                    }
                    break;
            }
        }
        var collapsed = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        if (collapsed.Length == 0)
        {
            return null;
        }

        var period = collapsed.IndexOf('.');
        return period > 0 ? collapsed[..(period + 1)] : collapsed;
    }

    private static IEnumerable<string> BuildResolverPaths(IEnumerable<string> assemblyPaths)
    {
        // MetadataLoadContext rejects duplicate AssemblyNames, so we dedupe by the
        // assembly's simple name (the filename without extension) and keep the first
        // winner. Target assemblies come first, then the running .NET runtime, then
        // peer shared frameworks (AspNetCore.App, WindowsDesktop.App) at their latest
        // version only.
        var bySimpleName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void Offer(string dllPath)
        {
            var simple = Path.GetFileNameWithoutExtension(dllPath);
            if (!bySimpleName.ContainsKey(simple))
            {
                bySimpleName[simple] = dllPath;
            }
        }

        foreach (var path in assemblyPaths)
        {
            Offer(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                foreach (var sibling in Directory.EnumerateFiles(dir, "*.dll"))
                {
                    Offer(sibling);
                }
            }
        }

        var runtime = RuntimeEnvironment.GetRuntimeDirectory();
        if (Directory.Exists(runtime))
        {
            foreach (var dll in Directory.EnumerateFiles(runtime, "*.dll"))
            {
                Offer(dll);
            }
        }

        foreach (var frameworkDir in DiscoverLatestSharedFrameworks(runtime))
        {
            foreach (var dll in Directory.EnumerateFiles(frameworkDir, "*.dll"))
            {
                Offer(dll);
            }
        }

        return bySimpleName.Values;
    }

    private static IEnumerable<string> DiscoverLatestSharedFrameworks(string runtimeDir)
    {
        // Runtime dir looks like .../shared/Microsoft.NETCore.App/X.Y.Z
        // Peer shared frameworks (AspNetCore.App, WindowsDesktop.App) sit under `shared`.
        // Only yield the highest-version subdirectory of each to avoid duplicates.
        var ncaDir = new DirectoryInfo(runtimeDir);
        var sharedRoot = ncaDir.Parent?.Parent;
        if (sharedRoot is null || !sharedRoot.Exists)
        {
            yield break;
        }

        foreach (var frameworkRoot in sharedRoot.EnumerateDirectories())
        {
            if (string.Equals(frameworkRoot.Name, ncaDir.Parent!.Name, StringComparison.OrdinalIgnoreCase))
            {
                // Already covered by RuntimeEnvironment.GetRuntimeDirectory().
                continue;
            }
            var latest = frameworkRoot.EnumerateDirectories()
                .OrderByDescending(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (latest is not null)
            {
                yield return latest.FullName;
            }
        }
    }

    private sealed record Catalog(
        ImmutableArray<ApiTypeSummary> Types,
        ImmutableDictionary<string, ApiTypeDetail> TypeDetails,
        ImmutableDictionary<string, ImmutableArray<ApiMember>> MembersByType,
        ImmutableDictionary<string, ApiMember> MembersByUid,
        ImmutableDictionary<string, ParsedXmlDoc> Xmldocs)
    {
        public static Catalog Empty { get; } = new(
            [],
            ImmutableDictionary<string, ApiTypeDetail>.Empty,
            ImmutableDictionary<string, ImmutableArray<ApiMember>>.Empty,
            ImmutableDictionary<string, ApiMember>.Empty,
            ImmutableDictionary<string, ParsedXmlDoc>.Empty);
    }
}
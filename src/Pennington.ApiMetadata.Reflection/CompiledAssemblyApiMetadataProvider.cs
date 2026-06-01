namespace Pennington.ApiMetadata.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
    public async Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync() => (await _catalog).Types;

    /// <inheritdoc />
    public async Task<ApiTypeDetail?> GetTypeAsync(string uid)
    {
        var cat = await _catalog;
        return cat.TypeDetails.TryGetValue(uid, out var d) ? d : null;
    }

    /// <inheritdoc />
    public async Task<ImmutableArray<ApiMember>> GetMembersAsync(
        string typeUid, MemberKind kind, AccessFilter access, MemberOrder order)
    {
        var cat = await _catalog;
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
    public async Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName)
    {
        var cat = await _catalog;
        return cat.Extensions.TryGetValue(receiverTypeName, out var entries) ? entries : [];
    }

    /// <inheritdoc />
    public async Task<ParsedXmlDoc> GetXmldocAsync(string uid)
    {
        var cat = await _catalog;
        return cat.Xmldocs.TryGetValue(uid, out var d) ? d : ParsedXmlDoc.Empty;
    }

    /// <inheritdoc />
    public async Task<ApiMember?> GetMemberAsync(string uid)
    {
        var cat = await _catalog;
        return cat.MembersByUid.TryGetValue(uid, out var m) ? m : null;
    }

    private async Task<Catalog> LoadAsync()
    {
        var assemblyPaths = new List<string>();
        foreach (var dir in _options.AssemblyDirectories)
        {
            if (Directory.Exists(dir))
            {
                assemblyPaths.AddRange(Directory.EnumerateFiles(dir, "*.dll", SearchOption.TopDirectoryOnly));
            }
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

        var resolver = new PathAssemblyResolver(BuildResolverPaths(assemblyPaths));
        using var ctx = new MetadataLoadContext(resolver);

        // Pass 1: load each assembly and accumulate one global uid → raw xmldoc map. The map
        // spans assemblies so <inheritdoc/> can resolve against a base declared elsewhere.
        var rawByUid = new Dictionary<string, string>(StringComparer.Ordinal);
        var loaded = new List<(Assembly Asm, string Name)>();
        foreach (var dll in assemblyPaths)
        {
            Assembly asm;
            try { asm = ctx.LoadFromAssemblyPath(dll); }
            catch { continue; }

            XmlDocFile.LoadInto(Path.ChangeExtension(dll, ".xml"), rawByUid);
            loaded.Add((asm, asm.GetName().Name ?? Path.GetFileNameWithoutExtension(dll)));
        }

        // Pass 2: reflect every exported type against the assembled doc map.
        var load = new LoadContext(rawByUid, _xmlDocParser, _highlighter);
        foreach (var (asm, name) in loaded)
        {
            Type[] types;
            try { types = asm.GetExportedTypes(); }
            catch { continue; }

            foreach (var t in types)
            {
                try { load.ReflectType(t, name); }
                catch
                {
                    // Skip any type whose metadata couldn't be fully resolved (e.g. a missing
                    // reference assembly). One bad type shouldn't blank the whole provider.
                }
            }
        }

        await Task.Yield(); // surface async contract without extra cost
        return load.ToCatalog();
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
                continue; // already covered by RuntimeEnvironment.GetRuntimeDirectory()
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

    /// <summary>Per-load accumulator: walks reflected types into the provider DTOs against a fixed uid → raw xmldoc map.</summary>
    private sealed class LoadContext(
        IReadOnlyDictionary<string, string> rawByUid,
        IXmlDocParser parser,
        ICodeHighlighter highlighter)
    {
        private readonly ImmutableArray<ApiTypeSummary>.Builder _types = ImmutableArray.CreateBuilder<ApiTypeSummary>();
        private readonly Dictionary<string, ApiTypeDetail> _typeDetails = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ApiMember>> _membersByType = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ApiMember> _membersByUid = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ParsedXmlDoc> _xmldocs = new(StringComparer.Ordinal);
        private readonly List<ExtensionMethodEntry> _extensions = [];

        public void ReflectType(Type type, string assemblyName)
        {
            if (ShouldSkipType(type))
            {
                return;
            }

            var uid = XmlDocIdFormatter.ForType(type);
            if (!ShouldDocument(type, uid))
            {
                return;
            }

            var parsed = ResolveTypeDoc(type, uid);
            _xmldocs[uid] = parsed;

            var summary = new ApiTypeSummary(
                Uid: uid,
                Name: DisplayTypeName(type),
                Namespace: type.Namespace ?? string.Empty,
                Assembly: assemblyName,
                Kind: ClassifyType(type),
                Summary: FirstSentence(parsed));
            _types.Add(summary);

            _typeDetails[uid] = new ApiTypeDetail(
                Summary: summary,
                Xmldoc: parsed,
                SignatureHtml: highlighter.Highlight(SignatureFormatter.TypeDeclaration(type), "csharp"),
                Inheritance: InheritanceChain(type),
                Implements: ImplementedInterfaces(type),
                Source: null);

            var isUnion = IsUnion(type);
            var list = new List<ApiMember>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var m in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AddMember(list, seen, m, inheritedFrom: null, suppressUndocumented: isUnion);
            }

            // Interfaces show members inherited from documented base interfaces (e.g. a recently
            // split IContentService : IContentEmitter), grouped under the declaring interface.
            if (type.IsInterface)
            {
                foreach (var baseInterface in type.GetInterfaces())
                {
                    if (!rawByUid.ContainsKey(XmlDocIdFormatter.ForType(baseInterface)))
                    {
                        continue; // skip undocumented framework interfaces (IDisposable, ...)
                    }

                    foreach (var m in baseInterface.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        AddMember(list, seen, m, inheritedFrom: baseInterface, suppressUndocumented: false);
                    }
                }
            }

            if (isUnion)
            {
                foreach (var caseMember in BuildUnionCases(type))
                {
                    if (seen.Add(caseMember.Uid))
                    {
                        list.Add(caseMember);
                        _membersByUid[caseMember.Uid] = caseMember;
                        _xmldocs[caseMember.Uid] = caseMember.Xmldoc;
                    }
                }
            }

            if (list.Count > 0)
            {
                _membersByType[uid] = list;
            }

            if (type is { IsAbstract: true, IsSealed: true } && type.Name.EndsWith("Extensions", StringComparison.Ordinal))
            {
                CollectExtensions(type, assemblyName);
            }
        }

        public Catalog ToCatalog() => new(
            _types.OrderBy(t => t.FullTypeName, StringComparer.OrdinalIgnoreCase).ToImmutableArray(),
            _typeDetails.ToImmutableDictionary(),
            _membersByType.ToImmutableDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray()),
            _membersByUid.ToImmutableDictionary(),
            _xmldocs.ToImmutableDictionary(),
            _extensions
                .GroupBy(e => e.ReceiverTypeName, StringComparer.Ordinal)
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                          .ThenBy(e => e.Signature.Length)
                          .ToImmutableArray(),
                    StringComparer.Ordinal));

        private void AddMember(List<ApiMember> list, HashSet<string> seen, MemberInfo m, Type? inheritedFrom, bool suppressUndocumented)
        {
            ApiMember? apiMember;
            try { apiMember = TryReflectMember(m, inheritedFrom, suppressUndocumented); }
            catch { return; }
            if (apiMember is null || !seen.Add(apiMember.Uid))
            {
                return;
            }

            list.Add(apiMember);
            _membersByUid[apiMember.Uid] = apiMember;
            _xmldocs[apiMember.Uid] = apiMember.Xmldoc;
        }

        private ApiMember? TryReflectMember(MemberInfo m, Type? inheritedFrom, bool suppressUndocumented)
        {
            if (IsCompilerGenerated(m) || (m is FieldInfo { Name: "value__" }))
            {
                return null;
            }

            if (m is MethodInfo method && ShouldSkipMethod(method))
            {
                return null;
            }

            var kind = ClassifyMember(m);
            if (kind is null)
            {
                return null;
            }

            var uid = XmlDocIdFormatter.ForMember(m);
            if (string.IsNullOrEmpty(uid))
            {
                return null;
            }

            // Implicitly-declared members (record/class primary constructors, default
            // constructors, synthesized union plumbing) carry no xmldoc — CS1591 guarantees
            // every hand-written public member does. Skip implicitly-declared members
            // without hiding genuinely-undocumented members on third-party assemblies: only drop
            // undocumented constructors and undocumented members of a union.
            if ((m is ConstructorInfo || suppressUndocumented) && !rawByUid.ContainsKey(uid))
            {
                return null;
            }

            var parsed = ResolveMemberDoc(m, uid, out var hasInheritDoc);

            var parameters = ImmutableArray<ApiParameter>.Empty;
            string? returnTypeDisplay = null;
            var typeDisplay = string.Empty;
            var isRequired = false;
            string? defaultValue = null;

            switch (m)
            {
                case MethodInfo mi:
                    parameters = BuildParameters(mi.GetParameters(), parsed);
                    typeDisplay = SignatureFormatter.Display(mi.ReturnType);
                    if (!string.Equals(mi.ReturnType.FullName, "System.Void", StringComparison.Ordinal))
                    {
                        returnTypeDisplay = typeDisplay;
                    }
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
                    if (fi.IsLiteral)
                    {
                        defaultValue = SignatureFormatter.FormatConstant(fi.GetRawConstantValue(), fi.FieldType);
                    }
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
                DefaultValue: defaultValue,
                IsRequired: isRequired,
                HasInheritDocDirective: hasInheritDoc,
                Xmldoc: parsed,
                SignatureHtml: highlighter.Highlight(SignatureFormatter.MemberDeclaration(m), "csharp"),
                Parameters: parameters,
                ReturnTypeDisplay: returnTypeDisplay,
                InheritedFromUid: inheritedFrom is null ? null : XmlDocIdFormatter.ForType(inheritedFrom),
                InheritedFromName: inheritedFrom?.Name);
        }

        private IEnumerable<ApiMember> BuildUnionCases(Type type)
        {
            // Each case surfaces as the single parameter of a generated constructor; the compiler
            // emits one such ctor per case (the case type is never the union itself).
            var caseTypes = type.GetConstructors()
                .Where(c => c.GetParameters().Length == 1)
                .Select(c => c.GetParameters()[0].ParameterType)
                .Where(ct => ct != type)
                .Distinct();

            foreach (var caseType in caseTypes)
            {
                var uid = XmlDocIdFormatter.ForType(caseType);
                var raw = rawByUid.GetValueOrDefault(uid);
                yield return new ApiMember(
                    Uid: uid,
                    Name: DisplayTypeName(caseType),
                    Kind: MemberKind.UnionCases,
                    TypeDisplay: SignatureFormatter.Display(caseType),
                    DefaultValue: null,
                    IsRequired: false,
                    HasInheritDocDirective: raw is not null && raw.Contains("inheritdoc", StringComparison.Ordinal),
                    Xmldoc: ResolveTypeDoc(caseType, uid),
                    SignatureHtml: highlighter.Highlight(SignatureFormatter.TypeDeclaration(caseType), "csharp"),
                    Parameters: ImmutableArray<ApiParameter>.Empty,
                    ReturnTypeDisplay: null);
            }
        }

        private void CollectExtensions(Type type, string assemblyName)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (!IsExtensionMethod(method))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    continue;
                }

                var receiver = StripArity(parameters[0].ParameterType.Name);
                var uid = XmlDocIdFormatter.ForMember(method);
                if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(uid))
                {
                    continue;
                }

                _extensions.Add(new ExtensionMethodEntry(
                    Name: MemberDisplayName(method),
                    Signature: SignatureFormatter.ExtensionSignature(method),
                    Package: assemblyName,
                    Uid: uid,
                    ReceiverTypeName: receiver,
                    Xmldoc: ResolveMemberDoc(method, uid, out _)));
            }
        }

        private bool ShouldDocument(Type type, string uid)
        {
            // Only document types carrying their own xmldoc,
            // excluding delegates, attributes, Razor components, and the top-level Program class.
            // Generated component/infrastructure types have no `///` doc, so the xmldoc check
            // alone filters most of them — the base-type checks catch the documented stragglers.
            if (!rawByUid.ContainsKey(uid) || type.Name == "Program")
            {
                return false;
            }

            return !InheritsFrom(type, "System.MulticastDelegate")
                && !InheritsFrom(type, "System.Attribute")
                && !InheritsFrom(type, "Microsoft.AspNetCore.Components.ComponentBase");
        }

        private static bool InheritsFrom(Type type, string baseFullName)
        {
            for (var b = type.BaseType; b is not null; b = b.BaseType)
            {
                if (string.Equals(b.FullName, baseFullName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private ParsedXmlDoc ResolveTypeDoc(Type type, string uid)
        {
            var raw = ReflectionInheritDocResolver.Resolve(type, rawByUid) ?? rawByUid.GetValueOrDefault(uid);
            return string.IsNullOrWhiteSpace(raw) ? ParsedXmlDoc.Empty : parser.Parse(raw);
        }

        private ParsedXmlDoc ResolveMemberDoc(MemberInfo member, string uid, out bool hasInheritDoc)
        {
            var rawSelf = rawByUid.GetValueOrDefault(uid);
            hasInheritDoc = rawSelf is not null && rawSelf.Contains("inheritdoc", StringComparison.Ordinal);

            var resolved = ReflectionInheritDocResolver.Resolve(member, rawByUid) ?? rawSelf;
            resolved = ReflectionRecordParamFallback.Resolve(resolved, member, rawByUid);
            return string.IsNullOrWhiteSpace(resolved) ? ParsedXmlDoc.Empty : parser.Parse(resolved);
        }

        private static bool IsExtensionMethod(MethodInfo m)
            => m.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute");

        private static string StripArity(string name)
        {
            var tick = name.IndexOf('`');
            return tick < 0 ? name : name[..tick];
        }
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

    private static bool ShouldSkipType(Type t) =>
        // Filter out compiler-generated helper types. Public nested types are kept:
        // GetExportedTypes() already returns them flattened, so the top-level pass
        // documents each one once (gated on having its own xmldoc by ShouldDocument).
        t.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

    private static bool ShouldSkipMethod(MethodInfo m)
    {
        // Skip property/event accessors and operators. Accessors surface via the property/event
        // itself; the member table lists only ordinary methods, so operators (op_Implicit,
        // op_Equality, ...) are excluded.
        var name = m.Name;
        return name.StartsWith("get_", StringComparison.Ordinal)
            || name.StartsWith("set_", StringComparison.Ordinal)
            || name.StartsWith("add_", StringComparison.Ordinal)
            || name.StartsWith("remove_", StringComparison.Ordinal)
            || name.StartsWith("op_", StringComparison.Ordinal);
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

        if (typeof(MulticastDelegate).IsAssignableFrom(t.BaseType))
        {
            return ApiTypeKind.Delegate;
        }

        if (IsRecord(t))
        {
            return ApiTypeKind.Record;
        }

        if (t.IsValueType)
        {
            return ApiTypeKind.Struct;
        }

        return ApiTypeKind.Class;
    }

    private static bool IsUnion(Type t)
    {
        if (!t.IsValueType)
        {
            return false;
        }

        // Both the C# 15 `union` keyword and the polyfill emit [Union] on the struct and
        // implement IUnion — either signal alone is enough.
        if (t.GetCustomAttributesData().Any(a =>
                a.AttributeType.Name == "UnionAttribute"
                && a.AttributeType.Namespace == "System.Runtime.CompilerServices"))
        {
            return true;
        }

        return t.GetInterfaces().Any(i =>
            i.Name == "IUnion" && i.Namespace == "System.Runtime.CompilerServices");
    }

    private static bool IsRecord(Type t)
        => t.GetMethods().Any(m => m.Name == "<Clone>$" && m.DeclaringType == t);

    private static string DisplayTypeName(Type t)
    {
        // Use the bare type name without generic-arity backtick or type-argument list.
        // The full generic form lives in the signature HTML; the short name here also
        // drives the page slug.
        var name = t.Name;
        var tick = name.IndexOf('`');
        return tick < 0 ? name : name[..tick];
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
        return ifaces.Length == 0 ? [] : ifaces.Select(XmlDocIdFormatter.ForType).ToImmutableArray();
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

    private sealed record Catalog(
        ImmutableArray<ApiTypeSummary> Types,
        ImmutableDictionary<string, ApiTypeDetail> TypeDetails,
        ImmutableDictionary<string, ImmutableArray<ApiMember>> MembersByType,
        ImmutableDictionary<string, ApiMember> MembersByUid,
        ImmutableDictionary<string, ParsedXmlDoc> Xmldocs,
        ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>> Extensions)
    {
        public static Catalog Empty { get; } = new(
            [],
            ImmutableDictionary<string, ApiTypeDetail>.Empty,
            ImmutableDictionary<string, ImmutableArray<ApiMember>>.Empty,
            ImmutableDictionary<string, ApiMember>.Empty,
            ImmutableDictionary<string, ParsedXmlDoc>.Empty,
            ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>.Empty);
    }
}

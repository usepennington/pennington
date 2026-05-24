namespace Pennington.FrontMatter;

using System.Collections.Concurrent;
using SharpYaml;
using SharpYaml.Serialization;

/// <summary>
/// Routes a type to the registered <see cref="YamlSerializerContext"/> that knows it — the
/// built-in <see cref="PenningtonYamlContext"/>, a satellite package context, or one a user
/// added via <see cref="Infrastructure.PenningtonExtensions.AddPenningtonYamlContext"/> — and
/// falls back to reflection for everything else. A source-generated context only serves the
/// types it was generated for and rejects foreign options, so each type is dispatched to its
/// own context rather than combined into a single resolver.
/// </summary>
public sealed class PenningtonYamlContextProvider
{
    /// <summary>A provider seeded with only the built-in context, for non-DI use (tests, scripts).</summary>
    public static PenningtonYamlContextProvider Default { get; } = new([PenningtonYamlContext.Default]);

    private readonly YamlSerializerContext[] _contexts;
    private readonly ConcurrentDictionary<Type, YamlSerializerContext?> _byType = new();

    /// <summary>Initializes the provider with the serializer contexts registered in DI.</summary>
    /// <param name="contexts">Registered contexts; the built-in <see cref="PenningtonYamlContext"/> is always present.</param>
    public PenningtonYamlContextProvider(IEnumerable<YamlSerializerContext> contexts)
        => _contexts = contexts as YamlSerializerContext[] ?? [.. contexts];

    /// <summary>
    /// Deserializes <paramref name="yaml"/> into <typeparamref name="T"/> using the source-generated
    /// context that covers <typeparamref name="T"/>, or reflection when none does.
    /// </summary>
    /// <param name="yaml">Raw YAML text.</param>
    public T? Deserialize<T>(string yaml)
    {
        var context = _byType.GetOrAdd(typeof(T), ResolveContext);
        return context is not null
            ? YamlSerializer.Deserialize<T>(yaml, context)
            : YamlSerializer.Deserialize<T>(yaml, PenningtonYaml.ReflectionOptions);
    }

    private YamlSerializerContext? ResolveContext(Type type)
    {
        foreach (var context in _contexts)
        {
            if (context.GetTypeInfo(type, context.Options) is not null)
            {
                return context;
            }
        }

        return null;
    }
}

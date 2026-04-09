namespace System.Runtime.CompilerServices;

#if !NET11_0_OR_GREATER
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class UnionAttribute<T> : Attribute;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class UnionTypeAttribute : Attribute;
#endif

internal interface IUnion;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class UnionAttribute : Attribute;

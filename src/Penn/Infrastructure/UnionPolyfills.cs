// Polyfill for C# 15 union types — required until .NET 11 RTM includes these in the BCL.
// The compiler recognizes the union keyword but needs these runtime types to emit the code.
namespace System.Runtime.CompilerServices;

/// <summary>Marker interface for compiler-generated union structs.</summary>
public interface IUnion { }

/// <summary>Attribute applied by the compiler to union struct declarations.</summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class UnionAttribute : Attribute { }

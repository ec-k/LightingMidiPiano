#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// A dummy type that allows the use of C# 9.0 init-only properties on older .NET versions.
    /// This class is recognized by the compiler only and does not affect runtime behavior.
    /// </summary>
    internal static class IsExternalInit
    {
    }
}
#endif

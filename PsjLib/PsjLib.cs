global using System.Collections.Generic;
global using System.Reflection;

namespace PsjLib;

/// <summary>
/// Provides version metadata for the managed <c>PsjLib</c> package.
/// </summary>
public static class LibraryVersion
{
    /// <summary>
    /// Semantic version of this library build.
    /// Value is sourced from assembly metadata generated from the project file.
    /// </summary>
    public static string Version =>
        typeof(LibraryVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? typeof(LibraryVersion).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";
}

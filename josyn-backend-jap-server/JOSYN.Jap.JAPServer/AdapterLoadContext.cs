using System.Reflection;
using System.Runtime.Loader;

namespace JOSYN.Jap.JAPServer;

/// <summary>
/// Isolated <see cref="AssemblyLoadContext"/> for loading company adapter assemblies.
/// Delegates assemblies already loaded in the default context back to it, preserving
/// type identity for interface casts (e.g. <c>IConfigSource</c> from
/// <c>JOSYN.Backend.AdapterContracts</c>). All other dependencies are resolved from
/// the adapter assembly's own directory.
/// </summary>
internal sealed class AdapterLoadContext(string adapterAssemblyPath) : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver = new(adapterAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // If the assembly is already loaded in the default context, reuse it.
        // This ensures shared contracts (AdapterContracts, ResultPattern) have the
        // same type identity in both the host and the adapter.
        var already = Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        if (already is not null)
            return already;

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is not null ? LoadFromAssemblyPath(path) : null;
    }
}

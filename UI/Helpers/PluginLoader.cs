using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace UI.Helpers;

public class PluginLoader : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoader(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Assembly? hostAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName));

        if (hostAssembly != null)
            return hostAssembly;

        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

        if (assemblyPath != null)
            return LoadFromAssemblyPath(assemblyPath);

        return null;
    }
}

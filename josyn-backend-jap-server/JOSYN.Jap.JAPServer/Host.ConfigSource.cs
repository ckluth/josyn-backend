using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
#pragma warning disable CA1859
    private static Result<IConfigSource> ResolveConfigSource(IBootstrapConfig config)
#pragma warning restore CA1859
    {
        if (config.ConfigSourceType is null)
            return new SqlConfigSource(config.SessionStoreConnectionString);

        return LoadAdapterConfigSource(config.ConfigSourceType);
    }

    private static Result<IConfigSource> LoadAdapterConfigSource(string typeName)
    {
        try
        {
            var parts = typeName.Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                return Result.Error(
                    $"Ungültiger ConfigSourceType-Wert: '{typeName}'. " +
                    $"Erwartet: 'FullTypeName, AssemblyName'");

            var assemblyFileName = parts[1] + ".dll";
            var adaptersFolder   = Path.Combine(AppContext.BaseDirectory, "Adapters");
            var assemblyPath     = Path.Combine(adaptersFolder, assemblyFileName);

            if (!File.Exists(assemblyPath))
                return Result.Error(
                    $"Adapter-Assembly nicht gefunden: '{assemblyPath}'. " +
                    $"Stelle sicher, dass die Assembly im 'Adapters/'-Ordner liegt.");

            var alc      = new AdapterLoadContext(assemblyPath);
            var assembly = alc.LoadFromAssemblyPath(assemblyPath);
            var type     = assembly.GetType(parts[0]);

            if (type is null)
                return Result.Error(
                    $"Adapter-Typ '{parts[0]}' nicht gefunden in '{assemblyPath}'.");

            if (Activator.CreateInstance(type) is not IConfigSource source)
                return Result.Error(
                    $"Typ '{parts[0]}' implementiert IConfigSource nicht.");

            return Result<IConfigSource>.Success(source);
        }
        catch (Exception ex) { return ex; }
    }
}

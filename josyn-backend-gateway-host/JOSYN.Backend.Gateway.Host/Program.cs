namespace JOSYN.Backend.Gateway.Host;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var app = HostFactory.Create(args);
        await app.RunAsync();
    }
}

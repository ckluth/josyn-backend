namespace JOSYN.Backend.GlobalConfig;

/// <summary>
/// PoC placeholder implementation of <see cref="IGlobalConfig"/> with compile-time constants.
/// Known limitation: values are machine-specific developer paths, identical in nature to the
/// hardcoded session key in <c>JOSYN.Jap.JAPServer</c>. Replace with a real config source
/// (file-based, registry, or company config system) before production use.
/// </summary>
public sealed class HardcodedGlobalConfig : IGlobalConfig
{
    /// <inheritdoc/>
    public string SessionStoreConnectionString =>
        "Server=localhost\\SQLEXPRESS01;Database=josyn-db-local;User Id=tu.josyn;Password=josyn;TrustServerCertificate=True;";

    /// <inheritdoc/>
    public string JapServerExePath =>
        @"C:\Temp\VS.OUT\JOSYN\JOSYN.Jap.JAPServer\bin\Release\JOSYN.Jap.JAPServer.exe";
}

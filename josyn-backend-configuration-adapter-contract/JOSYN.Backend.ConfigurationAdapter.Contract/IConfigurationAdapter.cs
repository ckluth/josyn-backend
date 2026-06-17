using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.ConfigurationAdapter.Contract;

/// <summary>
/// JIP protocol contract for the ConfigurationAdapter.
/// Implemented by the out-of-process adapter EXE; called by JAPServer
/// to retrieve configuration values from the company configuration store.
/// </summary>
public interface IConfigurationAdapter
{
    /// <summary>
    /// Returns the configuration value for the given <paramref name="settingPath"/>.
    /// Called by JAPServer when a job invokes <c>IJosynApplicationProtocol.GetConfigValue</c>.
    /// </summary>
    Task<Result<string>> GetConfigValue(string settingPath);
}

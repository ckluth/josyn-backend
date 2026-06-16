using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.IdentityAdapter.Contract;

/// <summary>
/// JIP protocol contract for the IdentityAdapter (ADR-017B-03).
/// Implemented by the out-of-process adapter EXE; called by JAPServer
/// before spawning <c>job.exe</c> to resolve impersonation credentials.
/// </summary>
public interface IIdentityAdapter
{
    /// <summary>
    /// Returns the password for the given <paramref name="username"/>.
    /// Called by JAPServer with the <c>TechnicalUserName</c> from <c>JobRegistry</c>.
    /// </summary>
    Task<Result<string>> GetPassword(string username);
}

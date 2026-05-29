using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStarter;

/// <inheritdoc cref="ISessionStarter"/>
public static class SessionStarter
{
    /// <inheritdoc cref="ISessionStarter.StartSession"/>
    public static Result<Guid> StartSession(StartSessionRequest request) =>
        new Error("Noch nicht implementiert.");
}

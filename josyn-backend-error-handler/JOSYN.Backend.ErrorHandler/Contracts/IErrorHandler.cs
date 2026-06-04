namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// Platform-wide error reporting contract.
/// Implementations are responsible for notification and durable storage.
/// </summary>
public interface IErrorHandler
{
    /// <summary>Reports an error by message.</summary>
    void Handle(string message);

    /// <summary>Reports an error by message and associated exception.</summary>
    void Handle(string message, Exception exception);
}

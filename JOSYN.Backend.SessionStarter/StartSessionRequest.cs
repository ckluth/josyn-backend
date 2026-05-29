namespace JOSYN.Backend.SessionStarter;

/// <summary>
/// The logical name and launch intent for a new job session.
/// </summary>
/// <param name="JobName">
/// The logical name that identifies the job in the job repository.
/// </param>
/// <remarks>
/// PoC-minimal contract. Additional fields (job executable path, pre-loaded arguments,
/// impersonation context) will be introduced during migration from the legacy backend.
/// </remarks>
public sealed record StartSessionRequest(string JobName);

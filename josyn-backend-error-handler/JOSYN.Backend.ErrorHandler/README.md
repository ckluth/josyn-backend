# JOSYN.Backend.ErrorHandler

Platform-wide error reporting endpoint for the JOSYN backend.
See [ADR-006](../../../josyn-platform/repos/josyn-backend/decisions/ADR-006-error-handler.md) for full rationale.

## Contract

`IErrorHandler` — fire-and-forget; never throws; callers are unaware of persistence failures.

```csharp
void Handle(
    string   message,
    string?  callStack        = null,
    string?  exceptionDetails = null,
    string?  jobName          = null,
    Guid?    sessionGuid      = null,
    [CallerMemberName] string caller = "");
```

## Error record

`IErrorRecord` / `ErrorRecord` — stored in `josyn.ErrorStore` (JOSYN Storage Realm).

Error kind is inferred from nullable context fields:

| `JobName` | `SessionGuid` | Kind |
|-----------|---------------|------|
| `null`    | `null`        | System error |
| set       | `null`        | Job error, no session |
| set       | set           | Job error, with session |

## Production implementation

`SqlErrorHandler(string connectionString)` — persists to `josyn.ErrorStore` via EF Core.
Falls back to `LocalLog` (`JOSYN.Commons.Log`) when SQL storage is unavailable.

## Placement rule

Backend-only. Called at every point where a `Result` chain terminates without a further
caller, and when `PutError` arrives from a job via IPC.
`job.exe` cannot reference this package — `PutError` over JIP is the bridge.

## Dependencies

- `JOSYN.Commons.Log` — LocalLog fallback
- `JOSYN.Foundation.ResultPattern` — platform baseline
- `Microsoft.EntityFrameworkCore.SqlServer` — storage implementation

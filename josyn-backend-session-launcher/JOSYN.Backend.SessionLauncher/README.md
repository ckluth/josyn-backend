# JOSYN.Backend.SessionLauncher

Orchestrator-side launcher for JOSYN job sessions. Part of the JOSYN Backend.

Provides `ISessionLauncher` / `SessionLauncher`: validates job registration, resolves
`TechnicalUserName` from `JobRegistry`, builds a `SessionStartSpec`, writes it to a temp file,
and spawns `JAPServer.exe JOSYN-START @<path>`.

## Usage

```csharp
ISessionLauncher launcher = new SessionLauncher(bootstrapConfig, jobRegistry);

var result = launcher.LaunchSession(new SessionLaunchRequest
{
    JobTypeName       = "demojob",
    Arguments         = "[Arguments]\nSomeParam=value",
    CallerUser        = Environment.UserName,
    CallerDomain      = Environment.UserDomainName,
    CallerApplication = AppDomain.CurrentDomain.FriendlyName,
    CallerMachine     = Environment.MachineName
});
```

## Dependencies

- `JOSYN.Backend.SessionLauncherContract`
- `JOSYN.Backend.BootstrapConfig`
- `JOSYN.Backend.JobRegistry`
- `JOSYN.Foundation.PropertyBag`
- `JOSYN.Foundation.ResultPattern`

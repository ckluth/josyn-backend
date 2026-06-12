# JOSYN.Backend.SessionLauncherContract

Shared data contract for the JOSYN session-start protocol. Part of the JOSYN Backend.

Provides `SessionStartRequest` — the record serialized by orchestrators and deserialized
by `JAPServer` in `JOSYN-START` mode.

## Usage

```csharp
var request = new SessionStartRequest
{
    JobTypeName       = "demojob",
    Arguments         = "[Arguments]\nSomeParam=value",
    CallerUser        = Environment.UserName,
    CallerDomain      = Environment.UserDomainName,
    CallerApplication = AppDomain.CurrentDomain.FriendlyName,
    CallerMachine     = Environment.MachineName
};
```

`TechnicalUserName` is populated by `JOSYN.Backend.SessionLauncher` — do not set it manually.

## Dependencies

- `JOSYN.Foundation.ResultPattern`

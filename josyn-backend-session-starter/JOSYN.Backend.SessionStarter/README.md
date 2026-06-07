# JOSYN.Backend.SessionStarter

**Session lifecycle entry point for JOSYN backend components.**

Provides `ISessionStarter` — the contract for starting a job session. `SessionStarter`
allocates a GUID, persists the session in the store, and spawns `JAPServer.exe` to handle
the session lifecycle.

## Responsibilities

1. Validate that `JAPServer.exe` exists at the configured path
2. Allocate a new session GUID
3. Persist the session (with arguments) via `ISessionStore`
4. Spawn `JAPServer.exe JOSYN-IPC <guid>` — fire-and-forget; JAPServer runs independently
5. Return the GUID to the caller

## Usage

```csharp
IGlobalConfig config = new HardcodedGlobalConfig();
ISessionStore store  = new SessionStore(config.SessionStoreConnectionString);
ISessionStarter starter = new SessionStarter(store, config);

var result = starter.StartSession("MyJobType", iniArguments);
if (result.Succeeded)
    Console.WriteLine($"Session started: {result.Value}");
```

## Known limitations

- If `Process.Start` succeeds but JAPServer crashes after the 500 ms startup window,
  the session row in the store remains with an empty `Result` field. A session status
  column would be needed to distinguish "running" from "failed-to-start".
- `HardcodedGlobalConfig.JapServerExePath` is a developer-machine compile-time path.
  Replace with a real config source before production use.

## Dependencies

| Package | Role |
|---------|------|
| `JOSYN.Backend.SessionStore` | Persists session records |
| `JOSYN.Backend.GlobalConfig` | Supplies `JapServerExePath` |
| `JOSYN.Foundation.ResultPattern` | Error-as-value result type |

---

MIT License — Copyright © 2026 HAEVG AG

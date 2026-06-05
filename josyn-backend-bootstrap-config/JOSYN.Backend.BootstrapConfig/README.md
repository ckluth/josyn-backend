# JOSYN.Backend.GlobalConfig

**Runtime configuration contract for JOSYN backend components.**

Provides `IGlobalConfig` — the stable interface that backend components use to obtain
connection strings and runtime paths. `HardcodedGlobalConfig` is the PoC placeholder
implementation with compile-time developer-machine constants.

## Known PoC limitation

`HardcodedGlobalConfig` uses hardcoded paths and connection strings. It is intentionally
equivalent in status to the fake session key in `JOSYN.Jap.JAPServer` — a known shortcut
that will be replaced by a real configuration source (file-based, registry, or company
config system) when the platform moves beyond the PoC phase.

## Usage

```csharp
IGlobalConfig config = new HardcodedGlobalConfig();
var store = new SessionStore(config.SessionStoreConnectionString);
```

## Dependencies

None. This package has no NuGet dependencies.

---

MIT License — Copyright © 2026 HAEVG AG
